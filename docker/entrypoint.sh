#!/usr/bin/env bash
set -euo pipefail

PORT="${PORT:-3000}"
export PORT

dotnet /app/CalendarBooking.Api.dll --urls "http://127.0.0.1:5005" &
api_pid=$!

for _ in $(seq 1 120); do
	if curl -sf "http://127.0.0.1:5005/public/event-types" >/dev/null 2>&1; then
		break
	fi
	if ! kill -0 "${api_pid}" 2>/dev/null; then
		echo "API exited before becoming ready" >&2
		exit 1
	fi
	sleep 0.5
done

export NITRO_HOST=127.0.0.1
export NITRO_PORT=3000
export NODE_ENV=production
node /app/frontend/.output/server/index.mjs &
nitro_pid=$!

for _ in $(seq 1 180); do
	if curl -sf "http://127.0.0.1:${NITRO_PORT}/booking" >/dev/null 2>&1; then
		break
	fi
	if ! kill -0 "${nitro_pid}" 2>/dev/null; then
		echo "Nitro exited before becoming ready" >&2
		exit 1
	fi
	sleep 0.5
done

exec caddy run --config /app/Caddyfile --adapter caddyfile
