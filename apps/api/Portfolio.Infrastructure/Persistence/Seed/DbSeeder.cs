using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Seed;

/// <summary>
/// Dev-only seed data for Phase 4, so the gallery/lightbox has real
/// photos to display before the upload pipeline (Phase 5) or admin
/// CMS (Phase 6) exist. Runs once, only in Development, only if the
/// Photos table is empty - never overwrites real content once the
/// admin app starts creating it.
///
/// EXIF fields below are pulled from the actual source files (Sony
/// A6000, two lenses), not invented - this is what makes the
/// DataLabel device on each photo real rather than decorative.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(PortfolioDbContext dbContext)
    {
        if (dbContext.Photos.Any())
        {
            return;
        }

        var category = new Category
        {
            Name = "Festival & Devotional",
            Slug = "festival-devotional",
        };
        dbContext.Categories.Add(category);

        var album = new Album
        {
            Title = "Ganpati Utsav — Pune, 2023",
            Slug = "ganpati-utsav-pune-2023",
            Description = "Ganesh Chaturthi visarjan night, photographing several of Pune's iconic " +
                "\"manache\" (honored) Ganpati mandals - Kasba Ganapati, Tambdi Jogeshwari, Guruji " +
                "Talim, Shrimant Bhausaheb Rangari, and Shrimant Dagdusheth Halwai - shot handheld " +
                "through the night on a Sony A6000.",
            Category = category,
            DisplayOrder = 0,
            IsPublished = true,
            CreatedAtUtc = new DateTime(2023, 9, 21, 0, 0, 0, DateTimeKind.Utc),
        };
        dbContext.Albums.Add(album);

        const string sonyMake = "Sony";
        const string sonyModel = "ILCE-6000";
        const string zoomLens = "E 55-210mm F4.5-6.3 OSS";
        const string primeLens = "Sigma 16mm F1.4 DC DN";

        var photos = new List<Photo>
        {
            Photo("kasaba-ganapati-01", "Kasba Ganapati", "One of Pune's first-honored Ganpati mandals.", "kasaba-ganapati-01", 6000, 4000, "2023-09-21T02:20:35Z", zoomLens, 202.0m, "1/200s", 2500, 6.3m),
            Photo("kasaba-ganapati-02", "Kasba Ganapati", "One of Pune's first-honored Ganpati mandals.", "kasaba-ganapati-02", 6000, 4000, "2023-09-21T02:20:43Z", zoomLens, 132.0m, "1/200s", 1600, 5.6m),
            Photo("shrimant-bhausaheb-rangari", "Shrimant Bhausaheb Rangari Ganapati", "Wide shot on the 16mm prime, close to the idol.", "shrimant-bhausaheb-rangari", 6000, 4000, "2023-09-21T03:00:44Z", primeLens, 16.0m, "1/160s", 3200, 6.3m),
            Photo("ganpati-06", "Ganpati Mandal", "Telephoto detail shot through the crowd.", "ganpati-06", 4000, 6000, "2023-09-21T03:08:03Z", zoomLens, 210.0m, "1/160s", 2000, 6.3m),
            Photo("ganpati-07", "Ganpati Mandal", "Close crop on the idol's face and ornamentation.", "ganpati-07", 4000, 6000, "2023-09-21T03:17:58Z", zoomLens, 116.0m, "1/320s", 640, 7.1m),
            Photo("shrimant-dagdusheth-ganapati", "Shrimant Dagdusheth Halwai Ganapati", "One of the most visited Ganpati mandals in Pune.", "shrimant-dagdusheth-ganapati", 6000, 4000, "2023-09-21T03:16:26Z", zoomLens, 98.0m, "1/320s", 640, 7.1m),
            Photo("ganpati-08", "Ganpati Mandal", "Mid-telephoto framing of the idol and stage.", "ganpati-08", 4000, 6000, "2023-09-21T03:47:39Z", zoomLens, 111.0m, "1/320s", 400, 5.6m),
            Photo("ganpati-09", "Ganpati Mandal", "Full telephoto reach at 204mm, low light.", "ganpati-09", 4000, 6000, "2023-09-21T04:13:16Z", zoomLens, 204.0m, "1/200s", 3200, 6.3m),
            Photo("guruji-talim", "Guruji Talim Ganapati", "Wide 16mm shot, one of Pune's oldest mandal traditions.", "guruji-talim", 4000, 6000, "2023-09-21T04:51:44Z", primeLens, 16.0m, "1/160s", 200, 1.4m),
            Photo("tambdi-jogeshwari", "Tambdi Jogeshwari Ganapati", "Long telephoto portrait at 89mm.", "tambdi-jogeshwari", 6000, 4000, "2023-09-21T05:01:35Z", zoomLens, 89.0m, "1/320s", 320, 5.0m),
            Photo("ganpati-10", "Ganpati Mandal", "Closing shot of the night, 147mm telephoto.", "ganpati-10", 6000, 4000, "2023-09-21T05:01:25Z", zoomLens, 147.0m, "1/320s", 400, 5.6m),
        };

        for (var i = 0; i < photos.Count; i++)
        {
            photos[i].DisplayOrder = i;
            photos[i].Album = album;
        }

        dbContext.Photos.AddRange(photos);
        await dbContext.SaveChangesAsync();

        Photo Photo(
            string slug, string title, string caption, string folder,
            int width, int height, string takenAtIso, string lens,
            decimal focalLength, string shutter, int iso, decimal aperture) => new()
        {
            Title = title,
            Slug = slug,
            Caption = caption,
            ThumbnailPath = $"/uploads/photos/{folder}/thumb.jpg",
            MediumPath = $"/uploads/photos/{folder}/medium.jpg",
            LargePath = $"/uploads/photos/{folder}/large.jpg",
            Width = width,
            Height = height,
            TakenAtUtc = DateTime.Parse(takenAtIso).ToUniversalTime(),
            CameraMake = sonyMake,
            CameraModel = sonyModel,
            LensModel = lens,
            FocalLengthMm = focalLength,
            ShutterSpeed = shutter,
            Iso = iso,
            Aperture = aperture,
            IsPublished = true,
            UploadedAtUtc = DateTime.UtcNow,
        };
    }
}
