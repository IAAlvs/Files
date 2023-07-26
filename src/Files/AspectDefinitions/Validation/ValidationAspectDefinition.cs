using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;


namespace Files.AspectDefinitions;

public class ValidationAspectDefinition
{
    public static void DefineAspect(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddFluentValidation(cfg =>
        {
            cfg.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        });
    }

    public static void ConfigureAspect(WebApplication app)
    {
        throw new NotImplementedException();
    }
}