import argparse
from pathlib import Path
from time import perf_counter
from urllib.parse import quote

from shared.console import show_error
from shared.foundry import FoundryRestClient, FoundrySettings, output_text, require_id, rest_client
from shared.reports import write_csv

from .training_data import INSTRUCTIONS, Example, prepare, validate

HEADER = (
    "case",
    "model",
    "expected",
    "predicted",
    "correct",
    "latency_ms",
    "input_tokens",
    "output_tokens",
    "error",
)


def compare(
    client: FoundryRestClient, fine_tuned_deployment: str, examples: list[Example], output: Path
) -> None:
    rows = []
    for number, example in enumerate(examples, 1):
        for name, deployment in (
            ("baseline", client.settings.deployment),
            ("fine-tuned", fine_tuned_deployment),
        ):
            started = perf_counter()
            try:
                response = client.post(
                    "responses",
                    {
                        "model": deployment,
                        "instructions": INSTRUCTIONS,
                        "input": example.message,
                        "store": False,
                    },
                )
                text = output_text(response).strip().upper()
                usage = response.get("usage") or {}
                error_text = ""
            except Exception as error:
                text, usage, error_text = "", {}, str(error)
            elapsed_ms = round((perf_counter() - started) * 1000)
            correct = not error_text and text == example.label
            status = f"ERROR {error_text}" if error_text else "PASS" if correct else "FAIL"
            print(f"{number:2} {name:<12} expected={example.label:<12} actual={text:<12} {status}")
            rows.append(
                (
                    number,
                    name,
                    example.label,
                    text,
                    str(correct).lower(),
                    elapsed_ms,
                    usage.get("input_tokens"),
                    usage.get("output_tokens"),
                    error_text,
                )
            )
    write_csv(output, HEADER, rows)
    print(f"Comparison saved: {output.resolve()}")


def main() -> int:
    parser = argparse.ArgumentParser(description="Prepare data and explore Azure fine-tuning.")
    commands = parser.add_subparsers(dest="command")
    prepare_parser = commands.add_parser("prepare")
    prepare_parser.add_argument("folder", type=Path, nargs="?", default=Path("fine-tuning-data"))
    validate_parser = commands.add_parser("validate")
    validate_parser.add_argument("paths", type=Path, nargs="+")
    upload_parser = commands.add_parser("upload")
    upload_parser.add_argument("path", type=Path)
    create_parser = commands.add_parser("create-job")
    create_parser.add_argument("train_file")
    create_parser.add_argument("validation_file")
    create_parser.add_argument("model")
    create_parser.add_argument("--confirm-costs", action="store_true")
    status_parser = commands.add_parser("status")
    status_parser.add_argument("job")
    compare_parser = commands.add_parser("compare")
    compare_parser.add_argument("deployment")
    compare_parser.add_argument("test_file", type=Path)
    compare_parser.add_argument(
        "output", type=Path, nargs="?", default=Path("fine-tuning-comparison.csv")
    )
    args = parser.parse_args()

    try:
        if args.command is None:
            parser.print_help()
            return 0
        if args.command == "prepare":
            prepare(args.folder)
            print(
                f"Generated 40 training, 12 validation, and 12 held-out test examples in {args.folder.resolve()}."
            )
            return 0
        if args.command == "validate":
            for path in args.paths:
                print(f"{path}: {len(validate(path))} valid examples.")
            return 0

        examples = []
        if args.command == "upload":
            print(f"Validated {len(validate(args.path))} examples.")
        elif args.command == "compare":
            examples = validate(args.test_file)
        elif args.command == "create-job":
            if not args.confirm_costs:
                raise ValueError("Review model eligibility and charges, then add --confirm-costs.")
            if not all(
                value.casefold().startswith("file-")
                for value in (args.train_file, args.validation_file)
            ):
                raise ValueError("Provide two uploaded file IDs (file-...).")

        with rest_client(FoundrySettings.from_environment()) as client:
            if args.command == "upload":
                file = client.upload_file(args.path, purpose="fine-tune")
                print(f"Uploaded training file ID: {require_id(file)}")
            elif args.command == "create-job":
                job = client.post(
                    "fine_tuning/jobs",
                    {
                        "model": args.model,
                        "training_file": args.train_file,
                        "validation_file": args.validation_file,
                    },
                )
                print(f"Submitted fine-tuning job: {require_id(job)}")
            elif args.command == "status":
                job = client.get(f"fine_tuning/jobs/{quote(args.job, safe='')}")
                print(f"Job: {job.get('id', '(unknown)')}")
                print(f"Status: {job.get('status', '(unknown)')}")
                print(f"Fine-tuned model: {job.get('fine_tuned_model') or 'not yet available'}")
            elif args.command == "compare":
                compare(client, args.deployment, examples, args.output)
    except Exception as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
