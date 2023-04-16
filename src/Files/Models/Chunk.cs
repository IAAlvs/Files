using Files.Models;
public class Chunk
{
    public Chunk(Guid id, Guid fileId, int number, int size, int fileSize, string data, DateTime uploadDate, bool publicFile, string type, string filename)
    {
        Id = id;
        FileId = fileId;
        Number = number;
        Size = size;
        FileSize = fileSize;
        Data = data;
        UploadDate = uploadDate;
        PublicFile = publicFile;
        Type = type;
        Filename = filename;
    }

    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public int Number { get; set; }
    public int Size { get; set; }
    public int FileSize { get; set; }
    public string Data { get; set; }
    public DateTime UploadDate { get; set; } 
    public bool PublicFile { get; set; }
    public string Type { get; set; }
    public string Filename { get; set;}


    public static Chunk CreateFromDto(UploadChunkRequestDto dto, bool publicFile){
        return new Chunk(Guid.NewGuid(), dto.FileId, dto.Number, dto.Size, dto.FileSize, dto.Data, DateTime.UtcNow, publicFile, dto.Type, dto.FileName);
    }
}