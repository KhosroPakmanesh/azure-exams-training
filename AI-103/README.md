# Foundry C# Labs: .NET 11, 11 projects, 1 .slnx solution

A Visual Studio / .NET 11 solution with the original six C# console chat labs plus five separate lessons from Microsoft Learn **Develop generative AI apps that use tools**. All 11 apps share `Shared.FoundryClient`. Projects 07–11 cover the four named tools and the Margie’s Travel hands-on exercise.

**Original exercise:** https://microsoftlearning.github.io/mslearn-ai-studio/Instructions/Exercises/03-foundry-sdk.html

> Important: The new projects have not been compiled or tested against a live Microsoft Foundry deployment in this environment, which has no .NET SDK. Run `dotnet restore` and `dotnet build` locally. This source ZIP does not include compiled DLLs.

## .NET 11 upgrade (November 12, 2026)

- Targets **`net11.0`**, using **C# 15** and the release **.NET 11 SDK 11.0.100** (see `global.json`).
- Uses an XML solution **`FoundryCSharpLabs.slnx`** instead of the legacy `.sln` format. The .NET SDK supports `.slnx` directly.
- Centralizes **stable, verified NuGet versions** in `Directory.Packages.props`: **OpenAI 2.15.0** and **Azure.Identity 1.21.0**. Those are the newest stable published versions found on November 12, 2026, so no unnecessary prerelease upgrade was applied.
- Projects 01–06 retain their original features; projects 07–11 are separate console tool labs. The tool labs use a small REST adapter on the same Azure OpenAI v1 endpoint so their tool JSON mirrors the Microsoft Learn lessons.

## Solution layout

| Folder | Assembly | Project | Purpose |
| --- | --- | --- | --- |
| `src/01.BasicChat` | `FoundryLabs.BasicChat` | Console | Chat Completions API, one-turn requests |
| `src/02.ResponsesChat` | `FoundryLabs.ResponsesChat` | Console | Stateless Responses API |
| `src/03.ContextChat` | `FoundryLabs.ContextChat` | Console | Multi-turn chat using `previous_response_id` |
| `src/04.StreamingChat` | `FoundryLabs.StreamingChat` | Console | Streaming Responses API with context |
| `src/05.AsyncChat` | `FoundryLabs.AsyncChat` | Console | Async/await with Ctrl+C cancellation |
| `src/06.ChatHistory` | `FoundryLabs.ChatHistory` | Console | Persistent chats, list/new/resume/show |
| `src/07.CodeInterpreter` | `FoundryLabs.CodeInterpreter` | Console | `code_interpreter` for Python execution |
| `src/08.WebSearch` | `FoundryLabs.WebSearch` | Console | `web_search` for current information |
| `src/09.FileSearch` | `FoundryLabs.FileSearch` | Console | `file_search` with uploaded files and a vector store |
| `src/10.FunctionCalling` | `FoundryLabs.FunctionCalling` | Console | `function` with local `get_time` execution |
| `src/11.ToolsChat` | `FoundryLabs.ToolsChat` | Console | Margie’s Travel exercise: `file_search` + `web_search` |
| `src/Shared.FoundryClient` | `FoundryLabs.Shared` | Class library | Configuration, SDK clients, responses wrapper, persistence, shared DTOs |

## Prerequisites

1. Install the **.NET 11 SDK (11.0.100 or later compatible 11.x)**, Visual Studio 2026 v18.12+ / Visual Studio Code, and Azure CLI.
2. Sign in to https://ai.azure.com, create a Microsoft Foundry project, deploy a suitable model, and note the *Azure OpenAI endpoint* and **exact deployment name**. The original exercise uses `gpt-5.2`; use the deployment actually available in your subscription and region.
3. For keyless authentication: sign in with `az login` and grant your account **Cognitive Services OpenAI User** (or an equivalent role permitted to invoke your model) on the Azure OpenAI resource. Azure RBAC changes may take time to propagate.
4. Fill in `src/Shared.FoundryClient/.env` with your endpoint, deployment name, and optional Azure OpenAI API key. If the API key is blank, authenticate with **Microsoft Entra ID** (`az login`).
5. Network access is required from your machine for NuGet restore and Azure model calls. Azure AI usage can incur charges.

## Configure your credentials in the shared `.env` file

Open **`src/Shared.FoundryClient/.env`**. After creating a local `.env` from `.env.example`, this file is automatically loaded by the shared library for the eleven console apps. Fill it in:

```dotenv
AZURE_OPENAI_ENDPOINT=https://YOUR-RESOURCE.openai.azure.com/
MODEL_DEPLOYMENT=YOUR-DEPLOYMENT-NAME
AZURE_OPENAI_API_KEY=YOUR-AZURE-OPENAI-API-KEY
SYSTEM_INSTRUCTIONS=
```

