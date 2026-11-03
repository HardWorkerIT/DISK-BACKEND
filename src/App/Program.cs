using Access.Application.Interfaces;
using Access.Application.Services;
using Access.Presentation;
using App.Controllers;
using App.Infrastructure.Data;
using Common.Constants;
using Common.DTOs;
using Common.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Standards.Presentation.Controllers;
using System.Data;
using System.Reflection.Metadata;
using System.Text;
using Users.Application.Interfaces;
using Users.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using DotNetEnv;
using Sentry;
using Files.Application.Interfaces;
using Files.Application.Services;
using Common.Infrastructure.Services;
using App.Infrastructure.Services;
using App.Infrastructure.RateLimiting;

// Загружаем переменные окружения из .env файла
// Ищем файл .env в корне проекта, поднимаясь вверх от текущей директории
var envFile = ".env";
var searchDir = new DirectoryInfo(Directory.GetCurrentDirectory());
string? envPath = null;
while (searchDir != null)
{
    var path = Path.Combine(searchDir.FullName, envFile);
    if (File.Exists(path))
    {
        envPath = path;
        Env.Load(path);
        Console.WriteLine($"[INFO] Загружен .env файл из: {envPath}");
        break;
    }
    searchDir = searchDir.Parent;
}

var builder = WebApplication.CreateBuilder(args);

// Отключаем отслеживание изменений файлов конфигурации для решения проблемы с лимитом inotify
// Это предотвращает создание множества FileSystemWatcher instances
builder.Configuration.Sources.OfType<Microsoft.Extensions.Configuration.Json.JsonConfigurationSource>()
    .ToList()
    .ForEach(source => source.ReloadOnChange = false);

// Добавляем переменные окружения в конфигурацию ASP.NET Core
// Это гарантирует, что они будут доступны через builder.Configuration
builder.Configuration.AddEnvironmentVariables();

// Настройка Sentry для логирования ошибок
var sentryDsn = Environment.GetEnvironmentVariable("SENTRY_DSN")
    ?? builder.Configuration["SENTRY_DSN"]
    ?? string.Empty;
var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL")
    ?? builder.Configuration["LOG_LEVEL"]
    ?? "ERROR";
var isDebugMode = logLevel.Equals("DEBUG", StringComparison.OrdinalIgnoreCase);

if (!string.IsNullOrEmpty(sentryDsn))
{
    builder.WebHost.UseSentry(options =>
    {
        options.Dsn = sentryDsn;
        options.Debug = isDebugMode;

        // Настройка уровня логирования
        if (isDebugMode)
        {
            // В режиме DEBUG отправляем все логи (Debug, Info, Warning, Error)
            options.TracesSampleRate = 1.0; // 100% трассировок
            options.ProfilesSampleRate = 1.0; // 100% профилирования
            options.AttachStacktrace = true;
            options.SendDefaultPii = true; // Отправлять персональные данные для отладки
        }
        else
        {
            // В режиме ERROR отправляем только критические ошибки
            options.TracesSampleRate = 0.1; // 10% трассировок
            options.ProfilesSampleRate = 0.1; // 10% профилирования
            options.AttachStacktrace = true;
            options.SendDefaultPii = false; // Не отправлять персональные данные
            // Отправляем только Error и выше
            options.MinimumBreadcrumbLevel = LogLevel.Warning;
            options.MinimumEventLevel = LogLevel.Error;
        }

        options.Environment = builder.Environment.EnvironmentName;
        options.Release = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    });

    Console.WriteLine($"[INFO] Sentry настроен. Режим логирования: {logLevel}");
}
else
{
    Console.WriteLine("[WARN] SENTRY_DSN не указан, Sentry отключен");
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Forbidden/";
        options.Cookie.Name = Constants.AuthCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = async context =>
        {
            var token = context.Principal?.FindFirst("User")?.Value;
            var userId = context.Principal?.FindFirst("UserId")?.Value;
            var roleId = context.Principal?.FindFirst("RoleId")?.Value;
            UserDTOWithoutPassword? user = null;
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var accessService = context.HttpContext.RequestServices.GetRequiredService<IAccessService>();
                    user = await accessService.IdentifyAsync(token, context.HttpContext.RequestAborted);
                }
                catch (ArgumentNullException)
                {
                    // A missing, expired or revoked token is an invalid session.
                }
            }

            if (user is null || userId != user.Id.ToString() || roleId != user.RoleId.ToString())
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Разрешаем доступ к Swagger без авторизации
    options.FallbackPolicy = null; // Не требуем авторизацию по умолчанию
});

