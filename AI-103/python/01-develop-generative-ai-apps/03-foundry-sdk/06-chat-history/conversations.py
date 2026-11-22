"""Create, load, and atomically save a local conversation."""

import json
import os
from datetime import datetime, timezone
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import Any
from uuid import UUID, uuid4


def now_utc() -> str:
    return datetime.now(timezone.utc).isoformat()


class ConversationStore:
    def __init__(self) -> None:
        configured = os.getenv("CHAT_HISTORY_DIR")
        data_root = Path(os.getenv("LOCALAPPDATA") or Path.home() / ".local" / "share")
        self._legacy_directory = None
        if configured:
            self.directory = Path(configured)
        else:
            self.directory = (
                data_root / "AzureExamsTraining" / "AI-103" / "Python" / "Conversations"
            )
            self._legacy_directory = data_root / "FoundryPythonLabs" / "Conversations"
        self.directory.mkdir(parents=True, exist_ok=True)

    def create(self, title: str | None) -> dict[str, Any]:
        conversation = {
            "Id": str(uuid4()),
            "Title": title.strip()[:80] if title and title.strip() else "Untitled conversation",
            "CreatedAtUtc": now_utc(),
            "UpdatedAtUtc": now_utc(),
            "LastResponseId": None,
            "Messages": [],
        }
        self.save(conversation)
        return conversation

    def get(self, conversation_id: UUID) -> dict[str, Any] | None:
        for directory in self._read_directories():
            path = directory / f"{conversation_id}.json"
            try:
                return json.loads(path.read_text(encoding="utf-8-sig"))
            except FileNotFoundError:
                continue
        return None

    def list(self) -> list[dict[str, Any]]:
        paths: dict[str, Path] = {}
        for directory in self._read_directories():
            for path in directory.glob("*.json"):
                paths.setdefault(path.name.casefold(), path)
        conversations = (
            json.loads(path.read_text(encoding="utf-8-sig")) for path in paths.values()
        )
        return sorted(
            conversations,
            key=lambda item: datetime.fromisoformat(item["UpdatedAtUtc"]),
            reverse=True,
        )

    def save(self, conversation: dict[str, Any]) -> None:
        target = self.directory / f"{UUID(conversation['Id'])}.json"
        conversation["UpdatedAtUtc"] = now_utc()
        temporary = None
        try:
            with NamedTemporaryFile(
                mode="w", encoding="utf-8", dir=self.directory, suffix=".tmp", delete=False
            ) as output:
                temporary = Path(output.name)
                json.dump(conversation, output, ensure_ascii=False, indent=2)
            temporary.replace(target)
        finally:
            if temporary is not None:
                temporary.unlink(missing_ok=True)

    def append_exchange(
        self, conversation: dict[str, Any], prompt: str, answer: str, response_id: str
    ) -> dict[str, Any]:
        updated = {
            **conversation,
            "LastResponseId": response_id,
            "Messages": [
                *conversation["Messages"],
                {"Role": "user", "Content": prompt, "CreatedAtUtc": now_utc()},
                {"Role": "assistant", "Content": answer, "CreatedAtUtc": now_utc()},
            ],
        }
        self.save(updated)
        return updated

    def _read_directories(self) -> tuple[Path, ...]:
        # Histories saved in the new directory take precedence over the previous defaults.
        if self._legacy_directory is None:
            return (self.directory,)
        return self.directory, self._legacy_directory
