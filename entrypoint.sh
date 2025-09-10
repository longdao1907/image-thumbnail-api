#!/usr/bin/env sh
set -e
PORT="${PORT:-8080}"
export ASPNETCORE_URLS="http://127.0.0.1:${PORT}"
exec dotnet ImageAPI.dll