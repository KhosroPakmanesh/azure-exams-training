from shared.console import prompts, show_error
from shared.foundry import FoundrySettings, sdk_client


def main() -> int:
    print("=== 03: Context-aware Responses API ===")
    try:
        settings = FoundrySettings.from_environment()
        with sdk_client(settings) as client:
            previous_response_id = None
            for prompt in prompts():
                try:
                    # Pass the last successful response ID to retain the context.
                    response = client.responses.create(
                        model=settings.deployment,
                        instructions=settings.instructions,
                        input=prompt,
                        previous_response_id=previous_response_id,
                    )
                    print(f"Assistant: {response.output_text}\n")
                    previous_response_id = response.id
                except Exception as error:
                    show_error(error)
    except Exception as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
