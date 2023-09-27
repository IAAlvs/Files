namespace Files.Models;
using Amazon.S3.Model;

public record UploadChunkRequestDto(Guid FileId, int Number, string Data, int Size, int FileSize, string Type, string FileName);
public record FileInfoBasedOnCHunks(Guid FileId, int Number, int Size, int FileSize, string Type, string FileName);
public record ChunksByRequestDto(int iteration, int numberOfChunks);

public class ChunkedUploadDto
{
    public string Base64Chunk { get; set; }
    public string FileId { get; set; }
    public int TotalChunks { get; set; }
    public int PartNumber { get; set; }
/*     public ChunkedUploadDto(string base64Chunk, string fileId, int totalChunks, int partNumber)
    {
        base64Chunk = Base64Chunk;
        fileId = FileId;
        totalChunks = TotalChunks;
        partNumber = PartNumber;
    } */
    public void Deconstruct(out string base64Chunk, out string fileId, out int totalChunks, out int partNumber)
    {
        base64Chunk = Base64Chunk;
        fileId = FileId;
        totalChunks = TotalChunks;
        partNumber = PartNumber;
    }
}

public class ChunkedUploadDtoAws : ChunkedUploadDto
{
    public List<PartETag> Etags { get; set; }
    public string UploadId { get; set; }
    public ChunkedUploadDtoAws(){
    }
    public ChunkedUploadDtoAws(string base64Chunk, string fileId, int totalChunks, int partNumber, List<PartETag> etags, string uploadId)
    {
        base64Chunk = Base64Chunk;
        fileId = FileId;
        totalChunks = TotalChunks;
        partNumber = PartNumber;
        etags = Etags;
        uploadId = UploadId;
    }
    public void Deconstruct(out string base64Chunk, out string fileId, out int totalChunks, out int partNumber, out List<PartETag> etags, out string uploadId)
    {
        base64Chunk = Base64Chunk;
        fileId = FileId;
        totalChunks = TotalChunks;
        partNumber = PartNumber;
        etags = Etags;
        uploadId = UploadId;
    }
}

public record ChunkedUploadRes();

public record ChunkedUploadResAws(string UploadId, List<PartETag> ETags) : ChunkedUploadRes();
