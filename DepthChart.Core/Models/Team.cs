namespace DepthChart.Core.Models;

public record Team(string Id, string Name, Sport Sport)
{
    public override string ToString() => $"{Name} ({Sport})";
}