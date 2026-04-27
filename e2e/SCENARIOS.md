# E2E user scenarios (Playwright)

These scenarios are exercised against the real backend and the Nuxt app started by `make e2e` (the runner runs a **production build** and **`nuxi preview`**, the same class of output you ship; see `e2e/README.md`).

## Selector policy

Prefer [Playwright `getByRole`](https://playwright.dev/docs/locators#locate-by-role) (`link`, `button`, `textbox`, etc.) and accessible names that match the visible English UI. Use `data-testid` or raw CSS only when role+name is not enough; document the exception in the spec.

**Debugging in Cursor:** the Playwright and Chrome DevTools MCP tools can help inspect snapshots, network, and the console when a scenario fails.

## S1 — Guest books a time (main path)

**Precondition:** an event type exists (seeded before the test run, e.g. `POST /admin/event-types`).

1. Open the public booking catalog at `/booking`.
2. Open an event type (link whose name is the type title).
3. On the slot page, a day with availability is available; select a time slot, then use **Continue**.
4. On the confirm page, submit **Confirm booking** (optional guest name).
5. **Expect:** a success notification with text like **Booking confirmed** (see `frontend/i18n/locales/en.json`), and navigation back to `/booking` while the request went to the real API (`NUXT_PUBLIC_API_BASE_URL` points at the running backend, not the Prism mock on 4010).

## S2 — Optional follow-up (not required for S1)

Assert via `GET /admin/bookings` (Playwright `request` fixture) that the new booking appears. This is optional; API coverage already exists in the backend test project.
