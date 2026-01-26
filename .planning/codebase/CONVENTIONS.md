# Coding Conventions

**Analysis Date:** 2026-01-24

## Naming Patterns

**Files:**
- PascalCase for all file names matching class name
- Example: `ActionResolver.cs`, `AttackRequest.cs`, `CharacterEdit.cs`
- Test files: `{Feature}Tests.cs` (e.g., `CombatTests.cs`, `ActionResolverTests.cs`)
- Separate behavior classes: `{Behavior}Behavior.cs` (e.g., `DrugBehavior.cs`, `ItemEffectBehavior.cs`)

**Functions/Methods:**
- PascalCase for all public methods
- PascalCase for private methods
- Examples: `Resolve()`, `BuildAbilityScore()`, `ApplyMultipleActionPenalty()`
- Factory methods use `Create` or `With` prefix: `WithFixed4dFPlus()`, `CreateBasicSkill()`

**Variables:**
- camelCase for local variables and parameters
- Examples: `actionRequest`, `effectiveAS`, `combatantId`, `diceRoll`

**Types:**
- PascalCase for all classes, structs, enums, interfaces
- Interfaces prefixed with `I`: `IDiceRoller`, `ITimeEventSubscriber`, `IActivityLogService`
- Type parameters prefixed with `T`: `BusinessBase<T>`, `BusinessListBase<T,C>`

**Fields:**
- Private fields: `_camelCase` prefix with underscore
  - Examples: `_diceRoller`, `_fudgeRolls`, `_defaultFudgeResult`
- Static private fields: `s_camelCase` prefix with `s_`
  - Example: `s_camelCase`
- Public/internal fields: PascalCase (rare, mostly constants)
- Constants: PascalCase
  - Examples: `PhysicalityTV`, `DefaultTV`
- Public static readonly: PascalCase
  - Examples: `PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id))`

## Code Style

**Formatting:**
- Tool: EditorConfig (`.editorconfig` enforced project-wide)
- Indentation: 2 spaces
- Line ending: CRLF
- No final newline on files

**Linting:**
- All C# files subject to `.editorconfig` rules
- Key enforced rules:
  - `indent_size = 2`
  - `csharp_new_line_before_open_brace = all` (opening braces on new line)
  - `csharp_prefer_braces = true` (always use braces for control flow)
  - `csharp_prefer_simple_using_statement = true` (prefer `using` over `using()`)
  - `dotnet_style_qualification_for_* = false` (no `this.` prefix)
  - `dotnet_style_require_accessibility_modifiers = for_non_interface_members` (explicit access modifiers required)
  - `dotnet_style_readonly_field = true:warning` (warn on mutable fields)

**Braces and Control Flow:**
- Opening braces always on new line: `if (...)\n{`
- Always use braces for all control structures (if/else/for/while/do)
- Indentation inside braces enabled

**Parentheses:**
- Use parentheses for clarity in arithmetic and relational operators
- Examples: `(attribute + skill - 5)`, `(av - tv)`

## Import Organization

**Order:**
1. System and BCL namespaces (`using System`, `using System.Collections`, etc.)
2. Blank line
3. Third-party namespaces (`using Csla`, `using Microsoft.*`, etc.)
4. Blank line
5. Project namespaces (`using GameMechanics.*`, `using Threa.Dal.*`, etc.)

**Rule:** `dotnet_separate_import_directive_groups = true` and `dotnet_sort_system_directives_first = true`

**Path Aliases:**
- No aliases in use; all paths are fully qualified
- Example: `using GameMechanics.Actions;`, `using GameMechanics.Combat;`

**Using Statement Placement:**
- `csharp_using_directive_placement = outside_namespace` - all `using` statements outside namespace block

## Error Handling

**Patterns:**
- Use explicit exception types, not generic `Exception`
- Null coalescing with throw: `_diceRoller = diceRoller ?? throw new ArgumentNullException(nameof(diceRoller))`
- Validation exceptions:
  - `ArgumentNullException` for null parameters
  - `ArgumentOutOfRangeException` for invalid ranges
  - `ArgumentException` for invalid values in general
  - `InvalidOperationException` for state violations or resource exhaustion

**Examples:**
```csharp
// From ActionResolver.cs
_diceRoller = diceRoller ?? throw new ArgumentNullException(nameof(diceRoller));

// From ActionPoints.cs
throw new InvalidOperationException("Insufficient AP");

// From Currency.cs
throw new ArgumentOutOfRangeException(nameof(value), "Coin count cannot be negative.");
```

**Business Logic Validation:**
- CSLA `BusinessRules` used for domain validation on `BusinessBase<T>` objects
- Examples in `CharacterEdit.cs`: `BusinessRules.CheckRules()` on property changes
- Custom `BusinessRule` subclasses for complex validations: `FatigueBase`, `VitalityBase`, `AttributeSumValidation`

