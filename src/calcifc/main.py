"""CLI entrypoint for CalcIFC."""

from __future__ import annotations

import argparse
import json
from pathlib import Path

from calcifc.services.checks import ZoneConfig, estimate_actions
from calcifc.services.ifc_importer import parse_ifc_summary


def _load_zones(config_path: Path) -> dict[str, ZoneConfig]:
    data = json.loads(config_path.read_text(encoding="utf-8"))
    zones: dict[str, ZoneConfig] = {}
    for code, payload in data.items():
        zones[code] = ZoneConfig(
            code=code,
            label=payload["label"],
            snow_kN_m2=payload["snow_kN_m2"],
            wind_kN_m2=payload["wind_kN_m2"],
        )
    return zones


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="CalcIFC prototype")
    parser.add_argument("--ifc", required=True, help="Chemin vers le fichier IFC")
    parser.add_argument("--zone", required=True, help="Code de zone (ex: FR-A)")
    parser.add_argument(
        "--output",
        default="report.json",
        help="Chemin du rapport JSON de sortie",
    )
    return parser


def main() -> None:
    parser = build_parser()
    args = parser.parse_args()

    config_path = Path(__file__).parent / "config" / "zones.json"
    zones = _load_zones(config_path)

    zone = zones.get(args.zone)
    if zone is None:
        available = ", ".join(sorted(zones.keys()))
        raise SystemExit(f"Zone inconnue: {args.zone}. Disponibles: {available}")

    summary = parse_ifc_summary(Path(args.ifc))
    report = estimate_actions(summary, zone)

    output_path = Path(args.output)
    payload = {
        "ifc": {
            "source": report.ifc.source,
            "beams": report.ifc.beams,
            "columns": report.ifc.columns,
            "other_elements": report.ifc.other_elements,
        },
        "zone": {
            "code": report.zone.code,
            "label": report.zone.label,
            "snow_kN_m2": report.zone.snow_kN_m2,
            "wind_kN_m2": report.zone.wind_kN_m2,
        },
        "totals": {
            "elements": report.total_elements,
            "estimated_snow_kN": report.estimated_snow_kN,
            "estimated_wind_kN": report.estimated_wind_kN,
        },
        "notes": [
            "Rapport préliminaire. Les vérifications Eurocode complètes doivent être ajoutées.",
            "L'import IFC est simplifié (scan textuel).",
        ],
    }

    output_path.write_text(json.dumps(payload, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"Rapport écrit dans {output_path}")


if __name__ == "__main__":
    main()
