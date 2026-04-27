#!/usr/bin/env bash
# Orchestrates API → seed → nuxi build → nuxi preview → Playwright. Run from repository root: bash e2e/scripts/run-e2e.sh
# Uses a production Nuxt build with NUXT_PUBLIC_API_BASE_URL set to the e2e API, then `npx nuxi preview` (node-server output).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
API_PORT="${E2E_API_PORT:-5005}"
# Default avoids clashing with a local Nuxt dev server on 3000.
FRONTEND_PORT="${E2E_FRONTEND_PORT:-3123}"
API_URL="${E2E_API_URL:-http://127.0.0.1:${API_PORT}}"
export E2E_BASE_URL="${E2E_BASE_URL:-http://127.0.0.1:${FRONTEND_PORT}}"

cleanup() {
  pkill -f "nuxi preview.*--port ${FRONTEND_PORT}" 2>/dev/null || true
  if [[ -n "${NUXT_PID:-}" ]]; then kill "${NUXT_PID}" 2>/dev/null || true; fi
  if [[ -n "${API_PID:-}" ]]; then kill "${API_PID}" 2>/dev/null || true; fi
}
trap cleanup EXIT

# Free ports if a previous run left processes behind (avoids duplicate seed / EADDRINUSE).
if command -v fuser >/dev/null 2>&1; then
  fuser -k "${API_PORT}/tcp" 2>/dev/null || true
  fuser -k "${FRONTEND_PORT}/tcp" 2>/dev/null || true
  sleep 0.5
fi

cd "$ROOT"
npm --prefix frontend ci
npm --prefix e2e ci
if [[ -n "${CI:-}" ]]; then
  (cd e2e && npx playwright install-deps chromium)
fi
dotnet restore backend/CalendarBooking.slnx

dotnet run --project backend/src/CalendarBooking.Api/CalendarBooking.Api.csproj --no-restore > /tmp/calendar-e2e-api.log 2>&1 &
API_PID=$!

for _ in $(seq 1 120); do
  if curl -sf "${API_URL}/public/event-types" > /dev/null; then
    break
  fi
  sleep 0.5
  if ! kill -0 "${API_PID}" 2>/dev/null; then
    echo "API process exited. Log:" >&2
    cat /tmp/calendar-e2e-api.log >&2
    exit 1
  fi
done

curl -sSf -X POST "${API_URL}/admin/event-types" \
  -H 'Content-Type: application/json' \
  -d @"${ROOT}/e2e/seed-payload.json"

cd "$ROOT/frontend"
export NUXT_PUBLIC_API_BASE_URL="${API_URL}"
npx nuxi build
export NITRO_HOST=127.0.0.1
# Long-form --port so cleanup's pkill pattern matches; Nitro also honors NITRO_PORT from this CLI.
nohup npx nuxi preview --port "${FRONTEND_PORT}" > /tmp/calendar-e2e-preview.log 2>&1 &
NUXT_PID=$!
cd "$ROOT"

# Preview server may take a moment; require HTTP 2xx (curl -f fails on 4xx/5xx).
for _ in $(seq 1 180); do
  if curl -sf "http://127.0.0.1:${FRONTEND_PORT}/booking" > /dev/null; then
    break
  fi
  sleep 0.5
  if ! kill -0 "${NUXT_PID}" 2>/dev/null; then
    echo "Nuxt preview process exited. Log:" >&2
    cat /tmp/calendar-e2e-preview.log >&2
    exit 1
  fi
done

export E2E_BASE_URL
npm --prefix e2e test
