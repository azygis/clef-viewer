using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace ClefViewer.API.Tests.Unit.Business;

public static class EventStub
{
    public static readonly IReadOnlyList<LogEvent> Events = CreateEvents();

    private static List<LogEvent> CreateEvents()
    {
        const string json =
            """
            {"@t":"2017-04-20T04:24:47.0251719Z","@mt":"Loop {Counter} done","Counter":0}
            {"@t":"2017-04-20T04:24:47.0371689Z","@l":"Warning","@mt":"Loop {Counter} done","Counter":1}
            {"@t":"2017-04-20T04:24:47.0471689Z","@mt":"Failed to do a thing","@l":"Error"}
            {"@t":"2017-04-20T04:24:47.0651719Z","@mt":"Loop {Counter} done","Counter":14}
            """;
        using var textReader = new StringReader(json);
        using var reader = new LogEventReader(textReader);
        var result = new List<LogEvent>();

        while (reader.TryRead(out var evt))
        {
            result.Add(evt);
        }

        return result;
    }
}