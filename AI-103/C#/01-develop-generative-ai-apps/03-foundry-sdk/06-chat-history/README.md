# Chat History

Save conversations locally and resume a Responses API conversation by its stored response ID. Follow the stages in [Program.cs](Program.cs).

## Execution flow

```mermaid
flowchart TD
    A["Load or create a conversation"] --> B["Send a turn with its saved response ID"] --> C["Save the exchange to local JSON"]
```

## Run

From `AI-103/C#`, follow the [language setup](../../../README.md) for live Azure requests.

```sh
dotnet run --project "01-develop-generative-ai-apps/03-foundry-sdk/06-chat-history"
```

History commands are local. A chat turn makes one model request.

## Understand the results

The JSON stores plaintext messages and the last response ID. Local history can still be read if a server-side response expires.

## Learn by changing an input

Use `new Demo`, send a message, quit, restart, and use `list`, `use <id>`, and `show`.

## Verify

Run the exercise with a compatible deployment and inspect the behavior described above.

## Files to read

- [Program.cs](Program.cs) shows the main execution flow.
- [Shared Foundry settings](../../../shared/FoundryOptions.cs) load the endpoint, model deployment, and credentials.
- See [OutputPrinter.cs](OutputPrinter.cs) for the lesson’s console presentation.

[Other language](../../../../python/01-develop-generative-ai-apps/03-foundry-sdk/06-chat-history/README.md) · [Chapter study notes](../../../../study/01-develop-generative-ai-apps/03-foundry-sdk/README.md) · [AI-103 objectives](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-103)
