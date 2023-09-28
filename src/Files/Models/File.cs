namespace Files.Models;

//Model for File entity

public class File
{
    public Guid Id { get; set; }
    public DateTime UploadDate { get; set;}
    public string Type { get; set; }
    public string Name { get; set; }
    public int Size { get; set;}

    public string? Url { get; set; }
    public File(Guid id, DateTime uploadDate, string url, string type, string name, int size)
    {
        Id = id;
        UploadDate = uploadDate;
        Url = url;
        Type = type;
        Name = name;
        Size = size;
    }

    public static File CreateFromDto(string fileUrl, string fileType, string fileName, int size) =>
    new File(Guid.NewGuid(), DateTime.Now, fileUrl, type: fileType, name: fileName, size: size);
   /* DATA IS NOT SAVED IN DB, IS SAVED IN CLOUD STORAGE */
    public GetFileSummaryDto ToSummaryDto(string data) => new(this.Id, this.Name, this.Type, data, this.Size, this.Url);
    public GetFileSummaryDto ToSummaryUrlDto(string url) => new(this.Id, this.Name, this.Type, "", this.Size, url);
    public UploadFileResponseDto ToUploadFileResponseDto(string message) => new(this.Id, this.Url, message);
}