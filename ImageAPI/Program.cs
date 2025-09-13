using AutoMapper;
using Google.Cloud.SecretManager.V1;
using ImageAPI;
using ImageAPI.Core.Application.Interfaces;
using ImageAPI.Core.Application.Services;
using ImageAPI.Infrastructure.Persistence;
using ImageAPI.Infrastructure.Persistence.Repositories;
using ImageAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args); 

// Configure EF Core with PostgreSQL
string projectId = builder.Configuration.GetSection("Gcp").GetValue<string>("ProjectID") ?? throw new ArgumentNullException("Gcp Project ID not configured.");
string secretId = builder.Configuration.GetSection("Gcp").GetValue<string>("SecretID") ?? throw new ArgumentNullException("Gcp Secret ID not configured.");
string jwtSecret = builder.Configuration.GetSection("Gcp").GetValue<string>("JWTSecretKey") ?? throw new ArgumentNullException("Gcp JWTSecretKey not configured.");
string secretVersion = builder.Configuration.GetSection("Gcp").GetValue<string>("SecretVersion") ?? throw new ArgumentNullException("Gcp Secret Version not configured.");

//Init Secret Manager Client
SecretManagerServiceClient client = await SecretManagerServiceClient.CreateAsync();

//get the secret value for database connection
SecretVersionName secretVersionName = new(projectId, secretId, secretVersion);
AccessSecretVersionResponse result = await client.AccessSecretVersionAsync(secretVersionName);
string certContent = result.Payload.Data.ToStringUtf8();

string tempCertPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.crt");
await File.WriteAllTextAsync(tempCertPath, certContent);

//get the secret value for JWT
result = await client.AccessSecretVersionAsync(new SecretVersionName(projectId, jwtSecret, secretVersion));
string jwtKey = result.Payload.Data.ToStringUtf8();


//Prepare database connection string
string baseConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Default not configured.");
baseConnection += $"Trust Server Certificate=false;Root Certificate={tempCertPath};";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseNpgsql(baseConnection);

});

// Register Automapper
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


// Register repositories and services for Dependency Injection

builder.Services.AddScoped<IImageMetadataRepository, ImageMetadataRepository>();
builder.Services.AddScoped<IStorageService, GcsStorageService>();
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference= new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id=JwtBearerDefaults.AuthenticationScheme
                }
            }, new string[]{}
        }
    });
});
builder.AddAppAuthetication(jwtKey);
builder.Services.AddAuthorization();



var app = builder.Build();

// 2. Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
app.MapGet("/health", () => "OK");
ApplyMigrations();
app.Run();

void ApplyMigrations()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }
    }
    ;

}
