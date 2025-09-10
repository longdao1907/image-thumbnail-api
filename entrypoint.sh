#!/usr/bin/env sh
set -e
PORT="${PORT:-8080}"
export ASPNETCORE_URLS="https://0.0.0.0:${PORT}"
exec dotnet ImageAPI.dll