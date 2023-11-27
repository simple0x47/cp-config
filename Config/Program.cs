using System.Security.Claims;
using Core.Secrets;
using Cuplan.Config.Models;
using Cuplan.Config.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddScoped<IConfigBuilder, MicroconfigConfigBuilder>();
builder.Services.AddScoped<IPackager, ZipPackager>();

// Models
builder.Services.AddScoped<ConfigProvider>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}