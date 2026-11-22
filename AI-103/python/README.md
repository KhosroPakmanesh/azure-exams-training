# AI-103 Python labs

Use Python 3.10 or newer. From `AI-103/python`, create and activate a virtual environment, then install the lab dependencies.

```sh
python -m venv .venv
```

Activate the environment with `./.venv/Scripts/Activate.ps1` in Windows PowerShell or `source .venv/bin/activate` in Bash. Then install the dependencies.

```sh
python -m pip install -e .
```

## Configure Foundry

Copy [shared/.env.example](shared/.env.example) to `shared/.env`. Set `AZURE_OPENAI_ENDPOINT` to your Azure OpenAI resource URL and `MODEL_DEPLOYMENT` to the exact deployment name. Use `AZURE_OPENAI_API_KEY` for key authentication, or leave it blank and use `az login` with Microsoft Entra access to the resource. Process environment variables override the file. `SYSTEM_INSTRUCTIONS` is optional.

Python and C# each have a `shared` settings file. Use the same deployment and sample inputs when comparing their results.

## Debug in VS Code

Open `AI-103/python/` in VS Code and enable the Microsoft Python and Python Debugger extensions. Use **Python: Select Interpreter** to choose the interpreter in `.venv/`. In **Run and Debug**, select a lab from [launch.json](.vscode/launch.json) and press F5.

The file-search, travel-brochure, and RAG launches prompt for a document source or vector store ID. Launches marked **Preview** use the lab's offline preview. **14 Fine-Tuning (Help)** shows the available commands. Use the individual lab READMEs for arguments to run the other modes.

## Run

From `AI-103/python`, use the module name so Python can resolve the numbered course folders and the `shared` package.

```sh
python -m 01-develop-generative-ai-apps.03-foundry-sdk.01-basic-chat.main
```

The [lab index](../README.md) links to the run command, required inputs, and offline or live modes for every lab. Interactive chat uses `quit` to exit. Chat History stores local plaintext JSON and accepts `CHAT_HISTORY_DIR` as a process environment variable.

The default history folder is `AzureExamsTraining/AI-103/Python/Conversations` under `LOCALAPPDATA` on Windows or `~/.local/share` otherwise. The lab reads histories saved under the earlier default and writes new or updated conversations to the new folder.
