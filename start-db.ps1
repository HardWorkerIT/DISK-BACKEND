# PowerShell скрипт для запуска PostgreSQL в Docker

Write-Host "🐘 Запуск PostgreSQL в Docker..." -ForegroundColor Cyan

# Проверка установки Docker
try {
    docker --version | Out-Null
    Write-Host "✅ Docker установлен" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker не установлен или не запущен!" -ForegroundColor Red
    Write-Host "Установите Docker Desktop: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

# Проверка запущен ли контейнер
$containerRunning = docker ps --filter "name=diskapi-postgres-dev" --format "{{.Names}}"

if ($containerRunning -eq "diskapi-postgres-dev") {
    Write-Host "⚠️  PostgreSQL контейнер уже запущен" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Для перезапуска используйте:" -ForegroundColor Cyan
    Write-Host "  docker-compose -f docker-compose.dev.yml restart" -ForegroundColor White
    Write-Host ""
    Write-Host "Для остановки используйте:" -ForegroundColor Cyan
    Write-Host "  docker-compose -f docker-compose.dev.yml down" -ForegroundColor White
    exit 0
}

# Проверка существует ли контейнер (но остановлен)
$containerExists = docker ps -a --filter "name=diskapi-postgres-dev" --format "{{.Names}}"

if ($containerExists -eq "diskapi-postgres-dev") {
    Write-Host "🔄 Запуск существующего контейнера..." -ForegroundColor Yellow
    docker-compose -f docker-compose.dev.yml start
} else {
    Write-Host "🚀 Создание и запуск нового контейнера..." -ForegroundColor Cyan
    docker-compose -f docker-compose.dev.yml up -d
}

# Ожидание готовности базы данных
Write-Host ""
Write-Host "⏳ Ожидание готовности PostgreSQL..." -ForegroundColor Yellow

$maxAttempts = 30
$attempt = 0
$ready = $false

while ($attempt -lt $maxAttempts -and -not $ready) {
    $attempt++
    Start-Sleep -Seconds 1
    
    $healthStatus = docker inspect --format='{{.State.Health.Status}}' diskapi-postgres-dev 2>$null
    
    if ($healthStatus -eq "healthy") {
        $ready = $true
        Write-Host "✅ PostgreSQL готов к работе!" -ForegroundColor Green
    } else {
        Write-Host "." -NoNewline -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host ""

if ($ready) {
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║         PostgreSQL успешно запущен!                       ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Параметры подключения:" -ForegroundColor Cyan
    Write-Host "  Host:     localhost" -ForegroundColor White
    Write-Host "  Port:     5432" -ForegroundColor White
    Write-Host "  Database: diskapi" -ForegroundColor White
    Write-Host "  User:     postgres" -ForegroundColor White
    Write-Host "  Password: postgres" -ForegroundColor White
    Write-Host ""
    Write-Host "🔧 Полезные команды:" -ForegroundColor Cyan
    Write-Host "  Просмотр логов:  docker-compose -f docker-compose.dev.yml logs -f" -ForegroundColor Gray
    Write-Host "  Остановка:       docker-compose -f docker-compose.dev.yml stop" -ForegroundColor Gray
    Write-Host "  Удаление:        docker-compose -f docker-compose.dev.yml down" -ForegroundColor Gray
    Write-Host "  Удаление с БД:   docker-compose -f docker-compose.dev.yml down -v" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🚀 Теперь можно запустить приложение:" -ForegroundColor Cyan
    Write-Host "  cd src\App" -ForegroundColor White
    Write-Host "  dotnet run" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "❌ Не удалось дождаться готовности PostgreSQL" -ForegroundColor Red
    Write-Host "Проверьте логи: docker-compose -f docker-compose.dev.yml logs" -ForegroundColor Yellow
    exit 1
}

