using Files.AspectDefinitions;
using Files.AspectDefinitions.Authorization;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Starting up");
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    CorsAspectDefinition.DefineAspect(builder.Services, builder.Configuration);
    AuthorizationAspectDefinition.DefineAspect(builder.Services, builder.Configuration);
    ValidationAspectDefinition.DefineAspect(builder.Services, builder.Configuration);
    SerilogAspectDefinition.DefineAspect(builder);
    SwaggerEndpointDefinition.DefineServices(builder.Services);
    FilesEndpointDefinition.DefineServices(builder.Services);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    CorsAspectDefinition.ConfigureAspect(app);
    AuthorizationAspectDefinition.ConfigureAspect(app);
    SerilogAspectDefinition.ConfigureAspect(app);
    SwaggerEndpointDefinition.DefineEndpoints(app);
    FilesEndpointDefinition.DefineEndpoints(app);

    app.Run();
}
catch (Exception ex)
{
    var type = ex.GetType().Name;
    if (type.Equals("HostAbortedException", StringComparison.Ordinal)) throw;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal)) throw;

    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

