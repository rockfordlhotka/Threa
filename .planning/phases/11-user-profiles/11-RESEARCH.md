# Phase 11: User Profiles - Research

**Researched:** 2026-01-26
**Domain:** User profiles, Gravatar integration, Blazor form patterns, profanity filtering
**Confidence:** HIGH

## Summary

Phase 11 implements user profile management with display name editing, optional email for Gravatar avatars, and public profile viewing. The codebase already has extensive Gravatar research (`.planning/research/GRAVATAR.md`) confirming RadzenGravatar is available in Radzen.Blazor 8.4.2. The existing `PlayerEdit` business object and `/player/playeredit` page provide a foundation but need enhancement for the new requirements.

Key implementation areas:
1. **Profile editing** - Enhance existing PlayerEdit BO with email field (currently readonly), add display name validation with profanity filter, add Gravatar opt-out toggle
2. **Avatar component** - Create `UserAvatar.razor` wrapper using RadzenGravatar with initials fallback
3. **Public profiles** - Add read-only profile view for viewing other users (display name + avatar only)
4. **UI updates** - Display avatars in navigation bar, admin user list, and anywhere users are shown

**Primary recommendation:** Use RadzenGravatar (already included) for avatar display, Profanity.Detector (NuGet) for display name validation, enhance existing PlayerEdit pattern for profile editing.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Radzen.Blazor | 8.4.2 | RadzenGravatar component, TooltipService | Already installed, provides purpose-built Gravatar component |
| Profanity.Detector | 0.1.8 | Display name profanity validation | .NET Standard 2.0 compatible, handles Scunthorpe problem, 1.3M+ downloads |
| CSLA.NET | 9.1.0 | Business object validation rules | Already the pattern for all business objects |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| TooltipService | (Radzen) | Username tooltip on hover | When showing display name with username reveal |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| RadzenGravatar | Custom GravatarHelper | SHA256 vs MD5, more code to maintain |
| Profanity.Detector | Censored | Censored lacks Scunthorpe problem handling |
| Profanity.Detector | Custom word list | More maintenance, less comprehensive |

**Installation:**
```bash
dotnet add GameMechanics/GameMechanics.csproj package Profanity.Detector --version 0.1.8
```

## Architecture Patterns

### Recommended Project Structure
```
GameMechanics/Player/
├── PlayerEdit.cs          # Enhanced with email, UseGravatar
├── ProfileInfo.cs         # NEW: Read-only for public profile view
├── ProfileInfoList.cs     # Optional: if listing users with avatars
├── ProfanityRule.cs       # NEW: CSLA business rule for display name

Threa.Client/Components/
├── Shared/
│   └── UserAvatar.razor   # NEW: Wrapper for Gravatar + initials fallback
├── Pages/
│   ├── Player/
│   │   ├── PlayerEdit.razor    # Enhanced profile page
│   │   └── Profile.razor       # NEW: Public profile view
```

### Pattern 1: CSLA Profanity Validation Rule
**What:** Custom CSLA business rule using Profanity.Detector
**When to use:** Validating display name on input
**Example:**
```csharp
// Source: CSLA pattern from existing codebase, Profanity.Detector API
using Csla.Rules;
using Profanity.Detector;

public class NoProfanityRule : BusinessRule
{
    private readonly ProfanityFilter _filter;

    public NoProfanityRule(IPropertyInfo primaryProperty)
        : base(primaryProperty)
    {
        _filter = new ProfanityFilter();
    }

    protected override void Execute(IRuleContext context)
    {
        var value = (string?)context.InputPropertyValues[PrimaryProperty];
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (_filter.ContainsProfanity(value))
            {
                context.AddErrorResult("Display name contains inappropriate content");
            }
        }
    }
}
```

### Pattern 2: UserAvatar Component with Gravatar/Initials Fallback
**What:** Reusable component that shows Gravatar when email available, initials otherwise
**When to use:** Everywhere avatars are displayed
**Example:**
```razor
<!-- Source: .planning/research/GRAVATAR.md recommendation -->
@if (!string.IsNullOrWhiteSpace(Email) && UseGravatar)
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
    [Parameter] public bool UseGravatar { get; set; } = true;
    [Parameter] public bool Round { get; set; } = true;

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

### Pattern 3: Display Name with Username Tooltip
**What:** Show display name with username revealed on hover
**When to use:** User lists, nav bar, profile displays
**Example:**
```razor
<!-- Source: Radzen TooltipService (already registered in Program.cs) -->
@inject TooltipService TooltipService

