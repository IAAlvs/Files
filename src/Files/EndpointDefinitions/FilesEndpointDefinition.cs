using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Files.Interfaces;
using Files.Repositories;
using Files.Services;
using Files.Models;
using Files.Clients;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace Files.AspectDefinitions;

public class FilesEndpointDefinition{
    
    public const string API_VERSION = "v1";
    
    public static void DefineServices(IServiceCollection services)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
        var filesOptionsBuilder = new DbContextOptionsBuilder<FilesDbContext>();
        filesOptionsBuilder.UseNpgsql(config["DB_CONNECTION"]);
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddDbContext<FilesDbContext>(options => options.UseNpgsql(config["DB_CONNECTION"]));
        services.AddScoped<IFilesRepository, FilesRepository>();
        services.AddScoped<IRequestInvoker, RequestInvoker>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IFiles, FilesService>();
    }
    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
         //Build an file with previous chunks
        app.MapPost("/api/"+ API_VERSION+"/files/upload/{fileId}", UploadFile)  
            .WithName("Build and Upload like private File");
        // Upload a public file
        app.MapPost("/api/"+ API_VERSION+"/files/upload-public/{fileId}", UploadPublicFile)  
        .WithName("Build and Upload File");

        app.MapPost("/api/"+ API_VERSION+"/files/chunks", UploadChunks)  
            .WithName("Upload FileChunks");

        app.MapGet("/api/"+ API_VERSION+"/files/{fileId}", GetFileById)  
            .WithName("Get file by id");
    }
    [Authorize(Policy = "FilesPolicy")]
    internal async static Task<IResult> UploadFile(IFiles service, IValidator<string> _validator,
        [FromRoute(Name = "fileId")] string fileId)
    {
        try
        {
            var validation = _validator.Validate(fileId);
            if(!validation.IsValid){
                throw new ValidationException(validation.Errors);
            }
            return await service.UploadFile(Guid.Parse(fileId))
                is { } uploadFilesResponseDto
                ? Results.Ok(uploadFilesResponseDto)
                : Results.NotFound();
        }
        catch (Exception e)
        {
            var error = PrettifyErrorResult(e);
            if (error is null) throw;
            return error;
        }  
    }
    [Authorize(Policy = "FilesPolicy")]
    internal async static Task<IResult> UploadPublicFile(IFiles service, IValidator<string> _validator,
    [FromRoute(Name = "fileId")] string fileId)
    {
        try
        {
            
            var validation = _validator.Validate(fileId);
            if(!validation.IsValid){
                throw new ValidationException(validation.Errors);
            }
            return await service.UploadPublicFile(Guid.Parse(fileId))
                is { } uploadFilesResponseDto
                ? Results.Ok(uploadFilesResponseDto)
                : Results.NotFound();
        }
        catch (Exception e)
        {
            var error = PrettifyErrorResult(e);
            if (error is null) throw;
            return error;
        }  
    }
    [Authorize(Policy = "FilesPolicy")]
    internal async static Task<IResult> UploadChunks(IFiles service, IValidator<UploadChunkRequestDto> _validator,
    JsonNode body)
    {
        try
        {
            var uploadChunkDto = body.Deserialize<UploadChunkRequestDto>(options:new JsonSerializerOptions{
                PropertyNameCaseInsensitive = true
            });
            var validation = _validator.Validate(uploadChunkDto!);
            if(!validation.IsValid){
                throw new ValidationException(validation.Errors);
            }
            var response = await service.UploadChunks(uploadChunkDto!);
            return response is {} uploadChunkResponseDto
                ? Results.Ok(uploadChunkResponseDto)
                : Results.NotFound();
        }
        catch (Exception e)
        {
            var error = PrettifyErrorResult(e);
            if (error is null) throw;
            return error;
        }  
    }
    [Authorize(Policy = "FilesPolicy")]
    internal async static Task<IResult> GetFileById(IFiles service, IValidator<string> _validator,
    [FromRoute(Name = "fileId")] string fileId)
    {
        try
        {
            var validation = _validator.Validate(fileId);
            if(!validation.IsValid){
                throw new ValidationException(validation.Errors);
            }
            return await service.GetFileById(Guid.Parse(fileId))
                is { } GetFileSummaryDto
                ? Results.Ok(GetFileSummaryDto)
                : Results.NotFound();
        }
        catch (Exception e)
        {
            var error = PrettifyErrorResult(e);
            if (error is null) throw;
            return error;
        }  
    }
    private static ErrorResponseDto BuildErrorResponseDto(string errorMessage)
    {
        return new ErrorResponseDto(new List<string> { errorMessage });
    }   
    private static IResult? PrettifyErrorResult(Exception exc) => exc switch
    {
        ValidationException ex => Results.UnprocessableEntity(new { errors = ex.Errors.Select(x => $"{x.PropertyName} {x.ErrorMessage}") }),
        InvalidOperationException => Results.Conflict(BuildErrorResponseDto(exc.Message)),
        ArgumentNullException => Results.NotFound(BuildErrorResponseDto(exc.Message)),
        ArgumentException => Results.UnprocessableEntity(BuildErrorResponseDto(exc.Message)),
        KeyNotFoundException => Results.NotFound(BuildErrorResponseDto(exc.Message)),
        AggregateException => Results.Conflict(BuildErrorResponseDto(exc.Message)),
        ApplicationException => Results.Conflict(BuildErrorResponseDto(exc.Message)),
        FormatException => Results.Conflict(BuildErrorResponseDto(exc.Message)),
        JsonException => Results.BadRequest(BuildErrorResponseDto("Failed Json Parse. Invalid input pattern")),
        _ => null
    };
}