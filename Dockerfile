# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0-resolute AS build-api
WORKDIR /src
COPY backend/ backend/
RUN dotnet publish backend/src/CalendarBooking.Api/CalendarBooking.Api.csproj \
	-c Release \
	-o /app/publish

FROM node:24-bookworm-slim AS build-web
WORKDIR /app/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci --ignore-scripts
COPY frontend/ ./
ENV NUXT_PUBLIC_API_BASE_URL=
ENV NODE_ENV=production
RUN npx nuxi prepare && npx nuxi build

FROM node:24-bookworm-slim AS node-runtime

FROM mcr.microsoft.com/dotnet/aspnet:10.0-resolute AS final
WORKDIR /app

RUN apt-get update \
	&& apt-get install -y --no-install-recommends ca-certificates curl \
	&& curl -fsSL https://github.com/caddyserver/caddy/releases/download/v2.9.1/caddy_2.9.1_linux_amd64.tar.gz \
	| tar -xzf - -C /usr/local/bin caddy \
	&& chmod +x /usr/local/bin/caddy \
	&& apt-get autoremove -y \
	&& rm -rf /var/lib/apt/lists/*

COPY --from=node-runtime /usr/local/ /usr/local/
COPY --from=build-api /app/publish ./
COPY --from=build-web /app/frontend/.output ./frontend/.output
COPY docker/Caddyfile ./Caddyfile
COPY docker/entrypoint.sh ./entrypoint.sh
RUN chmod +x ./entrypoint.sh

EXPOSE 3000

ENTRYPOINT ["./entrypoint.sh"]
