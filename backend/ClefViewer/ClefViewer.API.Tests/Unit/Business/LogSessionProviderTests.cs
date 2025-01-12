using ClefViewer.API.Business;
using ClefViewer.API.Data;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Serilog.Events;

namespace ClefViewer.API.Tests.Unit.Business;

public sealed class LogSessionProviderTests
{
    private IClientProxy _clientProxy;
    private ILogSessionFilterFactory _filterFactory;
    private LogSessionProvider _sut;

    [SetUp]
    public void SetUp()
    {
        _clientProxy = Substitute.For<IClientProxy>();
        _filterFactory = Substitute.For<ILogSessionFilterFactory>();
        var hubContext = Substitute.For<IHubContext<LogHub>>();
        hubContext.Clients.All.Returns(_clientProxy);
        _sut = new LogSessionProvider(hubContext, _filterFactory);
    }

    [TearDown]
    public void TearDown() =>
        _sut.Dispose();

    [Test]
    public void AddSession_Should_AddSession_AndConfigureWatcher()
    {
        // ARRANGE
        var directoryPath = Path.GetFullPath("Unit/TestData");
        var paths = new[] { Path.Combine(directoryPath, "log-1.clef") };
        var logFiles = new LogFiles(paths, [new LogFile(paths[0], [])]);

        // ACT
        var sessionId = _sut.AddSession(logFiles);

        // ASSERT
        var watcher = _sut.GetSession(sessionId).Watchers.Single();
        Assert.Multiple(() =>
        {
            Assert.That(watcher.Path, Is.EqualTo(directoryPath));
            Assert.That(watcher.Filter, Is.EqualTo("log-1.clef"));
        });
    }

    [Test]
    public void ChangeWatchedFile_Should_EmitHubEvent()
    {
        // ARRANGE
        var sourceDirectoryPath = Path.GetFullPath("Unit/TestData");
        var targetDirectoryPath = Path.Combine(sourceDirectoryPath, "temp");
        Directory.CreateDirectory(targetDirectoryPath);
        var sourceFilePath = Path.Combine(sourceDirectoryPath, "log-2.json");
        var targetFilePath = Path.Combine(targetDirectoryPath, $"log-{Guid.NewGuid()}.json");
        File.Copy(sourceFilePath, targetFilePath, true);
        var paths = new[] { targetFilePath };
        var logFiles = new LogFiles(paths, [new LogFile(paths[0], [])]);
        var sessionId = _sut.AddSession(logFiles);
        var expectedEventArgs = new object[] { sessionId, targetFilePath };

        // ACT
        File.AppendAllLines(targetFilePath, [
            """
            {"@t":"2022-06-07T03:44:57.8532799Z","@mt":"Hello, {User}","User":"planet"}
            """
        ]);

        // ASSERT
        _clientProxy.Received().SendCoreAsync(LogHub.FilesChanged, Arg.Is<object[]>(o => o.SequenceEqual(expectedEventArgs)), CancellationToken.None);
    }

    [Test]
    public void GetSessions_MapsCorrectly()
    {
        // ARRANGE
        var path1 = Path.GetFullPath("Unit/TestData/log-1.clef");
        var path2 = Path.GetFullPath("Unit/TestData/log-2.json");
        var withOneEvent = _sut.AddSession(new LogFiles([path1], [new LogFile(path1, [new LogFileEntry(EventStub.Events[0])])]));
        var withTwoEvents = _sut.AddSession(new LogFiles([path2], [new LogFile(path2, [new LogFileEntry(EventStub.Events[0]), new LogFileEntry(EventStub.Events[1])])]));

        // ACT
        var sessions = _sut.GetSessions().OrderBy(x => x.EventCount).ToList();

        // ASSERT
        Assert.That(sessions, Has.Exactly(2).Items);
        AssertSession(sessions[0], withOneEvent, path1, 1);
        AssertSession(sessions[1], withTwoEvents, path2, 2);
        return;

        void AssertSession(LogSessionDTO session, Guid expectedId, string expectedPath, int expectedEventCount)
        {
            Assert.Multiple(() =>
            {
                Assert.That(session.Id, Is.EqualTo(expectedId));
                Assert.That(session.Paths, Is.EquivalentTo(new[] { expectedPath }));
                Assert.That(session.EventCount, Is.EqualTo(expectedEventCount));
            });
        }
    }

    [TestCase("asc", "Loop 0 done")]
    [TestCase("desc", "Loop 14 done")]
    public void GetEvents_Should_ReturnOrderedEvents_AndCorrectCounts(string sortOrder, string expectedFirstMessage)
    {
        // ARRANGE
        var request = new SearchLogEventsRequest(SortOrder: sortOrder);
        var path = Path.Combine("Unit", "TestData", "log-1.clef");
        var sessionId = _sut.AddSession(new LogFiles([path], [
            new LogFile(path, [
                new LogFileEntry(EventStub.Events[0]),
                new LogFileEntry(EventStub.Events[1]),
                new LogFileEntry(EventStub.Events[2]),
                new LogFileEntry(EventStub.Events[3])
            ])
        ]));

        // ACT
        var (events, counts) = _sut.GetEvents(sessionId, request);

        // ASSERT
        Assert.Multiple(() =>
        {
            Assert.That(events, Has.Exactly(4).Items);
            Assert.That(events[0].Message, Is.EqualTo(expectedFirstMessage));
            Assert.That(counts.Total, Is.EqualTo(4));
            Assert.That(counts.Info, Is.EqualTo(2));
            Assert.That(counts.Warning, Is.EqualTo(1));
            Assert.That(counts.Error, Is.EqualTo(1));
        });
    }

    [Test]
    public void GetEvents_Should_FilterEvents()
    {
        // ARRANGE
        const string expression = "expression";
        var request = new SearchLogEventsRequest(Expression: expression);
        var path = Path.Combine("Unit", "TestData", "log-1.clef");
        var sessionId = _sut.AddSession(new LogFiles([path], [
            new LogFile(path, [
                new LogFileEntry(EventStub.Events[0]),
                new LogFileEntry(EventStub.Events[1]),
                new LogFileEntry(EventStub.Events[2]),
                new LogFileEntry(EventStub.Events[3])
            ])
        ]));

        var filtered = false;
        _filterFactory.TryCreateFilter(expression, Arg.Any<EventFilter[]?>(), out Arg.Any<EventFilterFunc?>()).Returns(x =>
        {
            x[2] = (EventFilterFunc?) Filter;
            return true;

            bool Filter(LogEvent evt)
            {
                filtered = true;
                return true;
            }
        });

        // ACT
        _ = _sut.GetEvents(sessionId, request);

        // ASSERT
        Assert.That(filtered, Is.True);
    }

    [Test]
    public void GetEvents_Should_PageEvents()
    {
        // ARRANGE
        var request = new SearchLogEventsRequest(PageNumber: 2, PageSize: 1);
        var path = Path.Combine("Unit", "TestData", "log-1.clef");
        var sessionId = _sut.AddSession(new LogFiles([path], [
            new LogFile(path, [
                new LogFileEntry(EventStub.Events[0]),
                new LogFileEntry(EventStub.Events[1]),
                new LogFileEntry(EventStub.Events[2]),
                new LogFileEntry(EventStub.Events[3])
            ])
        ]));

        // ACT
        var (events, _) = _sut.GetEvents(sessionId, request);

        // ASSERT
        Assert.Multiple(() =>
        {
            Assert.That(events, Has.One.Items);
            Assert.That(events[0].Message, Is.EqualTo("Failed to do a thing"));
        });
    }
}