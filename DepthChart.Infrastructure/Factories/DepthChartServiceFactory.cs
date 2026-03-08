using DepthChart.Application.Services;
using DepthChart.Core.Interfaces;
using DepthChart.Core.Models;
using DepthChart.Infrastructure.Constants;
using DepthChart.Infrastructure.Validators;
using Microsoft.Extensions.Logging;

namespace DepthChart.Infrastructure.Factories;

public class DepthChartServiceFactory : IDepthChartServiceFactory
{
    private readonly IDepthChartRepository _repository;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DepthChartServiceFactory> _logger;
    
    private readonly IReadOnlyDictionary<Sport, IPositionValidator> _validators =
        new Dictionary<Sport, IPositionValidator>
        {
            { Sport.NFL, new NFLPositionValidator() }
        };
    
    public DepthChartServiceFactory(IDepthChartRepository repository, ILoggerFactory loggerFactory)
    {
        _repository = repository;
        _loggerFactory = loggerFactory;
        _logger        = loggerFactory.CreateLogger<DepthChartServiceFactory>();
    }
    
    public IDepthChartService Create(Team team)
    {
        ArgumentNullException.ThrowIfNull(team);
        
        
        if (!NFLTeams.IsValid(team.Id))
        {
            _logger.LogError("Invalid team id {TeamId}", team.Id);
            throw new ArgumentException(
                $"Invalid team id {team.Id}", nameof(team));
        }
        
        if (!_validators.TryGetValue(team.Sport, out var validator))
        {
            _logger.LogError("No position validator registered for {Sport}:{Team}", team.Sport, team.Name);
            throw new ArgumentException(
                $"No position validator registered for '{team.Sport}':", nameof(team));
        }

        var serviceLogger = _loggerFactory.CreateLogger<DepthChartService>();
        return new DepthChartService(_repository, validator, serviceLogger, team);
    }
}