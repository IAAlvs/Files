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
namespace Files.AspectDefinitions;

public class FilesEndpointDefinition{
    
    public const string API_VERSION = "v1";
    
    public static void DefineServices(IServiceCollection services)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
        var filesOptionsBuilder = new DbContextOptionsBuilder<FilesDbContext>();
        filesOptionsBuilder.UseNpgsql(config["DB_CONNECTION"]);

        services.AddDatabaseDeveloperPageExceptionFilter();
        //services.AddSingleton<ILogger>
        services.AddDbContext<FilesDbContext>(options => options.UseNpgsql(config["DB_CONNECTION"]));
        services.AddScoped<IFilesRepository, FilesRepository>();
        services.AddScoped<IRequestInvoker, RequestInvoker>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IFiles, FilesService>();
        //services.addHttpClient();
    }
    public static void DefineEndpoints(WebApplication app)
    {
        //Build an file with previous chunks
        app.MapPost("/api/"+ API_VERSION+"/files/upload/{fileId}", UploadFile)  
            .WithName("Build and Upload File");
        // Upload a public chunks
        app.MapPost("/api/"+ API_VERSION+"/files/chunks", UploadPublicChunck)  
            .WithName("Upload Public chunk");

        app.MapPost("/api/"+ API_VERSION+"/files/chunks-private", UploadChunck)  
            .WithName("Upload Private chunk");

        app.MapGet("/api/"+ API_VERSION+"/files/{fileId}", GetFileById)  
            .WithName("Get file by id");
    }
    internal async static Task<IResult> UploadFile(IFiles service,
        [FromRoute(Name = "fileId")] Guid fileId)
    {
        try
        {
            return await service.UploadFile(fileId)
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
    internal async static Task<IResult> UploadPublicChunck(IFiles service,
        UploadChunkRequestDto uploadChunkDto)
    {
        try
        {
            return await service.UploadPublicChunck(uploadChunkDto)
                is { } uploadChunkResponseDto
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
    internal async static Task<IResult> UploadChunck(IFiles service,
        UploadChunkRequestDto uploadChunkDto)
    {
        try
        {
            return await service.UploadPrivateChunck(uploadChunkDto)
                is { } uploadChunkResponseDto
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
    internal async static Task<IResult> GetFileById(IFiles service,
    [FromRoute(Name = "fileId")] Guid fileId)
    {
        try
        {
            return await service.GetFileById(fileId)
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

    private static IResult? PrettifyErrorResult(Exception exc)
    {
        return exc switch
        {
            (ValidationException ex) => Results.UnprocessableEntity(
                new {errors = ex.Errors.Select(x => x.ErrorMessage)}),
            (InvalidOperationException) => Results.Conflict(exc.Message),
            (ArgumentException) => Results.UnprocessableEntity(exc.Message),
            (KeyNotFoundException) => Results.NotFound(exc.Message),
            (AggregateException) => Results.Conflict(exc.Message),
            //(DuplicateNameException) => Results.Conflict(exc.Message),
            (ApplicationException) => Results.Conflict(exc.Message),
            (FormatException) => Results.Problem(exc.Message),
            _ => null
        };
    }
}