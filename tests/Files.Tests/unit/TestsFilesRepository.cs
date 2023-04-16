using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Text.Json;
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
         DateTime.UtcNow, false, "pdf", "filename");
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
         DateTime.UtcNow, false, "pdf", "filename");
        var chunk2 = new Chunk(idRepeated, Guid.NewGuid(), 1,100000,20000000, "123456789data",
         DateTime.UtcNow, false, "pdf", "filename");

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
         DateTime.UtcNow, false, "pdf", "filename");
        var chunk2 = new Chunk(Guid.NewGuid(), fileId, 1,100000,20000000, "123456789data",
         DateTime.UtcNow, false, "pdf", "filename");

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

           await _sut.UploadTemporalyChunk(chunk, false);
            
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
        InsertChunks(_sut, fileId, chunksToadd);

        string fileBuilt = await _sut.JoinChunksByFileId(fileId);

        Assert.Equal(fileExpected, fileBuilt);
    }
    [Fact]
    public async void With3ChunksAdded_DeleteChunksByFileId_ChunksDeleted()
    {
        int chunksToadd = 3;
        var _sut = new FilesRepository(_filesDb);
        var fileId = Guid.NewGuid();
        InsertChunks(_sut, fileId, chunksToadd);

        _sut.DeleteChunksByFileId(fileId);
        var elements = await _sut.GetChunksByFileIdAsync(fileId);

        Assert.Equal(0, elements.Count);
    }
    private static void InsertChunks(FilesRepository repo, Guid fileId, int n)
    {
        for (var i = 0; i < n; i++)
        {
            var data = $"chunk{i}";
            var chunk = new Chunk(Guid.NewGuid(),fileId, i,100000,20000000, $"chunk{i}",
            DateTime.UtcNow, false, "pdf", "filename");
            Task.FromResult(repo.AddChunkAsync(chunk));
        }
    }

}