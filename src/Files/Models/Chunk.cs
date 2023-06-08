using Files.Models;
public class Chunk
{
    public Chunk(Guid id, Guid fileId, int number, int size, int fileSize, string data, DateTime uploadDate, string type, string filename)
    {
        Id = id;
        FileId = fileId;
        Number = number;
        Size = size;
        FileSize = fileSize;
        Data = data;
        UploadDate = uploadDate;
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


    public static Chunk CreateFromDto(UploadChunkRequestDto dto){
        return new Chunk(Guid.NewGuid(), dto.FileId, dto.Number, dto.Size, dto.FileSize, dto.Data, DateTime.UtcNow, dto.Type, dto.FileName);
    }
}