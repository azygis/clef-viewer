namespace ClefViewer.API.Data;

public sealed record EventFilter(string Property, string Value);

public sealed record SearchLogEventsRequest(int PageNumber = 1, int PageSize = 40, string SortOrder = "desc", string? Expression = null, EventFilter[]? Filters = null);