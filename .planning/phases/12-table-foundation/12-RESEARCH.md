# Phase 12: Table Foundation - Research

**Researched:** 2026-01-26
**Domain:** Blazor CRUD with CSLA.NET business objects, campaign table management
**Confidence:** HIGH

## Summary

Phase 12 establishes the campaign table foundation for GMs to create and manage game sessions. The existing codebase already has substantial infrastructure: `TableEdit` CSLA business object with Theme and StartTimeSeconds properties, `ITableDal` with full CRUD operations, `GameTable` DTO, and a working `Tables.razor` page with modal creation dialog. The theme system exists (themes.css, theme.js, ThemeSwitcher.razor) but the CONTEXT.md indicates it's broken and needs repair.

The primary work is converting the modal-based creation to a dedicated page, adding theme selection with preview, implementing theme-specific default epoch times, and ensuring the list view shows the required columns sorted by creation date (newest first). The existing GmTable.razor serves as the campaign dashboard.

**Primary recommendation:** Extend existing `Tables.razor` to dedicated create page at `/campaigns/create`, add theme dropdown with preview card, fix theme infrastructure issues, and ensure list displays theme and epoch time columns.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object framework | Already used throughout, handles validation, data access |
| Radzen.Blazor | 8.4.2 | UI component library | Already used for data grids, dropdowns, forms |
| Bootstrap | 5.x | CSS framework | Already imported, used for layout and components |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Bootstrap Icons | 1.11.3 | Icon library | Theme indicators (bi-book, bi-cpu) |
| Microsoft.AspNetCore.Components | in SDK | Blazor framework | All page components |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Radzen DataGrid | Bootstrap table | Already using Radzen elsewhere, consistent UX |
| CSLA ViewModel | Direct IDataPortal | ViewModel provides better Blazor integration |

**Installation:**
No new packages required. All dependencies already present.

## Architecture Patterns

### Recommended Project Structure
```
Threa/Threa.Client/Components/Pages/GameMaster/
├── Campaigns.razor              # Campaign list page (/campaigns)
├── CampaignCreate.razor         # Campaign creation page (/campaigns/create)
└── CampaignDashboard.razor      # Campaign dashboard (reuse existing GmTable.razor pattern)

GameMechanics/GamePlay/
├── TableEdit.cs                 # Already exists - extend if needed
├── TableInfo.cs                 # Already exists - add Theme, StartTimeSeconds
└── TableList.cs                 # Already exists - add sorting

Threa.Dal/Dto/
└── GameTable.cs                 # Already has Theme and StartTimeSeconds
```

### Pattern 1: CSLA Edit Form with Direct Portal
**What:** Use IDataPortal directly for create/edit operations
**When to use:** Single-entity forms where ViewModel complexity isn't needed
**Example:**
```csharp
// Source: Existing Tables.razor pattern
@inject IDataPortal<GameMechanics.GamePlay.TableEdit> tablePortal

private async Task SaveNewTable()
{
    try
    {
        var newTable = await tablePortal.CreateAsync();
        newTable.Name = newTableName;
        newTable.Theme = selectedTheme;
        newTable.StartTimeSeconds = startTimeSeconds;
        await tablePortal.UpdateAsync(newTable);

        NavigationManager.NavigateTo($"/gamemaster/table/{newTable.Id}");
    }
    catch (Exception ex)
    {
        errorMessage = $"Failed to create table: {ex.Message}";
    }
}
```

### Pattern 2: Theme Application via JS Interop
**What:** Apply themes using existing JavaScript infrastructure
**When to use:** Theme preview and persistence
**Example:**
```csharp
// Source: Existing GmTable.razor pattern
@inject IJSRuntime JS

private async Task PreviewTheme(string theme)
{
    await JS.InvokeVoidAsync("threaTheme.apply", theme);
}

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Apply default theme
        await JS.InvokeVoidAsync("threaTheme.apply", "fantasy");
    }
}
```

