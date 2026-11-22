import sys

from shared.console import print_tool_answer, prompts, show_error
from shared.foundry import FoundrySettings, require_id, rest_client
from shared.vector_store import resolve_store


def main() -> int:
    print("=== 11: Margie's Travel (web_search + file_search) ===")
    print("Pass a brochure directory, PDFs, or a vs_... store ID.")
    print("Try: What's happening in San Francisco next month?")
    print("Then: What hotels does Margie's Travel offer there?")
    try:
        settings = FoundrySettings.from_environment()
        with rest_client(settings) as client:
            store_id = resolve_store(client, sys.argv[1:], "travel-brochures", pdf_only=True)
            tools = [
                {"type": "file_search", "vector_store_ids": [store_id]},
                {"type": "web_search"},
            ]
            instructions = (
                "You are a travel assistant that provides information on travel services "
                "available from Margie's Travel. Answer questions about services offered by "
                "Margie's Travel using the provided travel brochures. Search the web for "
                "general information about destinations or current travel advice. "
                "Do not invent travel services that are not documented in the brochures."
            )
            previous_response_id = None
            for prompt in prompts():
                try:
                    response = client.respond(
                        prompt, tools, previous_response_id, instructions=instructions
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
