using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Portfolio.Application.Photography;

namespace Portfolio.Infrastructure.Photography;

/// <summary>
/// Local-disk implementation of the storage abstraction. Path handling
/// is deliberately defensive: relativePath segments are never trusted
/// to be safe on their own (see the traversal check below).
///
/// The root folder is configurable (Storage:UploadsRootPath) - default
/// dev behavior is unchanged (wwwroot/uploads), but production should
/// point this outside the deployed app's publish folder, since
/// redeploying typically replaces that folder's contents wholesale
/// (see Phase 10 deployment guide).
/// </summary>
public class LocalPhotoStorageService : IPhotoStorageService
{
    private readonly string _uploadsRoot;

    public LocalPhotoStorageService(IHostEnvironment env, IOptions<StorageOptions> storageOptions)
    {
        _uploadsRoot = string.IsNullOrWhiteSpace(storageOptions.Value.UploadsRootPath)
            ? Path.Combine(env.ContentRootPath, "wwwroot", "uploads")
            : storageOptions.Value.UploadsRootPath;
    }

    public async Task<string> SaveVariantAsync(string relativePath, byte[] content, CancellationToken cancellationToken)
    {
        var fullPath = ResolveSafePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
        return "/uploads/" + relativePath.Replace('\\', '/');
    }

    public Task DeleteVariantAsync(string urlPath, CancellationToken cancellationToken)
    {
        var relative = urlPath.StartsWith("/uploads/") ? urlPath["/uploads/".Length..] : urlPath;
        var fullPath = ResolveSafePath(relative);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Resolves relativePath against the uploads root and verifies the
    /// result is still inside it - blocks "../../whatever" path
    /// traversal from ever writing or deleting outside wwwroot/uploads,
    /// regardless of what a caller passes in.
    /// </summary>
    private string ResolveSafePath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_uploadsRoot, relativePath));
        if (!fullPath.StartsWith(Path.GetFullPath(_uploadsRoot), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Resolved path escapes the uploads directory.");
        }
        return fullPath;
    }
}
