using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace First;

// siehe Generated clients: https://github.com/reactiveui/refit
//* https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-9.0
public class PaperlessClient
{
    private readonly HttpClient _httpClient;
    public PaperlessClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
    }
    public async Task<string> GetCustomFieldsAsync() => await _httpClient.GetStringAsync("custom_fields/");
}