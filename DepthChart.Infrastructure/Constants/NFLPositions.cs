namespace DepthChart.Infrastructure.Constants;

public static class NFLPositions
{
    public static readonly IReadOnlyList<string> AllNFLPositions = new[]
    {
        // Offense
        "QB", "RB", "LWR", "RWR", "SWR",
        "TE", "LT", "LG", "C", "RG", "RT",
        // Defense
        "LDE", "RDE", "NT", "LOLB", "LILB",
        "ROLB", "RILB", "LCB", "RCB", "SS", "FS", "NB",
        // Special Teams
        "PT", "PK", "LS", "H", "KO", "KR", "PR"
    };

    public static bool IsValid(string position) =>
        AllNFLPositions.Contains(position, StringComparer.OrdinalIgnoreCase);
}