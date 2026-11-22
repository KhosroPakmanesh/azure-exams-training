from datetime import datetime
from uuid import UUID

from shared.console import show_error
from shared.foundry import FoundrySettings, sdk_client

from .conversations import ConversationStore


def main() -> int:
    print("=== 06: Persistent chat history ===")
    print("Commands: new [title], list, use <conversation-id>, show, quit")
    print("Otherwise type a message to chat.\n")
    try:
        settings = FoundrySettings.from_environment()
        store = ConversationStore()
        active = None
        with sdk_client(settings) as client:
            while True:
                try:
                    text = input(f"{active['Title'] if active else 'history'}> ").strip()
                except EOFError:
                    return 0
                command = text.casefold()
                if command == "quit":
                    return 0
                if not text:
                    continue
                try:
                    if command == "list":
                        for item in store.list():
                            print(
                                f"{item['Id']}  {item['Title']}  ({len(item['Messages'])} messages)"
                            )
                    elif command == "new" or command.startswith("new "):
                        active = store.create(text[4:])
                        print(f"Created {active['Id']}\n")
                    elif command.startswith("use "):
                        try:
                            conversation_id = UUID(text[4:])
                        except ValueError:
                            print("Enter a valid conversation GUID.\n")
                            continue
                        active = store.get(conversation_id)
                        print(
                            f"Resumed: {active['Title']}\n"
                            if active
                            else "Conversation not found.\n"
                        )
                    elif command == "show":
                        if active:
                            for turn in active["Messages"]:
                                print(f"{turn['Role']}: {turn['Content']}")
                            print()
                        else:
                            print("No active conversation.\n")
                    else:
                        if active is None:
                            active = store.create(f"Chat {datetime.now():%Y-%m-%d %H:%M}")
                        response = client.responses.create(
                            model=settings.deployment,
                            instructions=settings.instructions,
                            input=text,
                            previous_response_id=active["LastResponseId"],
                        )
                        active = store.append_exchange(
                            active, text, response.output_text, response.id
                        )
                        print(f"Assistant: {response.output_text}\n")
                except Exception as error:
                    show_error(error)
    except Exception as error:
        show_error(error)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
