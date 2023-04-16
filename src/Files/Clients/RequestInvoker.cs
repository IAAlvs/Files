using Files.Interfaces;
namespace Files.Clients;

public class RequestInvoker : IRequestInvoker
{
    private static readonly HttpClient Client;

    static RequestInvoker()
    {
        Client = new();
    }

    public async Task<HttpResponseMessage> MakeRequest(HttpRequestMessage request) => await Client.SendAsync(request);
}