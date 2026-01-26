# Phase 9: Password Recovery - Research

**Researched:** 2026-01-26
**Domain:** Password Recovery, Multi-Step Wizard, Secret Answer Validation, Time-Based Lockout
**Confidence:** HIGH

## Summary

Phase 9 implements self-service password recovery using a 3-step wizard flow: (1) enter username, (2) answer secret question, (3) set new password. The research found substantial infrastructure already exists from Phase 8:

1. **Player DTO has SecretQuestion and SecretAnswer fields** - Added in Phase 8, stored lowercase/trimmed
2. **IPlayerDal.GetPlayerByEmailAsync(username)** - Returns Player with secret Q&A for validation
3. **BCrypt.Net-Next 4.0.3** - Already integrated for password hashing
4. **CSLA CommandBase pattern** - Established via UserValidation.cs for execute-only operations
5. **Login.razor and Register.razor** - Existing SSR form patterns to follow
6. **Radzen.Blazor 8.4.2** - Includes RadzenSteps component for multi-step wizard UI

The primary work is:
- Create PasswordRecovery.razor page with 3-step wizard UI (RadzenSteps or manual step control)
- Create PasswordRecoveryValidation CSLA command for secret answer validation
- Create PasswordReset CSLA business object for password update
- Implement time-based lockout using IMemoryCache for failed attempts

**Primary recommendation:** Use a single PasswordRecovery.razor page with component-level step state management. The page handles all three steps with conditional rendering based on step number. Use IMemoryCache for tracking failed attempts with automatic expiry for lockout.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object framework | Project standard, all BOs use CSLA |
| BCrypt.Net-Next | 4.0.3 | Password hashing | Already in use, 12-round cost factor |
| Radzen.Blazor | 8.4.2 | UI components (RadzenSteps, RadzenButton) | Project standard, provides wizard components |
| Microsoft.Extensions.Caching.Memory | (built-in) | IMemoryCache for lockout tracking | ASP.NET Core standard, already available |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Text.Json | (built-in) | JSON serialization | SQLite DAL stores Player as JSON |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| RadzenSteps wizard | Manual step div switching | RadzenSteps adds visual polish; manual is simpler for SSR-only pages |
| IMemoryCache lockout | Database lockout fields | Database adds persistence across restarts but complexity; IMemoryCache simpler for this use case |
| Single page with steps | Multi-page flow | Single page is simpler, no inter-page state needed |

**Installation:**
No additional packages needed - all dependencies already in project.

## Architecture Patterns

### Recommended Project Structure
```
GameMechanics/
└── Player/
    ├── UserRegistration.cs        # EXISTS - Registration
    ├── UserValidation.cs          # EXISTS - Login validation (pattern reference)
    ├── PasswordRecoveryValidation.cs   # TO CREATE - Validate secret answer
    └── PasswordReset.cs           # TO CREATE - Reset password after validation

Threa/Threa/
├── Components/Pages/
│   ├── Login.razor                # EXISTS - Pattern reference
│   ├── Register.razor             # EXISTS - Pattern reference
│   └── ForgotPassword.razor       # TO CREATE - Password recovery wizard
└── Services/
    └── RecoveryAttemptService.cs  # TO CREATE - Track failed attempts (IMemoryCache)

GameMechanics.Test/
└── PasswordRecoveryTests.cs       # TO CREATE - Unit tests
```

