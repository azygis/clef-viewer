using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace ClefViewer.API.Business;

public sealed record LogFiles([property: JsonIgnore] string[] Paths, List<LogFile> Files);

public sealed record LogFile([property: JsonIgnore] string Path, List<LogFileEntry> Entries)
{
    [JsonIgnore]
    public string DirectoryPath => System.IO.Path.GetDirectoryName(Path)!;

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

public sealed class LogFileReader : ILogFileReader
{
    private static readonly HashSet<string> AllowedExtensions = [".clef", ".json", ".txt"];

    public async Task<LogFiles> ReadLogFilesAsync(string[] paths, CancellationToken cancellationToken)
    {
        var filePaths = paths.Where(File.Exists).ToArray();
        var directoryPaths = paths.Where(Directory.Exists).ToArray();
        var directoryFiles = directoryPaths
            .Where(Directory.Exists)
            .SelectMany(x =>
                Directory.EnumerateFiles(x, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(y => AllowedExtensions.Contains(Path.GetExtension(y))));

        var allFilePaths = filePaths.Concat(directoryFiles).ToArray();

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
        await using var stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Seek(file.StreamPosition, SeekOrigin.Begin);
        using var streamReader = new StreamReader(stream);
        using var logEventReader = new LogEventReader(streamReader);
        while (logEventReader.TryRead(out var logEvent))
        {
            cancellationToken.ThrowIfCancellationRequested();
            entries.Add(new LogFileEntry(logEvent));
        }
        file.StreamPosition = stream.Position;
    }
}