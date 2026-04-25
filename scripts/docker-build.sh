#!/bin/bash
# Docker build and deployment script

set -e

VERSION=${1:-latest}
IMAGE_NAME="form2257:$VERSION"

echo "Building Docker image: $IMAGE_NAME"
echo "===================================="

# Build the image
docker build -t $IMAGE_NAME .

# Tag as latest
docker tag $IMAGE_NAME form2257:latest

echo ""
echo "✓ Docker image built successfully!"
echo ""
echo "To deploy, run:"
echo "  docker-compose up -d"
echo ""
echo "Or manually:"
echo "  docker run -d -p 5000:5000 \\
    -v form2257-data:/app/data \\
    -v form2257-logs:/app/logs \\
    $IMAGE_NAME"
