---
name: ask-llm
description: Send prompts to any LLM via the askllm command-line tool using OpenAI-compatible APIs
---

# ask-llm

Use the `askllm` command-line tool to send prompts to large language models from the terminal. It supports any LLM provider with an OpenAI-compatible API, including OpenRouter, OpenAI, Anthropic, Gemini, Groq, Mistral, DeepSeek, and more.

## When to use

Use this skill when you need to:
- Send a prompt to an LLM from the command line
- Query a specific model (e.g. GPT-4o, DeepSeek R1, Gemini)
- Read a prompt from a file and send it to an LLM
- Save an LLM response to a file
- Compare responses across different models
- Store default model settings for repeated use

## Installation

`askllm` is a standalone binary — it is **not** an NPM/Node.js package. It does not require any runtime. Install via Homebrew, Scoop, or download the binary from [GitHub Releases](https://github.com/yetanotherchris/ask-llm/releases/latest).

```bash
# macOS/Linux
brew tap yetanotherchris/ask-llm https://github.com/yetanotherchris/ask-llm
brew install ask-llm
```

If `askllm` is not found on the PATH, check if it is installed first before trying to use it.

## Prerequisites

The environment variable `ASKLLM_API_KEY` must be set with a valid API key for the LLM provider.

Optionally, `ASKLLM_API_ENDPOINT` can be set to a custom API endpoint. It defaults to OpenRouter (`https://openrouter.ai/api/v1`).

### Claude Code

Claude Code runs commands in a sandboxed shell that does **not** load the user's shell profile (`~/.bashrc`, `~/.zshrc`, etc.). However, it can read the local filesystem.

Users can store the API key persistently in their Claude Code user settings file:

- **macOS / Linux:** `~/.claude/settings.json`
- **Windows:** `C:\Users\<username>\.claude\settings.json`

```json
{
  "env": {
    "ASKLLM_API_KEY": "sk-your-key-here"
  }
}
```

This file is local to the user's machine, not committed to git, and is loaded automatically by Claude Code. Once configured, the key is available in every session.

If `ASKLLM_API_KEY` is not set:

1. Ask the user to provide their API key.
2. **On macOS / Linux:** If they provide a key, write it to `~/.claude/settings.json` using the Edit or Write tool (do **not** use bash — the shell is sandboxed). Read the file first; if it already exists, merge the key into the existing `env` object. If the file does not exist, create it with the structure shown above.
3. **On Windows:** Ask the user to paste the key in chat so you can export it for the current session. Also give them instructions on how to add it to their settings file at `C:\Users\<username>\.claude\settings.json` so it persists across sessions — show them the JSON snippet above and explain how to create the file and folder if they don't exist.
4. Export the key for the current session so it is available immediately:
   ```bash
   export ASKLLM_API_KEY="<key>"
   ```

### Claude Desktop

Claude Desktop uses a separate configuration file from Claude Code:

- **macOS:** `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Windows:** `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "env": {
    "ASKLLM_API_KEY": "sk-your-key-here"
  }
}
```

If `ASKLLM_API_KEY` is not set in Claude Desktop:

1. Ask the user to provide their API key.
2. Give them instructions on how to add it to their `claude_desktop_config.json` file at the platform-specific path above. Show them the JSON snippet and explain how to merge the `env` key into the existing config if the file already exists.
3. Export the key for the current session so it is available immediately:
   ```bash
   export ASKLLM_API_KEY="<key>"
   ```

### Claude Web

Claude Web runs in a remote sandbox with no access to the user's local filesystem. The API key cannot be persisted between sessions.

Before running `askllm`, check if `ASKLLM_API_KEY` is set:

```bash
echo "$ASKLLM_API_KEY"
```

If it is empty, **ask the user to provide their API key**. Once they provide it, export it for the session:

```bash
export ASKLLM_API_KEY="<key provided by the user>"
```

## Usage

```bash
# Send a prompt to a specific model
askllm --model "openrouter/auto" --prompt "Explain quicksort in one paragraph"

# Read prompt from a file
askllm --model "gpt-4o" --input-file "prompt.txt"

# Write response to a file
askllm --model "gpt-4o" --prompt "Write a haiku" --output-file "response.txt"

# Store model and options as defaults for future runs
askllm --model "deepseek/deepseek-r1-0528:free" --store

# After storing defaults, just provide the prompt directly
askllm "What is the meaning of life?"
```

## Options

| Option | Description |
|---|---|
| `--model <name>` | Model identifier (e.g. `openrouter/auto`, `gpt-4o`, `deepseek/deepseek-r1-0528:free`) |
| `--prompt <text>` | The prompt text to send to the model |
| `--input-file <path>` | Read the prompt from this file instead of `--prompt` |
| `--output-file <path>` | Write the model response to this file |
| `--store` | Save the current options as defaults for future runs |
| `--color <color>` | Console color name for the response output |
| `--version` | Show version information |
| `-h`, `--help` | Show help |

## Instructions

1. Always specify `--model` unless the user has previously stored defaults with `--store`.
2. When the user doesn't specify a model, suggest `openrouter/auto` as a reasonable default.
3. For prompts that contain special characters, quotes, or newlines, prefer writing the prompt to a temporary file and using `--input-file`.
4. Show the full command before executing it so the user can review it.
5. If the user wants to keep a long response, use `--output-file` to save it.
6. Either `--prompt` or `--input-file` is required — do not omit both.
