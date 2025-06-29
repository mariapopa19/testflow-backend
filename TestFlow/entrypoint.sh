#!/bin/bash

echo "Starting application initialization..."

# Run EF Core migrations
echo "Running EF Core migrations..."
dotnet ef database update --project ../TestFlow.Infrastructure/TestFlow.Infrastructure.csproj --startup-project ./TestFlow.API.csproj --connection "$ConnectionStrings__DefaultConnection"

if [ $? -eq 0 ]; then
    echo "Migrations completed successfully!"
else
    echo "Migration failed!"
    exit 1
fi

# Change to app directory and start the application
cd /app
echo "Starting the application..."
exec dotnet TestFlow.API.dll