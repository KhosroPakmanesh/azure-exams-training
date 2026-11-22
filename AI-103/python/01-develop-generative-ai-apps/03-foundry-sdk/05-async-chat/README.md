# Async Chat

Await a response and cancel a pending turn with Ctrl+C. Follow the stages in [main.py](main.py).

## Execution flow

```mermaid
flowchart TD
    A["Read a question"] --> B["Await a cancellable Responses request"] --> C["Show the reply or cancellation"]
```

## Run

From `AI-103/python`, follow the [language setup](../../../README.md) for live Azure requests.

```sh
python -m 01-develop-generative-ai-apps.03-foundry-sdk.05-async-chat.main
```

One model request per nonempty prompt. Cancelled requests may already have started remotely.

## Understand the results

A completed turn advances the previous response ID. A cancelled turn leaves that reference available for later prompts.

## Learn by changing an input

Ask for a long answer, cancel the request with Ctrl+C, and then send another question.

## Verify

Run the exercise with a compatible deployment and inspect the behavior described above.

## Files to read

- [main.py](main.py) shows the main execution flow.
- [Shared Foundry settings](../../../shared/foundry.py) load the endpoint, model deployment, and credentials.

[Other language](../../../../C%23/01-develop-generative-ai-apps/03-foundry-sdk/05-async-chat/README.md) · [Chapter study notes](../../../../study/01-develop-generative-ai-apps/03-foundry-sdk/README.md) · [AI-103 objectives](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-103)
