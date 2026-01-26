# Phase 10: Admin User Management - Research

**Researched:** 2026-01-26
**Domain:** Admin User Management, CSLA Business Objects, Role Assignment, Last-Admin Protection
**Confidence:** HIGH

## Summary

Phase 10 implements admin-only user management for viewing, enabling/disabling, and assigning roles to users. Research found that **most infrastructure already exists**:

1. **AdminUserList and AdminUserInfo** CSLA objects exist for listing users
2. **AdminUserEdit** exists with IsEnabled, IsAdministrator, IsGameMaster, IsPlayer properties
3. **Users.razor** and **UserEdit.razor** pages exist with basic functionality
4. **IPlayerDal.GetAllPlayersAsync()** provides data for enabled-admin counting
5. **QuickGrid with sortable columns** pattern established in GameMaster pages
6. **Radzen DialogService** available for confirmation dialogs and warnings

The primary work is:
- Add business rule for last-admin protection
- Enhance UserEdit.razor to be a modal instead of separate page
- Add warning dialogs for self-disable and self-demote scenarios
- Add disabled status icon indicator to Users.razor list
- Add sortable enabled/disabled column

**Primary recommendation:** Enhance the existing AdminUserEdit business object with a CSLA business rule for last-admin protection, then convert UserEdit to a modal dialog. Use existing Radzen DialogService patterns from TabPlayInventory.razor.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object framework | Project standard, AdminUserEdit already exists |
| Microsoft.AspNetCore.Components.QuickGrid | 10.0.1 | Data grid with sorting | Already used in Users.razor |
| Radzen.Blazor | 8.4.2 | UI components, DialogService | Already used for modal dialogs |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Bootstrap Icons | 1.11.3 | Icon library | Disabled status indicator |

**Installation:**
No additional packages needed - all dependencies already in project.

## Architecture Patterns

### Recommended Project Structure
```
GameMechanics/
└── Player/
    ├── AdminUserEdit.cs       # ENHANCE - Add last-admin rule
    ├── AdminUserInfo.cs       # EXISTS - No changes needed
    ├── AdminUserList.cs       # EXISTS - No changes needed
    └── Roles.cs               # EXISTS - No changes needed

Threa.Dal/
├── IPlayerDal.cs              # ENHANCE - Add CountEnabledAdminsAsync()
└── Dto/Player.cs              # EXISTS - No changes needed

Threa.Dal.MockDb/
└── PlayerDal.cs               # ENHANCE - Implement CountEnabledAdminsAsync()

Threa.Dal.SqlLite/
└── PlayerDal.cs               # ENHANCE - Implement CountEnabledAdminsAsync()

Threa/Threa.Client/Components/Pages/Admin/
├── Users.razor                # ENHANCE - Modal dialog, icons, sorting
└── UserEdit.razor             # CONVERT - To modal component (or inline in Users.razor)
```

### Pattern 1: CSLA Business Rule for Last-Admin Protection
**What:** Synchronous business rule that validates the system has at least one enabled admin
**When to use:** When saving AdminUserEdit with IsEnabled=false or IsAdministrator=false
**Example:**
```csharp
// Source: Based on existing ItemTypeRequiredRule pattern in ItemTemplateEdit.cs
/// <summary>
/// Validates that disabling user or removing Admin role doesn't leave system without admins.
/// Uses injected DAL to count enabled admins.
/// </summary>
public class LastAdminProtectionRule : Csla.Rules.BusinessRuleAsync
{
    private readonly IPropertyInfo _isEnabledProperty;
    private readonly IPropertyInfo _isAdministratorProperty;

    public LastAdminProtectionRule(
        IPropertyInfo idProperty,
        IPropertyInfo isEnabledProperty,
        IPropertyInfo isAdministratorProperty)
        : base(idProperty)
    {
        _isEnabledProperty = isEnabledProperty;
        _isAdministratorProperty = isAdministratorProperty;
        InputProperties.Add(idProperty);
        InputProperties.Add(isEnabledProperty);
        InputProperties.Add(isAdministratorProperty);
        IsAsync = true;
        ProvideTargetWhenAsync = true;
    }

    protected override async Task ExecuteAsync(IRuleContext context)
    {
        var target = (AdminUserEdit)context.Target;
        var isEnabled = (bool)context.InputPropertyValues[_isEnabledProperty]!;
        var isAdmin = (bool)context.InputPropertyValues[_isAdministratorProperty]!;

        // Only check if this user WAS an enabled admin and is being changed
        if (!isEnabled || !isAdmin)
        {
            // Inject DAL via ApplicationContext.CreateInstanceDI
            var dal = ApplicationContext.CreateInstanceDI<IPlayerDal>();
            var enabledAdminCount = await dal.CountEnabledAdminsAsync();

            // If this user is the only enabled admin and being disabled/demoted, block
            // We check if count is 1 AND this user is currently an enabled admin in DB
            if (enabledAdminCount <= 1)
            {
                var currentUser = await dal.GetPlayerAsync(target.Id);
                var currentRoles = currentUser?.Roles?.Split(',') ?? Array.Empty<string>();
                var wasEnabledAdmin = currentUser?.IsEnabled == true &&
                    currentRoles.Contains(Roles.Administrator, StringComparer.OrdinalIgnoreCase);

                if (wasEnabledAdmin)
                {
                    context.AddErrorResult("System must have at least one enabled administrator.");
                }
            }
        }
    }
}
```