### Pattern 1: Multi-Step Wizard with Manual State
**What:** Single page with step-based conditional rendering
**When to use:** SSR pages that need step navigation without full page reload
**Why chosen over RadzenSteps:** SSR pages with form posts work better with manual step control; RadzenSteps is designed for interactive mode
**Example:**
```razor
@page "/forgot-password"

<PageTitle>Threa - Password Recovery</PageTitle>

<h1>Password Recovery</h1>

<div class="mb-3">
    <strong>Step @CurrentStep of 3</strong>
</div>

@if (!string.IsNullOrEmpty(ErrorMessage))
{
    <div class="alert alert-danger">
        @ErrorMessage
        <button type="button" class="btn-close" @onclick="DismissError"></button>
    </div>
}

@if (CurrentStep == 1)
{
    <!-- Step 1: Enter Username -->
    <EditForm Model="Step1Model" OnSubmit="ValidateUsername" FormName="step1form">
        <div class="mb-3">
            <label class="form-label">Username</label>
            <InputText class="form-control" @bind-Value="Step1Model!.Username" />
        </div>
        <button type="submit" class="btn btn-primary">Continue</button>
        <a href="/login" class="btn btn-link">Back to Login</a>
    </EditForm>
}
else if (CurrentStep == 2)
{
    <!-- Step 2: Answer Secret Question -->
    <EditForm Model="Step2Model" OnSubmit="ValidateAnswer" FormName="step2form">
        <div class="mb-3">
            <label class="form-label">@SecretQuestion</label>
            <InputText class="form-control" @bind-Value="Step2Model!.Answer" />
        </div>
        <button type="submit" class="btn btn-primary">Continue</button>
        <button type="button" class="btn btn-secondary" @onclick="GoToStep1">Back</button>
    </EditForm>
}
else if (CurrentStep == 3)
{
    <!-- Step 3: Set New Password -->
    <EditForm Model="Step3Model" OnSubmit="ResetPassword" FormName="step3form">
        <div class="mb-3">
            <label class="form-label">New Password</label>
            <InputText type="password" class="form-control" @bind-Value="Step3Model!.NewPassword" />
            <small class="form-text text-muted">Minimum 6 characters</small>
        </div>
        <div class="mb-3">
            <label class="form-label">Confirm Password</label>
            <InputText type="password" class="form-control" @bind-Value="Step3Model!.ConfirmPassword" />
        </div>
        <button type="submit" class="btn btn-primary">Reset Password</button>
        <button type="button" class="btn btn-secondary" @onclick="GoToStep2">Back</button>
    </EditForm>
}
else if (CurrentStep == 4)
{
    <!-- Success State -->
    <div class="alert alert-success">
        Password reset successful! <a href="/login">Click here to log in.</a>
    </div>
}

@code {
    private int CurrentStep { get; set; } = 1;
    private string ErrorMessage { get; set; } = "";
    private string SecretQuestion { get; set; } = "";
    private int ValidatedUserId { get; set; }

    // Model classes and methods...
}
```

