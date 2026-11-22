import argparse
import json
from collections import Counter
from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from shared.console import show_error
from shared.reports import load_json, require_unique_ids

KINDS = {"feedback", "abuse", "verified_harm", "release_approval"}


@dataclass
class ReleaseState:
    release_stage: str = "canary"
    active_configuration: str = "policy-v2-candidate"
    previous_configuration: str = "policy-v1-approved"
    response_block_enabled: bool = False
    open_incidents: int = 0
    blocked_actors: set[str] = field(default_factory=set)


def apply_event(state: ReleaseState, event: dict[str, Any], abuse_counts: Counter[str]) -> str:
    kind, severity, verified = event["kind"], event["severity"], event["verified"]
    if kind == "feedback":
        if severity >= 3:
            state.open_incidents += 1
            return "Open review ticket; human triage required"
        return "Aggregate satisfaction signal; no automated intervention"
    if kind == "abuse":
        actor = event["actorToken"]
        abuse_counts[actor] += 1
        if abuse_counts[actor] >= 2 and verified:
            state.blocked_actors.add(actor)
            return "Simulate blocking verified repeat-abuse actor; human approval required in production"
        return "Watch for repeat events; no block yet"
    if kind == "verified_harm":
        state.open_incidents += 1
        if verified and severity >= 4:
            state.release_stage = "paused"
            state.response_block_enabled = True
            state.active_configuration = state.previous_configuration
            return "SIMULATED pause + response block + rollback to previous approved configuration"
        return "Escalate for human verification; no automatic rollback"
    if verified and state.release_stage != "paused":
        state.release_stage = "limited-release"
        return "Simulate approved limited rollout; monitor before general availability"
    return "Hold release; approvals cannot override a paused incident state"


def timestamp(event: dict[str, Any]) -> datetime:
    value = datetime.fromisoformat(event["timestamp"].replace("Z", "+00:00"))
    if value.tzinfo is None:
        raise ValueError(f"{event['id']} needs a timezone in its timestamp.")
    return value


def load_events(path: Path) -> list[dict[str, Any]]:
    events = load_json(path)
    require_unique_ids(events)
    for event in events:
        if (
            event.get("kind") not in KINDS
            or not event.get("summary", "").strip()
            or type(event.get("severity")) is not int
            or not 1 <= event["severity"] <= 5
            or not isinstance(event.get("verified"), bool)
        ):
            raise ValueError(f"Invalid event: {event['id']}")
        if event["kind"] == "abuse" and not event.get("actorToken", "").strip():
            raise ValueError(f"{event['id']} abuse event must have a pseudonymous actor token.")
    return sorted(events, key=timestamp)


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Simulate incident triage and a staged rollout locally."
    )
    parser.add_argument(
        "--events", type=Path, default=Path(__file__).parent / "samples/events.json"
    )
    parser.add_argument(
        "--output", type=Path, default=Path("responsible-ai-operations-report.json")
    )
    parser.add_argument("--preview", action="store_true")
    args = parser.parse_args()

    try:
        events = load_events(args.events)
        if args.preview:
            print(f"{len(events)} synthetic events, in chronological order:")
            for event in events:
                print(
                    f"{event['id']:<8} {event['kind']:<18} severity={event['severity']} verified={event['verified']}"
                )
            print("No Azure calls, policy updates, or output files.")
            return 0

        state = ReleaseState()
        abuse_counts: Counter[str] = Counter()
        decisions = []
        for event in events:
            action = apply_event(state, event, abuse_counts)
            decisions.append(
                {
                    "eventId": event["id"],
                    "timestamp": event["timestamp"],
                    "type": event["kind"],
                    "severity": event["severity"],
                    "action": action,
                }
            )
            print(f"{event['id']:<8} {event['kind']:<18} {action}")
        report = {
            "disclaimer": "SIMULATION ONLY: no Azure configuration, users, or deployments were changed.",
            "createdUtc": datetime.now(timezone.utc).isoformat(),
            "releaseStage": state.release_stage,
            "activeConfiguration": state.active_configuration,
            "previousConfiguration": state.previous_configuration,
            "responseBlockEnabled": state.response_block_enabled,
            "openIncidents": state.open_incidents,
            "simulatedBlockedActors": sorted(state.blocked_actors),
            "decisions": decisions,
        }
        args.output.write_text(json.dumps(report, indent=2), encoding="utf-8")
        print(
            f"Final simulated release stage: {state.release_stage}\nReport: {args.output.resolve()}"
        )
    except (ValueError, OSError, TypeError, KeyError) as error:
        show_error(error)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
