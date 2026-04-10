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

Claude Code can read `ASKLLM_API_KEY` from the environment. If it is not set, ask the user to provide their API key and export it for the current session:

```bash
export ASKLLM_API_KEY="<key>"
```

### Claude Desktop

Claude Desktop requires the user to be inside a project. The API key should be stored in a `.env` file (a shell-style environment variables file) in the project root:

```env
ASKLLM_API_KEY=sk-your-key-here
```

Each line uses `KEY=VALUE` format. Do not add quotes, spaces around `=`, or `export` prefixes.

If `ASKLLM_API_KEY` is not set, ask the user to provide their API key and write it to the `.env` file in the project root.

### Claude Web

The `askllm` binary is not available in Claude Web. Instead, query LLMs directly via the OpenRouter API using a non-streaming HTTP request.

Before making a request, ask the user for their OpenRouter API key if it is not already known. Also ask them which model to use. The `model` field must be set to an OpenRouter model identifier. Common models include:

| Model | Identifier |
|---|---|
| GPT-4o | `openai/gpt-4o` |
| Claude 3.5 Sonnet | `anthropic/claude-3.5-sonnet` |
| Gemini 2.0 Flash | `google/gemini-2.0-flash-001` |
| DeepSeek R1 | `deepseek/deepseek-r1` |
| Mistral Large | `mistralai/mistral-large` |
| Llama 3.1 70B | `meta-llama/llama-3.1-70b-instruct` |
| Auto (OpenRouter picks) | `openrouter/auto` |

**Request:**

```bash
curl -s https://openrouter.ai/api/v1/chat/completions \
  -H "Authorization: Bearer <OPENROUTER_API_KEY>" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "<MODEL_IDENTIFIER>",
    "messages": [{"role": "user", "content": "Your prompt here"}],
    "stream": false
  }'
```

**Response handling:**

- The response text is at `choices[0].message.content`.
- Token usage is in the `usage` object (`prompt_tokens`, `completion_tokens`).
- Errors return a JSON object with an `error` field containing a message and code.

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
