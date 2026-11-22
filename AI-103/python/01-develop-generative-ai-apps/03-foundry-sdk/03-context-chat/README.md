# Context Chat

Pass the last successful response ID to give the model conversation context. Follow the stages in [main.py](main.py).

## Execution flow

```mermaid
flowchart TD
    A["Read a follow-up"] --> B["Send it with the previous response ID"] --> C["Save the ID of the completed turn"]
```

## Run

From `AI-103/python`, follow the [language setup](../../../README.md) for live Azure requests.

```sh
python -m 01-develop-generative-ai-apps.03-foundry-sdk.03-context-chat.main
```

One model request per nonempty prompt.

## Understand the results

The model can resolve references to earlier turns while the service retains the response.

## Learn by changing an input

Ask about ELIZA, then ask how it compares to modern models. Compare the same prompts in Responses Chat.

## Verify

Run the exercise with a compatible deployment and inspect the behavior described above.

## Files to read

- [main.py](main.py) shows the main execution flow.
- [Shared Foundry settings](../../../shared/foundry.py) load the endpoint, model deployment, and credentials.

[Other language](../../../../C%23/01-develop-generative-ai-apps/03-foundry-sdk/03-context-chat/README.md) · [Chapter study notes](../../../../study/01-develop-generative-ai-apps/03-foundry-sdk/README.md) · [AI-103 objectives](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-103)
