namespace Files.Models;
public record GetFileSummaryDto(
    Guid Id,
    string FileName,
    string FileType,
    string Data,
    int FileSize,
    string? FileUrl);
public record UploadChunkResponseDto(
    Guid Id,
    string? Message
);
/* public record UploadChunkRequestDto(
    Guid FileId, 
    string FileName, 
    string Data, 
    string FileType, 
    int Size); */
public record UploadFileResponseDto(
    Guid Id,
    string? Url,
    string? Message
); 