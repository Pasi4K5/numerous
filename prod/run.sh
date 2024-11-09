#!/bin/bash

./backup.sh

echo "Updating and restarting containers..."

docker compose pull
docker compose up -d
