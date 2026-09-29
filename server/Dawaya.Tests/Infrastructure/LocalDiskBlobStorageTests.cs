using System.Text;
using Dawaya.Api.Infrastructure.Storage;

namespace Dawaya.Tests.Infrastructure;

public sealed class LocalDiskBlobStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "dawaya-blob-tests", Guid.NewGuid().ToString("N"));
    private readonly LocalDiskBlobStorage _storage;

    public LocalDiskBlobStorageTests() => _storage = new LocalDiskBlobStorage(_root);

    [Fact]
    public async Task Put_read_and_delete_a_blob()
    {
        await Put("users/u1/a.txt", "hello");

        await using (var read = await _storage.OpenReadAsync("users/u1/a.txt", CancellationToken.None))
            Assert.Equal("hello", await new StreamReader(read!).ReadToEndAsync());

        await _storage.DeleteAsync("users/u1/a.txt", CancellationToken.None);
        Assert.Null(await _storage.OpenReadAsync("users/u1/a.txt", CancellationToken.None));
    }

    [Fact]
    public async Task Delete_prefix_removes_only_that_users_files()
    {
        await Put("users/u1/records/a.pdf", "a");
        await Put("users/u1/scans/b.jpg", "b");
        await Put("users/u10/records/c.pdf", "c");

        await _storage.DeletePrefixAsync(BlobKeys.UserPrefix("u1"), CancellationToken.None);

        Assert.Null(await _storage.OpenReadAsync("users/u1/records/a.pdf", CancellationToken.None));
        Assert.Null(await _storage.OpenReadAsync("users/u1/scans/b.jpg", CancellationToken.None));
        await using var other = await _storage.OpenReadAsync("users/u10/records/c.pdf", CancellationToken.None);
        Assert.NotNull(other);
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("users/../../outside.txt")]
    [InlineData("/etc/passwd")]
    [InlineData("users\\u1\\a.txt")]
    [InlineData("users/./a.txt")]
    [InlineData("")]
    public async Task Keys_that_could_escape_the_root_are_rejected(string key)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Put(key, "x"));
    }

    [Fact]
    public async Task Delete_prefix_requires_a_trailing_slash()
    {
        // "users/u1" without a slash could also match "users/u10".
        await Assert.ThrowsAsync<ArgumentException>(() => _storage.DeletePrefixAsync("users/u1", CancellationToken.None));
    }

    private async Task Put(string key, string text)
    {
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes(text));
        await _storage.PutAsync(key, content, "text/plain", CancellationToken.None);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
