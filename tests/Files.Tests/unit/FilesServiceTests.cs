using Files.Services;
using Files.Models;
using Files.Repositories;
using Files.Interfaces;
using NSubstitute;

public class FilesServiceTests
{
    private readonly IFiles _service;
    private readonly IStorageService _storage;
    private readonly IFilesRepository _repository;

    public FilesServiceTests()
    {
        _storage = Substitute.For<IStorageService>();
        _repository = Substitute.For<IFilesRepository>();
        _repository.GetFileById(Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e47")).Returns(
            new Files.Models.File(Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e47"),
            DateTime.UtcNow, "url", "svg", "filename", "SGVsbG8gV29ybGQh".Count() )
        );
        _repository.GetChunksByFileIdAsync(Arg.Is<Guid>(s =>
            s== Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e30")
        )).Returns(
            new List<Chunk>{
                new Chunk(Guid.NewGuid(), Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e30"), 1, 
                15, 15, "SGVsbG8gV29ybGQh", DateTime.UtcNow, "svg", "filename")
            }
        );
        _repository.JoinChunks(Arg.Is<List<Chunk>>(cs => cs.Any(c => c.FileId
        == Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e30")))).Returns("SGVsbG8gV29ybGQh");
        _repository.GetChunksOrderedByFileIdAsync(

            Arg.Is<Guid>(g => g == Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e30"))
        ).Returns(
            new List<Chunk>{
                new Chunk(Guid.NewGuid(), Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e30"), 0, 
                16, 16, "SGVsbG8gV29ybGQh", DateTime.UtcNow, "svg", "filename")
            }
        );
        _storage.UploadPublicFile("29986eb9-47a2-48ef-8891-da5fc0f71e29", Arg.Any<string>()).Returns(
            "urlfile29986eb9-47a2-48ef-8891-da5fc0f71e29"
        );
        _storage.CheckIfExistsItem("29986eb9-47a2-48ef-8891-da5fc0f71e29").Returns(true);

        _repository.AddFile(Arg.Is<Files.Models.File>(f => 
        f.Id == Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e30"))).Returns(
            Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e30")
        );
        _repository.GetFileById(Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e26")).Returns(
            new Files.Models.File(Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e26"), 
            DateTime.UtcNow, "url", "svg", "filetest", 16)
        );
        _storage.GetFileById("29986eb9-47a2-48ef-8891-da5fc0f71e47").
        Returns("SGVsbG8gV29ybGQh");
        _storage.When(e => e.CheckIfExistsItem(Arg.Is<string>(s => s != "29986eb9-47a2-48ef-8891-da5fc0f71e47"))).Do(
            x => throw new ArgumentException("File does not exist for file id: {id}")
        );
        _repository.UploadTemporalyChunk(Arg.Is<UploadChunkRequestDto>(
            dto => dto.FileId == Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e25")
        )).Returns(true);

        _service = new FilesService(_storage, _repository);
        
    }
    [Fact]
    public async void GetFileById_FileExist_FileGiven()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e47"); 
        // When
        var myFile = await _service.GetFileById(id);
        // Then
        Assert.Equal(id, myFile!.Id);
        Assert.Equal("filename" , myFile!.FileName);
        Assert.Equal("svg", myFile!.FileType);
        Assert.Equal("SGVsbG8gV29ybGQh", myFile!.Data);
        Assert.Equal(16, myFile!.FileSize);
        Assert.Equal("url", myFile!.FileUrl);
    }
    [Fact]
    public async void GetFileById_FileDontExist_FileGiven()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e46"); 
        // When
        var myFile = await _service.GetFileById(id);
        // Then
        Assert.Null(myFile);
    }
    [Fact]
    public async void UploadPublicFile_ChunksExists_Ok()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e30"); 
        IStorageService lStorage = Substitute.For<IStorageService>();
        lStorage.CheckIfExistsItem("29986eb9-47a2-48ef-8891-da5fc0f71e30").Returns(false);
        lStorage.UploadPublicFile("29986eb9-47a2-48ef-8891-da5fc0f71e30", Arg.Any<string>()).Returns(
            "url"
        );
        IFiles lservice = new FilesService(lStorage, _repository);
        // When
        var myFile = await lservice.UploadPublicFile(id);
        // Then
        Assert.Equal(myFile!.Id, id);
        Assert.NotNull(myFile.Url);
    }
    [Fact]
    public async void UploadPublicFile_DontExistChunksExists_ArgumentNullException()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e28"); 
        IStorageService lStorage = Substitute.For<IStorageService>();
        lStorage.CheckIfExistsItem("29986eb9-47a2-48ef-8891-da5fc0f71e28").Returns(false);
        lStorage.UploadPublicFile("29986eb9-47a2-48ef-8891-da5fc0f71e328", Arg.Any<string>()).Returns(
            "url"
        );
        IFiles lservice = new FilesService(lStorage, _repository);
        // When
        // Then
        await Assert.ThrowsAnyAsync<ArgumentNullException>(async() => 
            await lservice.UploadPublicFile(id));
    }
    [Fact]
    public async void UploadPublicFile_AlreadyInFileIdInStorage_ArgumentException()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e27"); 
        IStorageService lStorage = Substitute.For<IStorageService>();
        lStorage.CheckIfExistsItem("29986eb9-47a2-48ef-8891-da5fc0f71e27").Returns(true);
        lStorage.UploadPublicFile("29986eb9-47a2-48ef-8891-da5fc0f71e327", Arg.Any<string>()).Returns(
            "url"
        );
        IFiles lservice = new FilesService(lStorage, _repository);
        // When
        // Then
        await Assert.ThrowsAnyAsync<ArgumentException>(async() => 
            await lservice.UploadPublicFile(id));
    }
    [Fact]
    public async void UploadPublicFile_FileAlreadyInFilesDb_ArgumentException()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e26"); 
        IStorageService lStorage = Substitute.For<IStorageService>();
        lStorage.CheckIfExistsItem("29986eb9-47a2-48ef-8891-da5fc0f71e26").Returns(false);
        lStorage.UploadPublicFile("29986eb9-47a2-48ef-8891-da5fc0f71e326", Arg.Any<string>()).Returns(
            "url"
        );
        IFiles lservice = new FilesService(lStorage, _repository);
        // When
        // Then
        await Assert.ThrowsAnyAsync<ArgumentException>(async() => 
            await lservice.UploadPublicFile(id));
    }
    [Fact]
    public async void UploadPrivateFile_ChunksExists_Ok()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e30"); 
        IStorageService lStorage = Substitute.For<IStorageService>();
        lStorage.CheckIfExistsItem("29986eb9-47a2-48ef-8891-da5fc0f71e30").Returns(false);
        lStorage.UploadFile("29986eb9-47a2-48ef-8891-da5fc0f71e30", Arg.Any<string>()).Returns(
            true
        );
        IFiles lservice = new FilesService(lStorage, _repository);
        // When
        var myFile = await lservice.UploadFile(id);
        // Then
        Assert.Equal(myFile!.Id, id);
    }
    [Fact]
    public async void UploadPrivateFile_DontExistChunksExists_ArgumentNullException()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e28"); 
        IStorageService lStorage = Substitute.For<IStorageService>();
        lStorage.CheckIfExistsItem("29986eb9-47a2-48ef-8891-da5fc0f71e28").Returns(false);
        lStorage.UploadFile("29986eb9-47a2-48ef-8891-da5fc0f71e328", Arg.Any<string>()).Returns(
            true
        );
        IFiles lservice = new FilesService(lStorage, _repository);
        // When
        // Then
        await Assert.ThrowsAnyAsync<ArgumentNullException>(async() => 
            await lservice.UploadFile(id));
    }
    [Fact]
    public async void UploadPrivateFile_AlreadyInFileIdInStorage_ArgumentException()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e27"); 
        IStorageService lStorage = Substitute.For<IStorageService>();
        lStorage.CheckIfExistsItem("29986eb9-47a2-48ef-8891-da5fc0f71e27").Returns(true);
        lStorage.UploadFile("29986eb9-47a2-48ef-8891-da5fc0f71e327", Arg.Any<string>()).Returns(
            true
        );
        IFiles lservice = new FilesService(lStorage, _repository);
        // When
        // Then
        await Assert.ThrowsAnyAsync<ArgumentException>(async() => 
            await lservice.UploadFile(id));
    }
    [Fact]
    public async void UploadPrivateFile_FileAlreadyInFilesDb_ArgumentException()
    {
        // Given
        Guid id = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e26"); 
        IStorageService lStorage = Substitute.For<IStorageService>();
        lStorage.CheckIfExistsItem("29986eb9-47a2-48ef-8891-da5fc0f71e26").Returns(false);
        lStorage.UploadFile("29986eb9-47a2-48ef-8891-da5fc0f71e326", Arg.Any<string>()).Returns(
            true
        );
        IFiles lservice = new FilesService(lStorage, _repository);
        // When
        // Then
        await Assert.ThrowsAnyAsync<ArgumentException>(async() => 
            await lservice.UploadFile(id));
    }
    [Fact]
    public async void UploadChunk_CorrectFormat_SuccessFullAdded()
    {
        // Given
        var fileId = Guid.Parse("29986eb9-47a2-48ef-8891-da5fc0f71e25");
        var uploadChunkDto = new UploadChunkRequestDto(fileId, 1, "SGVsbG8gV29ybGQh", 
        16, 16, "svg", "filenameex");
        // When
        var res = await _service.UploadChunks(uploadChunkDto);
        // Then
        Assert.IsType<UploadChunkResponseDto>(res);
        Assert.Equal("success", res.Message);
    }
    
}