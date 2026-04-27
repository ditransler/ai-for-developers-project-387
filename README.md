# Calendar Booking. Hexlet Edu Project

[![Actions Status](https://github.com/ditransler/ai-for-developers-project-387/actions/workflows/hexlet-check.yml/badge.svg)](https://github.com/ditransler/ai-for-developers-project-387/actions)
[![E2E](https://github.com/ditransler/ai-for-developers-project-387/actions/workflows/e2e.yml/badge.svg)](https://github.com/ditransler/ai-for-developers-project-387/actions)

## Project Description

“Calendar Booking” is a simplified time-booking service inspired by Cal.com.

Cal.com is a service where a user publishes available time slots, and another person selects a free time and books a meeting. Our application follows the same core scenario, a single use case: the owner publishes available time slots for meetings, and another user selects a free slot and books a call.

The project does not include authentication, user accounts, or integrations with external calendars. The functionality is intentionally limited to focus on the essentials.

As a result, you should end up with a small but complete application. A user can see available slots, choose a time, and make a booking, while the calendar owner can view a list of upcoming meetings. This is enough to go through the entire process from design to deploying the service in the cloud.

## What We Learn

In this project, we go through the full development cycle of a small web application with AI as a working tool. First, we need to define what exactly the application should do and establish a contract between the client and the server. Then, based on this contract, we sequentially build the interface, implement the backend, cover the main scenarios with tests, and prepare the service for deployment.

The project teaches not just how to write code, but how to structure development in stages. It’s important to first agree on system behavior, then implement it in the interface and on the server, and finally verify everything at the level of user scenarios.

The key working format in this project is that all development is done with the help of AI agents. Ideally, you don’t write a single line of code manually: you formulate tasks for the agent, review the result, and iteratively improve the solution.

The project can be completed using any technology stack. The limitation is not defined by the language or framework, but by the final result: the application must be packaged into a Docker image and run in a container. This approach allows you to focus on architecture and the API contract rather than on choosing specific tools.

### Design First and the API contract

We use a **Design First** workflow: we agree on behavior and the **HTTP API contract first**, then implement the **frontend and backend separately** against that shared contract. The contract is the single place where the two sides align, so neither has to reverse-engineer the other.

We describe the contract in **TypeSpec**. TypeSpec is a language for API definitions; from it we can emit **OpenAPI** and generate code for many stacks. It looks a little like TypeScript syntactically, but it is not a full programming language—only the API and related metadata belong there. See [TypeSpec](https://typespec.io/).

That contract-first flow matters for AI-assisted development: agents can implement from an explicit specification instead of reading large amounts of application code, which keeps work cheaper and easier to steer. When behavior changes, you update the contract and then adjust both client and server in step—without rediscovering the whole system each time.

## API contract (TypeSpec)

The HTTP contract and generated OpenAPI live under [`api-contract/`](api-contract/): [`api-contract/main.tsp`](api-contract/main.tsp) and [`api-contract/openapi.yaml`](api-contract/openapi.yaml).

From the repository root, install dependencies and compile:

```bash
cd api-contract && npm ci
cd .. && make tsp-openapi
```

## Frontend (Nuxt)

The web UI lives in [`frontend/`](frontend/). It talks to the API only via HTTP, using `NUXT_PUBLIC_API_BASE_URL` (see [`frontend/.env.example`](frontend/.env.example)).

Typical local setup in two terminals:

1. `make prism-mock` — OpenAPI mock on port 4010.
2. `cd frontend && npm ci && npm run dev` — Nuxt dev server.

Details: [`frontend/README.md`](frontend/README.md).

## E2E (Playwright)

End-to-end tests for the main booking path live in [`e2e/`](e2e/). From the repository root, run `make e2e` (starts the API, seeds data, **nuxi build** + **nuxi preview** for the frontend, runs Playwright; see `e2e/README.md`). For lint, format, and typecheck only: `make e2e-verify`.

## Docker

The root [`Dockerfile`](Dockerfile) builds a single image: the .NET API, the Nuxt/Nitro production server, and [Caddy](https://caddyserver.com/) on the front. Caddy listens on **`PORT`** (set by the host) and forwards `/public/*` and `/admin/*` to the API; everything else is served by Nitro.
Nuxt build/runtime in this image uses **Node.js 24**.

```bash
docker build -t calendar-booking .
docker run --rm -p 8080:8080 -e PORT=8080 calendar-booking
```

For [Render](https://render.com/), create a **Web Service**, choose **Docker**, point it at this repository and the root `Dockerfile`. Render injects `PORT` automatically.

## Playwright Agent CLI (coding agents)

The devcontainer image installs the [Playwright Agent CLI](https://playwright.dev/agent-cli/introduction) (`playwright-cli`) globally, including browsers. Agent-oriented skills live under [`.agents/skills/playwright-cli/`](.agents/skills/playwright-cli/) for tools like Cursor. Local snapshot YAML is written under `.playwright-cli/` and is gitignored.

When you bump `@playwright/cli` in [`.devcontainer/Dockerfile`](.devcontainer/Dockerfile), run `playwright-cli install --skills agents` from the repository root and commit any updates under `.agents/skills/playwright-cli/`.

## Links

[Cal.com](https://cal.com) — a reference service that can be used as a guide when designing the user flow.

**Deployed app (Render):** [https://calendar-booking-387.onrender.com](https://calendar-booking-387.onrender.com)
