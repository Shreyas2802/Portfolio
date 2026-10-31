# Photo storage: local disk or Azure Blob Storage

Uploaded photos (three resized versions per photo) go through `IPhotoStorageService`.
Two implementations exist, chosen by one setting:

| `Storage:Provider` | Class | Photos served from |
|---|---|---|
| `Local` (default) | `LocalPhotoStorageService` | the API itself, under `/uploads/...` |
| `AzureBlob` | `AzureBlobPhotoStorageService` | the storage account (or a CDN) directly |

Nothing else changes between the two: the upload service, controllers and frontend
only ever see the URL the storage service returns.

## How it works

- **Blob names mirror the local layout:** `photos/{slug}/thumb.jpg`, `medium.jpg`, `large.jpg`.
- **Uploads** set `Content-Type: image/jpeg` and `Cache-Control: public, max-age=31536000, immutable`
  (a photo's files never change in place; a re-upload gets a new slug).
- **Existing photos keep working.** Rows saved before the switch still have `/uploads/...`
  paths and are still served from disk. Deleting one of those goes to the local provider.
- **Startup** (only when the provider is `AzureBlob`):
  - fails immediately with a clear message if the connection string is missing,
  - creates the container with public read access for individual blobs (not listing),
  - applies a read-only CORS rule for `Storage:AzureBlob:CorsAllowedOrigins`.
- **Security headers:** the storage (or CDN) origin is added to the CSP `img-src`, and
  nothing else is.
- **CORS** is needed because the admin "set as hero" feature draws the photo onto a
  canvas to extract its colour palette, and browsers block reading pixels from another
  origin without it.

## Run it locally with Azurite

[Azurite](https://learn.microsoft.com/azure/storage/common/storage-use-azurite) is
Microsoft's local Azure Storage emulator. No Azure account is needed.

```bash
# 1. Start Azurite (pick one). Visual Studio 2022 can also start it for you.
npx azurite-blob --location .azurite --skipApiVersionCheck
# or
docker run -p 10000:10000 mcr.microsoft.com/azure-storage/azurite azurite-blob --blobHost 0.0.0.0 --skipApiVersionCheck

# 2. Point the API at it (stored outside the repo via user secrets)
cd apps/api/Portfolio.Api
dotnet user-secrets set "Storage:Provider" "AzureBlob"
dotnet user-secrets set "Storage:AzureBlob:ConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets set "Storage:AzureBlob:CorsAllowedOrigins:0" "http://localhost:5173"

# 3. Run the API and upload a photo from /admin/photos
dotnet run
```

New uploads now return URLs like
`http://127.0.0.1:10000/devstoreaccount1/photos/photos/{slug}/large.jpg`.
To switch back, remove the `Storage:Provider` secret
(`dotnet user-secrets remove "Storage:Provider"`).

## Use a real Azure storage account

1. In the Azure portal, create a **Storage account** (Standard, LRS is enough).
2. Under **Settings → Configuration**, turn on **Allow Blob anonymous access**.
   New accounts have it off. Without it the container is created private and the API
   logs an error explaining what to change.
3. Copy the connection string from **Security + networking → Access keys**.
4. Set these as environment variables on the server (e.g. in `web.config`, see
   `web.config.example`). Never put the connection string in `appsettings.json`.

   ```
   Storage__Provider=AzureBlob
   Storage__AzureBlob__ConnectionString=<connection string>
   Storage__AzureBlob__ContainerName=photos
   Storage__AzureBlob__CorsAllowedOrigins__0=https://portfolio.vistrut.com
   ```

5. Optional: put Azure Front Door / CDN in front and set
   `Storage__AzureBlob__PublicBaseUrl` to the CDN URL.

## Moving existing photos to Blob (optional)

Existing photos keep working from disk, so this isn't required. To move them anyway:

```bash
# Copy the files (same folder layout)
azcopy copy "C:\path\to\uploads\photos" "https://<account>.blob.core.windows.net/photos?<SAS>" --recursive
```

```sql
-- Point the rows at the blob URLs
DECLARE @base nvarchar(200) = 'https://<account>.blob.core.windows.net/photos/';
UPDATE Photos SET
  ThumbnailPath = REPLACE(ThumbnailPath, '/uploads/', @base),
  MediumPath    = REPLACE(MediumPath,    '/uploads/', @base),
  LargePath     = REPLACE(LargePath,     '/uploads/', @base)
WHERE ThumbnailPath LIKE '/uploads/%';
```

## Tests

- `Portfolio.Tests.Unit/BlobPathHelperTests.cs` covers path normalisation, traversal
  rejection, URL ↔ blob-name round trips and CSP origin resolution, with no Azure needed.
- `Portfolio.Tests.Integration/StorageProviderTests.cs` checks that `Local` is the default
  and that the CSP stays locked to `'self'`. Its `AzureBlobStorageTests` uploads,
  downloads (anonymously, with CORS) and deletes a real blob in Azurite. CI starts
  Azurite for it; locally it only runs when `AZURITE_CONNECTION_STRING` is set.
