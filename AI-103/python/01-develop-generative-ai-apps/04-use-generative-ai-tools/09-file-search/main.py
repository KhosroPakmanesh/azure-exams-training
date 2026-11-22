import sys

from shared.console import print_tool_answer, prompts, show_error
from shared.foundry import FoundrySettings, require_id, rest_client
from shared.vector_store import resolve_store


def main() -> int:
    print("=== 09: file_search tool ===")
    print("Pass a document directory, files, or a vs_... store ID.")
    try:
        settings = FoundrySettings.from_environment()
        with rest_client(settings) as client:
            store_id = resolve_store(client, sys.argv[1:], "file-search-lab")
            tools = [{"type": "file_search", "vector_store_ids": [store_id]}]
            previous_response_id = None
            for prompt in prompts():
                try:
                    response = client.respond(
                        prompt,
                        tools,
                        previous_response_id,
                        instructions="Answer questions using the supplied documents. "
                        "When the documents do not contain the answer, say so.",
                        include=["file_search_call.results"],
                    )
                    print_tool_answer(response)
                    previous_response_id = require_id(response)
                except Exception as error:
                    show_error(error)
    except Exception as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