### Pattern 3: Radzen DataGrid Without Sorting Controls
**What:** Display data grid with fixed sort order
**When to use:** When user-controlled sorting not needed
**Example:**
```razor
<!-- Source: Similar to existing Items.razor pattern -->
<RadzenDataGrid Data="@sortedTables" TItem="TableInfo"
    AllowSorting="false" AllowPaging="true" PageSize="20"
    RowSelect="@(item => NavigationManager.NavigateTo($"/gamemaster/table/{item.Id}"))"
    Style="cursor: pointer">
    <Columns>
        <RadzenDataGridColumn TItem="TableInfo" Property="Name" Title="Campaign Name" />
        <RadzenDataGridColumn TItem="TableInfo" Title="Theme">
            <Template Context="item">
                <ThemeIndicator Theme="@item.Theme" />
            </Template>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="TableInfo" Property="StartTimeSeconds" Title="Epoch Time" />
        <RadzenDataGridColumn TItem="TableInfo" Property="CreatedAt" Title="Created"
            FormatString="{0:g}" />
    </Columns>
</RadzenDataGrid>

@code {
    private IEnumerable<TableInfo>? sortedTables =>
        tables?.OrderByDescending(t => t.CreatedAt);
}
```

### Anti-Patterns to Avoid
- **Modal for complex creation:** Decision explicitly requires dedicated page, not modal
- **Custom theme CSS toggle:** Use existing JS interop pattern, not manual class toggling
- **Hard-coded epoch defaults:** Make theme-specific defaults configurable

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Theme switching | Manual CSS class manipulation | `threaTheme.apply(theme)` via JS Interop | Already handles sessionStorage, DOM attributes |
| Form validation | Custom validation logic | CSLA validation rules on TableEdit | Built-in support for Required, business rules |
| Data grid rendering | Bootstrap table with loops | RadzenDataGrid component | Already used, handles paging, row selection |
| Date formatting | Manual DateTime.ToString | RadzenDataGridColumn FormatString | Consistent with other grids |

**Key insight:** The existing codebase has complete theme and table infrastructure. The work is primarily UI reorganization (modal to page) and adding missing list columns, not building new systems.

## Common Pitfalls

### Pitfall 1: Theme Preview Not Reverting
**What goes wrong:** User previews sci-fi theme, cancels creation, but theme stays sci-fi
**Why it happens:** Theme is applied to DOM but not reset on cancellation
**How to avoid:** Store original theme on page load, restore on cancel/navigate away
**Warning signs:** Navigation away from create page with wrong theme applied

```csharp
private string? originalTheme;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        originalTheme = await JS.InvokeAsync<string>("threaTheme.get");
    }
}

private async Task Cancel()
{
    if (originalTheme != null)
    {
        await JS.InvokeVoidAsync("threaTheme.apply", originalTheme);
    }
    NavigationManager.NavigateTo("/campaigns");
}
```

### Pitfall 2: TableInfo Missing Theme/StartTimeSeconds
**What goes wrong:** List view can't display theme or epoch time
**Why it happens:** TableInfo.cs doesn't map these fields from GameTable DTO
**How to avoid:** Extend TableInfo with Theme and StartTimeSeconds properties
**Warning signs:** Null or default values in list columns

### Pitfall 3: Long Input Not Allowing Negative Values
**What goes wrong:** Sci-Fi negative epoch times rejected
**Why it happens:** HTML input type="number" with min="0" constraint
**How to avoid:** Remove min constraint, validate full long range
**Warning signs:** Unable to enter pre-1970 times

### Pitfall 4: Theme Infrastructure "Broken" Issues
**What goes wrong:** Theme switching doesn't apply correctly
**Why it happens:** CONTEXT.md notes existing theme infrastructure is broken
**How to avoid:** Test thoroughly, check sessionStorage persistence, verify CSS cascade
**Warning signs:** Theme not persisting, wrong CSS variables applied

Common broken theme issues to investigate:
1. `data-theme` attribute not being set on correct element
2. sessionStorage not persisting across navigation
3. CSS variables not cascading to all components
4. Theme JS not loading before Blazor hydration

## Code Examples

Verified patterns from existing codebase:

