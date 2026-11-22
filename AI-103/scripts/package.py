"""Package the AI-103 source, study notes, and published C# assemblies."""

import os
import tempfile
from pathlib import Path
from xml.etree import ElementTree
from zipfile import ZIP_DEFLATED, ZipFile

ROOT = Path(__file__).resolve().parents[1]
CSHARP = ROOT / "C#"
PUBLISH = ROOT / "artifacts/publish"
ARCHIVE = ROOT / "artifacts/AI-103-Training-With-CSharp-Binaries.zip"
ENTRIES = (
    "README.md",
    "CHECKLIST.md",
    ".gitignore",
    "C#",
    "python",
    "study",
    "scripts",
)
IGNORED = {
    ".vs",
    "obj",
    "bin",
    "artifacts",
    ".venv",
    "__pycache__",
    ".ruff_cache",
    ".pytest_cache",
}


def source_files(path: Path):
    if path.is_file():
        yield path
        return
    for directory, folders, files in os.walk(path):
        folders[:] = [
            name
            for name in folders
            if name not in IGNORED and not name.endswith(".egg-info")
        ]
        for name in files:
            normalized_name = name.casefold()
            if normalized_name == ".env" or (
                normalized_name.startswith(".env.")
                and normalized_name != ".env.example"
            ):
                continue
            if Path(name).suffix in (".user", ".suo", ".pyc", ".zip") or name.endswith(
                ".local.json"
            ):
                continue
            yield Path(directory) / name


def main() -> None:
    solution = ElementTree.parse(CSHARP / "AI-103.CSharp.slnx")
    project_paths = [
        CSHARP / project.attrib["Path"] for project in solution.findall(".//Project")
    ]
    publish_dirs = []
    for project_path in project_paths:
        assembly = ElementTree.parse(project_path).findtext(
            "./PropertyGroup/AssemblyName"
        )
        publish_dir = PUBLISH / project_path.stem
        if not (publish_dir / f"{assembly}.dll").is_file():
            raise SystemExit(f"Publish {project_path.stem} before packaging.")
        publish_dirs.append(publish_dir)

    with tempfile.NamedTemporaryFile(
        dir=ARCHIVE.parent, suffix=".zip", delete=False
    ) as output:
        temporary = Path(output.name)
    try:
        with ZipFile(temporary, "w", ZIP_DEFLATED) as archive:
            for entry in [*(ROOT / name for name in ENTRIES), *publish_dirs]:
                for path in source_files(entry):
                    archive.write(path, path.relative_to(ROOT))
        temporary.replace(ARCHIVE)
    finally:
        temporary.unlink(missing_ok=True)
    print(f"Created {ARCHIVE}")


if __name__ == "__main__":
    main()
