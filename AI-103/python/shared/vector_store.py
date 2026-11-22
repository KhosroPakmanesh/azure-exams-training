"""Validate a document selection, create a store, and wait for indexing."""

from pathlib import Path
from time import monotonic, sleep
from urllib.parse import quote

from shared.foundry import FoundryRestClient, require_id


def resolve_store(
    client: FoundryRestClient, arguments: list[str], name: str, *, pdf_only: bool = False
) -> str:
    if len(arguments) == 1 and arguments[0].startswith("vs_"):
        return arguments[0]

    extensions = {".pdf"} if pdf_only else {".pdf", ".txt", ".md", ".docx"}
    documents: dict[Path, None] = {}
    for argument in arguments:
        path = Path(argument)
        if path.is_dir():
            for child in sorted(path.iterdir()):
                if child.is_file() and child.suffix.lower() in extensions:
                    documents[child.resolve()] = None
        elif path.is_file() and path.suffix.lower() in extensions:
            documents[path.resolve()] = None
        else:
            raise ValueError(f"File/directory not found or unsupported type: {argument}")
    if not documents:
        raise ValueError("Supply supported files, a document directory, or an existing vs_... ID.")

    store_id = require_id(client.post("vector_stores", {"name": name}))
    print(f"Created vector store: {store_id}")
    print("Reuse this ID for later runs. Uploaded files and the store persist until deleted.")
    for path in documents:
        print(f"Uploading/indexing {path.name} ...", end=" ", flush=True)
        file_id = require_id(client.upload_file(path))
        store_files = f"vector_stores/{quote(store_id, safe='')}/files"
        client.post(store_files, {"file_id": file_id})
        wait_for_indexing(client, f"{store_files}/{quote(file_id, safe='')}")
        print("ready")
    return store_id


def wait_for_indexing(client: FoundryRestClient, path: str) -> None:
    deadline = monotonic() + 300
    while (remaining := deadline - monotonic()) > 0:
        status = client.get(path, timeout=remaining).get("status")
        if status == "completed":
            return
        if status not in ("in_progress", "pending"):
            raise RuntimeError(f"Indexing failed: {status}")
        sleep(min(2, max(0, deadline - monotonic())))
    raise TimeoutError("Indexing did not complete within five minutes.")
