# Phase 8: Registration Foundation - Research

**Researched:** 2026-01-26
**Domain:** User Registration, CSLA Business Objects, Password Hashing, First-User-Admin Pattern
**Confidence:** HIGH

## Summary

Phase 8 implements self-service user registration with automatic first-user-as-Admin assignment. The research found that substantial infrastructure already exists:

1. **Player DTO exists** with all needed fields (Id, Email, Name, Salt, HashedPassword, Roles, IsEnabled)
2. **IPlayerDal interface exists** with SavePlayerAsync, GetPlayerByEmailAsync, GetAllPlayersAsync methods
3. **BCrypt.Net-Next 4.0.3** already integrated for password hashing with 12-round salt generation
4. **Role system exists** with Administrator, GameMaster, Player roles as comma-separated string
5. **Login page exists** as a pattern for authentication UI
6. **CSLA business object patterns** well-established in codebase (PropertyInfo, AddBusinessRules, Create/Fetch/Insert)

The primary work is:
- Extend Player DTO with SecretQuestion and SecretAnswer fields
- Create UserRegistration CSLA business object for registration flow
- Create Registration.razor page following Login.razor pattern
- Implement first-user detection via GetAllPlayersAsync().Count == 0

**Primary recommendation:** Create a new `UserRegistration` CSLA command/business object that handles registration with validation, duplicate detection, password hashing, and first-user-admin assignment. Follow existing patterns exactly.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object framework | Project standard, all BOs use CSLA |
| BCrypt.Net-Next | 4.0.3 | Password hashing | Already in use, 12-round cost factor |
| Microsoft.Data.Sqlite | 10.0.1 | Database access | Project standard for persistence |
| Radzen.Blazor | 8.4.2 | UI components | Project standard, provides form inputs |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| MSTest | (project) | Unit testing | All tests use MSTest framework |
| System.Text.Json | (built-in) | JSON serialization | SQLite DAL stores Player as JSON |

**Installation:**
No additional packages needed - all dependencies already in project.

## Architecture Patterns

### Recommended Project Structure
```
GameMechanics/
└── Player/
    ├── Roles.cs              # EXISTS - Role constants
    ├── AdminUserEdit.cs      # EXISTS - Admin editing users
    ├── AdminUserInfo.cs      # EXISTS - User info for lists
    ├── AdminUserList.cs      # EXISTS - User list for admin
    ├── PlayerEdit.cs         # EXISTS - User profile editing
    ├── PlayerPassword.cs     # EXISTS - Password change
    ├── UserValidation.cs     # EXISTS - Login validation
    └── UserRegistration.cs   # TO CREATE - Registration business object

Threa.Dal/
├── IPlayerDal.cs             # EXTEND - Add secret Q&A methods if needed
└── Dto/
    └── Player.cs             # EXTEND - Add SecretQuestion, SecretAnswer

Threa.Dal.MockDb/
└── PlayerDal.cs              # EXISTS - Update for new fields

Threa.Dal.SqlLite/
└── PlayerDal.cs              # EXISTS - Update for new fields

Threa/Threa/Components/Pages/
├── Login.razor               # EXISTS - Auth pattern reference
└── Register.razor            # TO CREATE - Registration page

GameMechanics.Test/
└── RegistrationTests.cs      # TO CREATE - Unit tests
```

