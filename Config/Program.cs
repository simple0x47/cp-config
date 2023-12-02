using System.Security.Claims;
using Core.Secrets;
using Cuplan.Config.HealthChecks;
using Cuplan.Config.Models;
using Cuplan.Config.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["IdentityProvider:Authority"];
    options.Audience = builder.Configuration["IdentityProvider:Audience"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddNewtonsoftJson();

// Other dependencies
builder.Services.AddSingleton<ISecretsManager, BitwardenSecretsManager>();

// Services
builder.Services.AddSingleton<IDownloader, GitDownloader>();
builder.Services.AddSingleton<IConfigBuilder, MicroconfigConfigBuilder>();
builder.Services.AddSingleton<IPackager, ZipPackager>();

// Models
builder.Services.AddSingleton<ConfigProvider>();

// Health checks
builder.Services.AddHealthChecks().AddCheck<DownloadHealthCheck>("download_check");

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
}).RequireAuthorization();

app.Run();

public partial class Program
{
}