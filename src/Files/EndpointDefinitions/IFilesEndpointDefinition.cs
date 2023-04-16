namespace Files.EndpointDefinitions;

public interface IFilesEndpointDefinition
{
    void DefineEndpoints(WebApplication app);
    void DefineServices(IServiceCollection services);
    
}