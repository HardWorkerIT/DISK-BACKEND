#!/bin/bash

# Bash скрипт для запуска PostgreSQL в Docker (Linux/macOS)

echo "🐘 Запуск PostgreSQL в Docker..."

# Проверка установки Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker не установлен или не запущен!"
    echo "Установите Docker: https://www.docker.com/get-started"
    exit 1
fi

echo "✅ Docker установлен"

# Проверка запущен ли контейнер
if [ "$(docker ps -q -f name=diskapi-postgres-dev)" ]; then
    echo "⚠️  PostgreSQL контейнер уже запущен"
    echo ""
    echo "Для перезапуска используйте:"
    echo "  docker-compose -f docker-compose.dev.yml restart"
    echo ""
    echo "Для остановки используйте:"
    echo "  docker-compose -f docker-compose.dev.yml down"
    exit 0
fi

# Проверка существует ли контейнер (но остановлен)
if [ "$(docker ps -aq -f name=diskapi-postgres-dev)" ]; then
    echo "🔄 Запуск существующего контейнера..."
    docker-compose -f docker-compose.dev.yml start
else
    echo "🚀 Создание и запуск нового контейнера..."
    docker-compose -f docker-compose.dev.yml up -d
fi

# Ожидание готовности базы данных
echo ""
echo "⏳ Ожидание готовности PostgreSQL..."

max_attempts=30
attempt=0

while [ $attempt -lt $max_attempts ]; do
    attempt=$((attempt + 1))
    sleep 1
    
    health_status=$(docker inspect --format='{{.State.Health.Status}}' diskapi-postgres-dev 2>/dev/null)
    
    if [ "$health_status" = "healthy" ]; then
        echo ""
        echo "✅ PostgreSQL готов к работе!"
        echo ""
        echo "╔═══════════════════════════════════════════════════════════╗"
        echo "║         PostgreSQL успешно запущен!                       ║"
        echo "╚═══════════════════════════════════════════════════════════╝"
        echo ""
        echo "📋 Параметры подключения:"
        echo "  Host:     localhost"
        echo "  Port:     5432"
        echo "  Database: diskapi"
        echo "  User:     postgres"
        echo "  Password: postgres"
        echo ""
        echo "🔧 Полезные команды:"
        echo "  Просмотр логов:  docker-compose -f docker-compose.dev.yml logs -f"
        echo "  Остановка:       docker-compose -f docker-compose.dev.yml stop"
        echo "  Удаление:        docker-compose -f docker-compose.dev.yml down"
        echo "  Удаление с БД:   docker-compose -f docker-compose.dev.yml down -v"
        echo ""
        echo "🚀 Теперь можно запустить приложение:"
        echo "  cd src/App"
        echo "  dotnet run"
        echo ""
        exit 0
    fi
    
    echo -n "."
done

echo ""
echo "❌ Не удалось дождаться готовности PostgreSQL"
echo "Проверьте логи: docker-compose -f docker-compose.dev.yml logs"
exit 1

