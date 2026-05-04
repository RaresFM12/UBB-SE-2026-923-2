#!/bin/bash
set -e
cd "$(dirname "$0")/UBB-SE-2026-923-2/UBB-SE-2026-923-2.Data"
dotnet ef database drop --force
dotnet ef database update
