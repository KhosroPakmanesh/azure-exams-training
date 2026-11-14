# Foundry C# Labs: .NET 11, 8 projects, 1 .slnx solution

A Visual Studio / .NET 11 solution that translates the Microsoft Learning **Create a generative AI chat app** exercise into five C# console labs, then expands it with history, an HTTP API, and an interactive Blazor web UI. All projects share `Shared.FoundryClient`.

**Original exercise:** https://microsoftlearning.github.io/mslearn-ai-studio/Instructions/Exercises/03-foundry-sdk.html

> Important: The source package was assembled in an environment without the .NET SDK or access to NuGet. The projects have not been compiled or tested against a live Microsoft Foundry deployment here. Run `dotnet restore` and `dotnet build` on a development machine before use. No DLLs or executables are included in the supplied ZIP.

## .NET 11 upgrade (November 12, 2026)

- Targets **`net11.0`**, using **C# 15** and the release **.NET 11 SDK 11.0.100** (see `global.json`).
- Uses an XML solution **`FoundryCSharpLabs.slnx`** instead of the legacy `.sln` format. The .NET SDK supports `.slnx` directly.
- Centralizes **stable, verified NuGet versions** in `Directory.Packages.props`: **OpenAI 2.15.0** and **Azure.Identity 1.21.0**. Those are the newest stable published versions found on November 12, 2026, so no unnecessary prerelease upgrade was applied.
- The ASP.NET Core projects use the **Microsoft.NET.Sdk.Web** shared framework, so ASP.NET Core 11 is supplied by `net11.0` without separate NuGet references.
- Projects 01–08 retain their original features; shared SDK client code and JSON persistence are maintained.
- See `UPGRADE_NOTES.md` for migration details and version references.

## Solution layout

| Folder | Assembly | Project | Purpose |
| --- | --- | --- | --- |
| `src/01.BasicChat` | `FoundryLabs.BasicChat` | Console | Chat Completions API, one-turn requests |
| `src/02.ResponsesChat` | `FoundryLabs.ResponsesChat` | Console | Stateless Responses API |
| `src/03.ContextChat` | `FoundryLabs.ContextChat` | Console | Multi-turn chat using `previous_response_id` |
| `src/04.StreamingChat` | `FoundryLabs.StreamingChat` | Console | Streaming Responses API with context |
| `src/05.AsyncChat` | `FoundryLabs.AsyncChat` | Console | Async/await with Ctrl+C cancellation |
| `src/06.ChatHistory` | `FoundryLabs.ChatHistory` | Console | Persistent chats, list/new/resume/show |
| `src/07.ChatApi` | `FoundryLabs.ChatApi` | ASP.NET Core | REST API + SSE stream for conversations |
| `src/08.BlazorChat` | `FoundryLabs.BlazorChat` | Blazor Web App | Streaming chat interface and conversation list |
| `src/Shared.FoundryClient` | `FoundryLabs.Shared` | Class library | Configuration, SDK clients, responses wrapper, persistence, shared DTOs |

## Prerequisites

1. Install the **.NET 11 SDK (11.0.100 or later compatible 11.x)**, Visual Studio 2026 v18.12+ / Visual Studio Code, and Azure CLI.
2. Sign in to https://ai.azure.com, create a Microsoft Foundry project, deploy a suitable model, and note the *Azure OpenAI endpoint* and **exact deployment name**. The original exercise uses `gpt-5.2`; use the deployment actually available in your subscription and region.
3. For keyless authentication: sign in with `az login` and grant your account **Cognitive Services OpenAI User** (or an equivalent role permitted to invoke your model) on the Azure OpenAI resource. Azure RBAC changes may take time to propagate.
4. Fill in `src/Shared.FoundryClient/.env` with your endpoint, deployment name, and optional Azure OpenAI API key. If the API key is blank, authenticate with **Microsoft Entra ID** (`az login`).
5. Network access is required from your machine for NuGet restore and Azure model calls. Azure AI usage can incur charges.

## Configure your credentials in the shared `.env` file

