namespace DepthChart.Core.Models;

public record Player(int Number, string Name, string Position = "")
{
    public override string ToString() => $"#{Number} - {Name}";
}