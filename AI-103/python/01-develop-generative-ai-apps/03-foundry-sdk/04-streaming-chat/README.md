# Streaming Chat

Display text as the Responses API sends it and carry context forward after a completion event. Follow the stages in [main.py](main.py).

## Execution flow

```mermaid
flowchart TD
    A["Read a question"] --> B["Receive streamed text deltas"] --> C["Save the completed response ID"]
```

## Run

From `AI-103/python`, follow the [language setup](../../../README.md) for live Azure requests.

```sh
python -m 01-develop-generative-ai-apps.03-foundry-sdk.04-streaming-chat.main
```

One streaming model request per nonempty prompt.

## Understand the results

Text appears incrementally. The completion event supplies the ID for the next turn.

## Learn by changing an input

Ask for a long explanation, then send a follow-up about one point in it.

## Verify

Run the exercise with a compatible deployment and inspect the behavior described above.

## Files to read

- [main.py](main.py) shows the main execution flow.
- [Shared Foundry settings](../../../shared/foundry.py) load the endpoint, model deployment, and credentials.

[Other language](../../../../C%23/01-develop-generative-ai-apps/03-foundry-sdk/04-streaming-chat/README.md) · [Chapter study notes](../../../../study/01-develop-generative-ai-apps/03-foundry-sdk/README.md) · [AI-103 objectives](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-103)
