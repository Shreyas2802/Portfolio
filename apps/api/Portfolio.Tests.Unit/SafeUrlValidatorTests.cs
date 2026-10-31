using Portfolio.Application.Validation;
using Xunit;

namespace Portfolio.Tests.Unit;

public class SafeUrlValidatorTests
{
    [Theory]
    [InlineData("https://github.com/shreyasgune")]
    [InlineData("http://example.com")]
    [InlineData("https://example.com/path?query=1")]
    public void IsSafe_ValidHttpOrHttpsUrl_ReturnsTrue(string url)
    {
        Assert.True(SafeUrlValidator.IsSafe(url));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsSafe_EmptyOrNull_ReturnsTrue(string? url)
    {
        // Both GithubUrl and LiveUrl are optional fields - "not provided"
        // is a valid state, not a validation failure.
        Assert.True(SafeUrlValidator.IsSafe(url));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://example.com")]
    [InlineData("not a url at all")]
    [InlineData("//example.com")]
    public void IsSafe_UnsafeOrNonHttpScheme_ReturnsFalse(string url)
    {
        Assert.False(SafeUrlValidator.IsSafe(url));
    }
}
