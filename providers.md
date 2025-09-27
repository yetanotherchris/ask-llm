# Provider Examples

## Amazon Bedrock
- **OpenAI-compatible API URL:** `https://bedrock-runtime.{region}.amazonaws.com/openai/v1`
- **Notes:** Replace `{region}` with your AWS region (for example, `us-east-1`). Supported models are listed in the Bedrock console.
- **API key documentation:** [AWS Docs - Set up Amazon Bedrock access](https://docs.aws.amazon.com/bedrock/latest/userguide/security-iam.html)

## Anthropic (Claude)
- **OpenAI-compatible API URL:** `https://api.anthropic.com/v1/messages`
- **Notes:** Claude's native Messages API requires Anthropic-specific headers. For strict OpenAI format compatibility, route through an intermediary such as OpenRouter or Fireworks when selecting a Claude model.
- **API key documentation:** [Anthropic Docs - Create an API key](https://docs.anthropic.com/en/api/getting-started)

## Azure OpenAI Service
- **OpenAI-compatible API URL:** `https://{your-resource-name}.openai.azure.com/openai`
- **API key documentation:** [Microsoft Learn - Create an Azure OpenAI resource](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource)

## Cloudflare Workers AI
- **OpenAI-compatible API URL:** `https://api.cloudflare.com/client/v4/accounts/{account-id}/ai/v1`
- **Notes:** Replace `{account-id}` with your Cloudflare account identifier. Provide an `Authorization: Bearer` header with your API token when making requests.
- **API key documentation:** [Cloudflare Docs - Create an API token for Workers AI](https://developers.cloudflare.com/workers-ai/get-started/rest/)

## DeepInfra
- **OpenAI-compatible API URL:** `https://api.deepinfra.com/v1/openai`
- **API key documentation:** [DeepInfra Docs - Authentication](https://deepinfra.com/docs/getting-started#authentication)

## Fireworks AI
- **OpenAI-compatible API URL:** `https://api.fireworks.ai/inference/v1`
- **API key documentation:** [Fireworks Docs - Generate an API key](https://docs.fireworks.ai/docs/getting-started#generate-an-api-key)

## Google Gemini
- **OpenAI-compatible API URL:** `https://generativelanguage.googleapis.com/v1beta/openai`
- **API key documentation:** [Google AI Gemini Docs - Create an API key](https://ai.google.dev/gemini-api/docs/get-started#api-key)

## Google Vertex AI
- **OpenAI-compatible API URL:** `https://{location}-aiplatform.googleapis.com/v1/projects/{project-id}/locations/{location}/endpoints/openapi`
- **Notes:** Substitute `{project-id}` and `{location}` (for example, `us-central1`) for your Vertex AI deployment. The OpenAI-compatible API is currently in preview and may require enabling the feature flag in your project.
- **API key documentation:** [Google Cloud Docs - Set up authentication for Vertex AI](https://cloud.google.com/vertex-ai/docs/start/authentication)

## Groq
- **OpenAI-compatible API URL:** `https://api.groq.com/openai/v1`
- **API key documentation:** [Groq Console Docs - API Keys](https://console.groq.com/docs/api-keys)

## Mistral AI
- **OpenAI-compatible API URL:** `https://api.mistral.ai/v1`
- **API key documentation:** [Mistral Docs - Retrieve your API key](https://docs.mistral.ai/getting-started/quickstart/#retrieve-your-api-key)

## OpenAI
- **OpenAI-compatible API URL:** `https://api.openai.com/v1`
- **API key documentation:** [OpenAI Platform - API Keys](https://platform.openai.com/account/api-keys)

## OpenRouter
- **OpenAI-compatible API URL:** `https://openrouter.ai/api/v1`
- **API key documentation:** [OpenRouter Docs - Authentication](https://openrouter.ai/docs#authentication)

## Together AI
- **OpenAI-compatible API URL:** `https://api.together.xyz/v1`
- **API key documentation:** [Together AI Docs - Obtain an API key](https://docs.together.ai/docs/quickstart#obtain-an-api-key)

## xAI (Grok)
- **OpenAI-compatible API URL:** `https://api.x.ai/v1`
- **API key documentation:** [xAI Docs - Generate an API key](https://docs.x.ai/docs/getting-started#generate-your-api-key)
