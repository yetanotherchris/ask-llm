# ask-llm
Ask any large language model from your terminal using an OpenAI-compatible API. The app is a .NET 9 console application that uses Spectre.Console to provide a friendly command-line experience.

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

Scoop.sh on Windows:
```powershell
scoop bucket add ask-llm https://github.com/yetanotherchris/ask-llm
scoop install ask-llm
```

You can also download the latest release directly from the [Releases page](https://github.com/yetanotherchris/ask-llm/releases).

## Configuration

ask-llm reads its configuration from environment variables:

- `ASKLLM_API_KEY` (required): API key used to authenticate with your OpenAI-compatible endpoint.
- `ASKLLM_API_ENDPOINT` (optional): Override the default endpoint (`https://openrouter.ai/api/v1`).

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
    askllm --model <model_name> "<query>"

OPTIONS:
    -m, --model <model_name>    The model identifier to send the request to (required)
    -h, --help                  Show command help
```

### Examples

```powershell
askllm.exe --model "x-ai/grok-4-fast:free" "Write a haiku about dotnet"
```
```bash
./askllm --model gpt-4o-mini "Translate 'How are you?' to French"
askllm --model gpt-4o-mini "Summarise the latest commit"
```

If you clone the source (requires [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) preview or later):

```bash
dotnet restore
dotnet run --project src/AskLlm/AskLlm.csproj -- --model gpt-4o-mini "Hello there"
```

## Development

```bash
dotnet build
dotnet test
```

## License

ask-llm is licensed under the [MIT License](LICENSE).
