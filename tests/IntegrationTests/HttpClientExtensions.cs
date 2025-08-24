using System.Net.Http;
using System.Net.Http.Headers;

namespace IntegrationTests;

internal static class HttpClientExtensions
{
    internal static void UseJwt(this HttpClient client, string jwt)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    }
}
