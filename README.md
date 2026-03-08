A console application for managing NFL depth charts, built in C# (.NET 10). 
Support for extending to multiple sports is also provided.

## Solution Structure

```
DepthChart.sln
  ─ DepthChart.Core             # Core Models and interfaces 
  ─ DepthChart.Application      # Business logic
  ─ DepthChart.Infrastructure   # Repository, factory, validators & constants
  ─ DepthChart                  # Entry point and DI registration
  ─ DepthChart.Tests            # xUnit unit tests with NSubstitute
```

## Features

- Add a player to a position at a specific depth, or append to the end
- Remove a player from a position
- Get all backups for a player at a given position
- Print the full depth chart for a team
- Multi-sport support (NFL, MLB, NHL, NBA, etc)
- Multi-team support


## Key Design Decisions

**Layered architecture** - Core, Application, Infrastructure as separate projects to enforce strict dependency rules.

**Factory pattern** - `IDepthChartServiceFactory.Create(team)` returns a service scoped to a specific Team. Sport is derived from `Team.Sport`.

**Nested dictionary** - `Dictionary<Team, Dictionary<string, List<Player>>>` is used to be able to handle multiple positions in the same team as well as multiple teams that belong to different sports.

**Player identity** - a player is uniquely identified by their `Number`. A player can appear on multiple positions simultaneously.

**IPositionValidator** - sport-specific validation is injected. Adding a new sport requires only a new validator registered in the factory.


## Assumptions made

- Adding the same player again with a different position is given a priority during update. The player will be removed from the initial position and later moved to the new position.

- The requirements states "return empty list if not found" which could cause type inconsistency issues. The requirement has been satisfied, however, an internal method returning `Player? for type safety is used.

Two issues in the sample inputs were identified (have added documentationn in `Program.cs` too):
- `getBackups("QB", JaelonDarden)` returns `Scott Miller`. In the requirements - JaelonDarden is `LWR` not `QB`. However, this implementation returns empty list.
- `removePlayerFromDepthChart("WR", MikeEvans)` - MikeEvans was added as `LWR`. `WR` and `LWR` are treated as distinct positions. `LWR` is used in the sample.

- Position within the `Player` object are metadata only. Number is used to identify a player

## Running the App

- Please navigate to the project's root folder and run the following command:

```bash
dotnet run --project DepthChart
```

## Running Tests

```bash
dotnet test
```

## Extending to a New Sport

1. Add the sport to the `Sport` enum in `DepthChart.Core`
2. Add a positions constants class and teams validation class in `DepthChart.Infrastructure/Constants`
3. Add a validator implementing `IPositionValidator` in `DepthChart.Infrastructure/Validators`
4. Register the validator in `DepthChartServiceFactory._validators`
