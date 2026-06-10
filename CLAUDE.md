# ask-llm

A CLI tool for sending prompts to LLM providers via the OpenRouter API.

## Build

Always verify the project builds with AOT before committing. Use the same flags as the GHA release pipeline:

```bash
# Linux x64
dotnet publish AskLlm.csproj -c Release -r linux-x64 -p:PublishAot=true -p:StripSymbols=true

# Windows x64
dotnet publish AskLlm.csproj -c Release -r win-x64 -p:PublishAot=true -p:StripSymbols=true

# macOS x64
dotnet publish AskLlm.csproj -c Release -r osx-x64 -p:PublishAot=true -p:StripSymbols=true

# macOS ARM64
dotnet publish AskLlm.csproj -c Release -r osx-arm64 -p:PublishAot=true -p:StripSymbols=true
```

At minimum, run the linux-x64 build locally before pushing. AOT compilation is stricter than a regular build — code that compiles normally can still fail under AOT (e.g. string interpolation in `const` expressions, missing constructor overloads, reflection usage).

Output binaries land in `bin/Release/net10.0/<rid>/publish/`.

## Quick check (no AOT, fast)

```bash
dotnet build AskLlm.csproj -c Release
```

## Project structure

- `Commands/` — command implementations (`AskCommand`)
- `CommandLine/` — CLI wiring (`RootCommandFactory`, `EnvironmentVariableNames`, `DefaultsStore`)
- `Models/` — request/response models
- `Services/` — HTTP client and endpoint service
