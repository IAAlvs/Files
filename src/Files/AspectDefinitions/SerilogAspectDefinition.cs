using Serilog;
namespace Files.AspectDefinitions;

public class SerilogAspectDefinition
{
    public static void DefineAspect(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, lc) => lc
            .WriteTo.Console()
            .ReadFrom.Configuration(ctx.Configuration));
    }

    public static void ConfigureAspect(WebApplication app)
    {
        app.UseSerilogRequestLogging();
    }
}