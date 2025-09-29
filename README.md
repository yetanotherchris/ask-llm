# ask-llm
Ask an LLM (OpenAI-compatible API) something from your terminal.

## Example usage

These examples use [www.openrouter.ai's model names](https://openrouter.ai/models?q=free). See the [provider examples](providers.md) for configuring Gemini, Claude, ChatGPT etc.

```bash
# Powershell (use control panel/edit environmental variables to persist)
# $env:ASK_LLM_API_KEY="sk-123"

# Bash
# Persist the env var in ~/.bashrc 
export ASK_LLM_API_KEY='sk-123'

askllm --model "openrouter/auto" --prompt "Write a haiku about dotnet"
askllm --model "deepseek/deepseek-r1-0528:free" --prompt "What is love?" --store
askllm --prompt "Tell me about Camus's Myth of Sisyphus in one paragraph"

# Requires --store to be used first
askllm "'Wait! Wait! Now, bomb, consider this next question very carefully.  What is your one purpose in life?'"

echo "The tortoise lays on its back, its belly baking in the hot sun, beating its legs 
trying to turn itself over, but it can't. Not without your help. But you're not helping  
Why is that?" > input.txt

askllm --input-file "input.txt" --output-file "response.txt"

```

**You can use it with local models too:**
```bash
# Powershell examples
# ollama
$env:ASKLLM_API_KEY="(not used)"
$env:ASKLLM_API_ENDPOINT="http://localhost:11434/v1/"
askllm --model "gemma3:latest" --prompt "Tell me about the sky in 1 sentence"

# openweb-ui (settings->account to get an API key)
$env:ASKLLM_API_KEY="sk-123"
$env:ASKLLM_API_ENDPOINT="http://localhost:3000/api/"
askllm --model "gemma3:latest" --prompt "Tell me about the moon in 1 sentence"
```

## Download

[![GitHub Release](https://img.shields.io/github/v/release/yetanotherchris/ask-llm?logo=github&sort=semver)](https://github.com/yetanotherchris/ask-llm/releases/latest)  
*Note: you do not need .NET installed for askllm to work, it is standalone.*

**Package managers**

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

**Via your terminal**

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

And finally the [Github Releases page](https://github.com/yetanotherchris/ask-llm/releases).


## Configuration

ask-llm reads its configuration from environment variables:

- `ASK_LLM_API_KEY` (required): API key used to authenticate with your OpenAI-compatible endpoint.
- `ASK_LLM_API_ENDPOINT` (optional): Override the default endpoint (`https://openrouter.ai/api/v1`).

See the [provider examples](providers.md) for common OpenAI-compatible providers, their base URLs, and how to create API keys.

`ASK_LLM_DEFAULT` is used to store default command line options, but not the prompt. On Linux and Mac this will be written to your bash profile.

## Usage

```
Description:
  Send a prompt to an LLM provider.

Usage:
  askllm [options]

Options:
  --model <model_name>  The model identifier to send the request to.
  --prompt <prompt>     The prompt text to send to the model.
  --input-file <path>   Optional file path that supplies the prompt text.
  --output-file <path>  Optional file path to write the response to.
  --store               Store provided options (excluding --prompt) for future runs.
  --color <color>       Optional console color name used when rendering responses.
  --version             Show version information
  -?, -h, --help        Show help and usage information
```

#### Local development

Requires [.NET 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) or later  
Clone the repository, set your environmental variables and then use:

```bash
dotnet restore
dotnet run --project AskLlm.csproj -- --model "openai/gpt-5" "Write a C# app to write a C# app to write a C# app"
```
