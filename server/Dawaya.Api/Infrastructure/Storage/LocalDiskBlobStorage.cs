namespace Dawaya.Api.Infrastructure.Storage;

// Development implementation. Keys are "/"-separated relative paths; anything that could escape the
// root folder (.., absolute paths, backslashes) is rejected.
public sealed class LocalDiskBlobStorage : IBlobStorage
{
    private readonly string _root;

    public LocalDiskBlobStorage(string root)
    {
        _root = Path.GetFullPath(root);
        Directory.CreateDirectory(_root);
    }

    public async Task PutAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        var path = Resolve(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var file = File.Create(path);
        await content.CopyToAsync(file, ct);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken ct)
    {
        var path = Resolve(key);
        return Task.FromResult<Stream?>(File.Exists(path) ? File.OpenRead(path) : null);
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        var path = Resolve(key);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public Task DeletePrefixAsync(string prefix, CancellationToken ct)
    {
        if (!prefix.EndsWith('/')) throw new ArgumentException("Prefix must end with '/'.", nameof(prefix));
        var directory = Resolve(prefix.TrimEnd('/'));
        if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        return Task.CompletedTask;
    }

    private string Resolve(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Contains('\\', StringComparison.Ordinal) || key.StartsWith('/')
            || key.Split('/').Any(part => part is ".." or "."))
        {
            throw new ArgumentException("Invalid blob key.", nameof(key));
        }

        var path = Path.GetFullPath(Path.Combine(_root, key));
        if (!path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new ArgumentException("Invalid blob key.", nameof(key));
        return path;
    }
}
