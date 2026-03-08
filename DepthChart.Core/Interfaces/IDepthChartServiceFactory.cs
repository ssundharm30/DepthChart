using DepthChart.Core.Models;

namespace DepthChart.Core.Interfaces;

/// <summary>
/// Creates IDepthChartService instances associated to a specific team.
/// </summary>
public interface IDepthChartServiceFactory
{
    /// <summary>
    /// Returns an IDepthChartService associated to the given team.
    /// </summary>
    IDepthChartService Create(Team team);
}