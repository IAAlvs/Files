using FluentValidation;
using Files.Interfaces;
using FluentValidation.AspNetCore;

namespace Files.AspectDefinitions;

public class ValidationAspectDefinition
{
    [Obsolete]
    public static void DefineAspect(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFluentValidation(fv =>
        {
            fv.RegisterValidatorsFromAssemblyContaining<IFilesApiAssemblyMarker>();
            fv.DisableDataAnnotationsValidation = true;
        });
    }

    public static void ConfigureAspect(WebApplication app)
    {
        throw new NotImplementedException();
    }
}