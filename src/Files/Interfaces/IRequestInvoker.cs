namespace Files.Interfaces;
public interface IRequestInvoker 
{
    Task<HttpResponseMessage> MakeRequest(HttpRequestMessage request);
}