#!/usr/bin/env sh
set -e
PORT="${PORT:-8080}"
export ASPNETCORE_URLS="https://0.0.0.0:${PORT}"

# Tham số Cloud SQL Proxy
# BẮT BUỘC: tên kết nối instance dạng PROJECT:REGION:INSTANCE
: "${CLOUDSQL_INSTANCE:?durable-sky-471008-q2:us-central1:my-first-postgresql}"

# Cổng proxy local cho Postgres
DB_PROXY_PORT="${DB_PROXY_PORT:-5432}"

# Thêm flag tuỳ chọn cho proxy, ví dụ: --auto-iam-authn (IAM DB Auth), --private-ip (nếu instance chỉ Private IP)
PROXY_EXTRA_ARGS="${PROXY_EXTRA_ARGS:-}"

echo "[entrypoint] Starting Cloud SQL Proxy for ${CLOUDSQL_INSTANCE} on 127.0.0.1:${DB_PROXY_PORT}"
/usr/local/bin/cloud-sql-proxy \
  --structured-logs \
  --port="${DB_PROXY_PORT}" \
  "${PROXY_EXTRA_ARGS}" \
  "${CLOUDSQL_INSTANCE}" &
PROXY_PID=$!

# Bẫy tín hiệu để dừng gọn gàng
term_handler() {
  echo "[entrypoint] Caught termination signal, stopping app and proxy..."
  kill -TERM "${APP_PID:-0}" 2>/dev/null || true
  kill -TERM "${PROXY_PID:-0}" 2>/dev/null || true
  wait || true
}

trap term_handler TERM INT

# Start ứng dụng .NET (kết nối DB qua 127.0.0.1:${DB_PROXY_PORT}, sslmode=disable)
dotnet ImageAPI.dll &
APP_PID=$!

# Chờ app kết thúc, sau đó dừng proxy
wait "${APP_PID}" || true
kill -TERM "${PROXY_PID}" 2>/dev/null || true
wait "${PROXY_PID}" 2>/dev/null || true
