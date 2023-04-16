using Files.Models;
namespace Files.Interfaces;

public interface IFiles
{
    Task<GetFileSummaryDto?> GetFileById(Guid Id); 
    Task<UploadChunkResponseDto> UploadPublicChunck(UploadChunkRequestDto chunkRequestDto);
    Task<UploadChunkResponseDto> UploadPrivateChunck(UploadChunkRequestDto chunkRequestDto);
    Task<UploadFileResponseDto?> UploadFile(Guid Id);
}