<span @ref="displayNameRef"
      @onmouseenter="@(args => ShowUsernameTooltip())"
      @onmouseleave="@(args => TooltipService.Close())">
    @DisplayName
</span>

@code {
    ElementReference displayNameRef;
    [Parameter] public string? DisplayName { get; set; }
    [Parameter] public string? Username { get; set; }

    void ShowUsernameTooltip()
    {
        if (!string.IsNullOrWhiteSpace(Username))
        {
            TooltipService.Open(displayNameRef, $"@{Username}",
                new TooltipOptions { Position = TooltipPosition.Bottom });
        }
    }
}
```

### Pattern 4: DTO Field Extension for Gravatar Toggle
**What:** Add UseGravatar field to Player DTO
**When to use:** Database/DAL layer
**Example:**
```csharp
// Source: Existing Threa.Dal.Dto.Player pattern
public class Player
{
    // ... existing fields ...
    public bool UseGravatar { get; set; } = true;  // NEW: Default to using Gravatar
}
```

### Anti-Patterns to Avoid
- **Storing Gravatar URLs:** URLs are computed from email hash - don't store them
- **Loading Gravatar in SSR mode:** RadzenGravatar needs interactive mode for proper loading
- **Email in public profiles:** Email must never be exposed to other users - only compute avatar server-side or use hash

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Gravatar display | Manual URL generation + img tag | RadzenGravatar | Already included, handles hashing, sizing, fallback image |
| Profanity filtering | Custom word list + regex | Profanity.Detector | Handles Scunthorpe problem, maintained word list, 1.3M+ downloads |
| Tooltips | Custom CSS hover | TooltipService | Already registered in Program.cs, consistent styling |
| Initials extraction | Simple string split | Careful algorithm | Handle single names, punctuation, unicode edge cases |

**Key insight:** Gravatar integration and profanity filtering both have subtle edge cases (MD5 hashing requirements, Scunthorpe problem) that established libraries handle correctly.

## Common Pitfalls

### Pitfall 1: Email in Claims Gets Stale
**What goes wrong:** Claims contain email at login time; if user changes email, avatar shows old Gravatar until re-login
**Why it happens:** Claims are stored in authentication cookie, not re-fetched
**How to avoid:** Fetch current user data for avatar display, don't rely on claims for email
**Warning signs:** Avatar doesn't update after email change until logout/login

### Pitfall 2: RadzenGravatar in SSR Context
**What goes wrong:** RadzenGravatar may not render correctly during static server rendering
**Why it happens:** Blazor SSR prerenders without interactivity
**How to avoid:** Profile page uses `@rendermode InteractiveServer` (already the pattern for player pages)
**Warning signs:** Avatar appears briefly then disappears, or shows broken image

### Pitfall 3: Display Name Duplicate Confusion
**What goes wrong:** Two users with same display name creates confusion
**Why it happens:** Display names are intentionally allowed to duplicate (username is unique)
**How to avoid:** Always show username tooltip on hover; in admin contexts show username prominently
**Warning signs:** Users report confusion about who is who

### Pitfall 4: Profanity Filter False Positives
**What goes wrong:** Legitimate names blocked (e.g., "Scunthorpe", "Cockburn")
**Why it happens:** Naive substring matching
**How to avoid:** Profanity.Detector handles Scunthorpe problem; test with known edge cases
**Warning signs:** Users report names incorrectly rejected

### Pitfall 5: Email Hash Privacy
**What goes wrong:** Raw email transmitted to Gravatar
**Why it happens:** Misunderstanding of Gravatar protocol
**How to avoid:** RadzenGravatar hashes email client-side before URL generation
**Warning signs:** N/A - RadzenGravatar handles this correctly

## Code Examples

Verified patterns from official sources:

### CSLA Business Rule Pattern (from codebase)
```csharp
// Source: GameMechanics/Player/AdminUserEdit.cs LastAdminProtectionRule
protected override void AddBusinessRules()
{
    base.AddBusinessRules();
    BusinessRules.AddRule(new Required(NameProperty) { MessageText = "Display name is required" });
    BusinessRules.AddRule(new MinLength(NameProperty, 1) { MessageText = "Display name is required" });
    BusinessRules.AddRule(new MaxLength(NameProperty, 50) { MessageText = "Display name cannot exceed 50 characters" });
    BusinessRules.AddRule(new NoProfanityRule(NameProperty));
}
```

### Profanity.Detector Basic Usage
```csharp
// Source: https://github.com/stephenhaunts/ProfanityDetector
var filter = new ProfanityFilter();

// Check if text contains profanity
bool hasProfanity = filter.ContainsProfanity("some text");