### Campaign Creation Form
```razor
@* Source: Pattern from existing CharacterEdit.razor and Tables.razor *@
@page "/campaigns/create"
@rendermode InteractiveServer
@attribute [Authorize(Roles = "GameMaster,Administrator")]

@inject IDataPortal<GameMechanics.GamePlay.TableEdit> tablePortal
@inject NavigationManager NavigationManager
@inject IJSRuntime JS

<h3>Create New Campaign</h3>

@if (errorMessage != null)
{
    <div class="alert alert-danger">@errorMessage</div>
}

<div class="card mb-3">
    <div class="card-body">
        <div class="mb-3">
            <label class="form-label">Campaign Name</label>
            <input type="text" class="form-control" @bind="campaignName"
                   @bind:event="oninput" placeholder="e.g., Rise of the Dragon Lords" />
            @if (string.IsNullOrWhiteSpace(campaignName))
            {
                <div class="text-danger small">Campaign name is required</div>
            }
        </div>

        <div class="mb-3">
            <label class="form-label">Theme</label>
            <select class="form-select" @bind="selectedTheme" @bind:after="OnThemeChanged">
                <option value="fantasy">Fantasy (Arcanum)</option>
                <option value="scifi">Sci-Fi (Neon Circuit)</option>
            </select>
        </div>

        <!-- Theme Preview Card -->
        <div class="theme-preview mb-3 p-3 rounded"
             style="background: var(--color-card-bg); border: var(--border-style);">
            <h5 style="font-family: var(--font-display);">Theme Preview</h5>
            <p style="font-family: var(--font-body);">
                @(selectedTheme == "fantasy"
                    ? "Ancient tomes and flickering torchlight..."
                    : "Neon circuits pulse with digital energy...")
            </p>
        </div>

        <div class="mb-3">
            <label class="form-label">Starting Epoch Time</label>
            <input type="number" class="form-control" @bind="startTimeSeconds" />
            <small class="text-muted">
                @(selectedTheme == "fantasy"
                    ? "0 = Beginning of recorded time"
                    : "13569465600 = Year 2400 (suggested future)")
            </small>
        </div>

        <div class="d-flex gap-2">
            <button class="btn btn-primary" @onclick="CreateCampaign"
                    disabled="@(string.IsNullOrWhiteSpace(campaignName))">
                Create Campaign
            </button>
            <button class="btn btn-secondary" @onclick="Cancel">Cancel</button>
        </div>
    </div>
</div>

@code {
    private string campaignName = "";
    private string selectedTheme = "fantasy";
    private long startTimeSeconds = 0;
    private string? errorMessage;
    private string? originalTheme;

    private static readonly long SCI_FI_DEFAULT_EPOCH = 13569465600; // Year 2400

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            originalTheme = await JS.InvokeAsync<string>("threaTheme.get");
            await JS.InvokeVoidAsync("threaTheme.apply", "fantasy");
        }
    }

    private async Task OnThemeChanged()
    {
        startTimeSeconds = selectedTheme == "fantasy" ? 0 : SCI_FI_DEFAULT_EPOCH;
        await JS.InvokeVoidAsync("threaTheme.apply", selectedTheme);
    }

    private async Task CreateCampaign()
    {
        try
        {
            var newTable = await tablePortal.CreateAsync();
            newTable.Name = campaignName;
            newTable.Theme = selectedTheme;
            newTable.StartTimeSeconds = startTimeSeconds;
            await tablePortal.UpdateAsync(newTable);

            NavigationManager.NavigateTo($"/gamemaster/table/{newTable.Id}");
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to create campaign: {ex.Message}";
        }
    }

    private async Task Cancel()
    {
        if (originalTheme != null)
        {
            await JS.InvokeVoidAsync("threaTheme.apply", originalTheme);
        }
        NavigationManager.NavigateTo("/gamemaster/tables");
    }
}
```

