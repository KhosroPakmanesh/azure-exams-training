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
- [ ] Project 07 uses Code Interpreter for a Python calculation.
- [ ] Project 08 uses Web Search and provides source citations.
- [ ] Project 09 uploads a document into a vector store, waits for indexing, and answers a document question.
- [ ] Project 10 invokes the approved local `get_time` function and returns its output to the model.
- [ ] Project 11 answers a current destination question using web search, then a travel-brochure question using file search.

- [ ] `python3 scripts/verify-source.py` passes structural checks (no .NET SDK required).
- [ ] `dotnet sln FoundryCSharpLabs.slnx list` shows all twelve projects.

## Local credentials

- [ ] Copy `src/Shared.FoundryClient/.env.example` to `.env` if needed, then set the Foundry endpoint and exact deployment name.
- [ ] Put your API key in `AZURE_OPENAI_API_KEY` or leave it blank and run `az login`.
- [ ] Never commit your edited `.env` or include it in a public ZIP.

## File search resources

- [ ] Supply PDFs/documents or an existing `vs_...` ID to Project 09.
- [ ] Supply the official Margie’s Travel brochure PDFs or a prepared `vs_...` ID to Project 11.
- [ ] Record created vector store IDs for reuse. Delete unused vector stores and uploaded files in Azure to avoid charges.
