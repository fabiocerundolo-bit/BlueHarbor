using BlueHarbor.Application.Interfaces;
using BlueHarbor.Application.Services;
using BlueHarbor.Components;
using BlueHarbor.Infrastructure.Persistence;
using BlueHarbor.Infrastructure.Repositories;
using BlueHarbor.Security;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<BlueHarborDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IShipRepository, ShipRepository>();
builder.Services.AddScoped<IBerthRepository, BerthRepository>();
builder.Services.AddScoped<ISystemStateRepository, SystemStateRepository>();

// Services
builder.Services.AddScoped<IShipService, ShipService>();
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<ISystemService, SystemService>();
builder.Services.AddScoped<ITimeManagementService, TimeManagementService>();
builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Definisci la policy CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // Aggiungi le porte del tuo FE
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Seed Database
await app.InitializeDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// 2. Applica la policy CORS (DEVE stare prima di UseAuthorization)
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseHangfireDashboard("/hangfire");
app.MapControllers();

app.Run();