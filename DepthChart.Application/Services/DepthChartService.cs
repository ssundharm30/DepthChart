using DepthChart.Core.Interfaces;
using DepthChart.Core.Models;
using Microsoft.Extensions.Logging;

namespace DepthChart.Application.Services;

public class DepthChartService : IDepthChartService
{
    private readonly IDepthChartRepository _repository;
    private readonly IPositionValidator _positionValidator;
    private readonly ILogger<DepthChartService> _logger;
    private readonly Team _team;
    
    public DepthChartService(IDepthChartRepository repository, IPositionValidator positionValidator, ILogger<DepthChartService> logger, Team team)
    {
        _repository        = repository;
        _positionValidator = positionValidator;
        _logger            = logger;
        _team              = team;
    }
    
    /// <inheritdoc/>
    public void AddPlayerToDepthChart(string position, Player player, int? positionDepth = null)
    {
        ArgumentNullException.ThrowIfNull(player);
        
        try
        {
            _positionValidator.Validate(position);

            var playerList = _repository.GetPositionDepth(_team, position);
            
            var existingIndex = FindPlayerIndex(playerList, player);
            if (existingIndex >= 0)
            {
                playerList.RemoveAt(existingIndex);
            }

            if (positionDepth is null)
            {
                playerList.Add(player);
            }
            else
            {
                if (positionDepth < 0 || positionDepth > playerList.Count)
                {
                    throw new ArgumentException(
                        $"positionDepth {positionDepth} is out of range. Valid range is 0 to {playerList.Count}.",
                        nameof(positionDepth));
                }

                playerList.Insert(positionDepth.Value, player);
            }

            _repository.SavePositionDepth(_team, position, playerList);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error adding player {PlayerNumber} to {Team} at {Position}",
                player.Number, _team.Id, position);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public object RemovePlayerFromDepthChart(string position, Player player)
    {
        var removed = RemovePlayerFromDepthChartInternal(position, player);
        return removed is not null ? removed : new List<Player>();
    }
    
    /// <inheritdoc/>
    public List<Player> GetBackups(string position, Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        try
        {
            _positionValidator.Validate(position);

            var playerList = _repository.GetPositionDepth(_team, position);
            var index  = FindPlayerIndex(playerList, player);

            if (index < 0)
            {
                return [];
            }

            if (index == playerList.Count - 1)
            {
                return [];
            }

            var backups = playerList.GetRange(index + 1, playerList.Count - index - 1);
            return backups;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error getting backups for player {PlayerNumber} at {Position} on {Team}",
                player.Number, position, _team.Id);
            throw;
        }
    }
    
    public void GetFullDepthChart()
    {
        try
        {
            var populated = _repository.GetAllPopulatedPositions(_team).ToList();

            if (populated.Count == 0)
            {
                Console.WriteLine($"[{_team.Name}] Depth chart is empty.");
                return;
            }

            Console.WriteLine($"{_team.Name}");
            foreach (var (position, playerList) in populated)
            {
                var players = string.Join(", ", playerList.Select(p => $"(#{p.Number}, {p.Name})"));
                Console.WriteLine($"{position} – {players}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error printing depth chart for {Team}", _team.Id);
            throw;
        }
    }
    
    public Player? RemovePlayerFromDepthChartInternal(string position, Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        
        try
        {
            _positionValidator.Validate(position);

            var playerList = _repository.GetPositionDepth(_team, position);
            var index  = FindPlayerIndex(playerList, player);

            if (index < 0)
            {
                _logger.LogWarning(
                    "Player {PlayerNumber} not found at {Position} on {Team}",
                    player.Number, position, _team.Id);

                return null;
            }

            var removed = playerList[index];
            playerList.RemoveAt(index);
            _repository.SavePositionDepth(_team, position, playerList);

            _logger.LogInformation(
                "Player {PlayerNumber} ({PlayerName}) removed from {Position} on {Team}",
                removed.Number, removed.Name, position, _team.Id);

            return removed;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error removing player {PlayerNumber} from {Team} at {Position}",
                player.Number, _team.Id, position);
            throw;
        }
    }

    #region Private methods

    /// <summary>
    /// Finds a player in a playerList by their unique number. Returns -1 if not found.
    /// </summary>
    private static int FindPlayerIndex(List<Player> playerList, Player player) =>
        playerList.FindIndex(p => p.Number == player.Number);

    #endregion
}