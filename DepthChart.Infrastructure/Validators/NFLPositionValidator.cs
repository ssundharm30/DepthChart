using DepthChart.Core.Interfaces;
using DepthChart.Core.Models;
using DepthChart.Infrastructure.Constants;

namespace DepthChart.Infrastructure.Validators;

public class NFLPositionValidator : IPositionValidator
{
    /// <inheritdoc />
    public void Validate(string position)
    {
        if (string.IsNullOrWhiteSpace(position))
            throw new ArgumentException("Position cannot be null or empty.", nameof(position));

        if (!NFLPositions.IsValid(position))
            throw new ArgumentException(
                $"'{position}' is not a valid NFL position.", nameof(position));
    }
}