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

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var assembly = Assembly.GetExecutingAssembly();
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddEndpoints(assembly);

var app = builder.Build();

app.MapEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Alza API")
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

app.Run();
