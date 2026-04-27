FROM mcr.microsoft.com/devcontainers/dotnet:10.0-noble

USER root

RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get install -y --no-install-recommends ca-certificates curl gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_24.x | bash - \
    && apt-get install -y --no-install-recommends nodejs \
    && corepack disable \
    && npm install -g @playwright/cli@latest \
    && playwright-cli install-browser --with-deps \
    && apt-get clean -y && rm -rf /var/lib/apt/lists/*

USER vscode
