"""Very small IFC text scanner for beams/columns counts."""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


@dataclass(frozen=True)
class IfcSummary:
    source: str
    beams: int
    columns: int
    other_elements: int


def _iter_ifc_lines(path: Path) -> Iterable[str]:
    with path.open("r", encoding="utf-8", errors="ignore") as handle:
        for line in handle:
            yield line.upper()


def parse_ifc_summary(path: Path) -> IfcSummary:
    if not path.exists():
        raise FileNotFoundError(f"IFC file not found: {path}")

    beams = 0
    columns = 0
    other = 0

    for line in _iter_ifc_lines(path):
        if "IFCBEAM" in line:
            beams += 1
            continue
        if "IFCCOLUMN" in line:
            columns += 1
            continue
        if "IFC" in line:
            other += 1

    return IfcSummary(source=str(path), beams=beams, columns=columns, other_elements=other)