builder.Services.AddAuthenticationRateLimiting();

// Add services to the container.
// Регистрируем контроллеры из всех сборок
var mvcBuilder = builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Настраиваем JSON сериализацию для использования snake_case
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

// Настраиваем кастомный формат ответа для ошибок валидации
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = new Dictionary<string, string[]>();
        var namingPolicy = JsonNamingPolicy.SnakeCaseLower;

        foreach (var keyValuePair in context.ModelState)
        {
            var key = keyValuePair.Key;
            // Преобразуем ключ в snake_case
            var snakeCaseKey = namingPolicy.ConvertName(key);

            var errorMessages = keyValuePair.Value.Errors
                .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Ошибка валидации" : e.ErrorMessage)
                .ToArray();

            if (errorMessages.Length > 0)
            {
                errors[snakeCaseKey] = errorMessages;
            }
        }

        var response = Common.DTOs.ApiResponse.ErrorResult(
            "VALIDATION_ERROR",
            "Ошибки валидации",
            errors
        );

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
    };

    // Отключаем автоматическое использование ProblemDetails
    options.SuppressMapClientErrors = true;
});


// Настройка Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Основная информация об API
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Disk Backend API",
        Description = "API для управления файлами и папками",
        Contact = new OpenApiContact
        {
            Name = "API Support"
        }
    });

    // Настраиваем сериализацию с использованием snake_case для схем
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    // Добавляем поддержку Cookie Authentication в Swagger
    options.AddSecurityDefinition("CookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = Constants.AuthCookieName,
        Description = "Авторизация через Cookie. Используйте эндпоинт /api/v1/access/authentificate для получения cookie."
    });

    // Применяем требование авторизации ко всем операциям
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CookieAuth"
                }
            },
            Array.Empty<string>()
        }
    });

    // Группировка эндпоинтов по тегам (из ApiExplorerSettings)
    options.TagActionsBy(api =>
    {
        if (!string.IsNullOrEmpty(api.GroupName))
            return new[] { api.GroupName };

        var controllerName = api.ActionDescriptor?.RouteValues?.ContainsKey("controller") == true
            ? api.ActionDescriptor.RouteValues["controller"]
            : null;

        return new[] { controllerName ?? "Default" };
    });

    // Включаем все API эндпоинты
    options.DocInclusionPredicate((name, api) => true);

    // Включаем XML комментарии для документации из всех модулей
    var xmlFiles = new[]
    {
        "App.xml",
        "Files.xml",
        "Users.xml",
        "Access.xml",
        "Common.xml"
    };

    foreach (var xmlFileName in xmlFiles)
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        }
    }
});

// Add Entity Framework
// Настройка строки подключения к PostgreSQL
// Приоритет: переменные окружения > appsettings.json > значения по умолчанию

// Получаем строку подключения из appsettings.json (если есть)
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString;

if (!string.IsNullOrEmpty(defaultConnectionString))
{
    // Используем строку подключения из appsettings.json
    connectionString = defaultConnectionString;
    Console.WriteLine("[INFO] Используется строка подключения из appsettings.json");
}
else
{
    // Формируем строку подключения из переменных окружения или значений по умолчанию
    var dbHost = builder.Configuration["DB_HOST"]
        ?? Environment.GetEnvironmentVariable("DB_HOST")
        ?? "localhost";

    var dbPort = builder.Configuration["DB_PORT"]
        ?? Environment.GetEnvironmentVariable("DB_PORT")
        ?? "5432";

    var dbDatabase = builder.Configuration["DB_DATABASE"]
        ?? Environment.GetEnvironmentVariable("DB_DATABASE")
        ?? "diskapi";  // Исправлено: было "standardbackend"

    var dbUsername = builder.Configuration["DB_USERNAME"]
        ?? Environment.GetEnvironmentVariable("DB_USERNAME")
        ?? "postgres";

    var dbPassword = builder.Configuration["DB_PASSWORD"]
        ?? Environment.GetEnvironmentVariable("DB_PASSWORD")
        ?? "postgres";

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbDatabase};Username={dbUsername};Password={dbPassword}";

    // Логируем используемые значения (без пароля)
    if (builder.Environment.IsDevelopment())
    {
        Console.WriteLine("[DEBUG] Формирование строки подключения из переменных окружения:");
        Console.WriteLine($"[DEBUG]   DB_HOST: {dbHost}");
        Console.WriteLine($"[DEBUG]   DB_PORT: {dbPort}");
        Console.WriteLine($"[DEBUG]   DB_DATABASE: {dbDatabase}");
        Console.WriteLine($"[DEBUG]   DB_USERNAME: {dbUsername}");
        Console.WriteLine($"[DEBUG] Connection String: Host={dbHost};Port={dbPort};Database={dbDatabase};Username={dbUsername};Password=***");
    }
}

