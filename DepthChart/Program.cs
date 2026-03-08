using DepthChart.Core.Interfaces;
using DepthChart.Core.Models;
using DepthChart.Infrastructure.Constants;
using DepthChart.Infrastructure.Factories;
using DepthChart.Infrastructure.Repositories;
using DepthChart.Infrastructure.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    //Not going to print logs to keep console clean for output
    builder
        .SetMinimumLevel(LogLevel.Debug);
    //.AddConsole();
});

services.AddSingleton<IDepthChartRepository, DepthChartRepository>();
services.AddSingleton<IDepthChartServiceFactory, DepthChartServiceFactory>();
var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IDepthChartServiceFactory>();

// Create team
var buccaneers = factory.Create(NFLTeams.TampaBayBuccaneers);

// Create players
var tomBrady      = new Player(12, "Tom Brady");
var blaineGabbert = new Player(11, "Blaine Gabbert");
var kyleTrask     = new Player(2,  "Kyle Trask");
var mikeEvans     = new Player(13, "Mike Evans");
var jaelonDarden = new Player(1, "Jaelon Darden");
var scottMiller   = new Player(10, "Scott Miller");

// Add players
buccaneers.AddPlayerToDepthChart("QB",  tomBrady,      0);
buccaneers.AddPlayerToDepthChart("QB",  blaineGabbert, 1);
buccaneers.AddPlayerToDepthChart("QB",  kyleTrask,     2);
buccaneers.AddPlayerToDepthChart("LWR", mikeEvans,     0);
buccaneers.AddPlayerToDepthChart("LWR", jaelonDarden,  1);
buccaneers.AddPlayerToDepthChart("LWR", scottMiller,   2);


Console.WriteLine("getBackups(QB, Tom Brady):");
PrintList(buccaneers.GetBackups("QB", tomBrady));

// NOTE: requirement shows getBackups("QB", JaelonDarden) returning Scott Miller —
// which is a typo since Darden is LWR not QB.
Console.WriteLine("getBackups(QB, Jaelon Darden)");
PrintList(buccaneers.GetBackups("QB", jaelonDarden));

Console.WriteLine("getBackups(QB, Mike Evans):");
PrintList(buccaneers.GetBackups("QB", mikeEvans));

Console.WriteLine("getBackups(QB, Blaine Gabbert):");
PrintList(buccaneers.GetBackups("QB", blaineGabbert));

Console.WriteLine("getBackups(QB, Kyle Trask):");
PrintList(buccaneers.GetBackups("QB", kyleTrask));

Console.WriteLine("\ngetFullDepthChart (before remove)");
buccaneers.GetFullDepthChart();

Console.WriteLine("\nremovePlayerFromDepthChart\n");

// NOTE: requirement calls removePlayerFromDepthChart("WR", MikeEvans) but Evans was
// added as "LWR". Assuming WR and LWR are different positions and using LWR as the correct position.
var removed = buccaneers.RemovePlayerFromDepthChart("LWR", mikeEvans);
Console.WriteLine($"Removed: {removed}");

Console.WriteLine("\ngetFullDepthChart (after remove)\n");
buccaneers.GetFullDepthChart();


static void PrintList(List<Player> players)
{
    if (players.Count == 0)
        Console.WriteLine("[]");
    else
        players.ForEach(p => Console.WriteLine($"{p}"));
    Console.WriteLine();
}