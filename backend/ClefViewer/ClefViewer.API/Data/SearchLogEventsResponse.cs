using ClefViewer.API.Business;

namespace ClefViewer.API.Data;

public sealed record SearchLogEventsResponse(LogFileEntry[] Events, int TotalEvents);