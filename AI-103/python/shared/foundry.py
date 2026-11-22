"""Configure Azure OpenAI SDK and REST clients for the AI-103 labs."""

from contextlib import asynccontextmanager, contextmanager, nullcontext
from dataclasses import dataclass, field
from os import environ
from pathlib import Path
from typing import TYPE_CHECKING, Any, AsyncIterator, Iterator
from urllib.parse import urlsplit

if TYPE_CHECKING:
    from openai import AsyncOpenAI, OpenAI

DEFAULT_INSTRUCTIONS = (
    "You are a helpful AI assistant that answers questions and provides information."
)
SDK_SCOPE = "https://ai.azure.com/.default"
REST_SCOPE = "https://cognitiveservices.azure.com/.default"


@dataclass(frozen=True)
class FoundrySettings:
    endpoint: str
    deployment: str
    api_key: str | None = field(repr=False)
    instructions: str = DEFAULT_INSTRUCTIONS

    @classmethod
    def from_environment(cls) -> "FoundrySettings":
        from dotenv import dotenv_values

        settings_path = Path(__file__).with_name(".env")
        file_settings = dotenv_values(settings_path, interpolate=False, encoding="utf-8-sig")

        def setting(name: str) -> str | None:
            value = environ.get(name)
            if value and value.strip():
                return value
            value = file_settings.get(name)
            return value if value and value.strip() else None

        endpoint = setting("AZURE_OPENAI_ENDPOINT")
        deployment = setting("MODEL_DEPLOYMENT") or setting("AZURE_OPENAI_DEPLOYMENT")
        if not endpoint or not deployment:
            raise ValueError(
                "Set AZURE_OPENAI_ENDPOINT and MODEL_DEPLOYMENT in python/shared/.env "
                "or the process environment."
            )

        url = urlsplit(endpoint.strip())
        if url.scheme != "https" or not url.hostname or url.username or url.password:
            raise ValueError("AZURE_OPENAI_ENDPOINT must be an absolute HTTPS resource URL.")
        if url.path.rstrip("/").lower() not in ("", "/openai/v1") or url.query or url.fragment:
            raise ValueError("Use the Azure OpenAI resource URL with an optional /openai/v1/ path.")
        return cls(
            endpoint=f"https://{url.netloc}/openai/v1/",
            deployment=deployment.strip(),
            api_key=setting("AZURE_OPENAI_API_KEY"),
            instructions=setting("SYSTEM_INSTRUCTIONS") or DEFAULT_INSTRUCTIONS,
        )


@contextmanager
def sdk_client(settings: FoundrySettings) -> Iterator["OpenAI"]:
    """Create an OpenAI SDK client on the Azure v1 endpoint."""
    from openai import OpenAI

    if settings.api_key:
        with OpenAI(base_url=settings.endpoint, api_key=settings.api_key) as client:
            yield client
        return

    from azure.identity import DefaultAzureCredential, get_bearer_token_provider

    with DefaultAzureCredential() as credential:
        token_provider = get_bearer_token_provider(credential, SDK_SCOPE)
        with OpenAI(base_url=settings.endpoint, api_key=token_provider) as client:
            yield client


@asynccontextmanager
async def async_sdk_client(settings: FoundrySettings) -> AsyncIterator["AsyncOpenAI"]:
    """Create an async client so a pending request can be cancelled."""
    from openai import AsyncOpenAI

    if settings.api_key:
        async with AsyncOpenAI(base_url=settings.endpoint, api_key=settings.api_key) as client:
            yield client
        return

    from azure.identity.aio import DefaultAzureCredential, get_bearer_token_provider

    async with DefaultAzureCredential() as credential:
        token_provider = get_bearer_token_provider(credential, SDK_SCOPE)
        async with AsyncOpenAI(base_url=settings.endpoint, api_key=token_provider) as client:
            yield client


class FoundryError(RuntimeError):
    """A non-successful Azure OpenAI HTTP response."""

    def __init__(self, status: int, reason: str, body: str):
        self.status = status
        self.body = body
        super().__init__(f"Foundry returned HTTP {status} ({reason}): {body[:1800]}")


class FoundryRestClient:
    """Send the JSON and multipart requests shown in the tool and evaluation labs."""

    def __init__(self, http: Any, settings: FoundrySettings, credential: Any):
        self.http = http
        self.settings = settings
        self.credential = credential

    def get(self, path: str, **options: Any) -> dict[str, Any]:
        return self._send("GET", path, **options)

    def post(self, path: str, payload: dict[str, Any]) -> dict[str, Any]:
        return self._send("POST", path, json=payload)

    def upload_file(self, path: Path, purpose: str = "assistants") -> dict[str, Any]:
        with path.open("rb") as source:
            return self._send(
                "POST",
                "files",
                data={"purpose": purpose},
                files={"file": (path.name, source, "application/octet-stream")},
            )

    def respond(
        self,
        user_input: str | list[dict[str, Any]],
        tools: list[dict[str, Any]],
        previous_id: str | None = None,
        *,
        instructions: str | None = None,
        include: list[str] | None = None,
    ) -> dict[str, Any]:
        payload = {
            "model": self.settings.deployment,
            "instructions": instructions
            if instructions is not None
            else self.settings.instructions,
            "input": user_input,
            "tools": tools,
            "store": True,
        }
        if previous_id:
            payload["previous_response_id"] = previous_id
        if include:
            payload["include"] = include
        return self.post("responses", payload)

    def _send(self, method: str, path: str, **options: Any) -> dict[str, Any]:
        if self.settings.api_key:
            headers = {"api-key": self.settings.api_key}
        else:
            token = self.credential.get_token(REST_SCOPE)
            headers = {"Authorization": f"Bearer {token.token}"}
        response = self.http.request(method, path, headers=headers, **options)
        if not response.is_success:
            raise FoundryError(response.status_code, response.reason_phrase, response.text)
        return response.json()


@contextmanager
def rest_client(settings: FoundrySettings) -> Iterator[FoundryRestClient]:
    import httpx

    if settings.api_key:
        credential_context = nullcontext()
    else:
        from azure.identity import DefaultAzureCredential

        credential_context = DefaultAzureCredential()
    with (
        credential_context as credential,
        httpx.Client(base_url=settings.endpoint, timeout=300) as http,
    ):
        yield FoundryRestClient(http, settings, credential)


def require_id(response: dict[str, Any]) -> str:
    response_id = response.get("id")
    if not isinstance(response_id, str) or not response_id.strip():
        raise ValueError("The service response did not include an ID.")
    return response_id


def response_parts(response: dict[str, Any]) -> Iterator[dict[str, Any]]:
    for item in response.get("output", []):
        yield from item.get("content", [])


def output_text(response: dict[str, Any]) -> str:
    text = "".join(
        part.get("text") or ""
        for part in response_parts(response)
        if part.get("type") == "output_text"
    )
    return text or "(No assistant text was returned.)"


def source_citations(response: dict[str, Any]) -> list[str]:
    citations: dict[str, None] = {}
    for part in response_parts(response):
        for annotation in part.get("annotations", []):
            if annotation.get("type") == "url_citation" and annotation.get("url"):
                citations[annotation["url"]] = None
            if annotation.get("type") == "file_citation" and annotation.get("filename"):
                citations[f"File: {annotation['filename']}"] = None
    return list(citations)
