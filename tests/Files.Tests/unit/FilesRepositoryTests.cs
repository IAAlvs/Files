using Microsoft.EntityFrameworkCore;
using Files.Repositories;
using Files.Models;

public class FilesRepositoryTests
{
    private readonly FilesDbContext _filesDb;
    //Configuration to use sqlite
    public FilesRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FilesDbContext>()
                            .UseSqlite(@"Data Source=./testdb.db")
                            .Options;
        _filesDb = new FilesDbContext(options);
        _filesDb.Database.EnsureDeleted();
        _filesDb.Database.EnsureCreated();
    }
    [Fact]
    public async void WithNewChunk_AddAsync_AddsNewChunk()
    {
        var chunk = new Chunk(Guid.NewGuid(), Guid.NewGuid(), 0,100000,20000000, "123456789data",
         DateTime.UtcNow, "pdf", "filename");
        var countBefore = _filesDb.Chunks.Count();

        var _sut = new FilesRepository(_filesDb);
        await _sut.AddChunkAsync(chunk);

        Assert.Equal(countBefore + 1, _filesDb.Chunks.Count());
    }
    [Fact]
    public async void WithDuplicatedChunk_AddsAsync_Throws()
    {
        var idRepeated = Guid.NewGuid();
        var chunk = new Chunk(idRepeated, Guid.NewGuid(), 0,100000,20000000, "123456789data",
         DateTime.UtcNow, "pdf", "filename");
        var chunk2 = new Chunk(idRepeated, Guid.NewGuid(), 1,100000,20000000, "123456789data",
         DateTime.UtcNow, "pdf", "filename");

        var countBefore = _filesDb.Chunks.Count();

        var _sut = new FilesRepository(_filesDb);
        await _filesDb.Chunks.AddAsync(chunk);
        await _filesDb.SaveChangesAsync();

        await Assert.ThrowsAnyAsync<Exception>(async() => {
            await _filesDb.Chunks.AddAsync(chunk2);
            await _filesDb.SaveChangesAsync();
    });
    }   
    public async void WithSameNumberAndFileId_AddChunkAsync_Throws()
    {
        var fileId = Guid.NewGuid();
        var chunk = new Chunk(Guid.NewGuid(), fileId, 1,100000,20000000, "123456789data",
         DateTime.UtcNow, "pdf", "filename");
        var chunk2 = new Chunk(Guid.NewGuid(), fileId, 1,100000,20000000, "123456789data",
         DateTime.UtcNow, "pdf", "filename");

        var countBefore = _filesDb.Chunks.Count();

        var _sut = new FilesRepository(_filesDb);
        await _filesDb.Chunks.AddAsync(chunk);
        await _filesDb.SaveChangesAsync();

        await Assert.ThrowsAnyAsync<Exception>(async() => await _filesDb.Chunks.AddAsync(chunk2));
    } 

    [Fact]
    public async void WithFileIdFieldNotFound_DeleteAsync_NotThrows()
    {
        var randomFileId = Guid.NewGuid();
        var notExistanceElements = await _filesDb.Chunks.Where(chunk => chunk.FileId.Equals(randomFileId)).ToListAsync();
        _filesDb.Chunks.RemoveRange(notExistanceElements);
    }
    [Fact]
    public async void With10ChunksAdded_UploadTemporalyChunk_LastChunkNumberPropertyIs9()
    {
        int chunksToadd = 10;
        var _sut = new FilesRepository(_filesDb);
        var fileId = Guid.NewGuid();
        int expectedNumberOfChunk = 9;

        for (int i = 0; i < chunksToadd; i++)
        {
            var chunk = new UploadChunkRequestDto(fileId, i, $"chunk{i}", 100, 1000, "pdf", "filename");

           await _sut.UploadTemporalyChunk(chunk);
            
        }
        var totalChunk = await _sut.GetChunksOrderedByFileIdAsync(fileId);
        var lastChunk = totalChunk.Last();
        
        Assert.Equal(expectedNumberOfChunk, lastChunk.Number);
    }
    [Fact]
    public async void With3ChunksAdded_JoinChunksByFileId_FileBuildCorrectly()
    {
        int chunksToadd = 3;
        string fileExpected = "chunk0chunk1chunk2";
        var _sut = new FilesRepository(_filesDb);
        var fileId = Guid.NewGuid();
        InsertChunks(_sut, fileId, chunksToadd, null);

        string fileBuilt = await _sut.JoinChunksByFileId(fileId);

        Assert.Equal(fileExpected, fileBuilt);
    }
    [Fact]
    public async void With3ChunksAdded_DeleteChunksByFileId_ChunksDeleted()
    {
        int chunksToadd = 3;
        var _sut = new FilesRepository(_filesDb);
        var fileId = Guid.NewGuid();
        InsertChunks(_sut, fileId, chunksToadd, null);

        _sut.DeleteChunksByFileId(fileId);
        var elements = await _sut.GetChunksByFileIdAsync(fileId);

        Assert.Equal(0, elements.Count);
    }
    
    [Fact]
    public async void With3ChunksAdded_GetChunksByFileId_ChunksToCount3()
    {
        int chunksToadd = 3;
        var _sut = new FilesRepository(_filesDb);
        var fileId = Guid.NewGuid();
        InsertChunks(_sut, fileId, chunksToadd, null);

        var elements = await _sut.GetChunksByFileIdAsync(fileId);

        Assert.Equal(chunksToadd, elements.Count);
    }
    [Fact]
    public async void With3ChunksAdded_GetChunksOrderedByFileID_FirstNumber0LastNumber2()
    {
        int chunksToadd = 3;
        var _sut = new FilesRepository(_filesDb);
        var fileId = Guid.NewGuid();
        InsertChunks(_sut, fileId, chunksToadd, null);

        var elements = await _sut.GetChunksOrderedByFileIdAsync(fileId);

        Assert.Equal(0, elements.First().Number);
        Assert.Equal(2, elements.Last().Number);
    }
    [Fact]
    public async void FromUploadChunkRequestDto_UploadTemporalyChunk_ChunkAdded()
    {
        var id = Guid.NewGuid();
        var _sut = new FilesRepository(_filesDb);
        var dto = new UploadChunkRequestDto(id, 0, "1234", 4, 40, "pdf", "filename");

        var result = await _sut.UploadTemporalyChunk(dto);
        //chunk added
        Assert.True(result);
    }
    [Fact]
    public async void AfterChunksUploaded_JoinChunksByFileId_FullyStringReturned()
    {
        var id = Guid.NewGuid();
        var _sut = new FilesRepository(_filesDb);
        //Expected to insert to chunks of size 5(10/2) with character n
        InsertChunks(_sut, id, 2, 10);
        var result = await _sut.JoinChunksByFileId(id);
        //chunk added
        Assert.Equal("nnnnnnnnnn", result);
    }
    [Fact]
    public async void AddFile_SuccessAdition(){
        var _sut = new FilesRepository(_filesDb);
        var fileId = Guid.NewGuid();
        var file = new Files.Models.File(fileId, DateTime.UtcNow, "nnnnnnnnnn", "pdf", "filetest",10);

        var fileAdded = await _sut.AddFile(file);

        Assert.Equal(fileId, fileAdded);
    }
    [Fact]
    public async void AddingFile_FileAddedTwice_Exception(){
        var _sut = new FilesRepository(_filesDb);
        var fileId = Guid.NewGuid();
        var file = new Files.Models.File(fileId, DateTime.UtcNow, "url", "pdf", "filetest",10);

        var fileAdded = await _sut.AddFile(file);

        await Assert.ThrowsAnyAsync<DbUpdateException>(async () => await _sut.AddFile(file));
    }
        [Fact]
    public async void FileAdded_GetFileById_fileGotten(){
        var _sut = new FilesRepository(_filesDb);
        var fileId = Guid.NewGuid();
        var uploadDate = DateTime.UtcNow;
        var file = new Files.Models.File(fileId, uploadDate, "nnnnnnnnnn", "pdf", "filetest",10);

        var fileAdded = await _sut.AddFile(file);
        var fileGotten = await _sut.GetFileById(fileId);


        Assert.Equal(fileId ,fileGotten!.Id);
        Assert.Equal(uploadDate, fileGotten!.UploadDate);
        Assert.Equal("nnnnnnnnnn", fileGotten!.Url);
        Assert.Equal("pdf", fileGotten!.Type);
        Assert.Equal("filetest", fileGotten!.Name);
        Assert.Equal(10, fileGotten!.Size);
    }

    private static void InsertChunks(FilesRepository repo, Guid fileId, int n, int? size)
    {
        if(size != null)
        {   
            int mysize = (int)size;
            for (var i = 0; i < n; i++)
            {
                char character = char.Parse("n");
                var data = new string(character, mysize/n);
                var chunk = new Chunk(Guid.NewGuid(),fileId,i, mysize/n,n, data,
                DateTime.UtcNow, "pdf", "filename");
                Task.FromResult(repo.AddChunkAsync(chunk));
            }

        }
        else{
            for (var i = 0; i < n; i++)
            {
                var data = $"chunk{i}";
                var chunk = new Chunk(Guid.NewGuid(),fileId, i,100000,20000000, $"chunk{i}",
                DateTime.UtcNow, "pdf", "filename");
                Task.FromResult(repo.AddChunkAsync(chunk));
            }
        }
    }

}