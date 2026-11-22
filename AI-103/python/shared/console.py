"""Common interactive prompts and response formatting."""

import sys
from typing import Any, Iterator

from shared.foundry import output_text, source_citations


def prompts() -> Iterator[str]:
    while True:
        try:
            prompt = input("you> ").strip()
        except EOFError:
            return
        if prompt.casefold() == "quit":
            return
        if prompt:
            yield prompt


def print_tool_answer(response: dict[str, Any]) -> None:
    print(f"Assistant: {output_text(response)}")
    for source in source_citations(response):
        print(f"  Source: {source}")
    print()


def show_error(error: Exception) -> None:
    print(f"Error: {error}", file=sys.stderr)