### Pattern 1: CSLA BusinessBase for Registration
**What:** Use BusinessBase<T> for registration data with validation rules
**When to use:** When registration needs validation, business rules, and data persistence
**Example:**
```csharp
// Based on existing PlayerPassword.cs and AdminUserEdit.cs patterns
[Serializable]
public class UserRegistration : BusinessBase<UserRegistration>
{
    public static readonly PropertyInfo<string> UsernameProperty = RegisterProperty<string>(nameof(Username));
    public string Username
    {
        get => GetProperty(UsernameProperty);
        set => SetProperty(UsernameProperty, value);
    }

    public static readonly PropertyInfo<string> PasswordProperty = RegisterProperty<string>(nameof(Password));
    public string Password
    {
        get => GetProperty(PasswordProperty);
        set => SetProperty(PasswordProperty, value);
    }

    public static readonly PropertyInfo<string> SecretQuestionProperty = RegisterProperty<string>(nameof(SecretQuestion));
    public string SecretQuestion
    {
        get => GetProperty(SecretQuestionProperty);
        set => SetProperty(SecretQuestionProperty, value);
    }

    public static readonly PropertyInfo<string> SecretAnswerProperty = RegisterProperty<string>(nameof(SecretAnswer));
    public string SecretAnswer
    {
        get => GetProperty(SecretAnswerProperty);
        set => SetProperty(SecretAnswerProperty, value);
    }

    protected override void AddBusinessRules()
    {
        base.AddBusinessRules();
        BusinessRules.AddRule(new Csla.Rules.CommonRules.Required(UsernameProperty));
        BusinessRules.AddRule(new Csla.Rules.CommonRules.Required(PasswordProperty));
        BusinessRules.AddRule(new Csla.Rules.CommonRules.MinLength(PasswordProperty, 6));
        BusinessRules.AddRule(new Csla.Rules.CommonRules.Required(SecretQuestionProperty));
        BusinessRules.AddRule(new Csla.Rules.CommonRules.Required(SecretAnswerProperty));
    }

    [Create]
    private void Create()
    {
        using (BypassPropertyChecks)
        {
            Username = string.Empty;
            Password = string.Empty;
            SecretQuestion = string.Empty;
            SecretAnswer = string.Empty;
        }
        BusinessRules.CheckRules();
    }

    [Insert]
    private async Task Insert([Inject] IPlayerDal dal)
    {
        // Check for duplicate username
        var existing = await dal.GetPlayerByEmailAsync(Username);
        if (existing != null)
            throw new DuplicateKeyException($"Username '{Username}' is already registered");

        // Determine if this is the first user (becomes Admin)
        var allPlayers = await dal.GetAllPlayersAsync();
        bool isFirstUser = !allPlayers.Any();

        // Hash password
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password, salt);

        // Create player with appropriate role
        var player = new Threa.Dal.Dto.Player
        {
            Email = Username, // Username is stored in Email field (legacy naming)
            Name = Username,  // Initial display name same as username
            Salt = salt,
            HashedPassword = hashedPassword,
            SecretQuestion = SecretQuestion,
            SecretAnswer = SecretAnswer.Trim().ToLowerInvariant(), // Case-insensitive
            Roles = isFirstUser ? Roles.Administrator : Roles.Player,
            IsEnabled = true
        };

        var result = await dal.SavePlayerAsync(player);
        LoadProperty(IdProperty, result.Id);
    }
}
```

### Pattern 2: Registration Page Following Login Pattern
**What:** SSR page with form handling
**When to use:** Registration workflow
**Example:**
```razor
@page "/register"

@using GameMechanics.Player
@using Microsoft.AspNetCore.Authentication
@using Microsoft.AspNetCore.Authentication.Cookies

@inject IDataPortal<UserRegistration> portal
@inject NavigationManager NavigationManager

<PageTitle>Threa - Register</PageTitle>

<h1>Create Account</h1>

<div class="alert-danger">@Message</div>

<EditForm Model="RegistrationModel" OnSubmit="RegisterUser" FormName="registerform">
    <div>
        <label>Username</label>
        <InputText @bind-Value="RegistrationModel!.Username" />
    </div>
    <div>
        <label>Password (min 6 characters)</label>
        <InputText type="password" @bind-Value="RegistrationModel!.Password" />
    </div>
    <div>
        <label>Secret Question</label>
        <InputText @bind-Value="RegistrationModel!.SecretQuestion" />
    </div>
    <div>
        <label>Secret Answer</label>
        <InputText @bind-Value="RegistrationModel!.SecretAnswer" />
    </div>
    <button class="btn btn-primary">Register</button>
    <a href="/login" class="btn btn-link">Already have an account? Login</a>
</EditForm>

@code {
    [SupplyParameterFromForm]
    public RegistrationInfo? RegistrationModel { get; set; }

    protected override void OnInitialized()
    {
        RegistrationModel ??= new();
    }

    public string Message { get; set; } = "";

    private async Task RegisterUser()
    {
        Message = "";
        try
        {
            var registration = await portal.CreateAsync();
            registration.Username = RegistrationModel!.Username;
            registration.Password = RegistrationModel!.Password;
            registration.SecretQuestion = RegistrationModel!.SecretQuestion;
            registration.SecretAnswer = RegistrationModel!.SecretAnswer;

            await registration.SaveAsync();

            // Redirect to login after successful registration
            NavigationManager.NavigateTo("/login?registered=true");
        }
        catch (DataPortalException ex)
        {
            Message = ex.BusinessExceptionMessage;
        }
        catch (DuplicateKeyException)
        {
            Message = "Username is already taken. Please choose a different username.";
        }
    }

    public class RegistrationInfo
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SecretQuestion { get; set; } = string.Empty;
        public string SecretAnswer { get; set; } = string.Empty;
    }
}
```