Open **`src/Shared.FoundryClient/.env`**. This file is included with the downloaded source ZIP and is automatically loaded by the shared library for the six console apps and the REST API. Fill it in:

```dotenv
AZURE_OPENAI_ENDPOINT=https://YOUR-RESOURCE.openai.azure.com/
MODEL_DEPLOYMENT=YOUR-DEPLOYMENT-NAME
AZURE_OPENAI_API_KEY=YOUR-AZURE-OPENAI-API-KEY
SYSTEM_INSTRUCTIONS=
```

- Find the resource **endpoint**, **deployment name**, and **API key** in Microsoft Foundry or the Azure portal. Use the Azure OpenAI resource URL, **not** a Foundry project endpoint containing `/api/projects/`.
- If using **Microsoft Entra ID** instead of an API key, leave `AZURE_OPENAI_API_KEY=` blank and run `az login` with an account granted the required role.
- Empty `SYSTEM_INSTRUCTIONS` uses a default system prompt.
- `.env` is ignored by Git, but the provided download contains an **empty template** for you to fill in. Do not commit, email, or publish the edited file. `.env.example` is a safe blank template if you need to recreate it.
- Explicit process environment variables, if set, **override** values in the `.env` file. The file loader does not overwrite system environment variables.
- File loading works when starting an application through `dotnet run` or Visual Studio from the **source checkout**. If you deploy/publish apps without the source tree, set environment variables or provide a secret manager in the deployment environment. The build-and-package scripts exclude the populated `.env` so secrets do not enter publish ZIPs.

| Variable | Required | Purpose |
| --- | --- | --- |
| `AZURE_OPENAI_ENDPOINT` | Yes | Azure OpenAI resource endpoint, not a project URL |
| `MODEL_DEPLOYMENT` | Yes | Your model deployment name |
| `AZURE_OPENAI_API_KEY` | No | If provided, uses API key; otherwise `DefaultAzureCredential` |
| `SYSTEM_INSTRUCTIONS` | No | Override default system prompt |

Other optional host settings can still be supplied as operating-system environment variables: `CHAT_HISTORY_DIR`, `CHAT_API_URL`, `CHAT_API_BIND_URL`, `BLAZOR_BIND_URL`.

**Security:** Do not commit API keys, tokens, or customer conversations. The local JSON files store chat messages as plain text. The API key is used on the server/console only, never in browser-side code.

## Verify source structure

From the solution directory, run `python3 scripts/verify-source.py` (or `python scripts/verify-source.py`) to validate the solution/project/package metadata without needing .NET.

## Restore and compile all nine assemblies

Open `FoundryCSharpLabs.slnx` in Visual Studio 2026 (v18.12+) or run:

```bash
dotnet restore FoundryCSharpLabs.slnx
dotnet build FoundryCSharpLabs.slnx --configuration Release
```

A successful Release build generates **nine DLLs** in `src/<project>/bin/Release/net11.0/`: eight application assemblies and one class library, plus the required SDK-generated runtime, dependency and host files. To put compiled outputs into a distribution folder, use `scripts/publish-all.ps1` (Windows PowerShell) or `scripts/publish-all.sh` (macOS/Linux) after the SDK is installed. To create a new ZIP that contains both source **and the compiled assemblies**, use `scripts/build-and-package.ps1` (PowerShell) or `scripts/build-and-package.sh` (macOS/Linux).

> The ZIP in this response contains complete source and build scripts, **not precompiled DLLs**. Compilation could not be verified in the file-generation environment.

## Run the applications in sequence

```bash
dotnet run --project src/01.BasicChat
dotnet run --project src/02.ResponsesChat
dotnet run --project src/03.ContextChat
dotnet run --project src/04.StreamingChat
dotnet run --project src/05.AsyncChat
dotnet run --project src/06.ChatHistory
```

Each console app accepts prompts and `quit`; project 06 also supports `new [title]`, `list`, `use <guid>`, and `show`.

Try asking `Tell me about the ELIZA chatbot` and then `How does it compare to modern LLMs?` in projects 02 and 03. Project 02 intentionally loses the referent. Project 03 retains it through `previous_response_id`.

