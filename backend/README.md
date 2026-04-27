# Calendar Booking API

ASP.NET Core **.NET 10** backend for the [HTTP contract](../api-contract/openapi.yaml): guest routes under `/public/...` and admin under `/admin/...` (no auth, separation by path only). Data is held in an **in-memory** SQLite store while the process runs (restarts clear data).

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (LTS; devcontainer image includes it)

## Run

From the repository root, after restore:

```bash
make backend-run
```

Or from `backend/`:

```bash
dotnet run --project src/CalendarBooking.Api/CalendarBooking.Api.csproj
```

Default URL is `http://127.0.0.1:5005` (see [launchSettings](src/CalendarBooking.Api/Properties/launchSettings.json)). Override, for example:

```bash
ASPNETCORE_URLS=http://0.0.0.0:8080 dotnet run --project src/CalendarBooking.Api/CalendarBooking.Api.csproj
```

### Makefile targets

- `make backend-restore` – `dotnet restore`
- `make backend-build` – `dotnet build`
- `make backend-run` – run the API
- `make backend-watch` – `dotnet watch run` for local development
- `make backend-test` – run the xUnit suite (see `tests/CalendarBooking.Tests/`)
- `make backend-format` / `make backend-format-check` – [dotnet format](https://learn.microsoft.com/dotnet/core/tools/dotnet-format) on the solution

## Frontend

Point the Nuxt app at this API, for example:

```bash
NUXT_PUBLIC_API_BASE_URL=http://127.0.0.1:5005
```

(Defaults in the project used Prism on port 4010; see [frontend README](../frontend/README.md).)

## CORS

The API allows any origin, method, and header (intended for local dev with the Nuxt dev server on another port). Tighten for production as needed.

## Code style

- [`.editorconfig`](.editorconfig) in this folder
- [Directory.Build.props](Directory.Build.props) – nullable reference types, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`

The solution file is [CalendarBooking.slnx](CalendarBooking.slnx) (.NET 10 XML solution format).

## Tests

- **xUnit** + **Microsoft.AspNetCore.Mvc.Testing** (`WebApplicationFactory<Program>`) for integration tests
- A fixed `TimeProvider` in tests so booking windows and slots are stable

Run:

```bash
make backend-test
```
