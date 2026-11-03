using Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Users.Domain.Entities;
using Users.Domain.Enums;

namespace App.Infrastructure.Data;

/// <summary>
/// Класс для инициализации базы данных начальными данными
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Инициализирует базу данных начальными данными (администратор)
    /// </summary>
    /// <param name="context">Контекст базы данных</param>
    /// <param name="configuration">Конфигурация приложения (опционально)</param>
    /// <param name="logger">Логгер (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public static async Task InitializeAsync(
        AppDbContext context, 
        IConfiguration? configuration = null,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Получаем строку подключения
            var connectionString = context.Database.GetConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Строка подключения к базе данных не найдена");
            }

            logger?.LogInformation("🔍 Проверка существования базы данных...");

            // Создаём базу данных, если её нет
            await EnsureDatabaseExistsAsync(connectionString, logger, cancellationToken);

            // Проверяем доступность базы данных
            logger?.LogInformation("🔍 Проверка доступности базы данных...");
            var canConnect = await context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                logger?.LogWarning("⚠️  База данных недоступна. Будет создана автоматически при применении миграций...");
            }
            else
            {
                logger?.LogInformation("✅ База данных доступна");
            }

            // Применяем миграции (MigrateAsync автоматически создаст таблицы, если их нет)
            logger?.LogInformation("🔄 Применение миграций к базе данных...");
            await context.Database.MigrateAsync(cancellationToken);
            logger?.LogInformation("✅ Миграции применены успешно!");

            // После применения миграций получаем список применённых миграций для логирования
            try
            {
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync(cancellationToken);
                if (appliedMigrations.Any())
                {
                    logger?.LogInformation("Применённые миграции ({Count}):", appliedMigrations.Count());
                    foreach (var migration in appliedMigrations)
                    {
                        logger?.LogInformation("  ✓ {Migration}", migration);
                    }
                }
                else
                {
                    logger?.LogWarning("⚠️  Не найдено применённых миграций (возможно, база данных была создана вручную)");
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки при получении списка миграций для логирования
                // Главное - миграции уже применены
                logger?.LogWarning(ex, "⚠️  Не удалось получить список применённых миграций для логирования (это не критично)");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Ошибка при применении миграций");
            logger?.LogError("Детали ошибки: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                logger?.LogError("Внутренняя ошибка: {InnerMessage}", ex.InnerException.Message);
            }
            throw; // Пробрасываем исключение, чтобы приложение не запустилось с неактуальной БД
        }

        // Проверяем, существует ли уже администратор
        logger?.LogInformation("👤 Проверка наличия администратора...");
        var adminExists = await context.Users
            .AnyAsync(u => u.RoleId == (int)UserRoles.Admin && u.DeletedAt == default, cancellationToken);

        if (!adminExists)
        {
            // Получаем данные администратора из конфигурации или используем значения по умолчанию
            var adminLogin = configuration?["ADMIN_LOGIN"] 
                ?? Environment.GetEnvironmentVariable("ADMIN_LOGIN") 
                ?? "admin";
            
            var configuredAdminPassword = configuration?["ADMIN_PASSWORD"]
                ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            var isDevelopment = string.Equals(
                configuration?["ASPNETCORE_ENVIRONMENT"] ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(configuredAdminPassword) && !isDevelopment)
            {
                throw new InvalidOperationException("ADMIN_PASSWORD must be configured outside the Development environment.");
            }

            var adminPassword = configuredAdminPassword ?? "admin123";
            
            var adminEmail = configuration?["ADMIN_EMAIL"] 
                ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL") 
                ?? "admin@diskapi.local";
            
            var adminUsername = configuration?["ADMIN_USERNAME"] 
                ?? Environment.GetEnvironmentVariable("ADMIN_USERNAME") 
                ?? "Администратор Системы";

            // Создаем администратора
            var admin = new User
            {
                Login = adminLogin,
                Email = adminEmail,
                Username = adminUsername,
                Password = PasswordHash.Create(adminPassword),
                RoleId = (int)UserRoles.Admin
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync(cancellationToken);

            logger?.LogInformation("╔═══════════════════════════════════════════════════════════╗");
            logger?.LogInformation("║     ✅ Администратор успешно создан!                       ║");
            logger?.LogInformation("╚═══════════════════════════════════════════════════════════╝");
            logger?.LogInformation("   Login:    {Login}", adminLogin);
            logger?.LogInformation("   Password: задан через конфигурацию и не выводится в лог");
            logger?.LogInformation("   Email:    {Email}", adminEmail);
            logger?.LogInformation("   Username: {Username}", adminUsername);
            logger?.LogInformation("   Role:     Admin");
            logger?.LogInformation("");
            logger?.LogWarning("⚠️  ВАЖНО: Измените пароль администратора после первого входа!");
            logger?.LogInformation("");
        }
        else
        {
            logger?.LogInformation("ℹ️  Администратор уже существует в базе данных");
        }
    }

    /// <summary>
    /// Создаёт базу данных PostgreSQL, если она не существует
    /// </summary>
    private static async Task EnsureDatabaseExistsAsync(
        string connectionString,
        ILogger? logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database;
            
            // Временно подключаемся к postgres (системная база данных)
            builder.Database = "postgres";
            var masterConnectionString = builder.ToString();

            await using var connection = new NpgsqlConnection(masterConnectionString);
            await connection.OpenAsync(cancellationToken);

            // Проверяем, существует ли база данных
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
            
            var exists = await command.ExecuteScalarAsync(cancellationToken);

            if (exists == null)
            {
                logger?.LogWarning("⚠️  База данных '{DatabaseName}' не существует. Создание...", databaseName);
                
                // Создаём базу данных
                command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
                await command.ExecuteNonQueryAsync(cancellationToken);
                
                logger?.LogInformation("✅ База данных '{DatabaseName}' успешно создана", databaseName);
            }
            else
            {
                logger?.LogInformation("✅ База данных '{DatabaseName}' уже существует", databaseName);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Ошибка при проверке/создании базы данных");
            throw;
        }
    }
}

