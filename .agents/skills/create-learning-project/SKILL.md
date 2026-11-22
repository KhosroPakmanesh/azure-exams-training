---
name: create-learning-project
description: Create or extend a C# or Python Azure exam learning lab from a course topic or user description in this repository.
---

# Create a learning project

Read [AGENTS.md](../../../AGENTS.md) and an existing lab from the same course module. Identify the exam objective, an input the learner can vary, and the output they should inspect.

1. Use the numbered course and lab paths shared by both language tracks and follow the [repository naming conventions](../../../AGENTS.md#naming). Match the paired lab's learning objective and inputs when one exists.
2. Use the language's shared Foundry setup and keep the lab flow visible in its own entry point. Register runnable labs in the language's IDE setup as described in [Target IDE and tooling layout](../../../AGENTS.md#target-ide-and-tooling-layout).
3. Write the lab README with the run instructions, observation, and exercise. Link the exam objective and any relevant chapter study notes.
4. Run the local checks applicable to the changed track. Report which commands ran and any checks that require a live deployment.
