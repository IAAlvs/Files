using Files.Services;
using System.Numerics;

using Xunit;

public class VerifyChunkTests
{
    [Fact]
    public void ChunksByRequest_ReturnsCorrectChunks()
    {
        // Arrange
        var verifyChunk = new VerifyChunk();
        int fileSize = 10000000;  // 10 MB
        int chunkNormalSize = 4096;  // 4 KB

        // Act
        var result = verifyChunk.ChunksByRequest(fileSize, chunkNormalSize);

        // Assert
        Assert.Equal(1, result.iteration); // 10 MB / 6 MB chunks
        Assert.Equal(1536, result.numberOfChunks); // Approximately 6 MB / 4 KB chunks
    }

    [Fact]
    public void BytesToMb_ConvertsBytesToMB()
    {
        // Arrange
        var verifyChunk = new VerifyChunk();
        BigInteger bytes = 10485760;  // 10 MB

        // Act
        var result = verifyChunk.BytesToMb(bytes);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void UrlAvailabilityBasedOnSize_CalculatesMinutes()
    {
        // Arrange
        var verifyChunk = new VerifyChunk();
        int fileSize = 10485760;  // 10 MB

        // Act
        var result = verifyChunk.UrlAvailabilityBasedOnSize(fileSize);

        // Assert
        Assert.Equal(5, result); // 5 + (10 MB / 100 MB)
    }

    [Fact]
    public void CheckDividedChunkErrorSize_ThrowsExceptionForInvalidSize()
    {
        // Arrange
        var verifyChunk = new VerifyChunk();
        int expectedBytesSize = 10000;
        int chunkSize = 11000; // This will exceed the error range

        // Act and Assert
        Assert.Throws<FormatException>(() => verifyChunk.CheckDividedChunkErrorSize(expectedBytesSize, chunkSize));
    }

    [Fact]
    public void CalculateBytesBasedOnBase64_CalculatesBytes()
    {
        // Arrange
        var verifyChunk = new VerifyChunk();
        string base64 = "SGVsbG8gV29ybGQ="; // "Hello World" in Base64

        // Act
        var result = verifyChunk.CalculateBytesBasedOnBase64(base64);

        // Assert
        Assert.Equal(12, result); // Length of "Hello World" in bytes
    }
}
