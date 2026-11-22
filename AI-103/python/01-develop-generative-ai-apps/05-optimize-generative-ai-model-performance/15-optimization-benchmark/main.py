import argparse
from collections import defaultdict
from pathlib import Path
from time import perf_counter
from typing import Any

from shared.console import show_error
from shared.foundry import FoundryRestClient, FoundrySettings, output_text, rest_client
from shared.reports import load_json, positive_limit, write_csv

HEADER = (
    "case_id",
    "task",
    "strategy",
    "expected",
    "output",
    "correct",
    "latency_ms",
    "input_tokens",
    "output_tokens",
    "error",
)
SIMPLE_INTENT = (
    "Classify the travel support intent into one of BOOKING, CANCELLATION, BAGGAGE, OTHER. "
    "Reply with one label."
)
FEW_SHOT = (
    "You are a travel support intent classifier. Reply with exactly one uppercase label: "
    "BOOKING, CANCELLATION, BAGGAGE, OTHER.\n"
    "Examples: Change my reservation => BOOKING; Refund my cancelled flight => CANCELLATION; "
    "What is my luggage allowance? => BAGGAGE; What's the weather? => OTHER. No extra text."
)
BASELINE_FACT = "Answer the travel customer question in one short sentence."
GROUNDED_FACT = (
    "Answer only when you have reliable information. Do not invent travel provider policies. "
    "If information is unavailable, reply 'I don't know based on the supplied information.'"
)
RAG_FACT = (
    "Answer only using documents available through file_search. "
    "When no source supports the answer, say you do not know. Cite sources when possible."
)


def evaluate(
    client: FoundryRestClient,
    *,
    case: dict[str, Any],
    task: str,
    strategy: str,
    deployment: str,
    instructions: str,
    store: str | None = None,
) -> tuple[object, ...]:
    question, expected = (
        (case["message"], case["expected"])
        if task == "intent"
        else (case["question"], case["expectedFact"])
    )
    started = perf_counter()
    try:
        if store:
            response = client.respond(
                question,
                [{"type": "file_search", "vector_store_ids": [store]}],
                instructions=instructions,
            )
        else:
            response = client.post(
                "responses",
                {
                    "model": deployment,
                    "instructions": instructions,
                    "input": question,
                    "store": False,
                },
            )
        elapsed_ms = round((perf_counter() - started) * 1000)
        answer = output_text(response).strip()
        actual, target = answer.casefold(), expected.casefold()
        correct = actual == target if task == "intent" else target in actual
        usage = response.get("usage") or {}
        print(
            f"{case['id']:<7} {strategy:<28} {'PASS' if correct else 'FAIL'} answer={answer[:90]}"
        )
        return (
            case["id"],
            task,
            strategy,
            expected,
            answer,
            str(correct).lower(),
            elapsed_ms,
            usage.get("input_tokens"),
            usage.get("output_tokens"),
            "",
        )
    except Exception as error:
        elapsed_ms = round((perf_counter() - started) * 1000)
        print(f"{case['id']:<7} {strategy:<28} ERROR {error}")
        return case["id"], task, strategy, expected, "", "false", elapsed_ms, None, None, str(error)


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Compare strategies separately on intent and policy facts."
    )
    parser.add_argument("--preview", action="store_true")
    parser.add_argument("--store")
    parser.add_argument("--fine-tuned-deployment")
    parser.add_argument("--limit", type=positive_limit)
    parser.add_argument("--output", type=Path, default=Path("optimization-benchmark.csv"))
    args = parser.parse_args()

    try:
        dataset = load_json(Path(__file__).parent / "samples/benchmark.json")
        intents = dataset["intents"][: args.limit]
        facts = dataset["facts"][: args.limit]
        if not intents or not facts:
            raise ValueError("Both task tracks need evaluation cases.")

        if args.preview:
            print(
                f"Offline preview: {len(intents)} intent tests and {len(facts)} policy-fact tests."
            )
            print("Intent: baseline, few-shot, and optionally a fine-tuned deployment.")
            print("Policy facts: baseline, grounding instructions, and optionally file_search.")
            return 0

        settings = FoundrySettings.from_environment()
        intent_strategies = [
            ("baseline", settings.deployment, SIMPLE_INTENT),
            ("few-shot", settings.deployment, FEW_SHOT),
        ]
        if args.fine_tuned_deployment:
            intent_strategies.append(("fine-tuned", args.fine_tuned_deployment, SIMPLE_INTENT))
        fact_strategies = [
            ("baseline", BASELINE_FACT, None),
            ("grounding-instructions-only", GROUNDED_FACT, None),
        ]
        if args.store:
            fact_strategies.append(("file-search-RAG", RAG_FACT, args.store))

        with rest_client(settings) as client:
            rows = [
                evaluate(
                    client,
                    case=case,
                    task="intent",
                    strategy=strategy,
                    deployment=deployment,
                    instructions=instructions,
                )
                for case in intents
                for strategy, deployment, instructions in intent_strategies
            ]
            rows.extend(
                evaluate(
                    client,
                    case=case,
                    task="policy facts",
                    strategy=strategy,
                    deployment=settings.deployment,
                    instructions=instructions,
                    store=store,
                )
                for case in facts
                for strategy, instructions, store in fact_strategies
            )
        write_csv(args.output, HEADER, rows)
        groups: dict[tuple[object, object], list[tuple[object, ...]]] = defaultdict(list)
        for row in rows:
            groups[row[1], row[2]].append(row)
        for (task, strategy), group in groups.items():
            completed = [row for row in group if not row[9]]
            correct = sum(row[5] == "true" for row in completed)
            print(
                f"{task} / {strategy}: {correct}/{len(completed)} correct, {len(group) - len(completed)} errors"
            )
        print(f"CSV: {args.output.resolve()}")
    except Exception as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
