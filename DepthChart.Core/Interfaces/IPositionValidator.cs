namespace DepthChart.Core.Interfaces;

/// <summary>
/// Validates that a position string is associated with a given sport
/// to allow sport specific implementations
/// </summary>
public interface IPositionValidator
{
    /// <summary>
    /// Throws ArgumentException if the position is not valid.
    /// </summary>
    void Validate(string position);
}