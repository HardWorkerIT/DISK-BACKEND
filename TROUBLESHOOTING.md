# 🔧 Решение проблемы подключения к PostgreSQL

## ❌ Ошибка

```
System.InvalidOperationException: An exception has been raised that is likely due to a transient failure.
---> Npgsql.NpgsqlException (0x80004005): Failed to connect to 127.0.0.1:5432
---> System.Net.Sockets.SocketException (111): Connection refused
```

**Причина:** PostgreSQL не запущен на порту 5432.

---

## ✅ Решения

### Решение 1: Запуск PostgreSQL через Docker (Рекомендуется)

#### Windows (PowerShell)

```powershell
# Запустить скрипт
.\start-db.ps1
```

#### Linux/macOS

```bash
# Сделать скрипт исполняемым
chmod +x start-db.sh

# Запустить скрипт
./start-db.sh
```

#### Альтернативный способ (вручную)

```bash
# Запустить только PostgreSQL
docker-compose -f docker-compose.dev.yml up -d

# Проверить статус
docker-compose -f docker-compose.dev.yml ps

# Посмотреть логи
docker-compose -f docker-compose.dev.yml logs -f postgres
```

---

### Решение 2: Использование локального PostgreSQL

Если у вас установлен PostgreSQL локально:

#### Windows

```powershell
# Запустить PostgreSQL сервис
Start-Service -Name postgresql*

# ИЛИ через pg_ctl
pg_ctl -D "C:\Program Files\PostgreSQL\16\data" start

# Проверить что работает
psql -U postgres -c "SELECT version();"
```

#### Linux

```bash
# Ubuntu/Debian
sudo systemctl start postgresql
sudo systemctl status postgresql

# Arch Linux
sudo systemctl start postgresql
```

#### macOS

```bash
# Через Homebrew
brew services start postgresql@16

# ИЛИ напрямую
pg_ctl -D /usr/local/var/postgres start
```

---

### Решение 3: Создание базы данных (если PostgreSQL запущен)

```bash
# Подключиться к PostgreSQL
psql -U postgres

# Создать базу данных
CREATE DATABASE diskapi;

# Выйти
\q
```

---

## 🔍 Диагностика проблемы

### Проверка доступности порта 5432

#### Windows

```powershell
# Проверить занят ли порт
netstat -an | findstr :5432

# Проверить процессы на порту
Get-NetTCPConnection -LocalPort 5432 -ErrorAction SilentlyContinue
```

#### Linux/macOS

```bash
# Проверить занят ли порт
netstat -tuln | grep 5432

# ИЛИ
lsof -i :5432

# ИЛИ через ss
ss -tuln | grep 5432
```

### Проверка Docker контейнеров

```bash
# Все запущенные контейнеры
docker ps

# Все контейнеры (включая остановленные)
docker ps -a

# Логи PostgreSQL контейнера
docker logs diskapi-postgres-dev
```

### Проверка подключения к PostgreSQL

```bash
# Через Docker контейнер
docker exec -it diskapi-postgres-dev psql -U postgres -d diskapi -c "SELECT 1"

# Локально
psql -h localhost -p 5432 -U postgres -d diskapi -c "SELECT 1"
```

---

## ⚙️ Конфигурация подключения

Убедитесь, что переменные окружения правильно настроены:

### Файл `.env` (в корне проекта)

```env
DB_HOST=localhost
DB_PORT=5432
DB_DATABASE=diskapi
DB_USERNAME=postgres
DB_PASSWORD=postgres
```

### Проверка переменных окружения

#### Windows (PowerShell)

```powershell
$env:DB_HOST
$env:DB_PORT
$env:DB_DATABASE
```

#### Linux/macOS

```bash
echo $DB_HOST
echo $DB_PORT
echo $DB_DATABASE
```

---

## 🚀 После запуска PostgreSQL

### 1. Применить миграции

```bash
cd src/App
dotnet ef database update
```

### 2. Запустить приложение

```bash
dotnet run
```

### 3. Проверить работу

Откройте в браузере:
- **Swagger UI**: http://localhost:8080/swagger
- **Health Check**: http://localhost:8080/

---

## 🛑 Остановка PostgreSQL

### Docker

```bash
# Остановить контейнер
docker-compose -f docker-compose.dev.yml stop

# Остановить и удалить контейнер
docker-compose -f docker-compose.dev.yml down

# Остановить и удалить контейнер + данные
docker-compose -f docker-compose.dev.yml down -v
```

### Локальный PostgreSQL

#### Windows

```powershell
Stop-Service -Name postgresql*
```

#### Linux

```bash
sudo systemctl stop postgresql
```

#### macOS

```bash
brew services stop postgresql@16
```

---

## 📊 Полезные команды Docker

```bash
# Перезапустить контейнер
docker restart diskapi-postgres-dev

# Войти в контейнер
docker exec -it diskapi-postgres-dev bash

# Открыть psql в контейнере
docker exec -it diskapi-postgres-dev psql -U postgres -d diskapi

# Посмотреть использование ресурсов
docker stats diskapi-postgres-dev

# Удалить все неиспользуемые контейнеры и образы
docker system prune -a
```

---

## 🆘 Дополнительная помощь

### Если ничего не помогает

1. **Проверьте логи приложения**
   ```bash
   cd src/App
   dotnet run --verbosity detailed
   ```

2. **Проверьте строку подключения в коде**
   - Файл: `src/App/Program.cs`
   - Строки: 266-290

3. **Попробуйте другой порт PostgreSQL**
   - Измените порт в `docker-compose.dev.yml`: `"5433:5432"`
   - Обновите переменную `DB_PORT=5433`

4. **Очистите Docker кеш**
   ```bash
   docker system prune -a --volumes
   docker-compose -f docker-compose.dev.yml up -d --force-recreate
   ```

---

## 📞 Контакты для поддержки

Если проблема не решена, создайте issue в репозитории с:
- Версией ОС
- Версией Docker
- Полным текстом ошибки
- Логами PostgreSQL

---

**Успешной разработки! 🚀**

