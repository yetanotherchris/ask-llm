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

## Prerequisites

The environment variable `ASKLLM_API_KEY` must be set with a valid API key for the LLM provider. If it is not set, inform the user they need to configure it first.

Optionally, `ASKLLM_API_ENDPOINT` can be set to a custom API endpoint. It defaults to OpenRouter (`https://openrouter.ai/api/v1`).

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
