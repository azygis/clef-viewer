using ClefViewer.API.Business;
using Serilog.Expressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddScoped<ILogFileReader, LogFileReader>();
builder.Services.AddSingleton<ILogSessionProvider, LogSessionProvider>();
builder.Services.AddSingleton<NameResolver, CustomMemberNameResolver>();

var app = builder.Build();

app.UseCors(b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapControllers();

app.Run();