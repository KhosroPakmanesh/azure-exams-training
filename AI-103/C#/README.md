# AI-103 C# labs

Install the .NET 11 SDK specified by [global.json](global.json). Open [AI-103.CSharp.slnx](AI-103.CSharp.slnx) in Visual Studio, choose a lab as the startup project, and press F5.

## Configure Foundry

Copy [shared/.env.example](shared/.env.example) to `shared/.env` and set `AZURE_OPENAI_ENDPOINT` to your Azure OpenAI resource URL, such as `https://YOUR-RESOURCE.openai.azure.com/`. Set `MODEL_DEPLOYMENT` to the name of the deployed model. The loader also accepts a resource URL ending in `/openai/v1/`.

Set `AZURE_OPENAI_API_KEY` for key authentication. Leave it blank and use `az login` with an account that has access to the deployment for Microsoft Entra ID authentication. Process environment variables override the file. Set `SYSTEM_INSTRUCTIONS` to override the chat default. Published apps can use process environment variables.

Pick a deployment that supports the API and tools your lab uses. Chat Completions and Responses capabilities vary by model. If a request returns 401 or 403, check the credential, tenant, and resource permissions. For 404 or unsupported-tool errors, verify the resource endpoint, model deployment name, and capabilities.

## Build and run

Run these commands from `AI-103/C#`.

```sh
dotnet restore AI-103.CSharp.slnx
dotnet build AI-103.CSharp.slnx -c Release --no-restore
dotnet run --project "01-develop-generative-ai-apps/03-foundry-sdk/01-basic-chat"
```

The [lab index](../README.md) links to the commands for each lab. Interactive chat uses `quit` to exit. Set `CHAT_HISTORY_DIR` in the process environment to choose where Chat History saves its local plaintext JSON.

The default history folder is `AzureExamsTraining/AI-103/CSharp/Conversations` under your local application data directory. The lab reads histories saved under the earlier default and writes new or updated conversations to the new folder.

## Publish and package

From `AI-103/C#`, run `./scripts/publish-all.ps1` in PowerShell or `bash scripts/publish-all.sh` in Bash. Published projects go to `AI-103/artifacts/publish/`.

From `AI-103`, run `./scripts/build-and-package.ps1` or `bash scripts/build-and-package.sh` to create `artifacts/AI-103-Training-With-CSharp-Binaries.zip`. The package includes source for both languages, study notes, and the published C# assemblies. The Bash publish helper and either packaging command require Python 3.10 or newer.
