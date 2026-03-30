using Business.Authentication;
using Business.Controllers;
using Business.Services;
using Data.Repositories;
using Domain.Authentication;
using Domain.Environment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Business
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection envToken = configuration.GetSection("Token");
            TokenSetting tokenSetting = envToken.Get<TokenSetting>() ?? throw new(nameof(TokenSetting));

            services.Configure<TokenSetting>(envToken);

            services.AddSingleton(sp =>
            {
                var storageOptions = new StorageSetting();
                configuration.GetSection("Storage").Bind(storageOptions);
                return storageOptions;
            });

            var userKey = Encoding.UTF8.GetBytes(tokenSetting.UserScheme.Key);
            
            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = AuthScheme.User.ToSchemeName();
                opt.DefaultChallengeScheme = AuthScheme.User.ToSchemeName();
            })
            .AddJwtBearer(AuthScheme.User.ToSchemeName(), opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = tokenSetting.Issuer,
                    ValidateAudience = true,
                    ValidAudience = tokenSetting.UserScheme.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(userKey),
                    ValidateLifetime = true
                };

                // SignalR WebSocket/SSE connections cannot send custom headers in browsers,
                // so the token is passed as an access_token query parameter.
                // This hook tells the JWT Bearer middleware to read it from there.
                opt.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSingleton<UploadHandler>();
            services.AddScoped<AuthenticationService>();
            services.AddScoped<AuthService>();
            services.AddScoped<StudentRepository>();
            services.AddScoped<UserRepository>();
            services.AddScoped<CandidacyTypeRepository>();
            services.AddScoped<ScrutinyRepository>();
            services.AddScoped<ScrutinySignRepository>();
            services.AddScoped<SlateRepository>();
            services.AddScoped<SlateCandidacyRepository>();
            services.AddScoped<VoteRepository>();
            services.AddScoped<CurrentUserContext>();

            return services;
        }
    }
}
