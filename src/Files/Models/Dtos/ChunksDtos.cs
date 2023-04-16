namespace Files.Models;

public record UploadChunkRequestDto(Guid FileId, int Number, string Data, int Size, int FileSize, string Type, string FileName);
