using DepthChart.Core.Models;

namespace DepthChart.Infrastructure.Constants;

public static class NFLTeams
{
    public static readonly Team TampaBayBuccaneers  = new("TBB",  "Tampa Bay Buccaneers", Sport.NFL);
    public static readonly IReadOnlyList<Team> AllNFLTeams = new[]
    {
        TampaBayBuccaneers
    };

    public static bool IsValid(string teamId) =>
        AllNFLTeams.Any(t => t.Id.Equals(teamId, StringComparison.OrdinalIgnoreCase));
}