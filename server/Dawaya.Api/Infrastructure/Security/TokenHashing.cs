using System.Security.Cryptography;
using System.Text;

namespace Dawaya.Api.Infrastructure.Security;

// Shared by refresh tokens now and share-link tokens later (BR-74): raw tokens are never stored.
public static class TokenHashing
{
    public static string NewRawToken(int bytes = 32) =>
        Base64UrlEncode(RandomNumberGenerator.GetBytes(bytes));

    public static string Sha256Hex(string rawToken) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
