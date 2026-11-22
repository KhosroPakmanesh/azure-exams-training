from shared.console import print_tool_answer, prompts, show_error
from shared.foundry import FoundrySettings, require_id, rest_client


def main() -> int:
    print("=== 07: code_interpreter tool ===")
    print("Try: Calculate the square root of 16 using Python.")
    try:
        settings = FoundrySettings.from_environment()
        with rest_client(settings) as client:
            tools = [{"type": "code_interpreter", "container": {"type": "auto"}}]
            previous_response_id = None
            for prompt in prompts():
                try:
                    response = client.respond(
                        prompt,
                        tools,
                        previous_response_id,
                        instructions="Use the python tool for calculations or data analysis. "
                        "Explain the result clearly.",
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