### Pattern 3: Password Hashing (Existing Pattern)
**What:** BCrypt with 12-round salt
**When to use:** All password storage
**Example:**
```csharp
// Source: AdminUserEdit.cs lines 124-126
if (string.IsNullOrWhiteSpace(player.Salt))
    player.Salt = BCrypt.Net.BCrypt.GenerateSalt(12);
player.HashedPassword = BCrypt.Net.BCrypt.HashPassword(NewPassword, player.Salt);
```

### Pattern 4: First-User Detection
**What:** Check if any users exist before assigning Admin role
**When to use:** During registration to determine if first user
**Example:**
```csharp
// Get all players to check if this is the first registration
var allPlayers = await dal.GetAllPlayersAsync();
bool isFirstUser = !allPlayers.Any();

// Assign role based on first-user status
player.Roles = isFirstUser ? Roles.Administrator : Roles.Player;
```

### Anti-Patterns to Avoid
- **Don't hash secret answer with BCrypt:** Store answer as lowercase/trimmed string for comparison (not a security credential like password)
- **Don't use Email field for email address:** In existing schema, Email field is the login username - keep this pattern
- **Don't auto-login after registration:** Redirect to login page for explicit login (simpler, matches user expectation)
- **Don't store plain text password even temporarily:** Hash immediately in Insert method

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Password hashing | Custom hash | BCrypt.Net.HashPassword with 12-round salt | Already integrated, cryptographically secure |
| Duplicate detection | Manual query + check | DuplicateKeyException from DAL | Existing pattern for unique constraint violations |
| Form validation | Client-side only | CSLA BusinessRules + DataAnnotations | Server-side validation, integrated with UI |
| User count query | New DAL method | GetAllPlayersAsync().Any() | Existing method sufficient |
| Role assignment | Hardcoded strings | Roles.Administrator, Roles.Player | Existing constants |

**Key insight:** The existing Player infrastructure handles almost everything. Only additions needed are SecretQuestion/SecretAnswer fields and the registration business object.

## Common Pitfalls

### Pitfall 1: Username as Email Field Confusion
**What goes wrong:** Using Email field for actual email addresses
**Why it happens:** Field name suggests email, but it's actually the login username
**How to avoid:** Document that Player.Email is the login username; actual email is optional (added in Phase 11)
**Warning signs:** Users trying to use email address format when it's not required

### Pitfall 2: Case-Sensitive Username Comparison
**What goes wrong:** "Admin" and "admin" treated as different users
**Why it happens:** String comparison defaults to case-sensitive
**How to avoid:** Use case-insensitive comparison in GetPlayerByEmailAsync or store usernames in lowercase
**Warning signs:** Duplicate usernames differing only in case

