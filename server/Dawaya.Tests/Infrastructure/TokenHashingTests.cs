using Dawaya.Api.Infrastructure.Security;

namespace Dawaya.Tests.Infrastructure;

public sealed class TokenHashingTests
{
    [Fact]
    public void Raw_tokens_are_random_url_safe_and_long()
    {
        var tokens = Enumerable.Range(0, 100).Select(_ => TokenHashing.NewRawToken()).ToList();

        Assert.Equal(100, tokens.Distinct().Count());
        Assert.All(tokens, t =>
        {
            Assert.True(t.Length >= 43);
            Assert.Matches("^[A-Za-z0-9_-]+$", t);
        });
    }

    [Fact]
    public void Hash_is_stable_lowercase_hex_and_differs_from_the_token()
    {
        var token = TokenHashing.NewRawToken();

        var hash = TokenHashing.Sha256Hex(token);

        Assert.Equal(hash, TokenHashing.Sha256Hex(token));
        Assert.Matches("^[0-9a-f]{64}$", hash);
        Assert.NotEqual(TokenHashing.Sha256Hex(token + "x"), hash);
    }
}
