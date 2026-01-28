using System.Reflection;
using AlzaEshop.API.Common;
using AlzaEshop.API.Common.Endpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddServices();

builder.Services.AddOpenApiDocuments();

var assembly = Assembly.GetExecutingAssembly();
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddEndpoints(assembly);
builder.Services.AddEndpointsVersioning();

var app = builder.Build();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    //// Apply migrations and seed database
    app.MapOpenApi("/openapi/{documentName}.json");
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Alza API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .AddDocument("v1", "Version 1", "/openapi/v1.json", true)
            .AddDocument("v2", "Version 2", "/openapi/v2.json");
    });
}

app.UseHttpsRedirection();

app.Run();
