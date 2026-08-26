"""Access to the package's shipped data files (JSON Schemas, mapping specs).

Uses importlib.resources rather than ``Path(__file__).parent`` so the package
keeps working when installed as a zipped wheel or bundled — a plain filesystem
path is only valid for an unpacked install.
"""

import json
from importlib import resources

PACKAGE = __package__ or "euromod_linking"


def _dir(name: str):
    return resources.files(PACKAGE) / name


def load_schema(name: str) -> dict:
    """A JSON Schema shipped under ``schemas/``."""
    return json.loads((_dir("schemas") / name).read_text(encoding="utf-8"))


def mapping_names() -> list[str]:
    """Names (stems) of the shipped mapping specs, sorted."""
    return sorted(p.name[:-5] for p in _dir("mappings").iterdir()
                  if p.name.endswith(".yaml"))


def load_mapping_text(name: str) -> str | None:
    """Raw YAML of a shipped mapping spec, or None when absent."""
    path = _dir("mappings") / f"{name}.yaml"
    if not path.is_file():
        return None
    return path.read_text(encoding="utf-8")
