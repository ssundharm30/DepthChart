using DepthChart.Core.Models;

namespace DepthChart.Core.Interfaces;

/// <summary>
/// Contains the business logic for managing depth chart.
/// Implementations are performed based on specific team (and sport) at construction time
/// through DepthChartServiceFactory
/// </summary>
public interface IDepthChartService
{
    /// <summary>
    /// Adds a player to the depth chart at a given position.
    /// If positionDepth is not provided, the player is appended to the end.
    /// Players at or below the insertion point are shifted down.
    /// If the player already exists at that position, they are moved to the new depth.
    /// </summary>
    void AddPlayerToDepthChart(string position, Player player, int? positionDepth = null);

    /// <summary>
    /// Removes a player from the depth chart at a given position.
    /// Returns the removed player, or empty list if the player was not found (To meet requirements document)
    /// </summary>
    object RemovePlayerFromDepthChart(string position, Player player);

    /// <summary>
    /// Returns all players listed below the given player at the given position.
    /// Returns an empty list if the player is not found or has no backups.
    /// </summary>
    List<Player> GetBackups(string position, Player player);

    /// <summary>
    /// Prints the full depth chart for all positions that have at least one player.
    /// </summary>
    void GetFullDepthChart();
}