using System.Text.Json;
using System.Text.Json.Serialization;

var argsList = args.ToList();
if (argsList.Contains("--help") || argsList.Count == 0)
{
    Console.WriteLine("CalcIFC prototype");
    Console.WriteLine("Usage: calcifc.exe --ifc <path> --zone <code> [--output report.json]");
    return;
}

string? ifcPath = GetArgumentValue(argsList, "--ifc");
string? zoneCode = GetArgumentValue(argsList, "--zone");
string outputPath = GetArgumentValue(argsList, "--output") ?? "report.json";

if (string.IsNullOrWhiteSpace(ifcPath) || string.IsNullOrWhiteSpace(zoneCode))
{
    Console.Error.WriteLine("Missing required arguments: --ifc and --zone are mandatory.");
    Environment.Exit(1);
}

var zonesPath = Path.Combine(AppContext.BaseDirectory, "config", "zones.json");
if (!File.Exists(zonesPath))
{
    Console.Error.WriteLine($"Zones config not found: {zonesPath}");
    Environment.Exit(1);
}

var zones = LoadZones(zonesPath);
if (!zones.TryGetValue(zoneCode, out var zone))
{
    var available = string.Join(", ", zones.Keys.OrderBy(code => code));
    Console.Error.WriteLine($"Unknown zone: {zoneCode}. Available: {available}");
    Environment.Exit(1);
}

var summary = ParseIfcSummary(ifcPath);
var report = EstimateActions(summary, zone);

var payload = new
{
    ifc = new
    {
        source = report.Summary.Source,
        beams = report.Summary.Beams,
        columns = report.Summary.Columns,
        other_elements = report.Summary.OtherElements
    },
    zone = new
    {
        code = report.Zone.Code,
        label = report.Zone.Label,
        snow_kN_m2 = report.Zone.SnowKnPerM2,
        wind_kN_m2 = report.Zone.WindKnPerM2
    },
    totals = new
    {
        elements = report.TotalElements,
        estimated_snow_kN = report.EstimatedSnowKn,
        estimated_wind_kN = report.EstimatedWindKn
    },
    notes = new[]
    {
        "Rapport préliminaire. Les vérifications Eurocode complètes doivent être ajoutées.",
        "L'import IFC est simplifié (scan textuel)."
    }
};

var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
});

File.WriteAllText(outputPath, json);
Console.WriteLine($"Rapport écrit dans {outputPath}");

static string? GetArgumentValue(IReadOnlyList<string> argsList, string key)
{
    var index = argsList.IndexOf(key);
    if (index == -1 || index + 1 >= argsList.Count)
    {
        return null;
    }

    return argsList[index + 1];
}

static Dictionary<string, ZoneConfig> LoadZones(string path)
{
    var json = File.ReadAllText(path);
    var zonePayloads = JsonSerializer.Deserialize<Dictionary<string, ZonePayload>>(json)
        ?? throw new InvalidOperationException("Invalid zones.json");

    return zonePayloads.ToDictionary(
        entry => entry.Key,
        entry => new ZoneConfig(
            entry.Key,
            entry.Value.Label,
            entry.Value.SnowKnPerM2,
            entry.Value.WindKnPerM2
        ));
}

static IfcSummary ParseIfcSummary(string path)
{
    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"IFC file not found: {path}");
    }

    var beams = 0;
    var columns = 0;
    var other = 0;

    foreach (var line in File.ReadLines(path))
    {
        var upper = line.ToUpperInvariant();
        if (upper.Contains("IFCBEAM", StringComparison.Ordinal))
        {
            beams += 1;
            continue;
        }

        if (upper.Contains("IFCCOLUMN", StringComparison.Ordinal))
        {
            columns += 1;
            continue;
        }

        if (upper.Contains("IFC", StringComparison.Ordinal))
        {
            other += 1;
        }
    }

    return new IfcSummary(path, beams, columns, other);
}

static PreliminaryReport EstimateActions(IfcSummary summary, ZoneConfig zone)
{
    var totalElements = summary.Beams + summary.Columns;
    var influenceArea = Math.Max(totalElements, 1) * 10.0;
    var estimatedSnow = influenceArea * zone.SnowKnPerM2;
    var estimatedWind = influenceArea * zone.WindKnPerM2;

    return new PreliminaryReport(summary, zone, totalElements, estimatedSnow, estimatedWind);
}

internal record ZonePayload(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("snow_kN_m2")] double SnowKnPerM2,
    [property: JsonPropertyName("wind_kN_m2")] double WindKnPerM2
);

internal record ZoneConfig(string Code, string Label, double SnowKnPerM2, double WindKnPerM2);

internal record IfcSummary(string Source, int Beams, int Columns, int OtherElements);

internal record PreliminaryReport(
    IfcSummary Summary,
    ZoneConfig Zone,
    int TotalElements,
    double EstimatedSnowKn,
    double EstimatedWindKn
);
