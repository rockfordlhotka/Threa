# Gravatar Integration Research

**Project:** Threa TTRPG Assistant
**Researched:** 2026-01-26
**Overall Confidence:** HIGH

## Overview

Gravatar (Globally Recognized Avatar) is a service that associates profile images with email addresses. When a site requests an avatar for an email address, Gravatar returns the user's uploaded avatar or a generated default image.

**Why Gravatar for Threa:**
- No image storage/hosting required on our end
- Users control their own avatar across all Gravatar-enabled sites
- Automatic fallback images for users without Gravatars
- Already integrated into Radzen.Blazor (our UI library)

## Key Finding: Radzen Already Has This

**Radzen.Blazor 8.4.2** (our current version) includes `RadzenGravatar`, a purpose-built component for displaying Gravatar avatars. This eliminates the need for custom implementation.

```razor
<RadzenGravatar Email="@user.Email" Size="64" />
```

This is the recommended approach for this project.

## Integration Steps

### Option 1: Use RadzenGravatar (Recommended)

Since Threa already uses Radzen.Blazor, no additional packages or custom code needed.

**Usage:**
```razor
@* Display avatar for a user *@
<RadzenGravatar Email="@user.Email" Size="48" AlternateText="@user.DisplayName" />
```

**RadzenGravatar Properties:**
| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Email` | `string?` | null | Email address to fetch avatar for |
| `Size` | `int` | 36 | Avatar size in pixels (square) |
| `AlternateText` | `string` | "gravatar" | Alt text for accessibility |

**Generated URL format:**
```
https://secure.gravatar.com/avatar/{md5_hash}?d=retro&s={size}
```

**Limitations of RadzenGravatar:**
- Uses MD5 hashing (Gravatar's legacy algorithm, still supported)
- Hardcoded default image style: `retro`
- No option to customize default image type
- No option to use SHA256 (Gravatar's newer recommended algorithm)

### Option 2: Custom Implementation

If more control is needed (custom default images, SHA256), implement manually.

## C# Implementation (If Custom Needed)

### SHA256 Approach (Gravatar's Current Recommendation)

```csharp
using System.Security.Cryptography;
using System.Text;

public static class GravatarHelper
{
    /// <summary>
    /// Generates a Gravatar URL for the given email address.
    /// </summary>
    /// <param name="email">User's email address (can be null)</param>
    /// <param name="size">Image size in pixels (1-2048, default 80)</param>
    /// <param name="defaultImage">Default image type when no Gravatar exists</param>
    /// <returns>Gravatar URL or null if email is null/empty</returns>
    public static string? GetGravatarUrl(
        string? email,
        int size = 80,
        GravatarDefault defaultImage = GravatarDefault.Identicon)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        // Gravatar requires: trim whitespace, convert to lowercase
        var normalizedEmail = email.Trim().ToLowerInvariant();

        // SHA256 hash (Gravatar's recommended algorithm as of 2024+)
        var hash = ComputeSha256Hash(normalizedEmail);

        // Build URL with parameters
        var defaultParam = GetDefaultParam(defaultImage);
        return $"https://gravatar.com/avatar/{hash}?s={size}&d={defaultParam}";
    }

    private static string ComputeSha256Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    private static string GetDefaultParam(GravatarDefault defaultImage)
    {
        return defaultImage switch
        {
            GravatarDefault.MysteryPerson => "mp",
            GravatarDefault.Identicon => "identicon",
            GravatarDefault.MonsterId => "monsterid",
            GravatarDefault.Wavatar => "wavatar",
            GravatarDefault.Retro => "retro",
            GravatarDefault.RoboHash => "robohash",
            GravatarDefault.Blank => "blank",
            GravatarDefault.NotFound => "404",
            _ => "identicon"
        };
    }
}

public enum GravatarDefault
{
    /// <summary>Simple silhouette (same for all emails)</summary>
    MysteryPerson,
    /// <summary>Geometric pattern unique to email hash</summary>
    Identicon,
    /// <summary>Generated monster unique to email hash</summary>
    MonsterId,
    /// <summary>Generated face unique to email hash</summary>
    Wavatar,
    /// <summary>8-bit arcade style face unique to email hash</summary>
    Retro,
    /// <summary>Generated robot unique to email hash</summary>
    RoboHash,
    /// <summary>Transparent PNG</summary>
    Blank,
    /// <summary>Return 404 error (no image)</summary>
    NotFound
}
```

### MD5 Approach (Legacy, Still Supported)

```csharp
// Only use if compatibility with RadzenGravatar is needed
private static string ComputeMd5Hash(string input)
{
    var bytes = MD5.HashData(Encoding.ASCII.GetBytes(input));
    return Convert.ToHexStringLower(bytes);
}
```

**Note:** Both MD5 and SHA256 are currently supported by Gravatar. SHA256 is recommended for new implementations due to better privacy (harder to reverse-engineer email from hash).

## Blazor Rendering Patterns

### Pattern 1: RadzenGravatar (Simplest)

```razor
@* In any Blazor component *@
<RadzenGravatar Email="@Email" Size="64" AlternateText="User avatar" />

