namespace ClefViewer.API.Data;

public sealed record SearchLogEventsRequest(int PageNumber = 1, int PageSize = 40);