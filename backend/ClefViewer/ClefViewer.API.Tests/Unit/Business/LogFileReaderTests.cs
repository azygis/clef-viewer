using ClefViewer.API.Business;

namespace ClefViewer.API.Tests.Unit.Business;

public sealed class LogFileReaderTests
{
    private LogFileReader _sut;

    [SetUp]
    public void SetUp() =>
        _sut = new LogFileReader(new FileSystemHandler());

    [Test]
    public async Task ReadLogFilesAsync_When_UsingDirectory_Should_ReadLogFiles()
    {
        // ARRANGE
        var directoryPath = Path.GetFullPath("Unit/TestData");

        // ACT
        var result = await _sut.ReadLogFilesAsync([directoryPath], CancellationToken.None);

        // ASSERT
        Assert.Multiple(() =>
        {
            Assert.That(result.Paths, Is.EqualTo(new[] { directoryPath }));
            Assert.That(result.Files, Has.Exactly(3).Items.And.All.Property(nameof(LogFile.DirectoryPath)).EqualTo(directoryPath));
            Assert.That(result.Files[0].Entries, Has.Exactly(2).Items);
            Assert.That(result.Files[1].Entries, Has.One.Items);
            Assert.That(result.Files[2].Entries, Has.One.Items);
        });
    }

    [Test]
    public async Task ReadLogFilesAsync_When_UsingFiles_Should_ReadLogFiles()
    {
        // ARRANGE
        var directoryPath = Path.GetFullPath("Unit/TestData");
        var path1 = Path.Combine(directoryPath, "log-1.clef");
        var path2 = Path.Combine(directoryPath, "log-4.tmp");
        var paths = new[] { path1, path2 };

        // ACT
        var result = await _sut.ReadLogFilesAsync(paths, CancellationToken.None);

        // ASSERT
        Assert.Multiple(() =>
        {
            Assert.That(result.Paths, Is.EqualTo(paths));
            Assert.That(result.Files, Has.Exactly(2).Items.And.All.Property(nameof(LogFile.DirectoryPath)).EqualTo(directoryPath));
            Assert.That(result.Files[0].Entries, Has.Exactly(2).Items);
            Assert.That(result.Files[1].Entries, Has.One.Items);
        });
    }

    [Test]
    public async Task AddLogEntriesAsync_Should_ExtendEntries_AndChangeStreamPosition()
    {
        // ARRANGE
        var sourceDirectoryPath = Path.GetFullPath("Unit/TestData");
        var targetDirectoryPath = Path.Combine(sourceDirectoryPath, "temp");
        Directory.CreateDirectory(targetDirectoryPath);
        var sourceFilePath = Path.Combine(sourceDirectoryPath, "log-2.json");
        var targetFilePath = Path.Combine(targetDirectoryPath, $"log-{Guid.NewGuid()}.json");
        File.Copy(sourceFilePath, targetFilePath, true);
        var paths = new[] { targetFilePath };

        var logFiles = await _sut.ReadLogFilesAsync(paths, CancellationToken.None);
        var logFile = logFiles.Files.Single();
        var initialStreamPosition = logFile.StreamPosition;

        await File.AppendAllLinesAsync(targetFilePath, [
            """
            {"@t":"2022-06-07T03:44:57.8532799Z","@mt":"Hello, {User}","User":"planet"}
            """
        ]);

        // ACT
        await _sut.AddLogEntriesAsync(logFile, CancellationToken.None);

        // ASSERT
        Assert.Multiple(() =>
        {
            Assert.That(logFile.Entries, Has.Exactly(2).Items);
            Assert.That(logFile.StreamPosition, Is.GreaterThan(initialStreamPosition));
        });
    }
}