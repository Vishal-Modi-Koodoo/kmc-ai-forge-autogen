using KMC_AI_Forge_BTL_Agent.Contracts;
using KMC_AI_Forge_BTL_Agent.Services;
using KMC_Forge_BTL_API.Services;
using KMC_Forge_BTL_Configurations;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Initialize the configuration singleton
AppConfiguration.Initialize(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KMC Forge BTL API", Version = "v1" });
});

builder.Services.AddTransient<IDocumentStorageService, DocumentStorageService>();
builder.Services.AddTransient<IDocumentRetrievalService, DocumentRetrievalService>();
builder.Services.AddTransient<IPortfolioValidationService, PortfolioValidationService>();
builder.Services.AddTransient<IAgentRuntime, AgentRuntime>();

// Add custom logging
builder.Services.AddSingleton<ILoggerFactory, CustomLoggerFactory>();
builder.Services.AddSingleton<IExternalScopeProvider, LoggerExternalScopeProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KMC Forge BTL API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.Run();
