using ClefViewer.API.Business;
using Serilog.Events;

namespace ClefViewer.API.Data;

public sealed class LogEventCounts
{
    public LogEventCounts(IEnumerable<LogFileEntry> entries)
    {
        foreach (var entry in entries)
        {
            Total++;
            switch (entry.Level)
            {
                case LogEventLevel.Debug:
                    Debug++;
                    break;
                case LogEventLevel.Error:
                    Error++;
                    break;
                case LogEventLevel.Fatal:
                    Fatal++;
                    break;
                case LogEventLevel.Information:
                    Info++;
                    break;
                case LogEventLevel.Verbose:
                    Verbose++;
                    break;
                case LogEventLevel.Warning:
                    Warning++;
                    break;
            }

            var template = entry.MessageTemplate;
            if (MessageTemplates.TryGetValue(template, out var templateCount))
            {
                templateCount++;
            }
            else
            {
                templateCount = 1;
            }
            MessageTemplates[template] = templateCount;
        }
    }

    public int Total { get; }
    public int Debug { get; }
    public int Error { get; }
    public int Fatal { get; }
    public int Info { get; }
    public int Verbose { get; }
    public int Warning { get; }
    public Dictionary<string, int> MessageTemplates { get; } = new();
}

public sealed record SearchLogEventsResponse(LogFileEntry[] Events, LogEventCounts Counts);