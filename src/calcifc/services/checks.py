"""Simplified checks and load estimation placeholders."""

from __future__ import annotations

from dataclasses import dataclass

from calcifc.services.ifc_importer import IfcSummary


@dataclass(frozen=True)
class ZoneConfig:
    code: str
    label: str
    snow_kN_m2: float
    wind_kN_m2: float


@dataclass(frozen=True)
class PreliminaryReport:
    ifc: IfcSummary
    zone: ZoneConfig
    total_elements: int
    estimated_snow_kN: float
    estimated_wind_kN: float


def estimate_actions(summary: IfcSummary, zone: ZoneConfig) -> PreliminaryReport:
    total_elements = summary.beams + summary.columns
    influence_area = max(total_elements, 1) * 10.0
    estimated_snow_kN = influence_area * zone.snow_kN_m2
    estimated_wind_kN = influence_area * zone.wind_kN_m2

    return PreliminaryReport(
        ifc=summary,
        zone=zone,
        total_elements=total_elements,
        estimated_snow_kN=estimated_snow_kN,
        estimated_wind_kN=estimated_wind_kN,
    )
