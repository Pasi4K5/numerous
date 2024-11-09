#!/bin/bash

echo "Backing up database..."

date=`date +%Y-%m-%d_%H-%M-%S`
file="/backup/backup_$date.sql"
source ./.env
docker exec numerous-db bash -c "
    mkdir /backup;
    pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB" -f "$file"
    "
echo "$file created."