// Регистрация DbContext с PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        // Включаем детальное логирование SQL запросов в режиме разработки
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register IDbContext
builder.Services.AddScoped<Common.Abstracts.IDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());
builder.Services.AddSingleton<Common.Infrastructure.Metrics.HttpMetricsRegistry>();
builder.Services.AddSingleton<App.Infrastructure.Storage.IFileStorage, App.Infrastructure.Storage.LocalFileStorage>();

// Add module services
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccessService, AccessService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddHostedService<TrashCleanupService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin() // Note: cannot use AllowCredentials with AllowAnyOrigin
              .AllowAnyMethod()
              .AllowAnyHeader();
        // For cookie support, uncomment and specify actual origins:
        // policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
        //       .AllowAnyMethod()
        //       .AllowAnyHeader()
        //       .AllowCredentials();
    });
});


var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Инициализация базы данных (применение миграций и создание администратора при первом запуске)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        logger.LogInformation("🚀 Начало инициализации базы данных...");

        var context = services.GetRequiredService<AppDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();

        // Инициализация включает:
        // 1. Автоматическое применение всех ожидающих миграций
        // 2. Создание администратора, если его нет
        await DatabaseInitializer.InitializeAsync(context, configuration, logger);

        logger.LogInformation("✅ Инициализация базы данных завершена успешно");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "❌ КРИТИЧЕСКАЯ ОШИБКА: Не удалось инициализировать базу данных");
        logger.LogError("Приложение не может быть запущено без инициализированной базы данных");

        // В production окружении прерываем запуск приложения
        if (app.Environment.IsProduction())
        {
            throw;
        }

        // В development окружении даём возможность продолжить (для отладки)
        logger.LogWarning("⚠️  Продолжение запуска в режиме разработки (база данных может быть недоступна)");
    }
}

// Настройка логирования в зависимости от уровня
if (isDebugMode)
{
    // В режиме DEBUG логируем все уровни
    app.Logger.LogInformation("🔍 Режим логирования: DEBUG - все логи будут отправлены в Sentry");
}
else
{
    // В режиме ERROR логируем только критические ошибки
    app.Logger.LogInformation("⚠️  Режим логирования: ERROR - только критические ошибки будут отправлены в Sentry");
}

// Получаем logger для логирования миграций (если еще не создан)
logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.

// Настраиваем базовый путь приложения
var basePath = Environment.GetEnvironmentVariable("BASE_PATH")
    ?? builder.Configuration["BASE_PATH"]
    ?? "/st";

if (!string.IsNullOrEmpty(basePath) && basePath != "/")
{
    app.UsePathBase(basePath);
}

// Настраиваем Swagger middleware
// Swagger JSON будет доступен по адресу: {basePath}/swagger/v1/swagger.json
app.UseSwagger();

// Swagger UI будет доступен по адресу: {basePath}/swagger
app.UseSwaggerUI(options =>
{
    // Настраиваем путь к JSON спецификации
    // Используем относительный путь, который учитывает PathBase автоматически
    options.SwaggerEndpoint("v1/swagger.json", "Disk Backend API v1");

    // Swagger UI будет доступен по пути /swagger
    options.RoutePrefix = "swagger";

    // Настройки интерфейса
    options.DocumentTitle = "Disk Backend API Documentation";
    options.EnableDeepLinking();
    options.EnableFilter();
    options.DisplayRequestDuration();
    options.DisplayOperationId();

    // Настройки по умолчанию
    options.DefaultModelsExpandDepth(2);
    options.DefaultModelExpandDepth(2);
    options.EnableTryItOutByDefault();
});


// Используем Sentry для отслеживания ошибок (должен быть первым в pipeline)
if (!string.IsNullOrEmpty(sentryDsn))
{
    app.UseSentryTracing();
}

app.UseExceptionHandlingMiddleware();
app.UseMiddleware<Common.Infrastructure.Middleware.HttpMetricsMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting(); // Необходимо для доступа к endpoint metadata в middleware
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthenticationMiddleware(); // Middleware для проверки авторизации (работает с атрибутом [RequireAuth])
app.UseAuthorizationMiddleware(); // Middleware для проверки прав доступа на основе ролей (работает с атрибутом [RequireRole])
app.UseAuthorization();
app.MapControllers();

app.Run();