### Pattern 2: Modal Dialog with Radzen DialogService
**What:** Open a Razor component as a modal dialog
**When to use:** Edit user from list without navigation
**Example:**
```razor
@* Based on existing TabPlayInventory.razor pattern *@
@inject DialogService DialogService

<button class="btn btn-sm btn-outline-primary" @onclick="() => EditUser(user.Id)">
    <i class="bi bi-pencil"></i>
</button>

@code {
    private async Task EditUser(int userId)
    {
        var result = await DialogService.OpenAsync<UserEditModal>(
            "Edit User",
            new Dictionary<string, object> { { "UserId", userId } },
            new DialogOptions
            {
                Width = "500px",
                CloseDialogOnOverlayClick = false,
                ShowClose = true
            });

        if (result == true)
        {
            // Refresh the user list
            await vm.RefreshAsync(() => userListPortal.FetchAsync());
            users = vm.Model?.AsQueryable();
            StateHasChanged();
        }
    }
}
```

### Pattern 3: QuickGrid with Sortable Columns
**What:** Add sorting capability to grid columns
**When to use:** User list with sortable status
**Example:**
```razor
@* Source: Based on existing GameMaster/Characters.razor pattern *@
<QuickGrid Items="users">
    <PropertyColumn Title="Username" Property="@(p => p.Email)" Sortable="true" />
    <PropertyColumn Title="Display Name" Property="@(p => p.Name)" Sortable="true" />
    <PropertyColumn Title="Roles" Property="@(p => p.Roles)" />
    <TemplateColumn Title="Status" Sortable="true" SortBy="@enabledSort">
        @if (context.IsEnabled)
        {
            <span class="badge bg-success">Enabled</span>
        }
        else
        {
            <span><i class="bi bi-lock-fill text-danger me-1"></i>Disabled</span>
        }
    </TemplateColumn>
    <TemplateColumn Title="Actions">
        <button class="btn btn-sm btn-outline-primary" @onclick="() => EditUser(context.Id)">
            Edit
        </button>
    </TemplateColumn>
</QuickGrid>

@code {
    // Custom sort for enabled/disabled
    GridSort<AdminUserInfo> enabledSort = GridSort<AdminUserInfo>
        .ByDescending(p => p.IsEnabled);
}
```

### Pattern 4: Warning Confirmation Dialog
**What:** Show warning but allow action to proceed
**When to use:** Admin disabling own account or removing own Admin role
**Example:**
```csharp
// Check if admin is about to disable themselves
if (CurrentUserId == editModel.Id && !editModel.IsEnabled)
{
    var confirmed = await DialogService.Confirm(
        "You're about to disable your own account. You will be logged out immediately. Continue?",
        "Warning",
        new ConfirmOptions
        {
            OkButtonText = "Disable Anyway",
            CancelButtonText = "Cancel"
        });
    if (confirmed != true) return;
}

// Check if admin is about to remove own Admin role
if (CurrentUserId == editModel.Id && !editModel.IsAdministrator)
{
    var confirmed = await DialogService.Confirm(
        "You're about to remove your own Admin access. You won't be able to access this page after saving. Continue?",
        "Warning",
        new ConfirmOptions
        {
            OkButtonText = "Remove Anyway",
            CancelButtonText = "Cancel"
        });
    if (confirmed != true) return;
}
```

### Anti-Patterns to Avoid
- **Don't navigate to separate page for edit:** Use modal dialog per CONTEXT.md decision
- **Don't allow last admin removal without hard block:** Business rule must prevent, not just warn
- **Don't show User role checkbox:** User role is implicit/always present per CONTEXT.md
- **Don't require confirmation for enable/disable:** Action is reversible, no confirmation needed per CONTEXT.md

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Modal dialogs | Custom modal HTML | Radzen DialogService.OpenAsync | Already integrated, handles focus, backdrop |
| Confirmation dialogs | Custom confirm | Radzen DialogService.Confirm | Already used in TabPlayInventory.razor |
| Sorting data grid | Manual sort logic | QuickGrid Sortable property | Built-in, handles UI indicators |
| Admin count query | LINQ over all users | DAL method CountEnabledAdminsAsync | More efficient, DB-level count |

