using KMC_AI_Forge_BTL_Agent.Contracts;
using KMC_AI_Forge_BTL_Agent.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.Run();
