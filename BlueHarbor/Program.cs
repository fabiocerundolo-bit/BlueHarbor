using BlueHarbor.Application.Interfaces;
using BlueHarbor.Application.Services;
using BlueHarbor.Infrastructure.Persistence;
using BlueHarbor.Infrastructure.Repositories;
using BlueHarbor.Security;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. SERVIZI
// ==========================================
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // ✅ OpenAPI nativo .NET 10 (NON AddSwaggerGen)

// Database
builder.Services.AddDbContext<BlueHarborDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IShipRepository, ShipRepository>();
builder.Services.AddScoped<IBerthRepository, BerthRepository>();
builder.Services.AddScoped<ISystemStateRepository, SystemStateRepository>();

// Services (SOLO quelli necessari)
builder.Services.AddScoped<IShipService, ShipService>();
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<ITimeManagementService, TimeManagementService>();

// Authentication & Authorization
builder.Services.AddAuthentication("Mock")
    .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>("Mock", null);
builder.Services.AddAuthorization();

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ==========================================
// 2. SEED DATABASE
// ==========================================
await app.InitializeDatabaseAsync();

// ==========================================
// 3. MIDDLEWARE PIPELINE
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // ✅ OpenAPI nativo
    app.MapScalarApiReference(); // ✅ UI Scalar (sostituisce SwaggerUI)
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseHangfireDashboard("/hangfire");

app.Run();