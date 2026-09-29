namespace Dawaya.Api.Infrastructure.Storage;

// CLAUDE.md rule 6: local disk in development, cloud in production. Short-lived signed URLs are
// added when the first file feature arrives (Slice 6/8).
public interface IBlobStorage
{
    Task PutAsync(string key, Stream content, string contentType, CancellationToken ct);
    Task<Stream?> OpenReadAsync(string key, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);
    // Deletes every blob whose key starts with the prefix (e.g. all of a user's files, BR-38).
    Task DeletePrefixAsync(string prefix, CancellationToken ct);
}

public static class BlobKeys
{
    // Every blob that belongs to a user lives under this prefix, so deleting the account removes all of them.
    public static string UserPrefix(string userId) => $"users/{userId}/";
}
