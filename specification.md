# AskLLM Application Specification

## Project Overview
Create a C# .NET 9 console application named "askllm" that queries Large Language Models via OpenAI-compatible APIs using dependency injection and displays responses using ANSI-colored console output.

## Technical Requirements

### Framework & Dependencies
- **Target Framework**: .NET 9
- **Application Type**: Console Application
- **Executable Name**: `askllm`
- **Namespace**: `AskLlm`

### Required NuGet Packages
- Crayon
- OpenAI (from https://github.com/openai/openai-dotnet)
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Configuration.EnvironmentVariables

### Test Project Dependencies
- Microsoft.NET.Test.Sdk
- xunit
- xunit.runner.visualstudio
- NSubstitute
- Shouldly

## Command Line Interface

### Command Structure
```bash
askllm <query> --model <model_name>
```

### Arguments & Options
1. **Query Argument** (Required)
   - Position: 0, using `[CommandArgument(0, "<query>")]`
   - Type: String
   - Description: The message/query to send to the LLM
   - Should accept multi-word queries (enclosed in quotes)

2. **Model Option** (Required)
   - Flag: `--model` or `-m`
   - Type: String
   - Description: The model name to use

## Environment Variables

1. **ASKLLM_API_KEY** (Required)
   - Purpose: API key for authentication
   - Must be non-empty

2. **ASKLLM_API_ENDPOINT** (Optional)
   - Purpose: API endpoint URL
   - Default: `https://openrouter.ai/api/v1`

## Architecture Requirements

### Dependency Injection
- Use Microsoft.Extensions.DependencyInjection
- Configure services using extension methods
- Inject services into commands via constructor injection

### Service Layer
- Create `IChatEndpointService` interface with:
  - `Task<ChatResponse> SendChatRequestAsync(ChatRequest request, CancellationToken cancellationToken = default)`
  - `bool IsConfigured { get; }` property
- Implement `ChatEndpointService` using OpenAI .NET SDK directly (no HTTP clients)
- Accept `AppSettings` via constructor injection

### Model Classes
- **ChatRequest**: Record with Message and Model properties
- **ChatResponse**: Record with Content, Model, Success, and optional ErrorMessage properties
- **AppSettings**: Class with ApiKey, ApiEndpoint, and IsValid properties

### Command Pattern
- Follow tiny-city repository patterns
- Use `AddCommand<T>()` pattern
- Commands inherit from `AsyncCommand<TSettings>`
- Use constructor injection for dependencies

## Error Handling

### Validation Requirements
- Validate environment variables exist and are non-empty
- Validate API endpoint format if provided
- Validate user input (empty queries, missing model)
- Handle service configuration errors

### API Error Handling
- Network connectivity issues
- Invalid API key
- Model not available
- Rate limiting
- Service unavailable

### Output Formatting
- Use ANSI escape sequences via Crayon for colored output
- Success responses should clearly label the model alongside the generated content
- Error messages should be displayed in red styling
- Include model information in successful responses

## Unit Testing Requirements

### Test Coverage
1. **AskCommand Tests**
   - Valid input execution
   - Service not configured scenarios
   - API error handling
   - Exception handling
   - Input validation

2. **ChatEndpointService Tests**
   - Configuration validation
   - Invalid configuration handling
   - Invalid request parameter handling

### Testing Framework Requirements
- Use NSubstitute for mocking `IChatEndpointService`
- Use Shouldly for all test assertions
- Use xUnit as test framework
- Create test helpers for mock service provider setup
- Test both success and failure scenarios
- Use Arrange-Act-Assert pattern with descriptive test names

## Implementation Notes

- Use OpenAI .NET SDK from https://github.com/openai/openai-dotnet
- Return exit code 0 for success, 1 for errors
- Load environment variables in `AppSettings.LoadFromEnvironment()` method
- Configure all services in `ServiceCollectionExtensions.AddAskLlmServices()`
- Use async/await throughout for all API calls
