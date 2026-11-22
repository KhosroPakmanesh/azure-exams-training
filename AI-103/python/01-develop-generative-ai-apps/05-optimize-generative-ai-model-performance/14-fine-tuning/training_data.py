"""Generate and validate fictional three-message JSONL examples."""

import json
from dataclasses import dataclass
from pathlib import Path

from shared.reports import load_json

INSTRUCTIONS = (
    "Classify the travel support customer's message into exactly one uppercase label: "
    "BOOKING, CANCELLATION, BAGGAGE, or OTHER. Do not add any explanation."
)
LABELS = {"BOOKING", "CANCELLATION", "BAGGAGE", "OTHER"}
ROLES = ("system", "user", "assistant")


@dataclass(frozen=True)
class Example:
    message: str
    label: str


def prepare(folder: Path) -> None:
    groups = load_json(Path(__file__).parent / "samples/phrases.json")
    folder.mkdir(parents=True, exist_ok=True)
    for name, start, stop in (("train", 0, 10), ("validation", 10, 13), ("test", 13, 16)):
        with (folder / f"{name}.jsonl").open("w", encoding="utf-8") as output:
            for label, phrases in groups.items():
                for phrase in phrases[start:stop]:
                    messages = [
                        {"role": role, "content": content}
                        for role, content in zip(ROLES, (INSTRUCTIONS, phrase, label))
                    ]
                    output.write(json.dumps({"messages": messages}, ensure_ascii=False) + "\n")


def validate(path: Path) -> list[Example]:
    examples = []
    observed = set()
    with path.open(encoding="utf-8-sig") as source:
        for line_number, line in enumerate(source, 1):
            try:
                record = json.loads(line)
                messages = record.get("messages")
                if not isinstance(messages, list) or len(messages) != len(ROLES):
                    raise ValueError("Expected exactly three messages (system, user, assistant).")
                contents = []
                for message, role in zip(messages, ROLES):
                    if not isinstance(message, dict) or message.get("role") != role:
                        raise ValueError(f"Expected message role {role}.")
                    content = message.get("content")
                    if not isinstance(content, str) or not content.strip():
                        raise ValueError(f"Message {role} requires content.")
                    contents.append(content)
                text, label = contents[1:]
                if label not in LABELS:
                    raise ValueError(
                        "Assistant label must be BOOKING, CANCELLATION, BAGGAGE, or OTHER."
                    )
                if text.casefold() in observed:
                    raise ValueError("Duplicate user message in file.")
                observed.add(text.casefold())
                examples.append(Example(text, label))
            except (AttributeError, ValueError) as error:
                raise ValueError(f"{path}:{line_number}: {error}") from error
    if not examples:
        raise ValueError(f"{path}: JSONL has no examples.")
    return examples
