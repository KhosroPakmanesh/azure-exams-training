# Azure exam training repository guidance

## Organize by exam and course

- Use `<exam>/<language>/<chapter>/<module>/<lab>/` for labs. Use the language folders `C#` and `python` and the same numbered, lowercase, hyphenated course and lab names in both.
- Keep matching C# and Python labs aligned on their learning objective, fictional inputs, and observable behavior. For AI-103, the solution records the current lab assignments. Use the [official exam study guide](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-103) to link to exam objectives.
- Keep personal study notes and questions at `<exam>/study/<chapter>/<module>/`. Link both implementations where they help explain an answer. Preserve the author's questions and answers when moving or editing them.
- Keep reusable Foundry configuration, credential selection, and connection code in `<exam>/<language>/shared/`. Projects own their prompts, domain types, sample inputs, validation rules, and explanations.

## Naming

- Use **Azure Exams Training** for the repository and the exam code for exam-scoped files. For AI-103, use `AI-103.CSharp.slnx`, the Python distribution `ai-103-training`, and the combined archive `AI-103-Training-With-CSharp-Binaries.zip`.
- Use `AzureExamsTraining.<ExamIdentifier>.<Lab>` for C# namespaces and executable assembly names. Use a valid C# identifier for the exam code, such as `AI103` for `AI-103`. Sync `RootNamespace`, source namespace declarations, and imports. Use the exam's `Shared` namespace for its shared code and `shared` for the assembly name.
- Use service names such as Foundry in the names of course modules and integrations that teach or use the service. Preserve access to data saved under a previous default name and honor explicit storage overrides.

## Target IDE and tooling layout

- Open `<exam>/C#/` in Visual Studio through its `.slnx`. Keep the solution, C# projects, Visual Studio launch configuration, `global.json`, and MSBuild props under `C#/`. Put C#-only publish tooling in `C#/scripts/`.
- Open `<exam>/python/` as a folder in VS Code. Use one track-level `pyproject.toml` and one local `.venv/`. Keep one named debug entry per runnable lab in `python/.vscode/launch.json`. Launch each lab by module from the Python workspace root so F5 uses the same entry point and import paths as `python -m`.
- Keep the matching chapter, module, and lab paths in both tracks. Keep exam-level documentation, study notes, and cross-language verification and packaging at the exam level. Keep IDE caches, personal launch settings, and virtual environments out of version control.
- When moving IDE or build files, update solution paths, documentation, verification, publishing, packaging, and CI commands to use the new locations.

## Show the lesson in the code

- Design each lab around an observation. State what the learner can change and what to inspect. In a comparison, use the same inputs and settings, vary the experiment variable, and give each model call its own conversation.
- Show preparation, model calls, local checks, and output in execution order. Validate required inputs before paid work. Make prompt instructions, model settings, tools, and the SDK request easy to find.
- Give each meaningful stage a focused responsibility. Let requests return responses, parsers produce typed values, validators report failures, and normalizers transform accepted data. Keep short operations inline and avoid an abstraction that hides the experiment.
- Use a lab-local `OutputPrinter` in C# for lesson output. Use text labels that work with redirected output. State the model-call plan and explain how to interpret results. Report errors and unavailable measurements accurately.
- Preserve a lab's settings, prompts, CLI modes, request counts, and report formats during reorganizations. Keep remote work explicit and use the existing offline modes where applicable.

## C# conventions

- Use explicit types at meaningful boundaries and include the full type name in owned identifiers. Examples are `string questionString`, `CancellationToken requestCancellationToken`, and `List<TestCase> testCases`. Keep external SDK identifiers and serialized field names stable.
- Use braces, block-bodied methods, simple LINQ for clear projections and filtering, and direct branches or loops for multi-step rules. Put multiple arguments, parameters, and initializer members on separate lines when formatting. Use `List<T>` by default for new learning-code collections and use an existing collection when suitable.
- Give public records their own files. Keep per-call state local. Prefer PascalCase for internal folders such as `Types` and `SampleData`. Arrange methods from the main operation to its supporting steps.
- Use the root `.editorconfig`. Use short comments to explain non-obvious decisions. Label major phases and number their steps in `Program.cs`, restarting for each phase.

## Python conventions

- Use type hints at meaningful boundaries, descriptive snake_case names, and small functions that follow the same experiment stages as C#. Prefer native SDK calls in SDK-focused lessons.
- Use `main.py` as the lab entry point and resolve bundled inputs relative to the lab's file. Include a `main()` guard and keep imports free of requests and credential resolution.
- Validate local inputs before opening a Foundry connection. Use context managers for files and clients. Keep remote errors distinct from failed model assertions.

## Document and verify

- For each lab, explain the concept, show a short Mermaid execution flow, document setup and run commands, explain the output, and include a small experiment the learner can try. Link relevant source files and official course references.
- Use the exam's local source verifier, build the C# solution, and smoke-check the changed Python entry points. Check that sample files resolve, documented paths exist, and filled `.env` files stay private.
- For requested tests, assert observable behavior with focused cases and clear Arrange, Act, and Assert phases. Keep deterministic checks independent of credentials. Report live Azure verification separately from local checks.
