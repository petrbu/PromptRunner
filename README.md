# PromptRunner

NET Framework 4.8 console app that unifies multiple text providers, with templated prompts and runtime data injection.

## Features

- **Multi-Provider Support** - Seamlessly switch between 5 different AI providers
- **Dynamic References** - Support for image and text placeholders in prompts
- **Configurable Execution** - Temperature, max tokens, and model overrides per prompt
- **Flexible Configuration** - Load different prompt configs via CLI
- **Multimodal Support** - Image analysis with automatic MIME type detection
- **CLI-Friendly** - Comprehensive command-line argument support

## Supported AI Providers

| Provider | Description |
|----------|-------------|
| **Anthropic** | Claude models (Opus, Sonnet, Haiku) | 
| **Azure OpenAI** | Microsoft's hosted OpenAI models |
| **OpenAI** | GPT models (GPT-5, GPT-4o, etc.) |
| **Google Gemini** | Multimodal AI with advanced capabilities |
| **Ollama** | Local open-source models |

Each provider requires different configuration settings in the `api_config.json` file.

## Requirements

- **.NET Framework 4.8** or higher
- **Windows 7 SP1** or newer (with .NET Framework 4.8 installed)
- **API keys** for the AI providers you intend to use
- **Visual Studio 2017 15.9+** or **Visual Studio 2019/2022** (for development)

### NuGet Dependencies

- `System.Text.Json` (8.0.5) - Automatically restored during build

## Quick Start

### 1. Configure API Keys

Copy `api_config_template.json` to `api_config.json` and add your API keys:

```json
{
  "ApiSettings": {
    "DefaultProvider": "Anthropic",
    "Providers": {
      "Anthropic": {
        "Uri": "https://api.anthropic.com",
        "DefaultModel": "claude-3-5-sonnet-20241022",
        "ApiKey": "YOUR_ANTHROPIC_API_KEY",
        "ApiVersion": "2023-06-01"
      },
      "OpenAI": {
        "Uri": "https://api.openai.com",
        "DefaultModel": "gpt-4o",
        "ApiKey": "YOUR_OPENAI_API_KEY"
      }
    },
    "GlobalSettings": {
      "DefaultTemperature": 0.5,
      "DefaultMaxOutputTokens": 2000,
      "DefaultTimeout": 30
    }
  }
}
```

### 2. Run the Application

```bash
# Using default configuration
PromptRunner.exe

# Or with .NET CLI (if SDK installed)
dotnet run

# With specific configuration
PromptRunner.exe --config "prompts_config - providers-anthropic.json"
```

## Command Line Arguments

```
--config <file>              Specify which prompt configuration file to use
--image <key=path>           Provide dynamic image references for prompts
--text <key=value>           Provide dynamic text references for prompts
--prompt-folder <folder>     Specify custom folder for prompt files
```

### Examples

#### Basic usage with default config:
```bash
PromptRunner.exe
```

#### Specifying a different provider:
```bash
PromptRunner.exe --config "prompts_config - providers-openai.json"
```

#### Using dynamic image references:
```bash
PromptRunner.exe --config "prompts_config - providers-azure.json" ^
  --image "enu-image=./prompts/truncation-csy/image_enu.bmp" ^
  --image "loc-image=./prompts/truncation-csy/image_csy.bmp"
```

#### Using dynamic text references:
```bash
PromptRunner.exe --text "project_name=My Project" ^
  --text "analysis_template=QA Analysis Template"
```

#### Combining multiple options:
```bash
PromptRunner.exe --config "prompts_config - providers-gemini.json" ^
  --prompt-folder "C:\my-custom-prompts" ^
  --image "screenshot=C:\images\sample.png" ^
  --text "context=Production environment"
```

## Configuration Files

### API Configuration (`api_config.json`)

Defines provider endpoints, models, and API keys:

```json
{
  "ApiSettings": {
    "DefaultProvider": "Anthropic",
    "Providers": {
      "Anthropic": {
        "Uri": "https://api.anthropic.com",
        "DefaultModel": "claude-3-5-sonnet-20241022",
        "ApiKey": "YOUR_API_KEY",
        "ApiVersion": "2023-06-01"
      }
    },
    "GlobalSettings": {
      "DefaultTemperature": 0.5,
      "DefaultMaxOutputTokens": 2000,
      "DefaultTimeout": 30
    }
  }
}
```

### Prompt Configuration

Located in `Utilities/Configuration/PromptConfigs/`, these JSON files define prompts:

```json
{
  "Prompts": [
    {
      "Id": "tech_trends",
      "Name": "Technology Trends",
      "Category": "Research",
      "FilePath": "prompts/tech_trends.txt",
      "Provider": "Anthropic",
      "TemperatureOverride": 0.1,
      "MaxTokensOverride": 500,
      "ModelOverride": "claude-3-5-sonnet-20241022"
    }
  ]
}
```

## Prompt File Format

Prompts use a simple text format with role markers:

```
### System
You are an AI assistant specializing in technology trends.

### User
What are the latest trends in AI and machine learning?
```

### Dynamic References in Prompts

#### Image References

Use `{img:key}` for dynamic images or `{image:path}` for direct paths:

```
### User
Analyze this screenshot: {img:sample_screen}
Also check this one: {image:./Resources/error_screen.png}
```

Run with:
```bash
PromptRunner.exe --image "sample_screen=C:\screenshots\app.png"
```

#### Text References

Use `{txt:key}` for dynamic text or `{text:content}` for direct insertion:

```
### User
Project: {txt:project_name}
Template: {txt:template}
Notes: {text:Focus on edge cases}
```

Run with:
```bash
PromptRunner.exe --text "project_name=MyApp" --text "template=QA Template"
```

#### Combined References

```
### System
You are analyzing software localization for {txt:project_name}.

### User
Compare these UI screenshots:
- Original: {img:enu_screen}
- Localized: {img:loc_screen}

Additional context: {text:Check for text truncation and alignment issues}
```

Run with:
```bash
PromptRunner.exe ^
  --text "project_name=VS Code Extension" ^
  --image "enu_screen=./images/english.png" ^
  --image "loc_screen=./images/localized.png"
```

## Building and Publishing

### Build
```bash
# Using MSBuild
msbuild PromptRunner.sln /p:Configuration=Release

# Or with .NET CLI (if SDK installed)
dotnet build -c Release
```

### Publish
```bash
dotnet publish -c Release -f net48
```

Output location: `bin\Release\net48\`

## Project Structure

```
PromptRunner/
├── AI/
│   ├── Clients/           # Provider-specific API clients
│   ├── Models/            # Data models
│   ├── ModelProcessor.cs  # Orchestrates processing
│   └── PromptManager.cs   # Manages prompts
├── Utilities/
│   ├── Configuration/     # Config loaders
│   └── References/        # Dynamic reference managers
├── prompts/               # Sample prompt files
└── Program.cs             # Application entry point
```

## Use Cases

- **Desktop Application Integration** - Reference as a library in WinForms/WPF apps
- **Automated Testing** - Batch process prompts for QA/testing scenarios
- **Content Generation** - Automate content creation workflows
- **Image Analysis** - Analyze screenshots, UI elements, design assets
- **Multi-Provider Comparison** - Test same prompts across different AI providers
- **Portfolio/Learning** - Educational reference for AI API integration

## License

MIT License - see [LICENSE](LICENSE) file for details.

---

**Note:** This is a personal project. API keys are required for the respective AI providers. Keep your `api_config.json` file secure and never commit it to version control.
