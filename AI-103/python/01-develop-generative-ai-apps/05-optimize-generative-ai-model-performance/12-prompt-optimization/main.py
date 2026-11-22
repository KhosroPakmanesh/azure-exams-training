import argparse
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path
from time import perf_counter
from typing import Any

from shared.console import show_error
from shared.foundry import FoundryRestClient, FoundrySettings, output_text, rest_client
from shared.reports import load_json, write_csv

LABELS = {"BOOKING", "CANCELLATION", "BAGGAGE", "OTHER"}
FEW_SHOT = (
    "You are a travel support intent classifier. Return exactly one label: "
    "BOOKING, CANCELLATION, BAGGAGE, or OTHER.\nExamples:\n"
    "Customer: I need to move my flight to Friday.\nLabel: BOOKING\n"
    "Customer: Please cancel the reservation and refund me.\nLabel: CANCELLATION\n"
    "Customer: Is my suitcase too heavy?\nLabel: BAGGAGE\n"
    "Customer: What is the weather in Paris?\nLabel: OTHER"
)
HEADER = (
    "variant",
    "id",
    "expected",
    "predicted",
    "raw",
    "correct",
    "latency_ms",
    "input_tokens",
    "output_tokens",
    "error",
)


@dataclass(frozen=True)
class Variant:
    name: str
    instructions: str
    temperature: float | None = None


VARIANTS = (
    Variant(
        "zero-shot",
        "Classify the travel customer message as BOOKING, CANCELLATION, BAGGAGE, or OTHER. "
        "Reply with one label.",
    ),
    Variant(
        "system-prompt",
        "You are a travel support intent classifier. Return exactly one uppercase label: "
        "BOOKING (new/change a reservation), CANCELLATION (cancel/refund), "
        "BAGGAGE (bags/weight/allowances), or OTHER. Do not explain your answer.",
    ),
    Variant("few-shot", FEW_SHOT),
    Variant("few-shot-temp-0", FEW_SHOT, 0.0),
)


def evaluate(
    client: FoundryRestClient, variant: Variant, case: dict[str, Any]
) -> tuple[object, ...]:
    body = {
        "model": client.settings.deployment,
        "instructions": variant.instructions,
        "input": case["message"],
        "store": False,
    }
    if variant.temperature is not None:
        body["temperature"] = variant.temperature
    started = perf_counter()
    try:
        response = client.post("responses", body)
        elapsed_ms = round((perf_counter() - started) * 1000)
        raw = output_text(response).strip()
        predicted = raw.upper()
        correct = predicted == case["expected"]
        usage = response.get("usage") or {}
        print(
            f"  {case['id']:<10} {'PASS' if correct else 'FAIL'} expected={case['expected']} actual={predicted}"
        )
        return (
            variant.name,
            case["id"],
            case["expected"],
            predicted,
            raw,
            str(correct).lower(),
            elapsed_ms,
            usage.get("input_tokens"),
            usage.get("output_tokens"),
            "",
        )
    except Exception as error:
        elapsed_ms = round((perf_counter() - started) * 1000)
        print(f"  {case['id']:<10} ERROR: {error}")
        return (
            variant.name,
            case["id"],
            case["expected"],
            "",
            "",
            "false",
            elapsed_ms,
            None,
            None,
            str(error),
        )


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Compare four prompts on the same intent examples."
    )
    parser.add_argument(
        "--dataset", type=Path, default=Path(__file__).parent / "samples/evaluation.json"
    )
    parser.add_argument("--output", type=Path, default=Path("prompt-optimization-results.csv"))
    parser.add_argument("--preview", action="store_true")
    args = parser.parse_args()
    try:
        cases = load_json(args.dataset)
        if not cases or any(
            not case.get("id", "").strip()
            or not case.get("message", "").strip()
            or case.get("expected") not in LABELS
            for case in cases
        ):
            raise ValueError("Each case needs an ID, a message, and a valid expected label.")

        if args.preview:
            print(f"Loaded {len(cases)} held-out examples. No API requests were made.")
            for variant in VARIANTS:
                temperature = (
                    variant.temperature if variant.temperature is not None else "model default"
                )
                print(f"\n[{variant.name}] temperature={temperature}\n{variant.instructions}")
            return 0

        rows = []
        with rest_client(FoundrySettings.from_environment()) as client:
            for variant in VARIANTS:
                print(f"\n{variant.name} ({len(cases)} requests):")
                rows.extend(evaluate(client, variant, case) for case in cases)
        write_csv(args.output, HEADER, rows)
        groups: dict[str, list[tuple[object, ...]]] = defaultdict(list)
        for row in rows:
            groups[str(row[0])].append(row)
        for name, group in groups.items():
            completed = [row for row in group if not row[9]]
            correct = sum(row[5] == "true" for row in completed)
            print(
                f"{name:<20} {correct}/{len(completed)} correct, {len(group) - len(completed)} errors"
            )
        print(f"Results saved: {args.output.resolve()}")
    except Exception as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
