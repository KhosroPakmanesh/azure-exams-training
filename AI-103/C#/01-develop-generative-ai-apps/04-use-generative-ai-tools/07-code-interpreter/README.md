# Code Interpreter

Give the model access to hosted Python for a calculation or data analysis. Follow the stages in [Program.cs](Program.cs).

## Execution flow

```mermaid
flowchart TD
    A["Declare the Code Interpreter tool"] --> B["Send the calculation through Responses"] --> C["Show the model explanation"]
```

## Run

From `AI-103/C#`, follow the [language setup](../../../README.md) for live Azure requests.

```sh
dotnet run --project "01-develop-generative-ai-apps/04-use-generative-ai-tools/07-code-interpreter"
```

One Responses request per prompt. Hosted tool use can incur additional charges.

## Understand the results

The model can use a hosted Python container and explain the result.

## Learn by changing an input

Ask for the square root of 16, then ask for an intermediate calculation and check it by hand.

## Verify

Run the exercise with a compatible deployment and inspect the behavior described above.

## Files to read

- [Program.cs](Program.cs) shows the main execution flow.
- [Shared Foundry settings](../../../shared/FoundryOptions.cs) load the endpoint, model deployment, and credentials.
- See [OutputPrinter.cs](OutputPrinter.cs) for the lesson’s console presentation.

[Other language](../../../../python/01-develop-generative-ai-apps/04-use-generative-ai-tools/07-code-interpreter/README.md) · [Chapter study notes](../../../../study/01-develop-generative-ai-apps/04-use-generative-ai-tools/README.md) · [AI-103 objectives](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-103)
