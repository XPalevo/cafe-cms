using Auth;
using Microsoft.EntityFrameworkCore;
using Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuth(new(),
    options => options.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=root",
        x => x.MigrationsAssembly(MigrationsAssembly.AssemblyName)));

builder.Services.AddAuthEndpoints();

var app = builder.Build();

app.UseAuth();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapAuthEndpoints();

app.MapGet("/", () => "Hello World!");
app.MapGet("/test", () => "Hello World!").RequireAuthorization();

app.Run();