### Pattern 2: CSLA CommandBase for Validation
**What:** CommandBase for operations that validate but don't persist
**When to use:** Secret answer validation (returns result, doesn't save)
**Example:**
```csharp
// Based on existing UserValidation.cs pattern
[Serializable]
public class PasswordRecoveryValidation : CommandBase<PasswordRecoveryValidation>
{
    public static readonly PropertyInfo<bool> IsValidProperty = RegisterProperty<bool>(nameof(IsValid));
    public bool IsValid
    {
        get => ReadProperty(IsValidProperty);
        private set => LoadProperty(IsValidProperty, value);
    }

    public static readonly PropertyInfo<int> UserIdProperty = RegisterProperty<int>(nameof(UserId));
    public int UserId
    {
        get => ReadProperty(UserIdProperty);
        private set => LoadProperty(UserIdProperty, value);
    }

    public static readonly PropertyInfo<string> SecretQuestionProperty = RegisterProperty<string>(nameof(SecretQuestion));
    public string SecretQuestion
    {
        get => ReadProperty(SecretQuestionProperty);
        private set => LoadProperty(SecretQuestionProperty, value);
    }

    [Execute]
    private async Task Execute(string username, string? answer, [Inject] IPlayerDal dal)
    {
        var player = await dal.GetPlayerByEmailAsync(username);

        if (player == null)
        {
            // User not found - but don't reveal this
            IsValid = false;
            return;
        }

        // Load secret question (always returned for display)
        SecretQuestion = player.SecretQuestion;
        UserId = player.Id;

        // If no answer provided, just fetching question
        if (string.IsNullOrEmpty(answer))
        {
            IsValid = false;
            return;
        }

        // Validate answer (case-insensitive, trimmed)
        var normalizedAnswer = answer.Trim().ToLowerInvariant();
        IsValid = player.SecretAnswer == normalizedAnswer;
    }
}
```

### Pattern 3: Password Reset Business Object
**What:** BusinessBase for password update operation
**When to use:** After successful secret answer validation
**Example:**
```csharp
[Serializable]
public class PasswordReset : BusinessBase<PasswordReset>
{
    public static readonly PropertyInfo<int> UserIdProperty = RegisterProperty<int>(nameof(UserId));
    public int UserId
    {
        get => GetProperty(UserIdProperty);
        set => SetProperty(UserIdProperty, value);
    }

    public static readonly PropertyInfo<string> NewPasswordProperty = RegisterProperty<string>(nameof(NewPassword));
    public string NewPassword
    {
        get => GetProperty(NewPasswordProperty);
        set => SetProperty(NewPasswordProperty, value);
    }

    protected override void AddBusinessRules()
    {
        base.AddBusinessRules();
        BusinessRules.AddRule(new Csla.Rules.CommonRules.Required(NewPasswordProperty)
            { MessageText = "Password is required" });
        BusinessRules.AddRule(new Csla.Rules.CommonRules.MinLength(NewPasswordProperty, 6)
            { MessageText = "Password must be at least 6 characters" });
    }

    [Create]
    private void Create() { }

    [Update]
    private async Task Update([Inject] IPlayerDal dal)
    {
        var player = await dal.GetPlayerAsync(UserId)
            ?? throw new InvalidOperationException("User not found");

        // Hash new password
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        player.HashedPassword = BCrypt.Net.BCrypt.HashPassword(NewPassword, salt);
        player.Salt = salt;

        await dal.SavePlayerAsync(player);
    }
}
```

### Pattern 4: IMemoryCache for Lockout Tracking
**What:** In-memory tracking of failed attempts with automatic expiry
**When to use:** Rate limiting failed secret answer attempts
**Example:**
```csharp
public interface IRecoveryAttemptService
{
    bool IsLockedOut(string username);
    int GetRemainingAttempts(string username);
    void RecordFailedAttempt(string username);
    void ClearAttempts(string username);
    TimeSpan? GetLockoutTimeRemaining(string username);
}

public class RecoveryAttemptService : IRecoveryAttemptService
{
    private readonly IMemoryCache _cache;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(30);

    public RecoveryAttemptService(IMemoryCache cache)
    {
        _cache = cache;
    }

    private string GetAttemptsKey(string username) => $"recovery_attempts_{username.ToLowerInvariant()}";
    private string GetLockoutKey(string username) => $"recovery_lockout_{username.ToLowerInvariant()}";

    public bool IsLockedOut(string username)
    {
        return _cache.TryGetValue(GetLockoutKey(username), out _);
    }

    public int GetRemainingAttempts(string username)
    {
        var key = GetAttemptsKey(username);
        var attempts = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = AttemptWindow;
            return 0;
        });
        return Math.Max(0, MaxAttempts - attempts);
    }

    public void RecordFailedAttempt(string username)
    {
        var key = GetAttemptsKey(username);
        var attempts = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = AttemptWindow;
            return 0;
        });

        attempts++;
        _cache.Set(key, attempts, AttemptWindow);

        if (attempts >= MaxAttempts)
        {
            // Lock out the user
            _cache.Set(GetLockoutKey(username), true, LockoutDuration);
        }
    }

    public void ClearAttempts(string username)
    {
        _cache.Remove(GetAttemptsKey(username));
        _cache.Remove(GetLockoutKey(username));
    }

    public TimeSpan? GetLockoutTimeRemaining(string username)
    {
        // IMemoryCache doesn't expose expiration time directly
        // Return null if not locked out, or estimated remaining time
        if (!IsLockedOut(username)) return null;
        return LockoutDuration; // Approximate
    }
}
```

### Anti-Patterns to Avoid
- **Don't reveal username existence:** Use generic message "If that username exists..." when username not found
- **Don't store lockout in component state:** Component state is per-connection; use IMemoryCache for server-wide lockout
- **Don't use RadzenSteps for SSR forms:** RadzenSteps works best with interactive mode; use manual step switching for SSR
- **Don't clear lockout on page refresh:** Lockout must persist in IMemoryCache, not component state
- **Don't hash secret answer:** Secret answer is stored as lowercase/trimmed string, not hashed (Phase 8 decision)

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Password hashing | Custom hash | BCrypt.Net.HashPassword with 12-round salt | Already integrated, cryptographically secure |
| User lookup | New DAL method | GetPlayerByEmailAsync(username) | Existing method returns all needed fields |
| Form validation | Client-side only | CSLA BusinessRules | Server-side validation, integrated with UI |
| Attempt tracking | Database table | IMemoryCache | Simpler, auto-expiry, no schema changes |
| Timing-safe comparison | String.Equals | Store answer lowercase, compare lowercase | Consistency with Phase 8 implementation |

**Key insight:** Phase 8 laid all the groundwork. SecretQuestion/SecretAnswer fields exist, BCrypt is integrated, and the DAL methods already return everything needed for password recovery.

## Common Pitfalls

### Pitfall 1: Username Enumeration
**What goes wrong:** Revealing whether username exists through different error messages
**Why it happens:** Showing "Username not found" vs "Wrong answer" leaks information
**How to avoid:** Always show generic message after Step 1: "If that username exists, the secret question will be shown." Then either show question (exists) or stay on Step 1 with generic message (doesn't exist but looks like success path)
**Warning signs:** Different UI behavior for existing vs non-existing usernames
**CONTEXT.md decision:** Generic success message to prevent enumeration

### Pitfall 2: Lockout State Lost on Server Restart
**What goes wrong:** IMemoryCache clears on app restart, allowing more attempts
**Why it happens:** In-memory storage is volatile
**How to avoid:** Accept this limitation for semi-private deployment; document that lockout resets on restart. Alternative: Add lockout fields to Player DTO if persistence needed
**Warning signs:** Attackers timing attempts around deployments
**Acceptable for:** Semi-private deployment with known user group

### Pitfall 3: Component State vs Server State
**What goes wrong:** Step state resets correctly, but lockout doesn't persist
**Why it happens:** Mixing component-local state (steps) with server-wide state (lockout)
**How to avoid:** Step state in component (resets on refresh per CONTEXT.md), lockout in IMemoryCache (persists across requests)
**Warning signs:** User can bypass lockout by refreshing page

### Pitfall 4: Case-Sensitive Answer Comparison
**What goes wrong:** "My Dog" doesn't match stored "my dog"
**Why it happens:** Forgetting to normalize before comparison
**How to avoid:** Always Trim().ToLowerInvariant() on input answer before comparing to stored answer
**Warning signs:** Users "know" their answer but validation fails
**Note:** Phase 8 stores answer as lowercase/trimmed; match that normalization

### Pitfall 5: Password Confirmation Mismatch
**What goes wrong:** User enters mismatched passwords, unclear error
**Why it happens:** Not validating confirmation field matches
**How to avoid:** Add client-side validation that NewPassword == ConfirmPassword before submit
**Warning signs:** User gets to login page but password doesn't work
**CONTEXT.md decision:** Require confirmation field, validate match

### Pitfall 6: Real-Time Validation Complexity in SSR
**What goes wrong:** Real-time validation not working in SSR page
**Why it happens:** SSR pages don't have real-time interactivity
**How to avoid:** Use @oninput with NavigationManager.NavigateTo workaround, or accept submit-time validation for SSR
**Warning signs:** Validation messages don't appear until form submit
**CONTEXT.md decision:** Real-time validation as user types (may require interactive mode or JavaScript)

## Code Examples

Verified patterns from existing codebase:

### Username Lookup Pattern
```csharp
// Source: IPlayerDal.GetPlayerByEmailAsync
// Returns null if not found, Player if found
var player = await dal.GetPlayerByEmailAsync(username);
if (player == null)
{
    // Handle not found - but don't reveal to user
}
```

### Secret Answer Comparison
```csharp
// Source: Phase 8 research - answer stored lowercase/trimmed
var normalizedInput = userAnswer.Trim().ToLowerInvariant();
bool isMatch = player.SecretAnswer == normalizedInput;
```

### Password Update Pattern
```csharp
// Source: Threa.Dal.SqlLite/PlayerDal.cs ChangePassword method
var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
player.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, salt);
player.Salt = salt;
await dal.SavePlayerAsync(player);
```

### CommandBase Execute Pattern
```csharp
// Source: GameMechanics/Player/UserValidation.cs
[Execute]
private async Task Execute(string param1, string param2, [Inject] IPlayerDal dal)
{
    // Validation logic
    // Set output properties
}
```

### SSR Form Pattern
```razor
// Source: Register.razor
<EditForm Model="FormModel" OnSubmit="HandleSubmit" FormName="formname">
    <InputText @bind-Value="FormModel!.Field" />
    <button type="submit">Submit</button>
</EditForm>

@code {
    [SupplyParameterFromForm]
    public FormModel? FormModel { get; set; }

    protected override void OnInitialized()
    {
        FormModel ??= new();
    }
}
```

### Error Display Pattern
```razor
// Source: Register.razor
@if (!string.IsNullOrEmpty(Message))
{
    <div class="alert alert-danger">@Message</div>
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Email-based recovery | Secret Q&A recovery | v1.1 decision | No email infrastructure needed |
| Database lockout | IMemoryCache lockout | Current recommendation | Simpler, no schema changes |
| Multi-page wizard | Single-page step wizard | Modern SPA pattern | Better UX, no page reload |

**Deprecated/outdated:**
- Email-based password recovery: Not applicable for this project (no email capability)
- Security questions with limited options: Free-form questions preferred

## Open Questions

Things that couldn't be fully resolved:

1. **Real-Time Validation in SSR**
   - What we know: CONTEXT.md requests real-time validation as user types
   - What's unclear: SSR pages don't support real-time interactivity without JavaScript or switching to interactive mode
   - Recommendation: Either (a) make the page InteractiveServer for real-time validation, or (b) use submit-time validation with clear error messages. Recommend (a) for better UX matching CONTEXT.md requirement.

2. **Exact Lockout Duration**
   - What we know: CONTEXT.md says "15-30 minutes"
   - What's unclear: Exact value to use
   - Recommendation: Use 15 minutes (lower end) - semi-private deployment doesn't need maximum security

3. **Exact Attempt Limit**
   - What we know: CONTEXT.md says "3-5 attempts"
   - What's unclear: Exact value to use
   - Recommendation: Use 5 attempts (upper end) - more user-friendly while still providing protection

## Sources

### Primary (HIGH confidence)
- `S:\src\rdl\threa\GameMechanics\Player\UserValidation.cs` - CSLA CommandBase pattern for validation
- `S:\src\rdl\threa\GameMechanics\Player\UserRegistration.cs` - CSLA BusinessBase pattern for registration
- `S:\src\rdl\threa\Threa\Threa\Components\Pages\Register.razor` - SSR form pattern
- `S:\src\rdl\threa\Threa\Threa\Components\Pages\Login.razor` - SSR form pattern
- `S:\src\rdl\threa\Threa.Dal\Dto\Player.cs` - Player DTO with SecretQuestion/SecretAnswer
- `S:\src\rdl\threa\Threa.Dal\IPlayerDal.cs` - DAL interface with GetPlayerByEmailAsync

### Secondary (MEDIUM confidence)
- [Radzen Blazor Steps documentation](https://blazor.radzen.com/steps) - RadzenSteps component reference
- [Microsoft IMemoryCache documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory) - Memory caching patterns
- `.planning/phases/08-registration-foundation/08-RESEARCH.md` - Phase 8 patterns and decisions

### Tertiary (LOW confidence)
- None required - all patterns verified from existing codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in use
- Architecture: HIGH - Follows existing CSLA and SSR patterns exactly
- Pitfalls: HIGH - Based on existing codebase review and CONTEXT.md decisions
- Lockout implementation: MEDIUM - IMemoryCache is standard but specific implementation untested

**Research date:** 2026-01-26
**Valid until:** 2026-02-26 (stable patterns, unlikely to change)