### Pitfall 3: Secret Answer Not Normalized
**What goes wrong:** "My Dog" doesn't match "my dog" or " my dog "
**Why it happens:** Forgetting to normalize before storage and comparison
**How to avoid:** Always Trim().ToLowerInvariant() on secret answer storage and comparison
**Warning signs:** Users unable to recover password with "correct" answer

### Pitfall 4: First-User Race Condition
**What goes wrong:** Two simultaneous registrations both become Admin
**Why it happens:** Check-then-insert without transaction/locking
**How to avoid:** Accept this as a low-probability edge case for semi-private deployment, or add retry logic that rechecks after insert
**Warning signs:** Multiple Admin users when only one should exist

### Pitfall 5: Password Too Short Validation Not Showing
**What goes wrong:** User submits short password, unclear error
**Why it happens:** CSLA MinLength rule message not displayed properly
**How to avoid:** Add clear MessageText to MinLength rule, display BrokenRulesCollection in UI
**Warning signs:** Silent validation failure, IsSavable returns false with no visible message

## Code Examples

Verified patterns from existing codebase:

### Player DTO with New Fields
```csharp
// Source: Threa.Dal/Dto/Player.cs - TO BE EXTENDED
namespace Threa.Dal.Dto;

public class Player
{
    public int Id { get; set; } = -1;
    public string Name { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;  // This is actually the USERNAME
    public string ImageUrl { get; set; } = string.Empty;
    public string? Roles { get; set; }
    public bool IsEnabled { get; set; } = true;

    // NEW for Phase 8 (AUTH-01)
    public string SecretQuestion { get; set; } = string.Empty;
    public string SecretAnswer { get; set; } = string.Empty;  // Stored lowercase, trimmed
}
```

### Existing Password Hashing Pattern
```csharp
// Source: Threa.Dal.SqlLite/PlayerDal.cs lines 214-224
if (string.IsNullOrWhiteSpace(existingPlayer.Salt))
    existingPlayer.Salt = BCrypt.Net.BCrypt.GenerateSalt(12);

var oldHashedPassword = BCrypt.Net.BCrypt.HashPassword(oldPassword, existingPlayer.Salt);
if (existingPlayer.HashedPassword != oldHashedPassword)
    throw new OperationFailedException($"Old password doesn't match...");

existingPlayer.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, existingPlayer.Salt);
```

### Existing Role Parsing Pattern
```csharp
// Source: AdminUserEdit.cs lines 86-90
var roles = data.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
    ?? Array.Empty<string>();
IsAdministrator = roles.Contains(Roles.Administrator, StringComparer.OrdinalIgnoreCase);
IsGameMaster = roles.Contains(Roles.GameMaster, StringComparer.OrdinalIgnoreCase);
IsPlayer = roles.Contains(Roles.Player, StringComparer.OrdinalIgnoreCase);
```

### Existing DataPortalException Handling Pattern
```csharp
// Source: Login.razor lines 74-78
catch (DataPortalException ex)
{
    Message = $"{ex.BusinessException.GetType().Name} {ex.BusinessExceptionMessage}";
}
```