## Logging

**Framework:** `console` (implicit via System.Diagnostics or CSLA framework logging)

**Patterns:**
- No explicit logging infrastructure in `GameMechanics` (domain logic layer)
- Logging typically handled by calling application (Blazor server, DAL implementations)
- Tracing via CSLA when enabled in data portal operations

## Comments

**When to Comment:**
- Use XML documentation (`///`) for all public types and public members
- XML comments required for:
  - Public classes and structs
  - Public methods and properties
  - Public enums and enum values
  - Interface members

**JSDoc/TSDoc:**
- Full XML documentation style with `<summary>`, `<param>`, `<returns>` tags
- Example from `ActionResolver.cs`:
  ```csharp
  /// <summary>
  /// Service that resolves skill-based actions following the
  /// universal action resolution framework defined in ACTIONS.md.
  /// </summary>
  public class ActionResolver
  {
    /// <summary>
    /// Resolves an action and returns the complete result.
    /// </summary>
    /// <param name="request">The action request with all parameters.</param>
    /// <returns>The action result with all calculated values.</returns>
    public ActionResult Resolve(ActionRequest request)
  ```

**Inline Comments:**
- Sparse use; code should be self-documenting
- Use only for non-obvious logic or game rules references
- Example from `AttackResolver.cs`:
  ```csharp
  // Step 1: Calculate effective AS with all modifiers
  int effectiveAS = request.GetEffectiveAS();

  // Step 2: Roll attack (4dF+)
  int attackRoll = _diceRoller.Roll4dFPlus();
  ```

## Function Design

**Size:** Functions typically 20-60 lines in resolver/service classes

**Parameters:**
- Use dependency injection for services (constructor injection)
- Use request objects for complex parameter sets: `ActionRequest`, `AttackRequest`, `DefenseRequest`
- Example from `ActionResolver`:
  ```csharp
  public ActionResolver(IDiceRoller diceRoller)
  {
    _diceRoller = diceRoller ?? throw new ArgumentNullException(nameof(diceRoller));
  }

  public ActionResult Resolve(ActionRequest request)
  ```

**Return Values:**
- Single return statement when possible (CSLA operations may have multiple returns for success/error)
- Use explicit return type objects: `ActionResult`, `AttackResult`, `DamageResult`
- Never use null to indicate error; use exception or result object pattern

## Module Design

**Exports:**
- All public classes/interfaces are module exports
- Single responsibility per class (e.g., `ActionResolver` handles action resolution only)
- Internal classes for implementation details prefixed with private/internal modifier

**Barrel Files:**
- No barrel files (`index.ts` equivalents) in this C# codebase
- Each namespace maps to directory hierarchy
- Example: `GameMechanics.Actions.ActionResolver` → `GameMechanics/Actions/ActionResolver.cs`

**Namespace Organization:**
- Root: `GameMechanics` (business objects)
- Sub-namespaces by feature: `GameMechanics.Actions`, `GameMechanics.Combat`, `GameMechanics.Magic`, `GameMechanics.Effects`, `GameMechanics.Time`
- Request/Result pairs grouped together: `GameMechanics.Actions.ActionRequest`, `GameMechanics.Actions.ActionResult`

## CSLA-Specific Conventions

**BusinessBase<T> Properties:**
- Use static `PropertyInfo<T>` pattern for all properties
- Private setter using `LoadProperty()` for calculated/child properties
- Public setter using `SetProperty()` for editable properties
- Pattern from `CharacterEdit.cs`:
  ```csharp
  public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
  [Required]
  [Display(Name = "Character name")]
  public string Name
  {
    get => GetProperty(NameProperty);
    set => SetProperty(NameProperty, value);
  }
  ```

**Child Collections:**
- Use `BusinessListBase<TList, TItem>` for collections
- Load child properties with `LoadProperty()` not `SetProperty()`
- Example: `AttributeEditList : BusinessListBase<AttributeEditList, AttributeEdit>`

**Data Operations:**
- Use CSLA attributes: `[Create]`, `[Fetch]`, `[Insert]`, `[Update]`, `[Delete]`
- Injected dependencies via parameter injection with `[Inject]` attribute
- Async data methods should be named `*Async()`: `FetchAsync()`, `InsertAsync()`

## Nullable Reference Types

**Status:** Enabled project-wide (`<Nullable>enable</Nullable>`)

**Conventions:**
- Nullable types: `string?`, `int?` (explicit null-coalescing)
- Non-nullable by default: `string` cannot be null
- Null checks required: `diceRoller ?? throw new ArgumentNullException(...)`
- No suppression (`!`) operator without justification in comments

---

*Convention analysis: 2026-01-24*