- Find the resource **endpoint**, **deployment name**, and **API key** in Microsoft Foundry or the Azure portal. Use the Azure OpenAI resource URL, **not** a Foundry project endpoint containing `/api/projects/`.
- If using **Microsoft Entra ID** instead of an API key, leave `AZURE_OPENAI_API_KEY=` blank and run `az login` with an account granted the required role.
- Empty `SYSTEM_INSTRUCTIONS` uses a default system prompt.
- The download includes `.env.example`, a safe blank template. Copy it to `.env` and enter your settings. `.env` is ignored by Git and excluded from release packages. Never commit or share a populated `.env`.
- Explicit process environment variables, if set, **override** values in the `.env` file. The file loader does not overwrite system environment variables.
- File loading works when starting an application through `dotnet run` or Visual Studio from the **source checkout**. If you deploy/publish apps without the source tree, set environment variables or provide a secret manager in the deployment environment. The build-and-package scripts exclude the populated `.env` so secrets do not enter publish ZIPs.

| Variable | Required | Purpose |
| --- | --- | --- |
| `AZURE_OPENAI_ENDPOINT` | Yes | Azure OpenAI resource endpoint, not a project URL |
| `MODEL_DEPLOYMENT` | Yes | Your model deployment name |
| `AZURE_OPENAI_API_KEY` | No | If provided, uses API key; otherwise `DefaultAzureCredential` |
| `SYSTEM_INSTRUCTIONS` | No | Override default system prompt |

The optional `CHAT_HISTORY_DIR` setting can still be supplied as an operating-system environment variable.

**Security:** Do not commit API keys, tokens, or customer conversations. The local JSON files store chat messages as plain text. The API key is used by the console applications.

## Verify source structure

From the solution directory, run `python3 scripts/verify-source.py` (or `python scripts/verify-source.py`) to validate the solution/project/package metadata without needing .NET.

## Restore and compile all twelve assemblies

Open `FoundryCSharpLabs.slnx` in Visual Studio 2026 (v18.12+) or run:

```bash
dotnet restore FoundryCSharpLabs.slnx
dotnet build FoundryCSharpLabs.slnx --configuration Release
```

A successful Release build generates **twelve DLLs** in `src/<project>/bin/Release/net11.0/`: eleven application assemblies and one class library, plus the required SDK-generated runtime, dependency and host files. To put compiled outputs into a distribution folder, use `scripts/publish-all.ps1` (Windows PowerShell) or `scripts/publish-all.sh` (macOS/Linux) after the SDK is installed. To create a new ZIP that contains both source **and the compiled assemblies**, use `scripts/build-and-package.ps1` (PowerShell) or `scripts/build-and-package.sh` (macOS/Linux).

> The ZIP in this response contains complete source and build scripts, **not precompiled DLLs**. Compilation could not be verified in the file-generation environment.

## Run the applications in sequence

```bash
dotnet run --project src/01.BasicChat
dotnet run --project src/02.ResponsesChat
dotnet run --project src/03.ContextChat
dotnet run --project src/04.StreamingChat
dotnet run --project src/05.AsyncChat
dotnet run --project src/06.ChatHistory
dotnet run --project src/07.CodeInterpreter
dotnet run --project src/08.WebSearch
dotnet run --project src/09.FileSearch -- ./path/to/documents
dotnet run --project src/10.FunctionCalling
dotnet run --project src/11.ToolsChat -- ./path/to/brochures
```

Each console app accepts prompts and `quit`; project 06 also supports `new [title]`, `list`, `use <guid>`, and `show`.

Try asking `Tell me about the ELIZA chatbot` and then `How does it compare to modern LLMs?` in projects 02 and 03. Project 02 intentionally loses the referent. Project 03 retains it through `previous_response_id`.

## Run the Microsoft Learn tool lessons (Projects 07–11)

These are separate console programs, not a replacement for Projects 01–06. Projects 07–11 use the existing `.env` and the same deployed Responses-compatible model. Their tool requests use a small shared HTTP client for the Azure OpenAI v1 REST endpoint, alongside the existing OpenAI and Azure.Identity SDK dependencies. Tool availability depends on your Foundry model, region, and resource configuration.

- **07 Code Interpreter:** no document setup. Try `Calculate the square root of 16 using Python.` The model runs Python in the hosted sandbox.
- **08 Web Search:** no document setup. Try a current-events query. Web Search availability and applicable Grounding with Bing terms depend on your Foundry resource and region.
- **09 File Search:** pass a document directory or one or more files (`.pdf`, `.txt`, `.md`, `.docx`). A new vector store is created, documents are uploaded and indexing is polled until ready, and you can ask document questions. Pass the displayed `vs_...` ID on later runs to reuse the store.
- **10 Function:** try `What is the current local time?`. The model can request the zero-argument `get_time` function; the C# program executes only that allowlisted function, returns its output through `function_call_output`, and waits for a final answer.
- **11 Tool-enabled chat:** the C# version of Microsoft's Margie's Travel exercise. Supply a directory containing the **official travel brochure PDFs** from the exercise repository, or a previously prepared `vs_...` store ID. Ask what's happening in a destination next month and follow up with which hotels Margie's Travel offers there. The same chat uses `web_search`, `file_search`, and `previous_response_id`. No brochures are bundled in this ZIP.

