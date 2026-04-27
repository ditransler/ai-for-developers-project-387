# Calendar Booking — frontend

Nuxt 3 app (Vue 3, TypeScript, [Nuxt UI](https://ui.nuxt.com/), [Nuxt i18n](https://i18n.nuxtjs.org/)). All data goes through the HTTP API described in [`../api-contract/openapi.yaml`](../api-contract/openapi.yaml).

## Prerequisites

- Node.js 24+
- npm

## Install

```bash
cd frontend && npm ci
```

## Environment

Copy [`.env.example`](.env.example) to `.env` and adjust if needed:

- **`NUXT_PUBLIC_API_BASE_URL`** — API origin (no trailing slash). Default in code is `http://localhost:4010` for a local [Prism](https://stoplight.io/open-source/prism) mock.

## Develop

1. Start a mock API from the **repository root** (see root `Makefile` target `prism-mock`) or point `NUXT_PUBLIC_API_BASE_URL` at your real backend (CORS must allow the Nuxt dev origin).

2. Run the app:

```bash
cd frontend && npm run dev
```

Open the URL shown in the terminal (usually `http://localhost:3000`).

## API types

Regenerate TypeScript types when the OpenAPI contract changes:

```bash
cd frontend && npm run typegen:api
```

(Requires an up-to-date `../api-contract/openapi.yaml`, e.g. `make tsp-openapi` from the repo root.)

## Build

```bash
cd frontend && npm run build
```

Preview production output:

```bash
node .output/server/index.mjs
```

## Dependency note

`package.json` uses **npm `overrides`** so `@nuxt/kit` and `@nuxt/schema` stay aligned with the installed `nuxt` version. If you upgrade Nuxt, refresh those overrides accordingly.
