#!/bin/bash
# Quantum Core X - Development Environment Startup Script

set -e

echo "🎮 Quantum Core X - Development Environment"
echo "==========================================="
echo ""

# Check if .env exists
if [ ! -f .env ]; then
    echo "📝 Creating .env from template..."
    cp .env.example .env
    echo "✅ Created .env file"
    echo "   You can edit it to customize settings"
    echo ""
fi

# Check if docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running!"
    echo "   Please start Docker Desktop and try again"
    exit 1
fi

# Parse command line arguments
PROFILE=""
if [ "$1" == "--with-tools" ]; then
    PROFILE="--profile tools"
    echo "🛠️  Starting with management tools (pgAdmin, Redis Commander)"
    echo ""
fi

# Start services
echo "🚀 Starting services..."
docker-compose -f docker-compose.dev.yml $PROFILE up -d

# Wait for services to be healthy
echo ""
echo "⏳ Waiting for services to be ready..."
echo "   This may take 30-60 seconds..."
echo ""

# Function to check if service is healthy
check_health() {
    local service=$1
    local max_attempts=30
    local attempt=1

    while [ $attempt -le $max_attempts ]; do
        if docker-compose -f docker-compose.dev.yml ps $service | grep -q "healthy"; then
            return 0
        fi
        echo -n "."
        sleep 2
        attempt=$((attempt + 1))
    done
    return 1
}

# Check PostgreSQL
echo -n "   Checking PostgreSQL"
if check_health db-postgres; then
    echo " ✅"
else
    echo " ⚠️  (timeout, but may still be starting)"
fi

# Check Redis
echo -n "   Checking Redis"
if check_health cache; then
    echo " ✅"
else
    echo " ⚠️  (timeout, but may still be starting)"
fi

echo ""
echo "📊 Service Status:"
docker-compose -f docker-compose.dev.yml ps
echo ""

# Print connection information
echo "🔗 Connection Information:"
echo "   Game Server:     localhost:13001"
echo "   Auth Server:     localhost:11002"
echo "   PostgreSQL:      localhost:5432"
echo "   Redis:           localhost:6379"

if [ "$PROFILE" != "" ]; then
    echo ""
    echo "🛠️  Management Tools:"
    echo "   pgAdmin:         http://localhost:5050"
    echo "   Redis Commander: http://localhost:8081"
fi

echo ""
echo "📝 Database Setup:"
echo "   Run migrations with:"
echo "   docker-compose -f docker-compose.dev.yml exec game dotnet ef database update"
echo ""

echo "📖 View Logs:"
echo "   All services: docker-compose -f docker-compose.dev.yml logs -f"
echo "   Game only:    docker-compose -f docker-compose.dev.yml logs -f game"
echo ""

echo "🛑 Stop Services:"
echo "   docker-compose -f docker-compose.dev.yml down"
echo ""

echo "✅ Development environment is ready!"
echo "   Check DOCKER_DEV.md for detailed documentation"
