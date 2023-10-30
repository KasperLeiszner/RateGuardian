using Microsoft.AspNetCore.Builder;
using RateGuardian;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateGuardian();

var app = builder.Build();

app.UseRateGuardian();

app.MapGet("/", () => { });

app.Run();