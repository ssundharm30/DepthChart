using DepthChart.Application.Services;
using DepthChart.Core.Interfaces;
using DepthChart.Core.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace DepthChart.Tests.Services;

public class DepthChartServiceTests
{

    private readonly IDepthChartRepository _fakeRepository;
    private readonly IPositionValidator    _fakeValidator;
    private readonly DepthChartService     _fakeDepthService;

    private static readonly Team   BuccaneerTeam = new("TBB", "Tampa Bay Buccaneers", Sport.NFL);
    private static readonly Player TomBrady      = new(12, "Tom Brady");
    private static readonly Player BlaineGabbert = new(11, "Blaine Gabbert");
    private static readonly Player KyleTrask     = new(2,  "Kyle Trask");
    private static readonly Player MikeEvans     = new(13, "Mike Evans");

    public DepthChartServiceTests()
    {
        _fakeRepository = Substitute.For<IDepthChartRepository>();
        _fakeValidator  = Substitute.For<IPositionValidator>();

        _fakeDepthService = new DepthChartService(
            _fakeRepository,
            _fakeValidator,
            NullLogger<DepthChartService>.Instance,
            BuccaneerTeam);
    }
    
    // AddPlayerToDepthChart

    [Fact]
    public void AddPlayer_WithPositionDepth_InsertsAtCorrectIndex()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, KyleTrask]);

        // Act
        _fakeDepthService.AddPlayerToDepthChart("QB", BlaineGabbert, 1);

        // Assert
        _fakeRepository.Received(1).SavePositionDepth(
            BuccaneerTeam, "QB",
            Arg.Is<List<Player>>(players =>
                players[0] == TomBrady &&
                players[1] == BlaineGabbert &&
                players[2] == KyleTrask));
    }

    [Fact]
    public void AddPlayer_WithoutPositionDepth_AppendsToEnd()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert]);

        // Act
        _fakeDepthService.AddPlayerToDepthChart("QB", KyleTrask);

        // Assert
        _fakeRepository.Received(1).SavePositionDepth(
            BuccaneerTeam, "QB",
            Arg.Is<List<Player>>(players =>
                players.Count == 3 &&
                players[2] == KyleTrask));
    }

    [Fact]
    public void AddPlayer_WhenPlayerAlreadyExists_MovesToNewDepth()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert, KyleTrask]);

        // Act
        _fakeDepthService.AddPlayerToDepthChart("QB", TomBrady, 2);

        // Assert
        _fakeRepository.Received(1).SavePositionDepth(
            BuccaneerTeam, "QB",
            Arg.Is<List<Player>>(players =>
                players[0] == BlaineGabbert &&
                players[1] == KyleTrask &&
                players[2] == TomBrady));
    }

    [Fact]
    public void AddPlayer_AtDepthZeroOnEmptyChart_InsertsSuccessfully()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB").Returns([]);

        // Act
        _fakeDepthService.AddPlayerToDepthChart("QB", TomBrady, 0);

        // Assert
        _fakeRepository.Received(1).SavePositionDepth(
            BuccaneerTeam, "QB",
            Arg.Is<List<Player>>(players => players.Count == 1 && players[0] == TomBrady));
    }

    [Fact]
    public void AddPlayer_WithOutOfBoundsDepth_ThrowsArgumentException()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert]);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _fakeDepthService.AddPlayerToDepthChart("QB", KyleTrask, 99));
    }

    [Fact]
    public void AddPlayer_WithNegativeDepth_ThrowsArgumentException()
    {
        //Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB").Returns([]);
        
        //Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _fakeDepthService.AddPlayerToDepthChart("QB", TomBrady, -1));
    }

    [Fact]
    public void AddPlayer_WithNullPlayer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _fakeDepthService.AddPlayerToDepthChart("QB", null));
    }

    [Fact]
    public void AddPlayer_WithInvalidPosition_ThrowsArgumentException()
    {
        // Arrange
        _fakeValidator
            .When(v => v.Validate("INVALID"))
            .Do(_ => throw new ArgumentException("Invalid position"));

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _fakeDepthService.AddPlayerToDepthChart("INVALID", TomBrady, 0));
    }

    [Fact]
    public void AddPlayer_DoesNotSave_WhenValidatorThrows()
    {
        // Arrange
        _fakeValidator
            .When(v => v.Validate(Arg.Any<string>()))
            .Do(_ => throw new ArgumentException("Invalid position"));

        // Act
        Assert.Throws<ArgumentException>(() =>
            _fakeDepthService.AddPlayerToDepthChart("INVALID", TomBrady, 0));

        // Assert
        _fakeRepository.DidNotReceive().SavePositionDepth(
            Arg.Any<Team>(), Arg.Any<string>(), Arg.Any<List<Player>>());
    }
    
    // RemovePlayerFromDepthChart

    [Fact]
    public void RemovePlayer_WhenFound_ReturnsRemovedPlayer()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert, KyleTrask]);

        // Act
        var result = _fakeDepthService.RemovePlayerFromDepthChart("QB", BlaineGabbert);

        // Assert
        Assert.Equal(BlaineGabbert, result);
    }

    [Fact]
    public void RemovePlayer_WhenFound_ShiftsRemainingPlayersUp()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert, KyleTrask]);

        // Act
        _fakeDepthService.RemovePlayerFromDepthChart("QB", BlaineGabbert);

        // Assert
        _fakeRepository.Received(1).SavePositionDepth(
            BuccaneerTeam, "QB",
            Arg.Is<List<Player>>(players =>
                players.Count == 2 &&
                players[0] == TomBrady &&
                players[1] == KyleTrask));
    }

    [Fact]
    public void RemovePlayer_WhenNotFound_ReturnsEmptyList()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert]);

        // Act
        var result = _fakeDepthService.RemovePlayerFromDepthChart("QB", MikeEvans);

        // Assert
        Assert.Empty((List<Player>)result);
    }

    [Fact]
    public void RemovePlayer_WhenNotFound_DoesNotSave()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady]);

        // Act
        _fakeDepthService.RemovePlayerFromDepthChart("QB", MikeEvans);

        // Assert
        _fakeRepository.DidNotReceive().SavePositionDepth(
            Arg.Any<Team>(), Arg.Any<string>(), Arg.Any<List<Player>>());
    }

    [Fact]
    public void RemovePlayer_WithNullPlayer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _fakeDepthService.RemovePlayerFromDepthChart("QB", null));
    }

    [Fact]
    public void RemovePlayer_WithInvalidPosition_ThrowsArgumentException()
    {
        _fakeValidator
            .When(v => v.Validate("INVALID"))
            .Do(_ => throw new ArgumentException("Invalid position"));

        Assert.Throws<ArgumentException>(() =>
            _fakeDepthService.RemovePlayerFromDepthChart("INVALID", TomBrady));
    }
    
    // GetBackups

    [Fact]
    public void GetBackups_ReturnsAllPlayersBelow()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert, KyleTrask]);

        // Act
        var backups = _fakeDepthService.GetBackups("QB", TomBrady);

        // Assert
        Assert.Equal(2, backups.Count);
        Assert.Equal(BlaineGabbert, backups[0]);
        Assert.Equal(KyleTrask,     backups[1]);
    }

    [Fact]
    public void GetBackups_WhenPlayerIsLast_ReturnsEmptyList()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert, KyleTrask]);

        // Act
        var backups = _fakeDepthService.GetBackups("QB", KyleTrask);

        // Assert
        Assert.Empty(backups);
    }

    [Fact]
    public void GetBackups_WhenPlayerNotOnChart_ReturnsEmptyList()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert]);

        // Act
        var backups = _fakeDepthService.GetBackups("QB", MikeEvans);

        // Assert
        Assert.Empty(backups);
    }

    [Fact]
    public void GetBackups_WhenDepthChartIsEmpty_ReturnsEmptyList()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB").Returns([]);
        
        // Act
        var backups = _fakeDepthService.GetBackups("QB", TomBrady);
        
        // Assert
        Assert.Empty(backups);
    }

    [Fact]
    public void GetBackups_WithNullPlayer_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _fakeDepthService.GetBackups("QB", null));
    }

    [Fact]
    public void GetBackups_WithInvalidPosition_ThrowsArgumentException()
    {
        // Arrange
        _fakeValidator
            .When(v => v.Validate("INVALID"))
            .Do(_ => throw new ArgumentException("Invalid position"));
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _fakeDepthService.GetBackups("INVALID", TomBrady));
    }
    
    // PrintFullDepthChart

    [Fact]
    public void PrintFullDepthChart_WithPlayers_WritesToConsole()
    {
        // Arrange
        _fakeRepository.GetAllPopulatedPositions(BuccaneerTeam)
            .Returns(new Dictionary<string, List<Player>>
            {
                { "QB",  [TomBrady, BlaineGabbert] },
                { "LWR", [MikeEvans]               }
            });

        // Act & Assert
        var exception = Record.Exception(() => _fakeDepthService.GetFullDepthChart());
        Assert.Null(exception);
    }

    [Fact]
    public void PrintFullDepthChart_WhenEmpty_WritesEmptyMessage()
    {
        // Arrange
        _fakeRepository.GetAllPopulatedPositions(BuccaneerTeam)
            .Returns(Enumerable.Empty<KeyValuePair<string, List<Player>>>());

        // Act & Assert
        var exception = Record.Exception(() => _fakeDepthService.GetFullDepthChart());
        Assert.Null(exception);
    }
    
    // Misc

    [Fact]
    public void AddPlayer_ToMultiplePositions_StoresIndependently()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB").Returns([]);
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "LWR").Returns([]);

        // Act
        _fakeDepthService.AddPlayerToDepthChart("QB",  TomBrady, 0);
        _fakeDepthService.AddPlayerToDepthChart("LWR", TomBrady, 0);

        // Assert
        _fakeRepository.Received(1).SavePositionDepth(
            BuccaneerTeam, "QB",
            Arg.Is<List<Player>>(players => players.Count == 1 && players[0] == TomBrady));

        _fakeRepository.Received(1).SavePositionDepth(
            BuccaneerTeam, "LWR",
            Arg.Is<List<Player>>(players => players.Count == 1 && players[0] == TomBrady));
    }

    [Fact]
    public void RemovePlayer_WhenOnlyPlayerOnChart_LeavesEmptyList()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB").Returns([TomBrady]);

        // Act
        var result = _fakeDepthService.RemovePlayerFromDepthChart("QB", TomBrady);

        // Assert
        Assert.Equal(TomBrady, result);
        _fakeRepository.Received(1).SavePositionDepth(
            BuccaneerTeam, "QB",
            Arg.Is<List<Player>>(players => players.Count == 0));
    }

    [Fact]
    public void RemovePlayer_WhenStarter_FirstBackupMovesUp()
    {
        // Arrange
        _fakeRepository.GetPositionDepth(BuccaneerTeam, "QB")
            .Returns([TomBrady, BlaineGabbert, KyleTrask]);

        // Act
        _fakeDepthService.RemovePlayerFromDepthChart("QB", TomBrady);

        // Assert
        _fakeRepository.Received(1).SavePositionDepth(
            BuccaneerTeam, "QB",
            Arg.Is<List<Player>>(players =>
                players.Count == 2 &&
                players[0] == BlaineGabbert &&
                players[1] == KyleTrask));
    }
}