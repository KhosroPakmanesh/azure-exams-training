#!/usr/bin/env python3
"""Check the AI-103 solution and matching lab paths without credentials."""

import argparse
import ast
import json
from pathlib import Path, PurePosixPath
from xml.etree import ElementTree

ROOT = Path(__file__).resolve().parents[1]
CSHARP = ROOT / "C#"
PYTHON = ROOT / "python"
SOLUTION = CSHARP / "AI-103.CSharp.slnx"
SHARED_PROJECT = "shared/shared.csproj"


def verify(projects: list[str], solution: ElementTree.Element) -> None:
    errors = []

    def check(condition: bool, message: str) -> None:
        if not condition:
            errors.append(message)

    check(len(projects) == len(set(projects)), "The solution lists duplicate projects.")
    check(
        SHARED_PROJECT in projects, "The solution must include the C# shared library."
    )
    discovered = {
        path.relative_to(CSHARP).as_posix() for path in CSHARP.glob("*/*/*/*.csproj")
    }
    check(
        set(projects) - {SHARED_PROJECT} == discovered,
        "The solution and C# lab folders disagree.",
    )
    for folder in solution.findall("./Folder"):
        for project in folder.findall("Project"):
            parts = PurePosixPath(project.attrib["Path"]).parts
            check(
                len(parts) == 4
                and "/".join(parts[:2]) == folder.attrib["Name"].strip("/"),
                f"{project.attrib['Path']} must match its solution chapter and module.",
            )

    expected_python = {PurePosixPath(path).parts[:-1] for path in discovered}
    found_python = {
        entry.parent.relative_to(PYTHON).parts for entry in PYTHON.glob("*/*/*/main.py")
    }
    check(
        expected_python == found_python,
        "Each C# lab needs a matching Python entry point.",
    )
    launch_file = PYTHON / ".vscode/launch.json"
    configurations = json.loads(launch_file.read_text(encoding="utf-8"))[
        "configurations"
    ]
    expected_modules = {".".join((*parts, "main")) for parts in found_python}
    check(
        len(configurations) == len(expected_modules)
        and {config.get("module") for config in configurations} == expected_modules,
        "VS Code must have exactly one launch entry per Python lab.",
    )
    names = [config.get("name") for config in configurations]
    check(
        all(names) and len(set(names)) == len(names),
        "VS Code launch names must be unique.",
    )
    for config in configurations:
        check(
            config.get("type") == "debugpy"
            and config.get("request") == "launch"
            and config.get("cwd") == "${workspaceFolder}"
            and config.get("console") == "integratedTerminal",
            f"{config.get('name')} should launch in the Python workspace terminal.",
        )

    package_versions = ElementTree.parse(CSHARP / "Directory.Packages.props")
    pinned = {
        entry.attrib["Include"]
        for entry in package_versions.findall(".//PackageVersion")
    }

    for rel in projects:
        project_path = CSHARP / rel
        if not project_path.is_file() or not project_path.resolve().is_relative_to(
            CSHARP.resolve()
        ):
            errors.append(f"Missing or misplaced C# project: {rel}")
            continue
        project = ElementTree.parse(project_path)
        namespace = project.findtext("./PropertyGroup/RootNamespace") or ""
        assembly = project.findtext("./PropertyGroup/AssemblyName")
        if rel == SHARED_PROJECT:
            check(
                namespace == "AzureExamsTraining.AI103.Shared" and assembly == "shared",
                "The shared project must use the exam namespace and the shared assembly name.",
            )
        else:
            check(
                namespace.startswith("AzureExamsTraining.AI103.")
                and namespace == assembly,
                f"{rel} must use matching exam-scoped namespace and assembly names.",
            )
        check(
            project.find("./PropertyGroup/TargetFramework") is None,
            f"{rel} should inherit its target framework.",
        )
        for package in project.findall(".//PackageReference"):
            check(
                package.attrib.get("Include") in pinned
                and package.get("Version") is None,
                f"{rel} should use central package versions.",
            )
        if rel != SHARED_PROJECT:
            parts = project_path.parent.relative_to(CSHARP).parts
            lab_python = PYTHON.joinpath(*parts)
            refs = project.findall(".//ProjectReference")
            check(
                len(refs) == 1
                and (
                    project_path.parent / refs[0].attrib["Include"].replace("\\", "/")
                ).resolve()
                == (CSHARP / SHARED_PROJECT).resolve(),
                f"{rel} should reference shared.csproj.",
            )
            check(
                project_path.stem == project_path.parent.name,
                f"{rel} must match its lab folder name.",
            )
            for lab in (project_path.parent, lab_python):
                check(
                    (lab / "README.md").is_file(),
                    f"Missing lab README: {lab.relative_to(ROOT)}",
                )
            for sample in (project_path.parent / "samples").glob("*"):
                if sample.is_file():
                    counterpart = lab_python / "samples" / sample.name
                    check(
                        counterpart.is_file()
                        and counterpart.read_bytes() == sample.read_bytes(),
                        f"Sample differs between languages: {sample.relative_to(ROOT)}",
                    )

    sdk = json.loads((CSHARP / "global.json").read_text(encoding="utf-8"))["sdk"]
    props = ElementTree.parse(CSHARP / "Directory.Build.props")
    check(sdk["version"].startswith("11."), "Expected a .NET 11 SDK.")
    check(
        props.findtext("./PropertyGroup/TargetFramework") == "net11.0",
        "Expected net11.0.",
    )
    for shared in (CSHARP / "shared", PYTHON / "shared"):
        example = shared / ".env.example"
        check(
            example.is_file(),
            f"Missing credentials template: {example.relative_to(ROOT)}",
        )
        if example.is_file():
            settings = example.read_text(encoding="utf-8-sig")
            for key in (
                "AZURE_OPENAI_ENDPOINT",
                "MODEL_DEPLOYMENT",
                "AZURE_OPENAI_API_KEY",
            ):
                check(
                    f"{key}=" in settings,
                    f"{example.relative_to(ROOT)} is missing {key}.",
                )

    for folder in [
        PYTHON / "shared",
        *(PYTHON.joinpath(*parts) for parts in found_python),
    ]:
        for source in folder.glob("*.py"):
            try:
                ast.parse(source.read_text(encoding="utf-8-sig"), filename=str(source))
            except SyntaxError as error:
                errors.append(str(error))

    if errors:
        raise SystemExit("\n".join(errors))
    print(
        f"Verified {len(discovered)} C# labs, {len(found_python)} Python labs and their VS Code launch entries."
    )


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--list-projects",
        action="store_true",
        help="List project paths relative to the solution.",
    )
    args = parser.parse_args()
    solution = ElementTree.parse(SOLUTION).getroot()
    projects = [project.attrib["Path"] for project in solution.findall(".//Project")]
    if args.list_projects:
        print("\n".join(projects))
    else:
        verify(projects, solution)


if __name__ == "__main__":
    main()
