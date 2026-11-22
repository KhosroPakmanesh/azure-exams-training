from shared.console import prompts, show_error
from shared.foundry import FoundrySettings, sdk_client


def main() -> int:
    print("=== 04: Streaming Responses API ===")
    try:
        settings = FoundrySettings.from_environment()
        with sdk_client(settings) as client:
            previous_response_id = None
            for prompt in prompts():
                print("Assistant: ", end="", flush=True)
                try:
                    with client.responses.create(
                        model=settings.deployment,
                        instructions=settings.instructions,
                        input=prompt,
                        previous_response_id=previous_response_id,
                        stream=True,
                    ) as stream:
                        completed = False
                        for event in stream:
                            if event.type == "response.output_text.delta":
                                print(event.delta, end="", flush=True)
                            elif event.type == "response.completed":
                                previous_response_id = event.response.id
                                completed = True
                        if not completed:
                            raise RuntimeError(
                                "The response stream closed without a completion event."
                            )
                    print("\n")
                except Exception as error:
                    print()
                    show_error(error)
    except Exception as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
