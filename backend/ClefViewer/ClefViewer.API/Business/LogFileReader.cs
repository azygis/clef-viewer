using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace ClefViewer.API.Business;

public sealed record LogFiles(string[] Paths, List<LogFile> Files)
{
    public long TotalSize => Files.Sum(f => f.Size);
}

public sealed record LogFile(string Path, long Size, List<LogFileEntry> Entries);

public sealed class LogFileEntry(LogEvent logEvent)
{
    private string? _message;
    private string? _exception;

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
            var fileName = Path.GetFileName(filePath);
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileSize = stream.Length;
            using var streamReader = new StreamReader(stream);
            using var logEventReader = new LogEventReader(streamReader);
            while (logEventReader.TryRead(out var logEvent))
            {
                entries.Add(new LogFileEntry(logEvent));
            }
            logFiles.Add(new LogFile(fileName, fileSize, entries));
        }
        return new LogFiles(paths, logFiles);
    }
}