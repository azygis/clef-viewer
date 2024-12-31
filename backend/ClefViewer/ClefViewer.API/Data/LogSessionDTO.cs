namespace ClefViewer.API.Data;

public sealed record LogSessionDTO(Guid Id, int EventCount, string[] Paths);