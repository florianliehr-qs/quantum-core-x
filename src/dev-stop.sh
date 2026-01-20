#!/bin/bash
# Quantum Core X - Development Environment Stop Script

set -e

echo "🛑 Stopping Quantum Core X Development Environment"
echo "=================================================="
echo ""

# Check for --clean flag
CLEAN=false
if [ "$1" == "--clean" ]; then
    CLEAN=true
fi

# Stop services
echo "📦 Stopping containers..."
docker-compose -f docker-compose.dev.yml down

if [ "$CLEAN" = true ]; then
    echo ""
    echo "🧹 Removing volumes (cleaning all data)..."
    docker volume rm quantumcore-postgres-dev-data 2>/dev/null || echo "   PostgreSQL volume already removed"
    docker volume rm quantumcore-redis-dev-data 2>/dev/null || echo "   Redis volume already removed"
    docker volume rm quantumcore-pgadmin-dev-data 2>/dev/null || echo "   pgAdmin volume already removed"
    echo ""
    echo "⚠️  All data has been removed!"
    echo "   Next start will create fresh databases"
else
    echo ""
    echo "💾 Data volumes preserved"
    echo "   Use './dev-stop.sh --clean' to remove all data"
fi

echo ""
echo "✅ Development environment stopped"
