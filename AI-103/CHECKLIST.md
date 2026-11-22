# AI-103 verification checklist

- [ ] From `AI-103`, `python scripts/verify-source.py` checks lab paths, sample data, project references, and VS Code launch entries.
- [ ] From `AI-103/C#`, `dotnet sln AI-103.CSharp.slnx list` lists all 19 labs and `shared`.
- [ ] From `AI-103/C#`, `dotnet restore AI-103.CSharp.slnx` and `dotnet build AI-103.CSharp.slnx -c Release --no-restore` succeed.
- [ ] The [Python environment](python/README.md) installs and runs the labs from `AI-103/python/`, including the VS Code launch entries.

## Offline exercises

- [ ] Preview labs 12 and 15 and check the dataset counts and planned variants.
- [ ] Prepare and validate lab 14's three data splits. Confirm 40 training, 12 validation, and 12 held-out examples.
- [ ] Preview labs 16–19. Generate the offline reports in labs 16, 17, and 19 and compare their risk scores, review labels, and release decisions across the two languages.
- [ ] Publish the C# labs and check that the archive includes both language tracks, the study notes, and all 20 published projects. Confirm that filled `.env` files are excluded.

## Live Azure exercises

Configure a deployment using the [C# setup](C%23/README.md) or [Python setup](python/README.md). Check the [lab index](README.md) for inputs and request counts.

- [ ] Ask standalone and follow-up questions in labs 01–03. Confirm streaming in lab 04 and cancel a request in lab 05.
- [ ] Create, list, and resume a local conversation in lab 06.
- [ ] Verify Python execution, web citations, indexed-file citations, the approved local function, and the combined tools in labs 07–11.
- [ ] Run small comparisons in labs 12–15 using a supported deployment. The lab 14 training job requires an explicit confirmation.
- [ ] Use manual labels in live lab 17 and review guardrail outcomes for the same cases in lab 18.

Keep the deployment and inputs consistent when comparing implementations. Record actual Azure results separately from the offline exercises.
