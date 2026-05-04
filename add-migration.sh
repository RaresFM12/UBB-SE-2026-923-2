#!/bin/bash
set -e
if [ -z "$1" ]; then echo "Usage: ./add-migration.sh <Name>"; exit 1; fi
cd "$(dirname "$0")/UBB-SE-2026-923-2/UBB-SE-2026-923-2.Data"
dotnet ef migrations add "$1"
