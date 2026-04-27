.DEFAULT_GOAL := help

.PHONY: help tsp-compile tsp-openapi prism-mock frontend-install frontend-dev frontend-build frontend-preview \
	backend-restore backend-build backend-run backend-watch backend-test backend-format backend-format-check \
	e2e e2e-verify

help:
	@echo "Available targets:"
	@echo "  make tsp-compile       - TypeSpec compile (api-contract/)"
	@echo "  make tsp-openapi       - emit OpenAPI to api-contract/openapi.yaml"
	@echo "  make prism-mock        - Prism mock HTTP server (port 4010) from api-contract/openapi.yaml"
	@echo "  make frontend-install  - npm ci in frontend/"
	@echo "  make frontend-dev      - nuxi dev (long-running); often pair with make prism-mock"
	@echo "  make frontend-build    - nuxi production build (frontend/)"
	@echo "  make frontend-preview  - production build then nuxi preview (avoids stale .output)"
	@echo "  make backend-restore  - dotnet restore (backend/CalendarBooking.slnx)"
	@echo "  make backend-build    - dotnet build (backend/CalendarBooking.slnx)"
	@echo "  make backend-run      - dotnet run the API (default port from launchSettings, 5005)"
	@echo "  make backend-watch     - dotnet watch run the API"
	@echo "  make backend-test      - dotnet test (backend/CalendarBooking.slnx)"
	@echo "  make backend-format    - dotnet format (write)"
	@echo "  make backend-format-check - dotnet format --verify-no-changes (CI)"
	@echo "  make e2e               - E2E: run API + seed + nuxi build + nuxi preview + Playwright (see e2e/)"
	@echo "  make e2e-verify        - eslint + prettier + tsc in e2e/ (no servers)"

tsp-compile:
	npm --prefix api-contract run tsp:compile

tsp-openapi:
	npm --prefix api-contract run tsp:openapi

prism-mock:
	npx --yes @stoplight/prism-cli mock api-contract/openapi.yaml -p 4010

frontend-install:
	npm --prefix frontend ci

frontend-dev:
	npm --prefix frontend run dev

frontend-build:
	npm --prefix frontend run build

# Rebuild, then serve .output (required after lockfile/override or Nitro changes)
frontend-preview: frontend-build
	npm --prefix frontend run preview

BACKEND_SLN := backend/CalendarBooking.slnx
BACKEND_PROJ := backend/src/CalendarBooking.Api/CalendarBooking.Api.csproj

backend-restore:
	dotnet restore $(BACKEND_SLN)

backend-build: backend-restore
	dotnet build $(BACKEND_SLN) --no-restore

backend-run: backend-restore
	dotnet run --project $(BACKEND_PROJ) --no-restore

backend-watch: backend-restore
	dotnet watch run --project $(BACKEND_PROJ) --no-restore

backend-test: backend-restore
	dotnet test $(BACKEND_SLN) --no-restore

backend-format: backend-restore
	dotnet format $(BACKEND_SLN) --no-restore

backend-format-check: backend-restore
	dotnet format $(BACKEND_SLN) --no-restore --verify-no-changes

e2e:
	bash e2e/scripts/run-e2e.sh

e2e-verify:
	npm --prefix e2e ci
	npm --prefix e2e run verify
