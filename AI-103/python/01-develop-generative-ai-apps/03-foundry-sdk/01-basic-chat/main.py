from shared.console import prompts, show_error
from shared.foundry import FoundrySettings, sdk_client


def main() -> int:
    """Send independent Chat Completions requests."""
    print("=== 01: Chat Completions API ===")
    try:
        settings = FoundrySettings.from_environment()
        with sdk_client(settings) as client:
            for prompt in prompts():
                try:
                    # Each turn contains only the system instruction and the current question.
                    completion = client.chat.completions.create(
                        model=settings.deployment,
                        messages=[
                            {"role": "system", "content": settings.instructions},
                            {"role": "user", "content": prompt},
                        ],
                    )
                    print(f"Assistant: {completion.choices[0].message.content or ''}\n")
                except Exception as error:
                    show_error(error)
    except Exception as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
