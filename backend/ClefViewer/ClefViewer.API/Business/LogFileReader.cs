using System.Collections.Frozen;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace ClefViewer.API.Business;

public sealed record LogFiles(string[] Paths, List<LogFile> Files);

public sealed record LogFile([property: JsonIgnore] string Path, List<LogFileEntry> Entries)
{
    [JsonIgnore]
    public string DirectoryPath { get; } = System.IO.Path.GetDirectoryName(Path)!;

    [JsonIgnore]
    public long StreamPosition { get; set; }
}

public sealed class LogFileEntry(LogEvent logEvent)
{
    private string? _message;
    private string? _exception;

    [JsonIgnore]
    public LogEvent Event => logEvent;

    [JsonConverter(typeof(StringEnumConverter))]
    public LogEventLevel Level => logEvent.Level;

    public string Message => _message ??= logEvent.RenderMessage();
    public string MessageTemplate => logEvent.MessageTemplate.Text;
    public string? Exception => _exception ??= logEvent.Exception?.ToString();
    public DateTimeOffset Timestamp => logEvent.Timestamp;
    public IReadOnlyDictionary<string, LogEventPropertyValue> Properties => logEvent.Properties;
}

public interface ILogFileReader
{
    Task<LogFiles> ReadLogFilesAsync(string[] paths, CancellationToken cancellationToken);
    Task AddLogEntriesAsync(LogFile logFile, CancellationToken cancellationToken);
}

public sealed class LogFileReader(IFileSystemHandler fileSystemHandler) : ILogFileReader
{
    private static readonly FrozenSet<string> AllowedExtensions = new[] { ".clef", ".json", ".txt" }.ToFrozenSet();

    public async Task<LogFiles> ReadLogFilesAsync(string[] paths, CancellationToken cancellationToken)
    {
        var (filePaths, directoryPaths) = fileSystemHandler.GetPathsToProcess(paths, AllowedExtensions);
        var allFilePaths = filePaths.Concat(directoryPaths).ToHashSet();
        var logFiles = new List<LogFile>();
        foreach (var filePath in allFilePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entries = new List<LogFileEntry>();
            var logFile = new LogFile(filePath, entries);
            await AddLogEntriesAsync(logFile, cancellationToken);
            logFiles.Add(logFile);
        }

        return new LogFiles(paths, logFiles);
    }

    public async Task AddLogEntriesAsync(LogFile file, CancellationToken cancellationToken)
    {
        var entries = file.Entries;
        file.StreamPosition = await fileSystemHandler.ProcessStreamAsync(file.Path, file.StreamPosition, streamReader =>
        {
            using var logEventReader = new LogEventReader(streamReader);
            while (logEventReader.TryRead(out var logEvent))
            {
                cancellationToken.ThrowIfCancellationRequested();
                entries.Add(new LogFileEntry(logEvent));
            }
        });
    }
}