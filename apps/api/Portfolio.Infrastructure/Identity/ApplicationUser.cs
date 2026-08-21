using Microsoft.AspNetCore.Identity;

namespace Portfolio.Infrastructure.Identity;

/// <summary>
/// Lives in Infrastructure rather than Domain, unlike every other
/// entity in this codebase - a deliberate exception. ApplicationUser
/// must inherit IdentityUser to get ASP.NET Core Identity's built-in
/// password hashing, lockout, and EF mapping, which means it's
/// inherently coupled to a framework/library the Domain project is
/// otherwise kept free of (see Domain.csproj: "no dependencies on
/// anything else in the solution or on any framework"). Putting a
/// framework-coupled type there to satisfy a naming convention would
/// be worse than the honest inconsistency of it living here instead.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
}
