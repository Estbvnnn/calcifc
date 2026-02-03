# CalcIFC

CalcIFC est un prototype de logiciel local (CLI) pour **importer un fichier IFC**, extraire des éléments structurels de base (poutres/colonnes) et produire un **rapport de pré‑vérification** configurable par **zone** (exposition neige/vent). L'objectif est de servir de **base extensible** vers un outil complet de calcul suivant l'Eurocode.

> ⚠️ Ce dépôt fournit une **architecture et un flux de travail minimal fonctionnel**. Il ne remplace pas un logiciel de calcul certifié. Il sert de socle de développement pour ajouter des modèles de charge, des vérifications EC3, des combinaisons d'actions, et une intégration BIM avancée.

## Fonctionnalités actuelles

- Import IFC simple (scan de texte) et comptage des éléments `IFCBEAM` et `IFCCOLUMN`.
- Sélection de zone (neige/vent) via un fichier de configuration.
- Génération d'un rapport JSON résumant les données importées et les charges *simplifiées*.

## Installation

```bash
python -m venv .venv
source .venv/bin/activate
pip install -e .
```

## Utilisation

```bash
calcifc --ifc chemin/vers/projet.ifc --zone FR-A --output rapport.json
```

## Structure

```
src/
  calcifc/
    main.py
    services/
      ifc_importer.py
      checks.py
    config/
      zones.json
```

## Feuille de route (exemples)

- Parse IFC robuste (IfcOpenShell).
- Détection des sections/nuances (S235/S355, HEA/IPE, etc.).
- Vérifications Eurocode 3 (ELU/ELS) et combinaisons d'actions.
- Persistences d'études, historique des versions, recalcul automatique.
- UI (desktop ou web) pour dessinateurs.

## Limites connues

- Import IFC basé sur un scan textuel (non fiable pour les modèles complexes).
- Charges et vérifications **simplifiées**.

## Licence

MIT
