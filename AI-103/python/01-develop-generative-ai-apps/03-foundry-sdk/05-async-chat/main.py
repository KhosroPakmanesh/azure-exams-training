import asyncio
import signal

from shared.console import prompts, show_error
from shared.foundry import FoundrySettings, async_sdk_client


async def main() -> int:
    print("=== 05: Async chatbot with cancellation ===")
    print("Press Ctrl+C during a model request to cancel that turn.\n")
    try:
        settings = FoundrySettings.from_environment()
        async with async_sdk_client(settings) as client:
            previous_response_id = None
            for prompt in prompts():
                print("Sending asynchronous request ...")
                loop = asyncio.get_running_loop()
                request = loop.create_task(
                    client.responses.create(
                        model=settings.deployment,
                        instructions=settings.instructions,
                        input=prompt,
                        previous_response_id=previous_response_id,
                    )
                )
                previous_handler = signal.getsignal(signal.SIGINT)

                def cancel_request(_signal: int, _frame: object) -> None:
                    loop.call_soon_threadsafe(request.cancel)

                signal.signal(signal.SIGINT, cancel_request)
                try:
                    answer = await request
                    print(f"Assistant: {answer.output_text}\n")
                    previous_response_id = answer.id
                except asyncio.CancelledError:
                    print("Request canceled.\n")
                except Exception as error:
                    show_error(error)
                finally:
                    signal.signal(signal.SIGINT, previous_handler)
    except Exception as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(asyncio.run(main()))
