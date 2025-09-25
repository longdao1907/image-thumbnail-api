using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ImageAPI
{
    public static class WebApplicationBuilderExtension
    {
        public static WebApplicationBuilder AddAppAuthetication(this WebApplicationBuilder builder, string jwtSecret)
        {
            var settingsSection = builder.Configuration.GetSection("ApiSettings");

            var issuer = settingsSection.GetValue<string>("Issuer");
            var audience = settingsSection.GetValue<string>("Audience");

            var key = Encoding.UTF8.GetBytes(jwtSecret);


            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuers = new[] { issuer, builder.Configuration.GetSection("ServiceTokenOptions").GetValue<string>("Issuer") },
                    ValidAudiences = new[] { audience, builder.Configuration.GetSection("ServiceTokenOptions").GetValue<string>("Audience") },
                    ValidateAudience = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });

            return builder;
        }


    }
}
