using Business.Authentication;
using Business.Controllers;
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
            });
            
            services.AddScoped<AuthenticationService>();
            services.AddScoped<AuthService>();
            services.AddScoped<StudentRepository>();
            services.AddScoped<UserRepository>();
            services.AddScoped<CurrentUserContext>();

            return services;
        }
    }
}