// Get list of detected profanities
var profanities = filter.DetectAllProfanities("some text");

// Add custom words
filter.AddProfanity("customword");

// Use allowlist for false positives
filter.AllowList.Add("Scunthorpe");
```

### RadzenGravatar from Radzen.Blazor
```razor
<!-- Source: https://blazor.radzen.com/gravatar -->
<RadzenGravatar Email="@user.Email"
                Size="48"
                AlternateText="@($"{user.DisplayName}'s avatar")" />
```

### Avatar Initials CSS
```css
/* Source: .planning/research/GRAVATAR.md */
.threa-avatar-initials {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, var(--rz-primary), var(--rz-primary-dark, var(--rz-primary)));
    color: var(--rz-on-primary, white);
    font-weight: 600;
    text-transform: uppercase;
    border-radius: 50%;
}
```

### Current PlayerEdit Enhancement Points
```csharp
// Source: GameMechanics/Player/PlayerEdit.cs (current state)
// NEEDS: Make Email editable (currently readonly via LoadProperty)
// NEEDS: Add UseGravatar property
// NEEDS: Add profanity validation rule for Name property

public static readonly PropertyInfo<string> EmailProperty = RegisterProperty<string>(nameof(Email));
public string Email
{
    get => GetProperty(EmailProperty);
    set => SetProperty(EmailProperty, value);  // CHANGE: was private set => LoadProperty
}

public static readonly PropertyInfo<bool> UseGravatarProperty = RegisterProperty<bool>(nameof(UseGravatar));
public bool UseGravatar
{
    get => GetProperty(UseGravatarProperty);
    set => SetProperty(UseGravatarProperty, value);
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| MD5 for Gravatar hash | SHA256 (recommended) | 2024+ | RadzenGravatar uses MD5 (still supported), custom impl could use SHA256 |
| Manual profanity lists | ML-based detection available | 2023+ | Profanity.Detector is sufficient; ML overkill for display names |
| Full page profile forms | Modal dialogs | 2025 | CONTEXT.md specifies full page at /profile route |

**Deprecated/outdated:**
- None relevant to this phase

## Open Questions

Things that couldn't be fully resolved:

1. **Email field naming ambiguity**
   - What we know: Player.Email currently stores username (legacy naming noted in UserRegistration.cs line 112: "Email field stores username (legacy naming)")
   - What's unclear: Should we add a new field (e.g., ContactEmail) or repurpose ImageUrl?
   - Recommendation: Add new ContactEmail field to Player DTO to avoid breaking existing username-based queries

2. **Claims update on profile change**
   - What we know: ClaimsIdentity is set at login with Name = user.Name, Email = user.Email
   - What's unclear: Whether claims should be refreshed when profile is updated
   - Recommendation: For avatar display, always fetch fresh data; don't rely on claims for display purposes

3. **Public profile route pattern**
   - What we know: CONTEXT.md specifies /profile for own profile
   - What's unclear: Route for viewing other users' profiles (/profile/{id}? /user/{username}?)
   - Recommendation: `/profile/{id:int}` for consistency with existing patterns like `/admin/users/{Id:int}`

## Sources

### Primary (HIGH confidence)
- `S:/src/rdl/threa/.planning/research/GRAVATAR.md` - Comprehensive Gravatar integration research
- `S:/src/rdl/threa/GameMechanics/Player/PlayerEdit.cs` - Existing profile edit pattern
- `S:/src/rdl/threa/GameMechanics/Player/AdminUserEdit.cs` - CSLA business rule patterns
- `S:/src/rdl/threa/Threa/Threa/Program.cs` - TooltipService registration confirmed
- Radzen Blazor 8.4.2 - RadzenGravatar component (installed in Threa.Client.csproj)

### Secondary (MEDIUM confidence)
- [NuGet: Profanity.Detector 0.1.8](https://www.nuget.org/packages/Profanity.Detector) - Profanity filtering library
- [GitHub: ProfanityDetector](https://github.com/stephenhaunts/ProfanityDetector) - API documentation
- [Radzen Tooltip Component](https://blazor.radzen.com/tooltip) - TooltipService usage

### Tertiary (LOW confidence)
- None - all critical findings verified with primary sources

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - RadzenGravatar confirmed in existing research, Profanity.Detector verified on NuGet/GitHub
- Architecture: HIGH - Follows established CSLA patterns from AdminUserEdit, PlayerEdit
- Pitfalls: HIGH - Based on existing codebase patterns and documented Gravatar behavior

**Research date:** 2026-01-26
**Valid until:** 30 days (stable libraries, well-documented patterns)
