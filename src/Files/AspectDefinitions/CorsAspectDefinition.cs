namespace Files.AspectDefinitions;

public class CorsAspectDefinition
{
    private const string MyAllowSpecificOrigins = "ApiCorsPolicy";

    public static void DefineAspect(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options => options.AddPolicy(MyAllowSpecificOrigins,
            builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }));
    }

    public static void ConfigureAspect(WebApplication app)
    {
        app.UseCors(MyAllowSpecificOrigins);
    }
}