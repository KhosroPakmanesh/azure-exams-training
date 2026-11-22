from shared.console import prompts, show_error
from shared.foundry import FoundrySettings, sdk_client


def main() -> int:
    print("=== 02: Responses API (stateless) ===")
    try:
        settings = FoundrySettings.from_environment()
        with sdk_client(settings) as client:
            for prompt in prompts():
                try:
                    # Send each question independently through the Responses API.
                    response = client.responses.create(
                        model=settings.deployment,
                        instructions=settings.instructions,
                        input=prompt,
                    )
                    print(f"Assistant: {response.output_text}\n")
                except Exception as error:
                    show_error(error)
    except Exception as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