@code {
    [Parameter] public string? Email { get; set; }
}
```

### Pattern 2: Custom with RadzenImage (More Control)

```razor
@inject GravatarHelper Gravatar

@if (!string.IsNullOrEmpty(AvatarUrl))
{
    <RadzenImage Path="@AvatarUrl"
                 Style="@($"width:{Size}px;height:{Size}px;border-radius:50%")"
                 AlternateText="@AltText" />
}
else
{
    @* Fallback when no email provided *@
    <div class="avatar-placeholder" style="@($"width:{Size}px;height:{Size}px")">
        <RadzenIcon Icon="person" />
    </div>
}

@code {
    [Parameter] public string? Email { get; set; }
    [Parameter] public int Size { get; set; } = 48;
    [Parameter] public string AltText { get; set; } = "User avatar";

    private string? AvatarUrl => GravatarHelper.GetGravatarUrl(Email, Size);
}
```

### Pattern 3: Reusable Avatar Component

```razor
@* Components/UserAvatar.razor *@
@if (!string.IsNullOrWhiteSpace(Email))
{
    <RadzenGravatar Email="@Email"
                    Size="@Size"
                    AlternateText="@(DisplayName ?? "User avatar")"
                    class="@CssClass" />
}
else
{
    @* No email - show initials or placeholder *@
    <div class="avatar-initials @CssClass"
         style="@($"width:{Size}px;height:{Size}px;font-size:{Size/2}px")">
        @GetInitials()
    </div>
}

@code {
    [Parameter] public string? Email { get; set; }
    [Parameter] public string? DisplayName { get; set; }
    [Parameter] public int Size { get; set; } = 48;
    [Parameter] public string? CssClass { get; set; }

    private string GetInitials()
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
            return "?";

        var parts = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();

        return DisplayName[..Math.Min(2, DisplayName.Length)].ToUpperInvariant();
    }
}
```

**CSS for initials fallback:**
```css
.avatar-initials {
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    background-color: var(--rz-primary);
    color: var(--rz-on-primary);
    font-weight: 600;
}
```

## Fallback Strategy

### When Email is Not Provided

Since email is optional in Threa, need fallback for users without email:

1. **Display initials from DisplayName** - Generate initials (e.g., "John Smith" -> "JS")
2. **Generic placeholder icon** - Use a person icon from Radzen's icon set
3. **Generated identicon from username** - Hash the username instead of email

**Recommendation:** Use initials from DisplayName with a colored background. This provides a personalized experience without requiring email.

### When Email Has No Gravatar

Gravatar automatically returns a default image. Options via the `d=` parameter:

| Value | Description | Best For |
|-------|-------------|----------|
| `identicon` | Geometric pattern unique to hash | Technical/developer apps |
| `retro` | 8-bit pixel art face | Gaming apps (good for TTRPG!) |
| `monsterid` | Generated monster | Fun/playful apps |
| `wavatar` | Generated cartoon face | Social apps |
| `robohash` | Generated robot | Sci-fi themed apps |
| `mp` | Gray silhouette | Professional apps |
| `blank` | Transparent | When you want custom CSS fallback |
| `404` | HTTP 404 error | When you need to detect no-avatar |

**Recommendation for Threa:** Use `retro` (8-bit style) or `monsterid` for the TTRPG theme. RadzenGravatar uses `retro` by default.

## Configuration Options

### Size Parameter

| Use Case | Recommended Size |
|----------|------------------|
| Navigation bar | 24-32px |
| Comment/message | 36-48px |
| Profile card | 64-96px |
| Full profile page | 128-256px |

Gravatar supports 1-2048 pixels. Images are always square.

### Rating Parameter (Not Typically Needed)

Gravatar allows users to self-rate their images. By default, only 'G' rated images are returned.

| Rating | Content |
|--------|---------|
| `g` | Suitable for all audiences (default) |
| `pg` | May contain mild content |
| `r` | May contain mature content |
| `x` | May contain explicit content |

**For Threa:** Leave at default (`g`). Adding `&r=g` explicitly is optional.

## Security and Privacy Considerations

### Email Hashing Security

**MD5 Concerns:**
- MD5 is cryptographically broken for security purposes
- Email addresses can be reverse-engineered from MD5 hashes using rainbow tables or GPU-accelerated attacks
- Research has shown ~10% of emails can be recovered from Gravatar MD5 hashes

**SHA256 Improvements:**
- Gravatar now recommends SHA256 over MD5
- Computationally harder to reverse
- Both algorithms currently supported by Gravatar

**Mitigations for Threa:**
1. **Email is optional** - Users who don't provide email have no hash exposure
2. **Users consent** - By providing email, users acknowledge it will be hashed for Gravatar
3. **Email not stored in hash** - Only the hash is transmitted to Gravatar, not the raw email
4. **HTTPS only** - Gravatar URLs use HTTPS (secure.gravatar.com)

### Privacy Trade-offs

Gravatar's design means the same hash appears on all sites using Gravatar. This allows:
- Cross-site user correlation by comparing hashes
- Potential identity linkage across unrelated sites

**For Threa:**
- This is acceptable for a TTRPG character sheet app
- Users can avoid by not providing email
- Consider noting this in privacy policy

## Performance Considerations

### CDN and Caching

**Gravatar's Infrastructure:**
- Images served from Gravatar's CDN (fast global delivery)
- Aggressive browser caching (images rarely change)
- No performance cost on Threa's servers

**Browser Caching:**
- Gravatar sets appropriate cache headers
- Same user's avatar loads instantly on subsequent page views
- No action needed from Threa

### Image Loading

**Best Practices:**
- Use appropriate size (don't request 256px for a 36px display)
- Consider `loading="lazy"` for images below the fold
- RadzenGravatar handles basic optimization

```razor
@* For manual images with lazy loading *@
<img src="@GravatarUrl" loading="lazy" width="48" height="48" alt="Avatar" />
```

## NuGet Packages Comparison

| Approach | Package | Pros | Cons |
|----------|---------|------|------|
| **RadzenGravatar** | Already included | No extra deps, consistent UI | MD5 only, limited customization |
| **Custom helper** | None needed | Full control, SHA256 support | More code to maintain |
| **GravatarSharp.Core** | 1.0.1.2 | Profile info + image | Adds Newtonsoft.Json dep |
| **gravatar-dotnet** | 0.1.3 | DI integration, no deps | Requires API key for some features |

**Recommendation:** Use RadzenGravatar (already included) for simplicity. Only implement custom helper if:
- SHA256 is required for privacy policy compliance
- Custom default image types are needed beyond `retro`

## Recommendations for Threa

### Implementation Plan

1. **Use RadzenGravatar** for all avatar displays (no additional packages needed)

2. **Create `UserAvatar.razor` wrapper component** that handles:
   - RadzenGravatar when email is provided
   - Initials fallback when email is missing
   - Consistent sizing across the app

3. **Default image style:** Keep `retro` (RadzenGravatar default) - fits TTRPG theme

4. **Standard sizes for Threa:**
   - Navigation: 32px
   - Player list: 48px
   - Profile page: 128px

### Example UserAvatar Component for Threa

```razor
@* Threa.Client/Components/UserAvatar.razor *@
@if (!string.IsNullOrWhiteSpace(Email))
{
    <RadzenGravatar Email="@Email"
                    Size="@Size"
                    AlternateText="@GetAltText()"
                    Style="@GetStyle()" />
}
else
{
    <div class="threa-avatar-initials" style="@GetInitialsStyle()">
        @GetInitials()
    </div>
}

