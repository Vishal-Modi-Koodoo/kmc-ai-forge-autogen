using KMC_AI_Forge_BTL_Agent.Contracts;
using KMC_AI_Forge_BTL_Agent.Services;
using KMC_Forge_BTL_API.Services;
using KMC_Forge_BTL_API.Hubs;
using KMC_Forge_BTL_Configurations;
using KMC_Forge_BTL_Database.Services;
using KMC_Forge_BTL_Database.Interfaces;
using KMC_Forge_BTL_Database.Repositories;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Initialize the configuration singleton
AppConfiguration.Initialize(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers();

// Configure request size limits for file uploads
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 500 * 1024 * 1024; // 500MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500 * 1024 * 1024; // 500MB
    options.ValueLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KMC Forge BTL API", Version = "v1" });
    
    // Configure Swagger for better file upload support
    c.OperationFilter<FileUploadOperationFilter>();
    
    // Add support for multipart/form-data
    c.AddSecurityDefinition("multipart/form-data", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "Content-Type"
    });
});

builder.Services.AddTransient<IDocumentStorageService, DocumentStorageService>();
builder.Services.AddTransient<IDocumentRetrievalService, DocumentRetrievalService>();
builder.Services.AddTransient<IPortfolioValidationService, PortfolioValidationService>();
builder.Services.AddTransient<IAgentRuntime, AgentRuntime>();
builder.Services.AddTransient<KMC_Forge_BTL_Core_Agent.Agents.LeadPortfolioAgent>();

// Add SignalR services
builder.Services.AddSignalR();
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();

// Add MongoDB services
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped<IPortfolioUploadRepository>(provider =>
{
    var mongoDbService = provider.GetRequiredService<MongoDbService>();
    var database = mongoDbService.GetDatabase();
    return new PortfolioUploadRepository(database);
});

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
        
        // Configure Swagger UI for better file upload experience
        c.DocumentTitle = "KMC Forge BTL API - File Upload";
        c.DefaultModelsExpandDepth(-1); // Hide schemas section
        c.DisplayRequestDuration();
        
        // Add custom CSS for better file upload UI
        c.InjectStylesheet("/swagger-ui/custom.css");
    });
}

app.UseHttpsRedirection();

// Enable static files for custom Swagger UI CSS
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Map SignalR hub
app.MapHub<DocumentProcessingHub>("/documentProcessingHub");

app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.Run();
