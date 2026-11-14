# Local verification checklist

This source ZIP has not been compiled on the generation machine, which lacks .NET. On your development machine:

- [ ] `dotnet --info` shows .NET 11 SDK (11.0.100 or newer compatible 11.x SDK).
- [ ] `dotnet restore FoundryCSharpLabs.slnx` succeeds.
- [ ] `dotnet build FoundryCSharpLabs.slnx -c Release` succeeds.
- [ ] Azure OpenAI endpoint + deployment + Azure CLI authentication or API key are configured.
- [ ] Project 01 Chat Completions returns one response.
- [ ] Project 02 Responses API returns one response.
- [ ] Project 03 tracks a follow-up question about ELIZA.
- [ ] Project 04 prints a streamed reply and maintains context.
- [ ] Project 05 awaits responses and can cancel with Ctrl+C.
- [ ] Project 06 creates, lists, and resumes a conversation after restarting.
- [ ] Project 07 `/api/health` returns success, API chat persists on disk, `/stream` delivers deltas and done.
- [ ] Project 08 displays saved chats and streamed response text at http://localhost:5081.

Before deploying publicly, add user authentication, per-user storage isolation, rate limits, network TLS, monitoring, retention policies, and a production-grade database.

- [ ] `python3 scripts/verify-source.py` passes structural checks (no .NET SDK required).
- [ ] `dotnet sln FoundryCSharpLabs.slnx list` shows all nine projects.

## Local credentials

- [ ] Edit `src/Shared.FoundryClient/.env` with your Foundry endpoint and exact deployment name.
- [ ] Put your API key in `AZURE_OPENAI_API_KEY` or leave it blank and run `az login`.
- [ ] Never commit your edited `.env` or include it in a public ZIP.
