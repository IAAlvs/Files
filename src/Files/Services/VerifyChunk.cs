using Files.Models;
using System.Numerics;
namespace Files.Services;
public interface IVerifyChunk{
    ChunksByRequestDto ChunksByRequest(int fileSize, int chunkNormalSize);
    int BytesToMb(BigInteger bytes);
    int UrlAvailabilityBasedOnSize(int fileSize);
    void CheckDividedChunkErrorSize(int expectedBytesSize, int chunkSize);
    int CalculateBytesBasedOnBase64 (string base64);

};

public  class VerifyChunk : IVerifyChunk
{
    public VerifyChunk(){}

    public ChunksByRequestDto ChunksByRequest(int fileSize, int chunkNormalSize){
        //Normal size is the size of all chunk except 1 in the most of times
        const int CHUNK_MIN_BYTES_SIZE = 6*1024*1024; //MB
        var iteration = Convert.ToInt32(fileSize/CHUNK_MIN_BYTES_SIZE);
        var numberOfChunks = Convert.ToInt32(Math.Round((double)(CHUNK_MIN_BYTES_SIZE/chunkNormalSize)));
        //Total chunks most of time will leave a remainer
        return new ChunksByRequestDto(iteration, numberOfChunks);  
    }
    public int BytesToMb(BigInteger bytes) => (int)(bytes / 1024 / 1024);

    public int UrlAvailabilityBasedOnSize(int fileSize)
    {
        var fileMbSize = BytesToMb(new BigInteger(fileSize));
        var temporalyUrlMinutes = 5 + (int)Math.Round((double)(fileMbSize / 100));
        return temporalyUrlMinutes;
    }
    public void CheckDividedChunkErrorSize(int expectedBytesSize, int chunkSize){
        var errorRange = ((expectedBytesSize*0.0001)<2)?2:expectedBytesSize*0.0001;
        if (Math.Abs(expectedBytesSize - chunkSize) > errorRange || Math.Abs(expectedBytesSize - chunkSize) > errorRange )
            throw new FormatException("Chunk size not according current chunk length");
    }

    public int CalculateBytesBasedOnBase64(string base64) => (int)(base64.Length/4)*3;

}