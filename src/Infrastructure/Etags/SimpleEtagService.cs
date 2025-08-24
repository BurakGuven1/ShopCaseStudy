using System.Security.Cryptography;
using System.Text;
using Application.Abstractions;

namespace Infrastructure.Etags;

public class SimpleEtagService : IEtagService
{
    public string ComputeForProductsList(string filterKey, DateTimeOffset? latestUpdated, int totalCount)
    {
        var payload = $"{filterKey}|{latestUpdated?.UtcTicks ?? 0}|{totalCount}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }
}
