using DepthChart.Core.Models;

namespace DepthChart.Core.Interfaces;

/// <summary>
/// In memory data store for the depth chart.
/// All operations are stored per team (team contains the sport),
/// allowing a single repository instance to serve all teams across all sports.
/// </summary>
public interface IDepthChartRepository
{
    /// <summary>
    /// Returns a new list of the ordered depth chart entries for a given team and position.
    /// Returns an empty list if no entries exist.
    /// </summary>
    List<Player> GetPositionDepth(Team team, string position);

    /// <summary>
    /// Saves/Updates depth chart entries for a given team, position and depth entries.
    /// </summary>
    void SavePositionDepth(Team team, string position, List<Player> players);

    /// <summary>
    /// Returns all positions with at least one player for a given team.
    /// </summary>
    IEnumerable<KeyValuePair<string, List<Player>>> GetAllPopulatedPositions(Team team);
}
