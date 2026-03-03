using Microsoft.EntityFrameworkCore;                       // Needed for UseSqlServer
using VulnScanner.Api.Data;                                // Needed for AppDbContext
using VulnScanner.Api.Services;

var builder = WebApplication.CreateBuilder(args);           // Creates the app builder (config + DI container)

builder.Services.AddControllers();                          // Enables controller endpoints
builder.Services.AddEndpointsApiExplorer();                 // Enables Swagger endpoint discovery
builder.Services.AddSwaggerGen();                           // Generates Swagger/OpenAPI docs
builder.Services.AddHttpClient(); // Enables IHttpClientFactory for outbound requests

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>(); // One shared queue instance
builder.Services.AddHostedService<ScanWorker>(); // Runs ScanWorker in background when app starts

builder.Services.AddScoped<ScanRunner>(); // New ScanRunner per scope (DbContext is scoped)
builder.Services.AddTransient<IScannerModule, SecurityHeadersModule>(); // Register one module

builder.Services.AddDbContext<AppDbContext>(options =>      // Registers AppDbContext into Dependency Injection
{
    options.UseSqlServer(                                   // Tells EF Core to use SQL Server provider
        builder.Configuration.GetConnectionString("SqlServer") // Reads connection string from appsettings
    );
});

var app = builder.Build();                                  // Builds the app pipeline

if (app.Environment.IsDevelopment())                        // Only in Development
{
    app.UseSwagger();                                       // Serve Swagger JSON
    app.UseSwaggerUI();                                     // Serve Swagger UI
}

app.UseHttpsRedirection();                                  // Redirect HTTP to HTTPS in dev

app.MapControllers();                                       // Maps controller routes

app.Run();                                                  // Starts listening for requests