@code {
    [Parameter] public string? Email { get; set; }
    [Parameter] public string? DisplayName { get; set; }
    [Parameter] public int Size { get; set; } = 48;
    [Parameter] public bool Round { get; set; } = true;

    private string GetAltText() =>
        string.IsNullOrWhiteSpace(DisplayName) ? "User avatar" : $"{DisplayName}'s avatar";

    private string GetStyle() =>
        Round ? "border-radius: 50%;" : "";

    private string GetInitialsStyle() =>
        $"width:{Size}px;height:{Size}px;font-size:{Size * 0.4}px;" +
        (Round ? "border-radius:50%;" : "");

    private string GetInitials()
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
            return "?";

        var parts = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
            : DisplayName[0].ToString().ToUpperInvariant();
    }
}
```

### CSS Addition

```css
/* Add to app.css or component styles */
.threa-avatar-initials {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, var(--rz-primary), var(--rz-primary-dark, var(--rz-primary)));
    color: var(--rz-on-primary, white);
    font-weight: 600;
    text-transform: uppercase;
}
```

## Sources

### Official Documentation
- [Gravatar Developer Docs - Avatars](https://docs.gravatar.com/sdk/images/)
- [Gravatar Developer Docs - Creating Hash](https://docs.gravatar.com/rest/hash/)
- [Radzen Blazor - RadzenGravatar](https://blazor.radzen.com/gravatar)
- [Radzen Blazor - RadzenGravatar API](https://blazor.radzen.com/docs/api/Radzen.Blazor.RadzenGravatar.html)
- [RadzenGravatar Source Code](https://github.com/radzenhq/radzen-blazor/blob/master/Radzen.Blazor/RadzenGravatar.razor.cs)

### Microsoft Documentation
- [SHA256 Class](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256)
- [Display images in Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/images-and-documents)

### Security References
- [WordPress Trac - Gravatar SHA256 Migration](https://core.trac.wordpress.org/ticket/60638)
- [Gravatar Privacy FAQs](https://support.gravatar.com/privacy-and-security/data-privacy/)
- [Wordfence - Gravatar Security Advisory](https://www.wordfence.com/blog/2016/12/gravatar-advisory-protect-email-address-identity/)

### Implementation Examples
- [C# Gravatar Hash Gist](https://gist.github.com/danesparza/973923)
- [elmah.io - .NET Gravatar Integration](https://blog.elmah.io/show-a-name-and-profile-photo-with-dotnet-and-gravatar/)
