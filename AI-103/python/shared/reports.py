"""Small file and measurement helpers used by the evaluation labs."""

import csv
import json
from pathlib import Path
from typing import Any, Iterable, Sequence


def load_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8-sig"))


def require_unique_ids(rows: list[dict[str, Any]]) -> None:
    ids = [str(row.get("id") or "").strip().casefold() for row in rows]
    if not ids or not all(ids) or len(set(ids)) != len(ids):
        raise ValueError("Provide a nonempty dataset with unique, nonblank IDs.")


def write_csv(path: Path, header: Sequence[str], rows: Iterable[Sequence[object]]) -> None:
    with path.open("w", encoding="utf-8", newline="") as report:
        writer = csv.writer(report)
        writer.writerow(header)
        writer.writerows(rows)


def positive_limit(text: str) -> int:
    number = int(text)
    if number <= 0:
        raise ValueError("Limit must be a positive integer.")
    return number


def review_outcome(prompt: str) -> str:
    try:
        rating = input(prompt).strip().casefold()
    except EOFError:
        rating = ""
    return {"p": "pass", "pass": "pass", "f": "fail", "fail": "fail"}.get(rating, "uncertain")
