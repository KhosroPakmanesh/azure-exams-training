import json
from datetime import datetime
from typing import Any

from shared.console import print_tool_answer, prompts, show_error
from shared.foundry import FoundryRestClient, FoundrySettings, require_id, rest_client

TOOLS = [
    {
        "type": "function",
        "name": "get_time",
        "description": "Return the current local date and time from this application's host computer.",
        "parameters": {
            "type": "object",
            "properties": {},
            "required": [],
            "additionalProperties": False,
        },
        "strict": True,
    },
]


def execute_time_call(call: dict[str, Any]) -> dict[str, str]:
    name = call.get("name")
    if name != "get_time":
        raise ValueError(f"Unexpected function requested: {name}")
    if json.loads(call.get("arguments") or "{}") != {}:
        raise ValueError("get_time takes no arguments.")
    print("[Executing approved local function: get_time]")
    local_time = datetime.now().astimezone()
    offset = local_time.strftime("%z")
    return {
        "type": "function_call_output",
        "call_id": call["call_id"],
        "output": f"{local_time:%Y-%m-%d %H:%M:%S} {offset[:3]}:{offset[3:]}",
    }


def complete_turn(
    client: FoundryRestClient, prompt: str, previous_response_id: str | None
) -> dict[str, Any]:
    response = client.respond(prompt, TOOLS, previous_response_id)
    for round_number in range(8):
        calls = [item for item in response.get("output", []) if item["type"] == "function_call"]
        if not calls:
            return response
        if round_number == 7:
            break
        results = [execute_time_call(call) for call in calls]
        response = client.respond(results, TOOLS, require_id(response))
    raise RuntimeError("Too many consecutive function-call rounds.")


def main() -> int:
    print("=== 10: function tool (get_time) ===")
    print("Try: What is the current local time?\n")
    try:
        settings = FoundrySettings.from_environment()
        with rest_client(settings) as client:
            previous_response_id = None
            for prompt in prompts():
                try:
                    response = complete_turn(client, prompt, previous_response_id)
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
