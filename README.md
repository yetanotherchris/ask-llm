# ask-llm
Ask any large language model from your terminal using an OpenAI-compatible API. The app is a .NET 9 console application that uses ANSI coloring via [Crayon](https://github.com/riezebosch/crayon) to provide a friendly command-line experience.

## Download

[![GitHub Release](https://img.shields.io/github/v/release/yetanotherchris/ask-llm?logo=github&sort=semver)](https://github.com/yetanotherchris/ask-llm/releases/latest)

You can download the latest version of ask-llm using PowerShell or Bash:

```powershell
Invoke-WebRequest -Uri "https://github.com/yetanotherchris/ask-llm/releases/latest/download/askllm.exe" -OutFile "askllm.exe"
```
```bash
wget -O askllm "https://github.com/yetanotherchris/ask-llm/releases/latest/download/askllm"
chmod +x askllm
```
```bash
curl -L "https://github.com/yetanotherchris/ask-llm/releases/latest/download/askllm" -o askllm
chmod +x askllm
```

Scoop on Windows:
```powershell
scoop bucket add ask-llm https://github.com/yetanotherchris/ask-llm
scoop install ask-llm
```

Homebrew on macOS/Linux:
```bash
brew tap yetanotherchris/ask-llm https://github.com/yetanotherchris/ask-llm
brew install ask-llm
```

You can also download the latest release directly from the [Releases page](https://github.com/yetanotherchris/ask-llm/releases).

## Configuration

ask-llm reads its configuration from environment variables:

- `ASKLLM_API_KEY` (required): API key used to authenticate with your OpenAI-compatible endpoint.
- `ASKLLM_API_ENDPOINT` (optional): Override the default endpoint (`https://openrouter.ai/api/v1`).

See [Provider Examples](providers.md) for common OpenAI-compatible providers, their base URLs, and how to create API keys.

### Setting environment variables

PowerShell:
```powershell
$env:ASKLLM_API_KEY = "sk-..."
$env:ASKLLM_API_ENDPOINT = "https://your-endpoint.example/v1" # optional
```

Bash:
```bash
export ASKLLM_API_KEY="sk-..."
export ASKLLM_API_ENDPOINT="https://your-endpoint.example/v1" # optional
```

## Usage

```
USAGE:
    askllm --model <model_name> --prompt "<prompt>"

OPTIONS:
    -m, --model <model_name>    The model identifier to send the request to (required)
    -h, --help                  Show command help
```

### Examples

```powershell
askllm.exe --model "x-ai/grok-4-fast:free" --prompt "Write a haiku about dotnet"
```
```bash
./askllm --model gpt-4o-mini --prompt "Translate 'How are you?' to French"
askllm --model gpt-4o-mini --prompt "Summarise the latest commit"
```

If you clone the source (requires [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) preview or later):

```bash
dotnet restore
dotnet run --project AskLlm.csproj -- --model gpt-4o-mini "Hello there"
```

### Publishing

Publishing uses Native AOT so the app starts quickly while remaining self-contained. When you're ready to ship a release build, run:

```bash
dotnet publish -c Release -r linux-x64 -p:StripSymbols=true
```

Replace `linux-x64` with the runtime identifier you need (for example, `win-x64` or `osx-arm64`). The output single-file binary bundles the required .NET runtime so it can run on machines without the SDK installed.

### Startup performance

The first-run startup time improves significantly when using the Native AOT publish profile. Measurements were taken inside a Ubuntu 22.04 container with `askllm --help` and a fresh `DOTNET_CLI_HOME` to avoid a warmed .NET runtime.

| Publish mode | Publish command | Cold start (s) |
| --- | --- | --- |
| Self-contained (JIT) | `dotnet publish -c Release -r linux-x64 -p:PublishAot=false -p:SelfContained=true -p:PublishSingleFile=true -p:StripSymbols=true` | 0.510 |
| Native AOT | `dotnet publish -c Release -r linux-x64 -p:StripSymbols=true` | 0.074 |

Your exact results will vary depending on hardware, runtime identifier, and the prompt you execute, but the relative difference between JIT and Native AOT publishing should remain similar.

## Development

```bash
dotnet build
dotnet test
```

## License

ask-llm is licensed under the [MIT License](LICENSE).
