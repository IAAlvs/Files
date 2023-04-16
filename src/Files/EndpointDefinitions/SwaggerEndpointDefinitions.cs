using Microsoft.OpenApi.Models;

namespace Files.AspectDefinitions;


public class SwaggerEndpointDefinition
{
    public const string apiVersion = "v1";

    public static void DefineServices(IServiceCollection services)
    {
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc(apiVersion, new OpenApiInfo()
            {
                Description =
                    "Service Files.",
                Title = "Files Api",
                Version = apiVersion,
                Contact = new OpenApiContact()
                {
                    Name = "IA TEAM",
                    Email = "isaacalf.alvs@gmail.com"
                }
            });
            x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            x.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
        });
    }

    public static void DefineEndpoints(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) return;
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api/"+apiVersion+"/files/swagger/{documentname}/swagger.json";
        });
        app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"{apiVersion}/swagger.json", "Files API V1");
                options.RoutePrefix = $"api/{apiVersion}/files/swagger";
            }
        );
    }
    
}