### CSLA MinLength Rule
```csharp
// Source: CSLA CommonRules namespace
// Password minimum length validation
BusinessRules.AddRule(new Csla.Rules.CommonRules.MinLength(PasswordProperty, 6)
{
    MessageText = "Password must be at least 6 characters"
});
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| MD5/SHA1 password hashing | BCrypt with work factor | Ongoing | More secure, adaptive cost |
| Separate salt storage | Salt embedded in BCrypt hash | BCrypt.Net handles | Simpler, less error-prone |
| Email-based recovery | Secret Q&A recovery | v1.1 decision | No email infrastructure needed |

**Deprecated/outdated:**
- MD5 password hashing: Replaced by BCrypt in this codebase
- Plain text secret answers: Store case-insensitive version only

## Existing Code Analysis

### What Already Exists (DO NOT RECREATE)

| Component | Location | Status | Notes |
|-----------|----------|--------|-------|
| Player DTO | Threa.Dal/Dto/Player.cs | Needs extension | Add SecretQuestion, SecretAnswer |
| IPlayerDal | Threa.Dal/IPlayerDal.cs | Complete | All needed methods exist |
| PlayerDal (MockDb) | Threa.Dal.MockDb/PlayerDal.cs | Needs update | Add secret fields to JSON |
| PlayerDal (SQLite) | Threa.Dal.SqlLite/PlayerDal.cs | Needs update | Add secret fields to JSON |
| Roles constants | GameMechanics/Player/Roles.cs | Complete | Administrator, GameMaster, Player |
| BCrypt integration | GameMechanics.csproj | Complete | Version 4.0.3 |
| Login page | Threa/Threa/Components/Pages/Login.razor | Complete | Pattern reference |
| DuplicateKeyException | Threa.Dal/DuplicateKeyException.cs | Complete | For unique constraint errors |

### What Needs to Be Created

| Component | Location | Priority | Dependencies |
|-----------|----------|----------|--------------|
| SecretQuestion/Answer on Player DTO | Threa.Dal/Dto/Player.cs | High | None |
| UserRegistration business object | GameMechanics/Player/UserRegistration.cs | High | Updated DTO |
| Register.razor page | Threa/Threa/Components/Pages/Register.razor | High | UserRegistration BO |
| RegistrationTests | GameMechanics.Test/RegistrationTests.cs | Medium | All above |

### Database Schema Considerations

SQLite stores Player as JSON blob with separate Email column for uniqueness index:
```sql
CREATE TABLE Players (
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Email TEXT NOT NULL,  -- This is the USERNAME, not email address
    Json TEXT             -- Contains all other fields as JSON
);
CREATE UNIQUE INDEX IX_Players_Email ON Players (Email);
```

New fields (SecretQuestion, SecretAnswer) will be stored in the JSON blob automatically - no schema migration needed.

## Open Questions

Things that couldn't be fully resolved:

1. **Auto-login After Registration**
   - What we know: Current design redirects to login page after registration
   - What's unclear: Whether UX would benefit from auto-login
   - Recommendation: Keep simple redirect to login; auto-login adds complexity (SignInAsync requires HttpContext) and security considerations

2. **Username Character Restrictions**
   - What we know: Currently no validation on username format
   - What's unclear: Whether to enforce alphanumeric-only, length limits, etc.
   - Recommendation: Add MinLength(3), MaxLength(50) rules; avoid special character restrictions for now

3. **Secret Question Suggestions**
   - What we know: Free-form text field for question
   - What's unclear: Whether to provide dropdown of common questions
   - Recommendation: Free-form is simpler; users know their own memorable questions

## Sources

### Primary (HIGH confidence)
- S:\src\rdl\threa\GameMechanics\Player\AdminUserEdit.cs - Role assignment, password hashing patterns
- S:\src\rdl\threa\Threa.Dal.SqlLite\PlayerDal.cs - Database operations, BCrypt verification
- S:\src\rdl\threa\Threa\Threa\Components\Pages\Login.razor - Auth UI pattern
- S:\src\rdl\threa\Threa.Dal\Dto\Player.cs - Current DTO structure
- S:\src\rdl\threa\.planning\REQUIREMENTS.md - AUTH-01, AUTH-02, AUTH-03 requirements

### Secondary (MEDIUM confidence)
- S:\src\rdl\threa\.planning\ROADMAP.md - Phase 8 definition and success criteria
- S:\src\rdl\threa\.planning\codebase\INTEGRATIONS.md - BCrypt integration details
- S:\src\rdl\threa\.planning\PROJECT.md - Key decisions (secret Q&A, first-user-admin)

### Tertiary (LOW confidence)
- None required - all patterns verified from existing codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in use
- Architecture: HIGH - Follows existing CSLA patterns exactly
- Pitfalls: HIGH - Based on existing codebase review and common auth issues
- Data layer: HIGH - Direct inspection of current schema and code

**Research date:** 2026-01-26
**Valid until:** 2026-02-26 (stable patterns, unlikely to change)