**Key insight:** The existing AdminUserEdit already has all the properties needed. Primary work is enhancing the UI to use modals and adding the last-admin protection rule.

## Common Pitfalls

### Pitfall 1: Race Condition on Last Admin Check
**What goes wrong:** Two admins simultaneously remove each other's Admin role
**Why it happens:** Check-then-save without locking
**How to avoid:** Accept as low-probability edge case; the business rule will catch the first save, and the second save's rule check will see the updated state
**Warning signs:** Database ends up with zero enabled admins

### Pitfall 2: Async Rule Not Awaited
**What goes wrong:** Save proceeds before async validation completes
**Why it happens:** CSLA requires proper async rule setup with IsAsync = true
**How to avoid:** Use BusinessRuleAsync base class with IsAsync = true and ProvideTargetWhenAsync = true
**Warning signs:** Validation passes even when it should fail

### Pitfall 3: Modal Not Refreshing List
**What goes wrong:** User saves changes but list shows old data
**Why it happens:** Forgetting to refresh after modal closes
**How to avoid:** Check dialog result and refresh vm.RefreshAsync on success
**Warning signs:** User has to manually refresh page to see changes

### Pitfall 4: Current User ID Not Available
**What goes wrong:** Self-disable/demote warning can't determine if it's the current user
**Why it happens:** User identity not passed to component
**How to avoid:** Inject AuthenticationStateProvider and get current user's Id claim
**Warning signs:** Warnings never appear even when admin is editing their own account

### Pitfall 5: DisplayName Field Confusion
**What goes wrong:** Email field edited instead of display name
**Why it happens:** Email field in DTO stores username, Name field stores display name
**How to avoid:** Only expose Name property in edit modal; Email/Username is read-only
**Warning signs:** User's login credentials change unexpectedly

## Code Examples

Verified patterns from existing codebase:

### Existing AdminUserEdit Business Object
```csharp
// Source: GameMechanics/Player/AdminUserEdit.cs
// Already has: Id, Email (username), Name, IsEnabled, IsAdministrator, IsGameMaster, IsPlayer, NewPassword
// Role parsing: lines 86-90
var roles = data.Roles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
    ?? Array.Empty<string>();
IsAdministrator = roles.Contains(Roles.Administrator, StringComparer.OrdinalIgnoreCase);
IsGameMaster = roles.Contains(Roles.GameMaster, StringComparer.OrdinalIgnoreCase);
IsPlayer = roles.Contains(Roles.Player, StringComparer.OrdinalIgnoreCase);
```

### Existing Users.razor List Page
```razor
// Source: Threa/Threa.Client/Components/Pages/Admin/Users.razor
// Already has: QuickGrid with Email, Name, Roles, Status columns
// Status template showing Enabled/Disabled badges
// Edit link navigation (to be converted to modal)
```

### Existing DialogService Confirm Pattern
```csharp
// Source: Threa/Threa.Client/Components/Pages/GamePlay/TabPlayInventory.razor lines 1215-1222
var confirmed = await DialogService.Confirm(
    $"Are you sure you want to drop {itemName}? This cannot be undone.",
    "Drop Item",
    new ConfirmOptions
    {
        OkButtonText = "Drop",
        CancelButtonText = "Cancel"
    });
```

### Existing DialogService.OpenAsync Pattern
```razor
// Source: TabPlayInventory.razor lines 1245-1254
var result = await DialogService.OpenAsync("Drop Container?", ds =>
    @<div>
        <p>@message</p>
        <div class="d-flex gap-2 justify-content-end mt-3">
            <button class="btn btn-secondary" @onclick="() => ds.Close(false)">Cancel</button>
            <button class="btn btn-outline-primary" @onclick='() => ds.Close("empty")'>Empty First</button>
            <button class="btn btn-danger" @onclick='() => ds.Close("drop")'>Drop All</button>
        </div>
    </div>
);
```

### Getting Current User ID
```csharp
// Pattern from existing authentication
@inject AuthenticationStateProvider AuthenticationStateProvider

@code {
    private int CurrentUserId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (user.Identity?.IsAuthenticated == true)
        {
            var idClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out var id))
            {
                CurrentUserId = id;
            }
        }
    }
}
```

