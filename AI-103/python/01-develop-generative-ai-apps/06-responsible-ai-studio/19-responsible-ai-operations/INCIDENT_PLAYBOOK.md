# Responsible AI incident playbook (exercise)

This document is a planning template, not a deployed control. Use it with Project 19's synthetic incident events.

## Before release

- Designate product, safety, operations, security, privacy, accessibility, and legal reviewers.
- Review known harms and residual risks with accountable owners.
- Use a phased rollout, beginning with a limited audience.
- Document approved model deployment, prompt/configuration version, and guardrail policy.
- Verify that a manual pause/rollback process can be executed by authorized operators.
- Publish transparent user instructions, limitations, escalation routes, and a feedback mechanism.

## On a reported incident

1. **Triage:** Record an incident reference, category, severity, and privacy-safe evidence. Do not paste raw personal information into logs.
2. **Verify:** Have an accountable reviewer decide whether harm or abuse is substantiated. Preserve uncertainty and disputed reports.
3. **Contain:** If necessary, suspend exposure or block the offending response pattern through independently deployed controls; obtain appropriate approvals.
4. **Rollback:** Follow the pretested deployment/configuration rollback procedure. Confirm the previously approved configuration works.
5. **Communicate:** Notify the appropriate on-call and stakeholder channels; consider user notification obligations and accessible alternatives.
6. **Recover:** Re-test known failing cases, verify mitigations, and require explicit approval before expanding release.
7. **Review:** Update risk registers, evaluation datasets, guardrails, and incident lessons learned.

## Data handling

Keep the minimum necessary metadata. Do not retain API keys, raw conversation data, account identifiers, or IP addresses in training artifacts. Pseudonymous identifiers still require appropriate privacy protections. Apply your organization's retention and access policies.

## Checklist for a real deployment

- [ ] Authorized, tested emergency pause and rollback
- [ ] Incident owner and severity matrix
- [ ] Human safety review and false-positive appeals
- [ ] Blocklist implementation governed by policy
- [ ] Feedback and complaint channel
- [ ] Protected telemetry with defined retention
- [ ] Security and privacy approval
- [ ] Documented recovery and re-release gates
