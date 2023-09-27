using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
/* using FluentValidation.AspNetCore;
using System.Reflection; */
using FluentValidation;
using Files.Interfaces;
using Files.Repositories;
using Files.Services;
using Files.Models;
using Files.Clients;
using Files.AspectDefinitions;
using NSubstitute;

public class FilesApiTests
{
    private readonly TestServer _server;
    public FilesApiTests(){
            _server = new TestServer(new WebHostBuilder()
            .ConfigureServices(services =>
            {
                var config = new ConfigurationBuilder().
                AddJsonFile("appsettings.Development.json").
                AddEnvironmentVariables().Build();
                var filesOptionsBuilder = new DbContextOptionsBuilder<FilesDbContext>();
                filesOptionsBuilder.UseSqlite("DataSource=file::memory:?cache=shared");
                services.AddSingleton<IConfiguration>(config);
                //Policy is required
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("FilesPolicy", policy =>
                    {
                        policy.RequireAssertion(context => true);
                    });
                });
                services.AddSingleton<IValidator<string>, GuidValidator>();
                services.AddSingleton<IValidator<UploadChunkRequestDto>, UploadChunksRequestDtoValidor>();


                var mockStorage = Substitute.For<IStorageService>();
                //MOCKING ITEMS STORAGE SERVICE
                    //Returnt for every call 
                mockStorage.CheckIfExistsItem(Arg.Any<string>()).Returns(false, false, false, false);
                //mockStorage.CheckIfExistsItem("e468e4c5-0b6d-4451-b75a-31f0dd2208e7").Returns(true);
                mockStorage.GetFileById("e468e4c5-0b6d-4451-b75a-31f0dd2208e7").Returns("SVG123");
                mockStorage.UploadFile(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
                mockStorage.UploadPublicFile(Arg.Any<string>(), Arg.Any<string>()).Returns("url");
                services.AddSingleton<IStorageService>(mockStorage);
                services.AddDbContext<FilesDbContext>(opt => opt.UseSqlite("DataSource=file::memory:?cache=shared"));
                services.AddScoped<IFilesRepository, FilesRepository>();
                services.AddScoped<IRequestInvoker, RequestInvoker>();
                services.AddScoped<IFiles, FilesService>();
                services.AddRouting();
                //services.AddControllers();
                var serviceProvider = services.BuildServiceProvider();

            // Ejecuta las migraciones en la base de datos de prueba
                var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FilesDbContext>();
                dbContext.Database.Migrate();
                dbContext.Database.EnsureCreated();
                dbContext.Database.EnsureDeleted();
                
            })
            .Configure(app => 
            {
                app.UseRouting();
                //app.UseAuthentication();
                app.UseAuthorization();
                app.UseEndpoints(appEndpoints => FilesEndpointDefinition.DefineEndpoints(appEndpoints));
                //FilesEndpointDefinition.DefineEndpoints(app.);
            }));
    }
    [Fact]
    public async Task Create_Chunk_ReturnsOk()
    {
        // Arrange
        var filesService = Substitute.For<IFiles>();
        var id = Guid.NewGuid();
        var newChunk = new UploadChunkRequestDto(id , 1, "SVG123", 4, 4, "svg", "filename");
        var newChunkRes = new UploadChunkResponseDto(id , "success");
        filesService.UploadChunks(newChunk).Returns(newChunkRes);

        var client = _server.CreateClient();

        var content = new StringContent(JsonSerializer.Serialize(newChunk), Encoding.UTF8, "application/json");
        // Act
        var response = await client.PostAsync("/api/v1/files/chunks", content);
        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var chunkCreated = JsonSerializer.Deserialize<UploadChunkResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(newChunkRes.Message, chunkCreated!.Message);
    }
    [Fact]
    public async Task UploadFiles_FileId_ReturnsOk()
    {
        // Arrange
        var filesService = Substitute.For<IFiles>();
        var fileId = Guid.NewGuid();
        var newChunk = new UploadChunkRequestDto(fileId , 0, "SVG12332", 6, 6, "svg", "filename");

        var uploadRes = new UploadFileResponseDto(fileId, null, "File uploaded successfully");
        filesService.UploadFile(fileId).Returns(uploadRes);
        var client = _server.CreateClient();
        var chunkContent = new StringContent(JsonSerializer.Serialize(newChunk),
        Encoding.UTF8, "application/json");

        var content = new StringContent("", Encoding.UTF8, "application/json");
        // Act
        //upload chuck according fileId
        var responseChunk = await client.PostAsync("/api/v1/files/chunks", chunkContent);
        responseChunk.EnsureSuccessStatusCode();
        var response = await client.PostAsync($"/api/v1/files/upload/{fileId}", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var fileUploaded = JsonSerializer.Deserialize<UploadFileResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(uploadRes.Id, fileUploaded!.Id);
    }
    [Fact]
    public async Task UploadPublicFiles_FileId_ReturnsOk()
    {
        // Arrange
        var filesService = Substitute.For<IFiles>();
        var fileId = Guid.NewGuid();
        var newChunk = new UploadChunkRequestDto(fileId , 1, "SVG123", 4, 4, "svg", "filename2");

        var uploadRes = new UploadFileResponseDto(fileId, "url", "File uploaded successfully");
        filesService.UploadPublicFile(fileId).Returns(uploadRes);
        var client = _server.CreateClient();
        var chunkContent = new StringContent(JsonSerializer.Serialize(newChunk),
         Encoding.UTF8, "application/json");

        var content = new StringContent("", Encoding.UTF8, "application/json");
        // Act
        //upload chuck according fileId
        var responseChunk = await client.PostAsync("/api/v1/files/chunks", chunkContent);
        responseChunk.EnsureSuccessStatusCode();
        var response = await client.PostAsync($"/api/v1/files/upload-public/{fileId}", content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var fileUploaded = JsonSerializer.Deserialize<UploadFileResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(uploadRes.Id, fileUploaded!.Id);
        Assert.Equal(uploadRes.Url, fileUploaded.Url);
        Assert.Equal(uploadRes.Message, fileUploaded.Message);
    }
    [Fact]
    public async Task UploadFiles_WrongDataChunks_ReturnsConflict()
    {
        // Arrange
        var filesService = Substitute.For<IFiles>();
        var fileId = Guid.NewGuid();
        var newChunk = new UploadChunkRequestDto(fileId , 1, "SVG123", 4, 4, "svg", "filename2");
        var newChunk2 = new UploadChunkRequestDto(fileId , 2, "SVG123", 4, 4, "svg", "filename2");
        var uploadRes = new UploadFileResponseDto(fileId, "url", "File uploaded successfully");
        filesService.UploadFile(fileId).Returns(uploadRes);
        var client = _server.CreateClient();
        var chunkContent = new StringContent(JsonSerializer.Serialize(newChunk),
         Encoding.UTF8, "application/json");
        var chunkContent2 = new StringContent(JsonSerializer.Serialize(newChunk2),
         Encoding.UTF8, "application/json");
        var content = new StringContent("", Encoding.UTF8, "application/json");
        // Act
        //upload chuck according fileId
        var responseChunk = await client.PostAsync("/api/v1/files/chunks", chunkContent);
        var responseChunk2 = await client.PostAsync("/api/v1/files/chunks", chunkContent2);
        responseChunk.EnsureSuccessStatusCode();
        responseChunk2.EnsureSuccessStatusCode();
        var response = await client.PostAsync($"/api/v1/files/upload-public/{fileId}", content);
        var responseContent = await response.Content.ReadAsStringAsync();        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    [Fact]
    public async Task UploadPublicFiles_WrongDataChunks_ReturnsConflict()
    {
        // Arrange
        var filesService = Substitute.For<IFiles>();
        var fileId = Guid.NewGuid();
        var newChunk = new UploadChunkRequestDto(fileId , 1, "SVG123", 4, 4, "svg", "filename2");
        var newChunk2 = new UploadChunkRequestDto(fileId , 2, "SVG123", 4, 4, "svg", "filename2");
        var uploadRes = new UploadFileResponseDto(fileId, "url", "File uploaded successfully");
        filesService.UploadPublicFile(fileId).Returns(uploadRes);
        var client = _server.CreateClient();
        var chunkContent = new StringContent(JsonSerializer.Serialize(newChunk),
         Encoding.UTF8, "application/json");
        var chunkContent2 = new StringContent(JsonSerializer.Serialize(newChunk2),
         Encoding.UTF8, "application/json");
        var content = new StringContent("", Encoding.UTF8, "application/json");
        // Act
        //upload chuck according fileId
        var responseChunk = await client.PostAsync("/api/v1/files/chunks", chunkContent);
        var responseChunk2 = await client.PostAsync("/api/v1/files/chunks", chunkContent2);
        responseChunk.EnsureSuccessStatusCode();
        responseChunk2.EnsureSuccessStatusCode();
        var response = await client.PostAsync($"/api/v1/files/upload-public/{fileId}", content);
        var responseContent = await response.Content.ReadAsStringAsync();        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    [Fact]
    public async Task UploadPrivateFiles_FileIdWithOutChunks_ReturnsConflict()
    {
        // Arrange
        var filesService = Substitute.For<IFiles>();
        var fileId = Guid.NewGuid();
        var client = _server.CreateClient();

        var content = new StringContent("", Encoding.UTF8, "application/json");
        // Act
        //upload chuck according fileId
        var response = await client.PostAsync($"/api/v1/files/upload/{fileId}", content);        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task UploadPublicFiles_FileIdWithOutChunks_ReturnsConflict()
    {
        // Arrange
        var filesService = Substitute.For<IFiles>();
        var fileId = Guid.NewGuid();
        var client = _server.CreateClient();

        var content = new StringContent("", Encoding.UTF8, "application/json");
        // Act
        //upload chuck according fileId
        var response = await client.PostAsync($"/api/v1/files/upload-public/{fileId}", content);        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task GetFileById_FileId_ReturnsFile()
    {
        // Arrange
        var filesService = Substitute.For<IFiles>();
        var fileId = Guid.Parse("e468e4c5-0b6d-4451-b75a-31f0dd2208e7");
        var newChunk = new UploadChunkRequestDto(fileId , 1, "SVG123", 4, 4, "svg", "filename2");
        var uploadRes = new UploadFileResponseDto(fileId, "url", "File uploaded successfully");
        filesService.UploadPublicFile(fileId).Returns(uploadRes);
        var client = _server.CreateClient();
        var chunkContent = new StringContent(JsonSerializer.Serialize(newChunk),
         Encoding.UTF8, "application/json");
        var content = new StringContent("", Encoding.UTF8, "application/json");
        // Act
        //upload chuck according fileId
        var responseChunk = await client.PostAsync("/api/v1/files/chunks", chunkContent);
        responseChunk.EnsureSuccessStatusCode();
        var responseUploadFile = await client.PostAsync($"/api/v1/files/upload-public/{fileId}", content);
        responseUploadFile.EnsureSuccessStatusCode();
        var responseGetFile = await client.GetAsync($"/api/v1/files/{fileId}");
        var responseContent = await responseGetFile.Content.ReadAsStringAsync();
        var fileGotten = JsonSerializer.Deserialize<GetFileSummaryDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        // Assert
        Assert.Equal(HttpStatusCode.OK, responseGetFile.StatusCode);
        Assert.Equal(uploadRes.Id, fileGotten!.Id);
        Assert.Equal(uploadRes.Url, fileGotten.FileUrl);
    }

    [Fact]
    public async Task GetFileById_NotFountFile_ReturnsNotFound()
    {
        // Arrange
        var fileId = Guid.Parse("e468e4c5-0b6d-4451-b75a-31f0dd2208e6");
        var client = _server.CreateClient();
        // Act
        var responseGetFile = await client.GetAsync($"/api/v1/files/{fileId}");
        var responseContent = await responseGetFile.Content.ReadAsStringAsync();
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, responseGetFile.StatusCode);
    }
}