### DAL Method for Counting Enabled Admins
```csharp
// TO ADD to IPlayerDal and implementations
Task<int> CountEnabledAdminsAsync();

// MockDb implementation:
public Task<int> CountEnabledAdminsAsync()
{
    lock (MockDb.Players)
    {
        var count = MockDb.Players.Count(p =>
            p.IsEnabled &&
            p.Roles != null &&
            p.Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Contains(Roles.Administrator, StringComparer.OrdinalIgnoreCase));
        return Task.FromResult(count);
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate edit page | Modal dialog | CONTEXT.md decision | Better UX, no navigation |
| Confirmation for enable/disable | No confirmation | CONTEXT.md decision | Faster workflow (reversible action) |
| Checkbox for User role | Implicit User role | CONTEXT.md decision | Cleaner UI, User role always present |

**Deprecated/outdated:**
- Navigation-based edit: Replaced by modal dialog per CONTEXT.md decision

## Existing Code Analysis

### What Already Exists (DO NOT RECREATE)

| Component | Location | Status | Notes |
|-----------|----------|--------|-------|
| AdminUserEdit | GameMechanics/Player/AdminUserEdit.cs | Needs enhancement | Add last-admin business rule |
| AdminUserInfo | GameMechanics/Player/AdminUserInfo.cs | Complete | No changes needed |
| AdminUserList | GameMechanics/Player/AdminUserList.cs | Complete | No changes needed |
| Users.razor | Threa.Client/Components/Pages/Admin/Users.razor | Needs enhancement | Add modal, icons, sorting |
| UserEdit.razor | Threa.Client/Components/Pages/Admin/UserEdit.razor | Convert to modal | Currently separate page |
| DialogService | Threa/Program.cs | Complete | Already registered |
| Roles constants | GameMechanics/Player/Roles.cs | Complete | Administrator, GameMaster, Player |
| User disabled check | GameMechanics/Player/UserValidation.cs | Complete | Already throws on disabled login |

### What Needs to Be Created/Modified

| Component | Location | Priority | Dependencies |
|-----------|----------|----------|--------------|
| CountEnabledAdminsAsync | IPlayerDal + implementations | High | None |
| LastAdminProtectionRule | GameMechanics/Player/AdminUserEdit.cs | High | DAL method |
| UserEditModal component | Threa.Client/Components/Pages/Admin/ | High | AdminUserEdit with rule |
| Enhanced Users.razor | Existing file | High | UserEditModal |

## Open Questions

Things that couldn't be fully resolved:

1. **Async Rule Dependency Injection**
   - What we know: CSLA 9 supports async rules with ProvideTargetWhenAsync
   - What's unclear: Best way to inject IPlayerDal into business rule
   - Recommendation: Use ApplicationContext.CreateInstanceDI<IPlayerDal>() or pass dal as InputProperty

2. **Modal Close on Success vs Manual Close**
   - What we know: CONTEXT.md says "Admin manually closes modal after reviewing success message"
   - What's unclear: How to show success inside modal while keeping it open
   - Recommendation: Add success message state to modal, keep Close button visible

3. **Disabled User Login Redirect**
   - What we know: UserValidation throws when disabled user tries to log in
   - What's unclear: Whether to improve error message to be more specific
   - Recommendation: Current message "Account is disabled" is sufficient per existing code

## Sources

### Primary (HIGH confidence)
- S:\src\rdl\threa\GameMechanics\Player\AdminUserEdit.cs - Existing business object
- S:\src\rdl\threa\Threa\Threa.Client\Components\Pages\Admin\Users.razor - Existing list page
- S:\src\rdl\threa\Threa\Threa.Client\Components\Pages\Admin\UserEdit.razor - Existing edit page
- S:\src\rdl\threa\Threa\Threa.Client\Components\Pages\GamePlay\TabPlayInventory.razor - DialogService patterns
- S:\src\rdl\threa\Threa\Threa.Client\Components\Pages\GameMaster\Characters.razor - QuickGrid sorting pattern
- S:\src\rdl\threa\.planning\phases\10-admin-user-management\10-CONTEXT.md - Phase decisions

### Secondary (MEDIUM confidence)
- [CSLA.NET IBusinessRuleAsync Interface](https://cslanet.com/6.0.0/html/interface_csla_1_1_rules_1_1_i_business_rule_async.html) - Async rule documentation
- [CSLA GitHub Discussions](https://github.com/MarimerLLC/csla/discussions/4230) - Async rule patterns

### Tertiary (LOW confidence)
- None required - all patterns verified from existing codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in use
- Architecture: HIGH - Follows existing CSLA and Radzen patterns exactly
- Pitfalls: HIGH - Based on existing codebase review and common admin patterns
- Business rules: MEDIUM - Async rule injection pattern needs validation during implementation

**Research date:** 2026-01-26
**Valid until:** 2026-02-26 (stable patterns, unlikely to change)
