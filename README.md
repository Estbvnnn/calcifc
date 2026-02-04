# CalcIFC (.NET)

CalcIFC est un prototype de logiciel local **générant un exécutable Windows (.exe)** pour **importer un fichier IFC**, extraire des éléments structurels de base (poutres/colonnes) et produire un **rapport de pré‑vérification** configurable par **zone** (exposition neige/vent). L'objectif est de servir de **base extensible** vers un outil complet de calcul suivant l'Eurocode.

> ⚠️ Ce dépôt fournit une **architecture et un flux de travail minimal fonctionnel**. Il ne remplace pas un logiciel de calcul certifié. Il sert de socle de développement pour ajouter des modèles de charge, des vérifications EC3, des combinaisons d'actions, et une intégration BIM avancée.

## Fonctionnalités actuelles

- Import IFC simple (scan de texte) et comptage des éléments `IFCBEAM` et `IFCCOLUMN`.
- Sélection de zone (neige/vent) via un fichier de configuration.
- Génération d'un rapport JSON résumant les données importées et les charges *simplifiées*.

## Pré‑requis

- .NET SDK 8.0

## Construire un .exe Windows

```bash
dotnet publish src/CalcIfcApp/CalcIfcApp.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Le binaire sera disponible sous :

```
src/CalcIfcApp/bin/Release/net8.0/win-x64/publish/CalcIfcApp.exe
```

## Utilisation

```bash
CalcIfcApp.exe --ifc chemin/vers/projet.ifc --zone FR-A --output rapport.json
```

## Structure

```
src/
  CalcIfcApp/
    Program.cs
    CalcIfcApp.csproj
    config/
      zones.json
```

## Feuille de route (exemples)

- Parse IFC robuste (IfcOpenShell ou parser natif).
- Détection des sections/nuances (S235/S355, HEA/IPE, etc.).
- Vérifications Eurocode 3 (ELU/ELS) et combinaisons d'actions.
- Persistences d'études, historique des versions, recalcul automatique.
- UI (desktop ou web) pour dessinateurs.

## Limites connues

- Import IFC basé sur un scan textuel (non fiable pour les modèles complexes).
- Charges et vérifications **simplifiées**.

## Licence

MIT
