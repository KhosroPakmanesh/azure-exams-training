import argparse
from pathlib import Path
from typing import Any

from shared.console import show_error
from shared.reports import load_json, require_unique_ids, write_csv

HEADER = (
    "id",
    "category",
    "scenario",
    "harm",
    "likelihood",
    "impact",
    "score",
    "priority",
    "owner",
    "mitigation",
    "evidence",
    "status",
)


def load_risks(path: Path) -> list[dict[str, Any]]:
    risks = load_json(path)
    require_unique_ids(risks)
    for risk in risks:
        required = ("scenario", "harm", "owner", "mitigation")
        scores = (risk.get("likelihood"), risk.get("impact"))
        if any(not str(risk.get(key) or "").strip() for key in required) or any(
            type(score) is not int or not 1 <= score <= 5 for score in scores
        ):
            raise ValueError(
                f"Invalid risk {risk['id']}: required fields and 1–5 scores are mandatory."
            )
    return risks


def rank_risks(risks: list[dict[str, Any]]) -> list[dict[str, Any]]:
    ranked = []
    for risk in risks:
        score = risk["likelihood"] * risk["impact"]
        priority = "HIGH" if score >= 15 else "MEDIUM" if score >= 8 else "LOW"
        ranked.append({**risk, "score": score, "priority": priority})
    return sorted(ranked, key=lambda risk: (-risk["score"], risk["id"]))


def main() -> int:
    parser = argparse.ArgumentParser(description="Rank and discuss a fictional AI risk register.")
    parser.add_argument("--input", type=Path, default=Path(__file__).parent / "samples/risks.json")
    parser.add_argument("--output", type=Path, default=Path("responsible-ai-risk-register.csv"))
    parser.add_argument("--preview", action="store_true")
    args = parser.parse_args()

    try:
        # Validate and rank the register before exporting it.
        ranked = rank_risks(load_risks(args.input))
        print("TRAVEL ASSISTANT RISK REGISTER (illustrative scoring)")
        for risk in ranked:
            print(
                f"{risk['id']:<8} {risk['category']:<22} "
                f"{risk['likelihood']} x {risk['impact']} = {risk['score']:<2} "
                f"{risk['priority']:<6} {risk['harm']}"
            )
        if args.preview:
            print("Preview only. Nothing written; no Azure connection required.")
        else:
            write_csv(
                args.output, HEADER, ([risk.get(field) for field in HEADER] for risk in ranked)
            )
            print(f"CSV exported to {args.output.resolve()}")
    except (ValueError, OSError, TypeError, KeyError) as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
