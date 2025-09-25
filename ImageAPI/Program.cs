using AutoMapper;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.SecretManager.V1;
using ImageAPI;
using ImageAPI.Core.Application.Interfaces;
using ImageAPI.Core.Application.Services;
using ImageAPI.Infrastructure.Persistence;
using ImageAPI.Infrastructure.Persistence.Repositories;
using ImageAPI.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var credentialJson = GetGoogleCredentials();
var credential = GoogleCredential.FromJson(credentialJson);
SecretManagerServiceClient client = new SecretManagerServiceClientBuilder { Credential = credential }.Build();

var tempCertPath = GetTempPath();
var certContent = GetCertContent(client);
string jwtKey = GetJwtKey(client);
await File.WriteAllTextAsync(tempCertPath, certContent);


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

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("ServiceOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("token_type", "service"); // <-- Kiểm tra claim ở đây!
    });
    options.AddPolicy("Both", policy =>
    {
        policy.RequireAuthenticatedUser();
        // Có thể yêu cầu claim không phải là service, hoặc một claim khác như "scope"
        policy.RequireAssertion(context =>
            !context.User.HasClaim(c => c.Type == "token_type" && c.Value == "service"));
    });
});



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

string GetTempPath()
{
    string tempCertPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.crt");
    return tempCertPath;
}

string GetJwtKey(SecretManagerServiceClient client)
{
    //get the secret value for JWT
    string projectId = builder.Configuration.GetSection("Gcp").GetValue<string>("ProjectID") ?? throw new ArgumentNullException("Gcp Project ID not configured.");
    string jwtSecret = builder.Configuration.GetSection("Gcp").GetValue<string>("JWTSecretKey") ?? throw new ArgumentNullException("Gcp JWTSecretKey not configured.");
    string secretVersion = builder.Configuration.GetSection("Gcp").GetValue<string>("SecretVersion") ?? throw new ArgumentNullException("Gcp Secret Version not configured.");
    AccessSecretVersionResponse result = client.AccessSecretVersion(new SecretVersionName(projectId, jwtSecret, secretVersion));
    string jwtKey = result.Payload.Data.ToStringUtf8();
    return jwtKey;
}

string GetGoogleCredentials()
{
    string projectId = builder.Configuration.GetSection("Gcp").GetValue<string>("ProjectID") ?? throw new ArgumentNullException("Gcp Project ID not configured.");
    string gcpCredentialsSecret = builder.Configuration.GetSection("Gcp").GetValue<string>("SACredentialsKey") ?? throw new ArgumentNullException("Gcp SACredentialsKey not configured.");
    string secretVersion = builder.Configuration.GetSection("Gcp").GetValue<string>("SecretVersion") ?? throw new ArgumentNullException("Gcp Secret Version not configured.");

    //Init Secret Manager Client
    var defaultCredential = GoogleCredential.GetApplicationDefault();
    Console.WriteLine(defaultCredential);
    SecretManagerServiceClient client = new SecretManagerServiceClientBuilder { Credential = defaultCredential }.Build();

    AccessSecretVersionResponse result = client.AccessSecretVersion(new SecretVersionName(projectId, gcpCredentialsSecret, secretVersion));
    string gcpCredentials = result.Payload.Data.ToStringUtf8();
    return gcpCredentials;
}

string GetCertContent(SecretManagerServiceClient client)
{
    // Configure EF Core with PostgreSQL
    string projectId = builder.Configuration.GetSection("Gcp").GetValue<string>("ProjectID") ?? throw new ArgumentNullException("Gcp Project ID not configured.");
    string secretId = builder.Configuration.GetSection("Gcp").GetValue<string>("SecretID") ?? throw new ArgumentNullException("Gcp Secret ID not configured.");
    string secretVersion = builder.Configuration.GetSection("Gcp").GetValue<string>("SecretVersion") ?? throw new ArgumentNullException("Gcp Secret Version not configured.");

    //get the secret value for database connection
    SecretVersionName secretVersionName = new(projectId, secretId, secretVersion);
    AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);
    string certContent = result.Payload.Data.ToStringUtf8();
    return certContent;
}

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
