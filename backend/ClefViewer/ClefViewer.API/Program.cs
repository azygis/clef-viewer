using ClefViewer.API.Business;
using Serilog.Expressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ClefViewer.API.Tests")]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging().AddControllers().AddNewtonsoftJson();
builder.Services.AddSignalR();
builder.Services.AddScoped<ILogFileReader, LogFileReader>();
builder.Services.AddSingleton<IFileSystemHandler, FileSystemHandler>();
builder.Services.AddSingleton<ILogSessionProvider, LogSessionProvider>();
builder.Services.AddSingleton<ILogSessionFilterFactory, LogSessionFilterFactory>();
builder.Services.AddSingleton<NameResolver, CustomMemberNameResolver>();

builder.WebHost.ConfigureKestrel((_, options) => options.ListenLocalhost(61455));

var app = builder.Build();

app.UseCors(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();
app.MapHub<LogHub>("log-hub");

app.Run();