using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Files.AspectDefinitions.Authorization;

public class AuthorizationAspectDefinition
{
    public static void DefineAspect(IServiceCollection services, IConfiguration configuration)
    {
        var domain = $"https://{configuration["AUTH0_DOMAIN"]}/";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = configuration["AUTH0_AUDIENCE"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

        services.AddAuthorization(options =>
        {
            //Change policy
            options.AddPolicy(
                "FilesPolicy", policy => policy.Requirements.Add(new HasScopeRequirement("all:files", domain))
            );
        });

        services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
    }

    public static void ConfigureAspect(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}