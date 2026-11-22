# Basic Chat

Send an independent Chat Completions request with a system instruction and your current question. Follow the stages in [main.py](main.py).

## Execution flow

```mermaid
flowchart TD
    A["Read the system instruction"] --> B["Send one Chat Completions request"] --> C["Print the assistant reply"]
```

## Run

From `AI-103/python`, follow the [language setup](../../../README.md) for live Azure requests.

```sh
python -m 01-develop-generative-ai-apps.03-foundry-sdk.01-basic-chat.main
```

One model request per nonempty prompt.

## Understand the results

A follow-up has no server-side reference to the earlier turn.

## Learn by changing an input

Ask about ELIZA, then ask how it compares to modern models. Compare the follow-up in Context Chat.

## Verify

Run the exercise with a compatible deployment and inspect the behavior described above.

## Files to read

- [main.py](main.py) shows the main execution flow.
- [Shared Foundry settings](../../../shared/foundry.py) load the endpoint, model deployment, and credentials.

[Other language](../../../../C%23/01-develop-generative-ai-apps/03-foundry-sdk/01-basic-chat/README.md) · [Chapter study notes](../../../../study/01-develop-generative-ai-apps/03-foundry-sdk/README.md) · [AI-103 objectives](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-103)
