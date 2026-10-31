using Portfolio.Infrastructure.Photography;
using Xunit;

namespace Portfolio.Tests.Unit;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Ganpati Utsav — Pune, 2023", "ganpati-utsav-pune-2023")]
    [InlineData("Hello World", "hello-world")]
    [InlineData("  Leading and trailing spaces  ", "leading-and-trailing-spaces")]
    [InlineData("Multiple---Dashes", "multiple-dashes")]
    [InlineData("Café à la mode", "caf-la-mode")]
    [InlineData("UPPERCASE", "uppercase")]
    [InlineData("123 Numbers 456", "123-numbers-456")]
    public void Slugify_ProducesExpectedUrlSafeSlug(string input, string expected)
    {
        Assert.Equal(expected, SlugHelper.Slugify(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    [InlineData("---")]
    public void Slugify_OnInputWithNoAlphanumericContent_ReturnsEmptyString(string input)
    {
        // The caller (AlbumManagementService, PhotoUploadService, etc.) is
        // responsible for falling back to a default like "album" or
        // "photo" when this returns empty - SlugHelper itself has no
        // opinion on what a sensible default title looks like.
        Assert.Equal(string.Empty, SlugHelper.Slugify(input));
    }
}
