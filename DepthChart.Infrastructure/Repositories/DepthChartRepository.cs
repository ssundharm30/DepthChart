using System.Collections.Concurrent;
using DepthChart.Core.Interfaces;
using DepthChart.Core.Models;
using Microsoft.Extensions.Logging;

namespace DepthChart.Infrastructure.Repositories;

public class DepthChartRepository : IDepthChartRepository
{
    private readonly Dictionary<Team, Dictionary<string, List<Player>>> _store = new();
    private readonly ILogger<DepthChartRepository> _logger;
    
    public DepthChartRepository(ILogger<DepthChartRepository> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public List<Player> GetPositionDepth(Team team, string position)
    {
        ArgumentNullException.ThrowIfNull(team);
        ArgumentException.ThrowIfNullOrWhiteSpace(position);

        if (_store.TryGetValue(team, out var positions) &&
            positions.TryGetValue(position, out var players))
        {
            return new List<Player>(players);
        }
        
        _logger.LogDebug(
            "GetPositionDepth {Sport}:{Team}:{Position} not found",
            team.Sport, team.Id, position);
        return [];
    }

    /// <inheritdoc />
    public void SavePositionDepth(Team team, string position, List<Player> players)
    {
        ArgumentNullException.ThrowIfNull(team);
        ArgumentException.ThrowIfNullOrWhiteSpace(position);
        ArgumentNullException.ThrowIfNull(players);

        if (!_store.TryGetValue(team, out var positions))
        {
            positions = new Dictionary<string, List<Player>>();
            _store[team] = positions;
        }
        
        positions[position] = new List<Player>(players);

        _logger.LogDebug(
            "SavePositionDepth {Sport}:{Team}:{Position} saved",
            team.Sport, team.Id, position);
    }

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<string, List<Player>>> GetAllPopulatedPositions(Team team)
    {
        ArgumentNullException.ThrowIfNull(team);

        if (!_store.TryGetValue(team, out var positions))
        {
            _logger.LogDebug(
                "GetAllPopulatedPositions {Sport}:{Team} - no positions found",
                team.Sport, team.Id);
            return [];
        }

        var results = positions
            .Where(kvp => kvp.Value.Count > 0)
            .Select(kvp => new KeyValuePair<string, List<Player>>(
                kvp.Key,
                new List<Player>(kvp.Value)))
            .ToList();
        
        _logger.LogDebug(
            "GetAllPopulatedPositions {Sport}:{Team} with {Count} positions",
            team.Sport, team.Id, results.Count);
        return results;
    }
}