### Theme Indicator Component
```razor
@* Source: Pattern from existing ThemeSwitcher.razor *@
@namespace Threa.Client.Components.Shared

@if (Theme == "scifi")
{
    <span class="badge" style="background: transparent; border: 1px solid var(--color-accent-primary); color: var(--color-accent-primary);">
        <i class="bi bi-cpu"></i> Sci-Fi
    </span>
}
else
{
    <span class="badge" style="background: var(--color-accent-primary); color: white;">
        <i class="bi bi-book"></i> Fantasy
    </span>
}

@code {
    [Parameter]
    public string Theme { get; set; } = "fantasy";
}
```

### Extended TableInfo with Theme
```csharp
// Source: Pattern from existing TableInfo.cs
public static readonly PropertyInfo<string> ThemeProperty = RegisterProperty<string>(nameof(Theme));
public string Theme
{
    get => GetProperty(ThemeProperty);
    private set => LoadProperty(ThemeProperty, value);
}

public static readonly PropertyInfo<long> StartTimeSecondsProperty = RegisterProperty<long>(nameof(StartTimeSeconds));
public long StartTimeSeconds
{
    get => GetProperty(StartTimeSecondsProperty);
    private set => LoadProperty(StartTimeSecondsProperty, value);
}

// In FetchChild method:
Theme = table.Theme ?? "fantasy";
StartTimeSeconds = table.StartTimeSeconds;
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Modal creation dialog | Dedicated create page | This phase | Better UX for multi-field forms |
| Tables.razor | Campaigns.razor | This phase | Clearer naming, campaign-focused |

**Deprecated/outdated:**
- Modal creation in Tables.razor: Still functional but being replaced per requirements
- GameTable without Theme: Already has Theme property, just needs UI exposure

## Open Questions

Things that couldn't be fully resolved:

1. **Theme Infrastructure Issues**
   - What we know: CONTEXT.md says theme switching is "broken" and needs repair
   - What's unclear: Specific failure modes not documented in provided context
   - Recommendation: During implementation, test thoroughly:
     - Check `data-theme` attribute on `<html>` element
     - Verify sessionStorage persistence
     - Test CSS variable cascade to all components
     - Test theme switching in GmTable.razor works
     - Document and fix any issues found

2. **Sci-Fi Default Epoch Value**
   - What we know: Should be "far future timestamp (Claude determines)"
   - What's unclear: Exact value not prescribed
   - Recommendation: Use 13569465600 (seconds since epoch for Jan 1, 2400)
     - 2400-01-01 00:00:00 UTC = 13569465600 seconds from Unix epoch
     - Easy to remember: Year 2400
     - Far enough future for sci-fi campaigns

3. **Campaign Dashboard Content**
   - What we know: GM opens campaign to access "management dashboard"
   - What's unclear: What exactly shows on initial dashboard view
   - Recommendation: Reuse existing GmTable.razor as-is initially; it already shows:
     - Table name and status
     - Theme switcher
     - Time controls (in Lobby state, shows start time input)
     - Character list (empty initially)
     - NPC panel

## Sources

### Primary (HIGH confidence)
- `S:/src/rdl/threa/GameMechanics/GamePlay/TableEdit.cs` - CSLA business object with Theme, StartTimeSeconds
- `S:/src/rdl/threa/Threa.Dal/Dto/GameTable.cs` - DTO with Theme, StartTimeSeconds
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GamePlay/Tables.razor` - Current table list/create modal
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor` - Campaign dashboard
- `S:/src/rdl/threa/Threa/Threa/wwwroot/css/themes.css` - Theme CSS variables
- `S:/src/rdl/threa/Threa/Threa/wwwroot/js/theme.js` - Theme JS interop
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/ThemeSwitcher.razor` - Theme switch component

### Secondary (MEDIUM confidence)
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GameMaster/Items.razor` - Radzen DataGrid pattern
- `S:/src/rdl/threa/CLAUDE.md` - Project conventions and CSLA patterns

### Tertiary (LOW confidence)
- None - all findings verified from codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - verified from existing project dependencies and patterns
- Architecture: HIGH - verified from existing codebase structure
- Pitfalls: MEDIUM - theme "broken" issue needs investigation during implementation

**Research date:** 2026-01-26
**Valid until:** 2026-02-26 (30 days - stable domain, existing patterns)