For the original exercise's brochures, see the [Microsoft Learning lab](https://microsoftlearning.github.io/mslearn-ai-studio/Instructions/Exercises/04a-use-own-data.html) and its [sample files repository](https://github.com/MicrosoftLearning/mslearn-ai-studio/tree/main/labfiles/tools/python/tools-app).

**Vector store costs and cleanup:** New stores and file uploads persist in your Azure resource after the program exits. Note the printed store ID, reuse it when appropriate, and delete unused stores and files through the service or Azure tooling. Documents are uploaded to your configured resource; don't use sensitive files without authorization.

**API choices:** The older six labs use `OpenAI` SDK types directly. The tool labs send the Learn examples' JSON payloads to `/openai/v1/responses`, `/openai/v1/files`, and `/openai/v1/vector_stores` using `HttpClient`. They reuse `FoundryOptions` and the `Azure.Identity` package, but obtain an Entra token for the `https://cognitiveservices.azure.com/.default` scope required by the resource REST API. `AZURE_OPENAI_API_KEY` continues to take precedence.

## Understand the architecture

```text
01.BasicChat ------------> Chat Completions SDK
02.ResponsesChat ---+
03.ContextChat -----+
04.StreamingChat ---+
05.AsyncChat -------+----> Shared.FoundryClient ----> Azure OpenAI /openai/v1/
06.ChatHistory -----+               +-- JSON conversation store
07.CodeInterpreter -+
08.WebSearch -------+
09.FileSearch ------+----> FoundryToolRestClient ----> Azure OpenAI v1 REST
10.FunctionCalling +               +-- FoundryVectorStoreSetup (09, 11)
11.ToolsChat -------+
```

The original lab's five stages are preserved as **distinct implementations** rather than one ever-changing program. Project 06 is a practical follow-on extension. Conversation continuity depends on the Foundry Responses service retaining the referenced response ID. If responses expire or cannot be retrieved, start a new conversation; storing local text alone is not enough to recreate all server-side context with `previous_response_id`.

## Troubleshooting

- **`AZURE_OPENAI_ENDPOINT` / `MODEL_DEPLOYMENT` missing:** fill in `src/Shared.FoundryClient/.env` before running a project (or provide environment variables).
- **401 / 403:** run `az login`, confirm the subscription/tenant, and verify resource RBAC. If you set `AZURE_OPENAI_API_KEY`, key auth takes precedence.
- **404:** use the *deployment name*, correct resource endpoint, and make sure the deployed model supports the API you are calling.
- **Unsupported API / model errors:** confirm the deployed model supports Chat Completions and/or Responses and the region exposes the needed API. Some Foundry models may support only one.
- **`previous_response_id` invalid:** begin a new chat; response continuity depends on server-side stored state.
- **NuGet errors:** verify NuGet network access and restore the centrally managed packages: `OpenAI 2.15.0` and `Azure.Identity 1.21.0`.
- **Tool not supported / 400:** choose a Responses-compatible model and region with the requested tool enabled.
- **File Search setup:** check document path, extension, vector store access, and indexing status. Retry with a saved `vs_...` ID instead of creating new stores.
- **Build warnings `OPENAI001`:** the official Responses SDK marks some APIs experimental, even when underlying REST features are available.

## References

- Microsoft Learn chat lab: https://microsoftlearning.github.io/mslearn-ai-studio/Instructions/Exercises/03-foundry-sdk.html
- Microsoft Learn tool module: https://learn.microsoft.com/en-us/training/modules/use-generative-ai-tools/
- Microsoft Learn Margie’s Travel exercise: https://microsoftlearning.github.io/mslearn-ai-studio/Instructions/Exercises/04a-use-own-data.html
- Azure OpenAI v1 Files and Vector Stores REST reference: https://learn.microsoft.com/en-us/rest/api/microsoft-foundry/azureopenai/files
- Foundry C# Responses API examples: https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/responses
- Chat Completions C# example: https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/chatgpt
- Official OpenAI .NET SDK: https://github.com/openai/openai-dotnet
- NuGet OpenAI: https://www.nuget.org/packages/OpenAI/2.15.0
- NuGet Azure.Identity: https://www.nuget.org/packages/Azure.Identity/1.21.0

## Update dependencies in the future

Use `Directory.Packages.props` to manage **direct** NuGet versions. To check newer compatible releases after restoring packages:

```bash
dotnet list FoundryCSharpLabs.slnx package --outdated
```

Upgrade only after reviewing package release notes and running a solution build. This repository intentionally pins latest **stable** versions verified on November 12, 2026 instead of floating versions or prereleases.
