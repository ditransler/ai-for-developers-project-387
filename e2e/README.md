# End-to-end tests (Playwright)

TypeScript tests under [`tests/`](tests/) use the real [.NET API](../backend/src/CalendarBooking.Api/) and the Nuxt app from [`frontend/`](../frontend/) built with **`nuxi build`**, then served with **`nuxi preview`** (see [scripts/run-e2e.sh](scripts/run-e2e.sh)). The runner sets `NUXT_PUBLIC_API_BASE_URL` to the e2e API before the build so the client matches the test backend.

- **Scenarios** — [SCENARIOS.md](SCENARIOS.md)
- **Run everything (from repo root):** `make e2e` — starts the API, seeds an event type, production-builds the frontend, runs `nuxi preview` on a localhost port, then runs Playwright
- **Lint / format / typecheck only (no browser):** `make e2e-verify` or `cd e2e && npm run verify`
- **Environment:** `E2E_BASE_URL` (default `http://127.0.0.1:3123`, see `e2e/scripts/run-e2e.sh`) is set by the Makefile runner; the frontend must be built with `NUXT_PUBLIC_API_BASE_URL` pointing at the same API the tests use (the runner sets this for `make e2e`). Override with `E2E_FRONTEND_PORT` / `E2E_BASE_URL` if needed.

To debug with servers already up and the same env as `make e2e`, run from `e2e/`: `npm test`.
