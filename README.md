# 🗂️ DiskAPI - Файловая система на ASP.NET Core

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16+-336791?logo=postgresql)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Swagger](https://img.shields.io/badge/API-Swagger-85EA2D?logo=swagger)](http://localhost:8080/swagger)

Современный RESTful API для управления файлами и папками, построенный на **Clean Architecture** с использованием модульной монолитной архитектуры.

---

## 📋 Содержание

- [Обзор проекта](#-обзор-проекта)
- [Архитектура](#-архитектура)
- [Технологический стек](#-технологический-стек)
- [Структура проекта](#-структура-проекта)
- [Ключевые особенности](#-ключевые-особенности)
- [Быстрый старт](#-быстрый-старт)
- [Конфигурация](#-конфигурация)
- [API Документация](#-api-документация)
- [Безопасность](#-безопасность)
- [Разработка](#-разработка)

---

## 🎯 Обзор проекта

**DiskAPI** — это высокопроизводительный backend для управления файловой системой, который предоставляет:

- 📁 **Управление файлами и папками** с иерархической структурой
- 👥 **Система пользователей** с ролевой моделью доступа
- 🔐 **Cookie-based аутентификация** для безопасного доступа
- 📊 **Централизованное логирование** всех операций
- 🔄 **Интеграция с Sentry** для мониторинга ошибок
- 📝 **Swagger UI** для интерактивной документации API

---

## 🏗️ Архитектура

### Архитектурный подход

Проект реализован по принципам **Clean Architecture** и **Modular Monolith**, что обеспечивает:

- ✅ Высокую модульность и слабую связанность
- ✅ Независимость от фреймворков и внешних зависимостей
- ✅ Легкость тестирования
- ✅ Возможность миграции к микросервисам

### Диаграмма архитектуры

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │  Controllers │  │  Middleware  │  │   Filters    │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                     Application Layer                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   Services   │  │    Mappers   │  │     DTOs     │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                       Domain Layer                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │   Entities   │  │  Value Obj   │  │  Interfaces  │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │ Repositories │  │   DbContext  │  │  Extensions  │       │
│  └──────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
```

### Модульная структура

```
┌──────────────────────────────────────────────────────────────┐
│                         App Module                            │
│              (Entry Point & Orchestration)                    │
└──────────────────────────────────────────────────────────────┘
        │                    │                    │
        ▼                    ▼                    ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│ Access Module │  │ Files Module  │  │ Users Module  │
│               │  │               │  │               │
│ • Auth        │  │ • Files       │  │ • Users       │
│ • Identity    │  │ • Folders     │  │ • Roles       │
│ • Tokens      │  │ • Upload      │  │ • CRUD        │
└───────────────┘  └───────────────┘  └───────────────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             ▼
                    ┌─────────────────┐
                    │  Common Module  │
                    │                 │
                    │ • Abstractions  │
                    │ • DTOs          │
                    │ • Middleware    │
                    │ • Extensions    │
                    └─────────────────┘
```

---

## 🛠️ Технологический стек

### Backend Framework
- **ASP.NET Core 9.0** - Современный веб-фреймворк
- **C# 12** - Последняя версия языка с новейшими фичами

### База данных
- **PostgreSQL 16+** - Надежная реляционная СУБД
- **Entity Framework Core 9.0** - ORM для работы с БД
- **Npgsql** - PostgreSQL provider для EF Core

### Аутентификация и авторизация
- **Cookie Authentication** - Встроенный механизм ASP.NET Core
- **Custom Middleware** - Кастомные middleware для проверки прав
- **Role-based Access Control** - Гибкая система ролей

### API Documentation
- **Swashbuckle.AspNetCore 9.0** - Генерация OpenAPI спецификации
- **Swagger UI** - Интерактивная документация API

### Мониторинг и логирование
- **Sentry** - Отслеживание ошибок в production
- **ILogger** - Встроенное логирование ASP.NET Core
- **Custom LogService** - Централизованный сервис логирования

### DevOps
- **Docker** - Контейнеризация приложения
- **Docker Compose** - Оркестрация контейнеров
- **DotNetEnv** - Управление переменными окружения

### Дополнительные инструменты
- **AutoMapper Pattern** - Маппинг между сущностями и DTO
- **Repository Pattern** - Абстракция доступа к данным
- **Generic Repository** - Универсальный репозиторий

---

## 📂 Структура проекта

```
DiskAPI/
├── src/
│   ├── App/                              # 🚀 Главный модуль приложения
│   │   ├── Controllers/                  # API контроллеры
│   │   │   ├── AppFilesController.cs     # Загрузка/скачивание файлов
│   │   │   ├── HealthController.cs       # Health check
│   │   │   └── LogsController.cs         # Управление логами
│   │   ├── Infrastructure/
│   │   │   ├── Data/
│   │   │   │   ├── AppDbContext.cs       # Главный DbContext
│   │   │   │   └── AppFile.cs            # Сущность файла
│   │   │   └── Services/
│   │   │       └── LogService.cs         # Сервис логирования
│   │   ├── Migrations/                   # EF Core миграции
│   │   ├── Application/                  # DTOs и маппинг
│   │   ├── Program.cs                    # 🎯 Entry point
│   │   ├── appsettings.json              # Конфигурация
│   │   └── App.csproj                    # Проект файл
│   │
│   ├── Common/                           # 📦 Общий модуль
│   │   ├── Abstracts/                    # Базовые классы
│   │   │   ├── BaseController.cs         # Базовый контроллер с методами
│   │   │   ├── BaseEntity.cs             # Базовая сущность
│   │   │   ├── BaseRequest.cs            # Базовый запрос
│   │   │   ├── GenericRepository.cs      # Generic Repository
│   │   │   └── IDbContext.cs             # Интерфейс DbContext
│   │   ├── Attributes/                   # Кастомные атрибуты
│   │   │   ├── RequireAuthAttribute.cs   # Атрибут авторизации
│   │   │   └── RequireRoleAttribute.cs   # Атрибут проверки роли
│   │   ├── Constants/
│   │   │   └── Constants.cs              # Константы приложения
│   │   ├── DTOs/                         # Data Transfer Objects
│   │   │   ├── ApiResponse.cs            # 🎯 Унифицированный ответ API
│   │   │   ├── PagedResponse.cs          # Пагинация
│   │   │   └── UserDTOs.cs               # DTOs пользователей
│   │   ├── Exceptions/                   # Кастомные исключения
│   │   ├── Extensions/                   # Extension методы
│   │   │   ├── EncryptExtension.cs       # Шифрование
│   │   │   ├── HashExtension.cs          # Хеширование
│   │   │   └── MiddlewareExtension.cs    # Регистрация middleware
│   │   ├── Infrastructure/
│   │   │   ├── Data/
│   │   │   │   └── Log.cs                # Сущность лога
│   │   │   ├── Middleware/               # Кастомные middleware
│   │   │   │   ├── AuthentificationMiddleware.cs
│   │   │   │   └── AuthorizationMiddleware.cs
│   │   │   └── Services/
│   │   │       └── ILogService.cs        # Интерфейс сервиса логов
│   │   └── Common.csproj
│   │
│   └── Modules/                          # 🧩 Бизнес-модули
│       │
│       ├── Access/                       # 🔐 Модуль аутентификации
│       │   ├── Application/
│       │   │   ├── DTOs/                 # Request/Response объекты
│       │   │   ├── Interfaces/
│       │   │   │   └── IAccessService.cs
│       │   │   └── Services/
│       │   │       └── AccessService.cs  # Логика аутентификации
│       │   ├── Domain/
│       │   │   └── Entities/
│       │   │       └── AccessToken.cs    # Токен доступа
│       │   ├── Infrastructure/
│       │   │   └── Repositories/
│       │   │       └── AccessTokenRepository.cs
│       │   ├── Presentation/
│       │   │   └── AccessController.cs   # API эндпоинты
│       │   └── Access.csproj
│       │
│       ├── Files/                        # 📁 Модуль файлов
│       │   ├── Application/
│       │   │   ├── DTOs/
│       │   │   │   ├── Base/
│       │   │   │   │   ├── FileDTO.cs
│       │   │   │   │   └── FolderDTO.cs
│       │   │   │   ├── Requests/         # CRUD запросы
│       │   │   │   └── Responses/        # CRUD ответы
│       │   │   ├── Interfaces/
│       │   │   │   ├── IFileService.cs
│       │   │   │   └── IFolderService.cs
│       │   │   ├── Mappers/              # Маппинг Entity ↔ DTO
│       │   │   │   ├── FileMapper.cs
│       │   │   │   └── FolderMapper.cs
│       │   │   └── Services/
│       │   │       ├── FileService.cs    # Логика работы с файлами
│       │   │       └── FolderService.cs  # Логика работы с папками
│       │   ├── Domain/
│       │   │   └── Entities/
│       │   │       ├── File.cs           # Сущность файла
│       │   │       └── Folder.cs         # Сущность папки
│       │   ├── Infrastructure/
│       │   │   └── Repository/
│       │   │       ├── FileRepository.cs
│       │   │       └── FolderRepository.cs
│       │   ├── Presentation/
│       │   │   └── Controllers/
│       │   │       ├── FilesController.cs
│       │   │       └── FoldersController.cs
│       │   └── Files.csproj
│       │
│       └── Users/                        # 👥 Модуль пользователей
│           ├── Application/
│           │   ├── DTOs/
│           │   │   ├── Requests/         # CRUD запросы
│           │   │   └── Responses/        # CRUD ответы
│           │   ├── Interfaces/
│           │   │   └── IUserService.cs
│           │   ├── Mappers/
│           │   │   └── UserMapper.cs
│           │   └── Services/
│           │       └── UserService.cs    # Логика управления пользователями
│           ├── Domain/
│           │   ├── Entities/
│           │   │   └── User.cs           # Сущность пользователя
│           │   └── Enums/
│           │       └── UserRoles.cs      # Роли пользователей
│           ├── Infrastructure/
│           │   └── Repositories/
│           │       └── UserRepository.cs
│           ├── Presentstion/
│           │   └── Controllers/
│           │       └── UsersController.cs
│           └── Users.csproj
│
├── docker-compose.yml                    # Docker Compose конфигурация
├── Dockerfile                            # Docker образ
├── .env                                  # Переменные окружения
├── SWAGGER_SETUP.md                      # Документация Swagger
└── README.md                             # Этот файл
```

---

## ✨ Ключевые особенности

### 1. 🎯 Унифицированный формат ответов API

Все API эндпоинты возвращают стандартизированный формат ответа:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }          // Статус операции
    public T? Data { get; set; }               // Данные ответа
    public string? Message { get; set; }       // Сообщение пользователю
    public string? Error { get; set; }         // Код ошибки
    public Dictionary<string, string[]>? ValidationErrors { get; set; }  // Ошибки валидации
}
```

**Пример успешного ответа:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "example.pdf",
    "folder_id": 5
  },
  "message": "Файл успешно создан",
  "error": null,
  "validation_errors": null
}
```

**Пример ответа с ошибкой валидации:**
```json
{
  "success": false,
  "data": null,
  "message": "Ошибки валидации",
  "error": "VALIDATION_ERROR",
  "validation_errors": {
    "name": ["Поле обязательно для заполнения"],
    "folder_id": ["Значение должно быть больше 0"]
  }
}
```

### 2. 🔐 Многоуровневая система безопасности

#### Cookie-based Authentication
```csharp
[HttpPost("authentificate")]
public async Task<ActionResult<ApiResponse<AuthentificateResponse>>> AuthentificateAsync(
    [FromBody] AuthentificateRequest request)
{
    // Валидация credentials
    AccessToken accessToken = await _service.AuthentificateAsync(
        request.Login, 
        request.Password, 
        cancellationToken);
    
    // Установка Cookie
    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme, 
        new ClaimsPrincipal(claimsIdentity));
}
```

#### Кастомные атрибуты авторизации
```csharp
[RequireAuth]                                    // Требует авторизации
[RequireRole(EUserRoles.Admin)]                  // Только админы
[RequireRole(EUserRoles.Moderator, EUserRoles.Admin)]  // Модераторы или админы
public async Task<ActionResult> ProtectedEndpoint()
{
    // Логика защищенного эндпоинта
}
```

#### Middleware Pipeline
```csharp
app.UseAuthentication();                         // Проверка Cookie
app.UseAuthenticationMiddleware();               // Кастомная проверка [RequireAuth]
app.UseAuthorizationMiddleware();                // Кастомная проверка [RequireRole]
app.UseAuthorization();                          // Встроенная авторизация
```

### 3. 📊 Централизованное логирование

```csharp
public interface ILogService
{
    Task LogAsync(long? userId, string action, string entityType, 
                  long? entityId, string description, CancellationToken ct);
    
    Task LogFromHttpContextAsync(HttpContext httpContext, string action, 
                                  string entityType, long? entityId, 
                                  string description, CancellationToken ct);
}
```

**Автоматическое логирование операций:**
```csharp
await _logService.LogFromHttpContextAsync(
    HttpContext,
    "FILE_CREATED",
    "File",
    file.Id,
    $"Создан файл: {file.Name}",
    cancellationToken);
```

### 4. 🗃️ Generic Repository Pattern

```csharp
public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> SelectByIdAsync(long id, CancellationToken ct);
    Task<IEnumerable<TEntity>> SelectAllAsync(CancellationToken ct);
    Task<TEntity> InsertAsync(TEntity entity, CancellationToken ct);
    Task UpdateAsync(TEntity entity, CancellationToken ct);
    Task DeleteAsync(long id, CancellationToken ct);
    Task<PagedResponse<TEntity>> SelectPagedAsync(int page, int perPage, CancellationToken ct);
}
```

### 5. 🎨 Snake_case JSON Serialization

Автоматическая конвертация всех JSON полей в snake_case:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    });
```

**C# Property** → **JSON Field**
- `FolderId` → `folder_id`
- `CreatedAt` → `created_at`
- `ValidationErrors` → `validation_errors`

### 6. 🔄 Пагинация из коробки

```csharp
public class PagedResponse<T>
{
    public IEnumerable<T> Items { get; set; }    // Элементы страницы
    public int TotalCount { get; set; }          // Всего элементов
    public int Page { get; set; }                // Текущая страница
    public int PerInPage { get; set; }           // Элементов на странице
    public int TotalPages { get; set; }          // Всего страниц
}
```

### 7. 📤 Загрузка файлов с UUID

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<List<Guid>>>> UploadAsync()
{
    foreach (var file in files)
    {
        var uuid = Guid.NewGuid();
        var fileExtension = Path.GetExtension(file.FileName);
        var uuidFileName = uuid.ToString() + fileExtension;
        
        // Сохранение файла
        // Создание записи в БД с UUID
    }
}
```

---

## 🚀 Быстрый старт

### Предварительные требования

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/get-started) (опционально)

### Локальная разработка

#### 1. Клонирование репозитория

```bash
git clone https://github.com/your-username/DiskAPI.git
cd DiskAPI
```

#### 2. Настройка переменных окружения

Создайте файл `.env` в корне проекта:

```env
# База данных
DB_HOST=localhost
DB_PORT=5432
DB_DATABASE=diskapi
DB_USERNAME=postgres
DB_PASSWORD=postgres

# Базовый путь API (опционально)
BASE_PATH=/st

# Sentry (опционально)
SENTRY_DSN=
LOG_LEVEL=ERROR
```

#### 3. Создание базы данных

```bash
# Подключитесь к PostgreSQL
psql -U postgres

# Создайте базу данных
CREATE DATABASE diskapi;
\q
```

#### 4. Применение миграций

```bash
cd src/App
dotnet ef database update
```

#### 5. Запуск приложения

```bash
dotnet run
```

Приложение будет доступно по адресу: `http://localhost:8080`

#### 6. Открытие Swagger UI

Откройте в браузере: `http://localhost:8080/swagger`

### 🐳 Docker

#### Запуск с Docker Compose

```bash
docker-compose up -d
```

Это запустит:
- PostgreSQL контейнер
- DiskAPI контейнер

### Автоматическое создание администратора

При первом запуске приложения автоматически создается администратор с учетными данными по умолчанию:

**Учетные данные по умолчанию:**
- **Login:** `admin`
- **Password:** `admin123`
- **Email:** `admin@diskapi.local`
- **Username:** `Администратор Системы`
- **Role:** `Admin` (ID: 1)

⚠️ **ВАЖНО:** Измените пароль администратора после первого входа!

### Настройка данных администратора

Вы можете настроить данные администратора через переменные окружения в файле `.env`:

```env
# Настройки администратора (опционально)
ADMIN_LOGIN=admin
ADMIN_PASSWORD=admin123
ADMIN_EMAIL=admin@diskapi.local
ADMIN_USERNAME=Администратор Системы
```

### Создание дополнительных пользователей

Для создания дополнительных пользователей используйте Swagger UI или API:

```bash
curl -X POST "http://localhost:8080/st/api/v1/users" \
  -H "Content-Type: application/json" \
  -H "Cookie: AuthCookie=..." \
  -d '{
    "login": "user1",
    "password": "password123",
    "email": "user1@example.com",
    "first_name": "Иван",
    "last_name": "Иванов",
    "role_id": 3
  }'
```

**Примечание:** Для создания пользователей требуется авторизация с ролью Admin.

---

## ⚙️ Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=diskapi;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Переменные окружения

| Переменная | Описание | По умолчанию |
|------------|----------|--------------|
| `DB_HOST` | Хост PostgreSQL | `localhost` |
| `DB_PORT` | Порт PostgreSQL | `5432` |
| `DB_DATABASE` | Имя БД | `diskapi` |
| `DB_USERNAME` | Пользователь БД | `postgres` |
| `DB_PASSWORD` | Пароль БД | `postgres` |
| `BASE_PATH` | Базовый путь API | `/st` |
| `SENTRY_DSN` | DSN для Sentry | `` |
| `LOG_LEVEL` | Уровень логирования | `ERROR` |
| `ADMIN_LOGIN` | Логин администратора (при первом запуске) | `admin` |
| `ADMIN_PASSWORD` | Пароль администратора (при первом запуске) | `admin123` |
| `ADMIN_EMAIL` | Email администратора (при первом запуске) | `admin@diskapi.local` |
| `ADMIN_USERNAME` | Имя администратора (при первом запуске) | `Администратор Системы` |

### Роли пользователей

```csharp
public enum EUserRoles
{
    User = 1,        // Обычный пользователь
    Moderator = 2,   // Модератор
    Admin = 3        // Администратор
}
```

---

## 📚 API Документация

### Базовый URL

```
http://localhost:8080/st/api/v1
```

### Swagger UI

Интерактивная документация доступна по адресу:
```
http://localhost:8080/swagger
```

### Группы эндпоинтов

#### 🔐 Access (Аутентификация)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| POST | `/access/authentificate` | Авторизация пользователя | ❌ |
| POST | `/access/identify` | Идентификация текущего пользователя | ❌ |
| DELETE | `/access/logout` | Выход из системы | ❌ |

#### 👥 Users (Пользователи)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/users` | Список пользователей | 👑 Admin |
| POST | `/users` | Создание пользователя | 👑 Admin |
| PUT | `/users` | Обновление пользователя | 👑 Admin |
| DELETE | `/users?id={id}` | Удаление пользователя | 👑 Admin |

#### 📁 Files (Файлы)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/files?group_id={id}` | Список файлов в папке | ❌ |
| POST | `/files` | Создание файла | 👮 Moderator/Admin |
| PUT | `/files` | Обновление файла | 👮 Moderator/Admin |
| DELETE | `/files?id={id}` | Удаление файла | 👮 Moderator/Admin |

#### 📂 File Folders (Папки)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/files/folders` | Список папок | ❌ |
| POST | `/files/folders` | Создание папки | 👮 Moderator/Admin |
| PUT | `/files/folders` | Обновление папки | 👮 Moderator/Admin |
| DELETE | `/files/folders?id={id}` | Удаление папки | 👮 Moderator/Admin |

#### 📤 Upload Files (Загрузка файлов)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/upload/files?uuid={uuid}` | Скачивание файла | ❌ |
| POST | `/upload/files` | Загрузка файлов | 👮 Moderator/Admin |

#### 📊 Logs (Логи)

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/logs` | Список логов | 👑 Admin |

#### 💚 Health

| Метод | Endpoint | Описание | Авторизация |
|-------|----------|----------|-------------|
| GET | `/` | Health check | ❌ |

### Примеры запросов

#### Авторизация

```bash
curl -X POST "http://localhost:8080/st/api/v1/access/authentificate" \
  -H "Content-Type: application/json" \
  -d '{
    "login": "admin",
    "password": "admin123"
  }'
```

**Ответ:**
```json
{
  "success": true,
  "data": {},
  "message": "Authentificate is successfull",
  "error": null
}
```

#### Создание папки

```bash
curl -X POST "http://localhost:8080/st/api/v1/files/folders" \
  -H "Content-Type: application/json" \
  -H "Cookie: AuthCookie=..." \
  -d '{
    "parent_id": null,
    "name": "Documents"
  }'
```

#### Получение списка файлов

```bash
curl -X GET "http://localhost:8080/st/api/v1/files?group_id=1&per_in_page=20&page=1"
```

**Ответ:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "folder_id": 1,
        "name": "document.pdf",
        "created_at": "2025-11-29T10:00:00Z",
        "updated_at": "2025-11-29T10:00:00Z"
      }
    ],
    "total_count": 1,
    "page": 1,
    "per_in_page": 20,
    "total_pages": 1
  },
  "message": "Файлы успешно получены"
}
```

---

## 🔒 Безопасность

### Аутентификация

- **Cookie-based** аутентификация с настраиваемым временем жизни
- Токены хранятся в защищенной куке `AuthCookie`
- Автоматическое продление сессии (sliding expiration)

### Хеширование паролей

```csharp
public static class HashExtension
{
    public static string ComputeSHA256Hash(this string input)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
```

### Middleware защиты

1. **AuthenticationMiddleware** - проверяет наличие и валидность токена
2. **AuthorizationMiddleware** - проверяет права доступа на основе ролей
3. **CORS Policy** - настроенная политика кросс-доменных запросов

### Best Practices

- ✅ Пароли хешируются с использованием SHA256
- ✅ Используется HTTPS в production
- ✅ Настроена защита от CSRF
- ✅ Интеграция с Sentry для мониторинга безопасности
- ✅ Логирование всех важных операций

---

## 💻 Разработка

### Создание нового модуля

```bash
# Структура нового модуля
src/Modules/NewModule/
├── Application/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Services/
├── Domain/
│   └── Entities/
├── Infrastructure/
│   └── Repositories/
└── Presentation/
    └── Controllers/
```

### Добавление новой миграции

```bash
cd src/App
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Запуск тестов

```bash
dotnet test
```

### Code Style

Проект следует стандартным соглашениям C#:
- **PascalCase** для классов, методов, свойств
- **camelCase** для локальных переменных
- **UPPER_CASE** для констант
- **snake_case** для JSON полей (автоматически)

### Структура коммитов

```
type(scope): subject

body

footer
```

Типы:
- `feat` - новая функциональность
- `fix` - исправление бага
- `docs` - изменения в документации
- `refactor` - рефакторинг кода
- `test` - добавление тестов
- `chore` - рутинные задачи

---

## 📈 Performance

### Оптимизации

- ✅ Асинхронные операции везде где возможно
- ✅ Connection pooling для PostgreSQL
- ✅ Lazy loading отключен для предотвращения N+1 проблемы
- ✅ Индексы на часто запрашиваемых полях
- ✅ Пагинация для больших списков

### Мониторинг

- **Sentry** - отслеживание ошибок и производительности
- **ILogger** - детальное логирование операций
- **Swagger** - мониторинг времени ответов эндпоинтов

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Автор

**Ваше имя**
- GitHub: [@your-username](https://github.com/your-username)
- Email: your.email@example.com

---

## 🙏 Благодарности

- [ASP.NET Core](https://github.com/dotnet/aspnetcore)
- [Entity Framework Core](https://github.com/dotnet/efcore)
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [Npgsql](https://github.com/npgsql/npgsql)
- [Sentry](https://sentry.io/)

---

<div align="center">

**[⬆ Вернуться к началу](#-diskapi---файловая-система-на-aspnet-core)**

Made with ❤️ using ASP.NET Core

</div>

