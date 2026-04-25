#!/bin/bash
# Build and run the application

set -e

echo "FBI Form 2257 - Build and Run Script"
echo "===================================="

# Check if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed. Please install .NET 9 SDK first."
    exit 1
fi

echo "✓ .NET SDK found: $(dotnet --version)"

# Restore dependencies
echo ""
echo "Restoring dependencies..."
dotnet restore

# Build the application
echo ""
echo "Building application..."
dotnet build -c Release

# Run tests (if any)
echo ""
echo "Running tests..."
dotnet test || echo "No tests found or tests skipped"

# Start the application
echo ""
echo "Starting application..."
echo "Application will be available at: http://localhost:5000"
echo ""
echo "Press Ctrl+C to stop the application"
echo ""

dotnet run
