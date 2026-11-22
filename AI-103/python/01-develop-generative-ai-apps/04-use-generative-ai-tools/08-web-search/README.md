# Web Search

Let the Responses API use web search and surface source annotations. Follow the stages in [main.py](main.py).

## Execution flow

```mermaid
flowchart TD
    A["Declare the web search tool"] --> B["Ask a time-sensitive question"] --> C["Print the reply and source URLs"]
```

## Run

From `AI-103/python`, follow the [language setup](../../../README.md) for live Azure requests.

```sh
python -m 01-develop-generative-ai-apps.04-use-generative-ai-tools.08-web-search.main
```

One Responses request per prompt. Search availability depends on the deployment.

## Understand the results

Source URLs appear when the service supplies citations. Inspect them for relevance and recency.

## Learn by changing an input

Ask for three recent Microsoft announcements. Check the dates and claims in the cited sources.

## Verify

Run the exercise with a compatible deployment and inspect the behavior described above.

## Files to read

- [main.py](main.py) shows the main execution flow.
- [Shared Foundry settings](../../../shared/foundry.py) load the endpoint, model deployment, and credentials.

[Other language](../../../../C%23/01-develop-generative-ai-apps/04-use-generative-ai-tools/08-web-search/README.md) · [Chapter study notes](../../../../study/01-develop-generative-ai-apps/04-use-generative-ai-tools/README.md) · [AI-103 objectives](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-103)