### Start the web API and UI

In terminal A:

```bash
dotnet run --project src/07.ChatApi
```

In terminal B:

```bash
dotnet run --project src/08.BlazorChat
```

Open **http://localhost:5081**. The UI calls the server-side API at `http://localhost:5080/`. It supports new conversations, selection of saved conversations, and incremental streamed responses. Both projects are designed to run on your machine; the API is **not authenticated** and should **never be exposed to the public internet** without adding proper authentication, authorization, rate limiting, persistence controls, and TLS.

### API endpoints

| Method | URL | Request | Response |
| --- | --- | --- | --- |
| GET | `/api/health` | none | health JSON |
| GET | `/api/conversations` | none | list of conversations |
| POST | `/api/conversations` | `{ "title": "My first chat" }` | new conversation record |
| GET | `/api/conversations/{id}` | none | conversation plus messages |
| POST | `/api/conversations/{id}/messages` | `{ "message": "Hello" }` | full assistant response |
| POST | `/api/conversations/{id}/stream` | `{ "message": "Hello" }` | `text/event-stream` with `delta`, `done`, `error` events |

Example using PowerShell:

```powershell
$chat = Invoke-RestMethod -Uri http://localhost:5080/api/conversations -Method Post -ContentType 'application/json' -Body '{"title":"API test"}'
Invoke-RestMethod -Uri "http://localhost:5080/api/conversations/$($chat.id)/messages" -Method Post -ContentType 'application/json' -Body '{"message":"Tell me about ELIZA."}'
```

The streaming endpoint uses **POST**, not a browser `EventSource` GET, so the Blazor app reads SSE from an `HttpClient` response stream. Multi-turn requests for the same conversation are serialized by an in-process semaphore. The filesystem persistence layer is suitable for a single local API process, not multi-instance deployment.

## Understand the architecture

```text
01.BasicChat ------------> Chat Completions SDK
02.ResponsesChat ---+
03.ContextChat -----+
04.StreamingChat ---+
05.AsyncChat -------+----> Shared.FoundryClient ----> Azure OpenAI /openai/v1/
06.ChatHistory -----+               |
07.ChatApi ---------+               +-- JSON conversation store
08.BlazorChat --------------HTTP/SSE--> 07.ChatApi
```

The original lab's five stages are preserved as **distinct implementations** rather than one ever-changing program. Projects 06–08 are practical follow-on extensions. Conversation continuity depends on the Foundry Responses service retaining the referenced response ID. If responses expire or cannot be retrieved, start a new conversation; storing local text alone is not enough to recreate all server-side context with `previous_response_id`.

## Troubleshooting

- **`AZURE_OPENAI_ENDPOINT` / `MODEL_DEPLOYMENT` missing:** fill in `src/Shared.FoundryClient/.env` before running a project (or provide environment variables).
- **401 / 403:** run `az login`, confirm the subscription/tenant, and verify resource RBAC. If you set `AZURE_OPENAI_API_KEY`, key auth takes precedence.
- **404:** use the *deployment name*, correct resource endpoint, and make sure the deployed model supports the API you are calling.
- **Unsupported API / model errors:** confirm the deployed model supports Chat Completions and/or Responses and the region exposes the needed API. Some Foundry models may support only one.
- **`previous_response_id` invalid:** begin a new chat; response continuity depends on server-side stored state.
- **Blazor cannot connect:** start `07.ChatApi` first and confirm `/api/health` returns HTTP 200. Ensure `CHAT_API_URL` matches the API URL.
- **NuGet errors:** verify NuGet network access and restore the centrally managed packages: `OpenAI 2.15.0` and `Azure.Identity 1.21.0`.
- **Build warnings `OPENAI001`:** the official Responses SDK marks some APIs experimental, even when underlying REST features are available.

## References

- Microsoft Learn lab: https://microsoftlearning.github.io/mslearn-ai-studio/Instructions/Exercises/03-foundry-sdk.html
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
