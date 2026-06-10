#!/bin/bash
set -euo pipefail

# Only run in remote (Claude Code on the web) environments
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

# Install .NET 10 SDK if not already present
if [ ! -f "$HOME/.dotnet/dotnet" ]; then
  echo "Installing .NET 10 SDK..."
  wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  /tmp/dotnet-install.sh --channel 10.0
  rm /tmp/dotnet-install.sh
else
  echo ".NET SDK already installed: $("$HOME/.dotnet/dotnet" --version)"
fi

# Persist DOTNET_ROOT and PATH for the session
echo "export DOTNET_ROOT=\"$HOME/.dotnet\"" >> "$CLAUDE_ENV_FILE"
echo "export PATH=\"\$PATH:\$DOTNET_ROOT:\$DOTNET_ROOT/tools\"" >> "$CLAUDE_ENV_FILE"

# Install AOT native toolchain (idempotent)
if ! command -v clang &>/dev/null || ! dpkg -s zlib1g-dev &>/dev/null 2>&1; then
  echo "Installing AOT native toolchain..."
  sudo apt-get install -y clang zlib1g-dev
else
  echo "AOT toolchain already installed."
fi

echo ".NET setup complete."
