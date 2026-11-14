#!/usr/bin/env python3
"""No-network structural verification of the .NET 11 SLNX solution; not a compiler."""
from pathlib import Path
import json
import xml.etree.ElementTree as ET
root = Path(__file__).resolve().parents[1]
failures = []
def check(ok, message):
    print(("OK " if ok else "FAIL ") + message)
    if not ok: failures.append(message)

sdk = json.loads((root / "global.json").read_text())["sdk"]
check(sdk["version"] == "11.0.100", "GA SDK version 11.0.100 pinned")
check(sdk["rollForward"] == "latestFeature", "Patch/feature updates remain possible")
props = ET.parse(root / "Directory.Build.props").getroot()
check(props.findtext("PropertyGroup/TargetFramework") == "net11.0", "Every project targets net11.0")
check(props.findtext("PropertyGroup/LangVersion") == "15.0", "C# language version 15")
solution = ET.parse(root / "FoundryCSharpLabs.slnx").getroot()
paths = [p.attrib["Path"] for p in solution.findall("Project")]
check(len(paths) == len(set(paths)) == 9, "One .slnx with nine unique projects")
check(not list(root.glob("*.sln")), "No legacy .sln alongside .slnx")
check(all((root / p).is_file() for p in paths), "All nine .csproj files exist")
packages = ET.parse(root / "Directory.Packages.props").getroot()
versions = {p.get("Include"): p.get("Version") for p in packages.findall("ItemGroup/PackageVersion")}
check(versions == {"OpenAI": "2.15.0", "Azure.Identity": "1.21.0"}, "Verified stable versions centralized")
for path in paths:
    project = ET.parse(root/path).getroot()
    check(project.find("PropertyGroup/TargetFramework") is None, f"{path}: inherits net11.0")
    for r in project.findall("ItemGroup/ProjectReference"):
        ref=(root / path).parent / r.get("Include").replace("\\", "/")
        check(ref.resolve().is_file(), f"{path}: reference {r.get('Include')} exists")
    for r in project.findall("ItemGroup/PackageReference"):
        check(r.get("Version") is None, f"{path}: package managed centrally")
        check(r.get("Include") in versions, f"{path}: package {r.get('Include')} is pinned")
for name in ("publish-all.sh", "publish-all.ps1", "build-and-package.sh", "build-and-package.ps1"):
    content=(root/"scripts"/name).read_text()
    check("FoundryCSharpLabs.slnx" in content, f"{name}: uses SLNX") if "publish" in name or "build" in name else None
    check(".sln'" not in content and "net8.0" not in content, f"{name}: no old target")
env_file = root / "src/Shared.FoundryClient/.env"
check(env_file.is_file(), "Shared project includes editable .env template")
check((root / "src/Shared.FoundryClient/.env.example").is_file(), "Safe .env.example is present")
check(all(f"{key}=" in env_file.read_text() for key in ("AZURE_OPENAI_ENDPOINT", "MODEL_DEPLOYMENT", "AZURE_OPENAI_API_KEY")), "Shared .env has all credential fields")
check((root / "src/Shared.FoundryClient/FoundryEnvFile.cs").is_file(), "Shared .env loader exists")
check("FoundryEnvFile.Load()" in (root / "src/Shared.FoundryClient/FoundryOptions.cs").read_text(), "Foundry options load shared .env")
if failures:
    raise SystemExit(f"{len(failures)} validation failure(s).")
print("PASS: .NET 11 project structure verified. C# compilation not performed.")
