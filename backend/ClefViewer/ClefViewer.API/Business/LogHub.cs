using Microsoft.AspNetCore.SignalR;

namespace ClefViewer.API.Business;

public sealed class LogHub : Hub
{
    public const string FilesChanged = "FilesChanged";
}