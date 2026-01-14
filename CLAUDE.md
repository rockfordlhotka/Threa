# Threa TTRPG Assistant - Claude Code Instructions

## Project Overview

Threa is a **TTRPG (tabletop role-playing game) Character Sheet Assistant** that helps players manage and play their characters, and helps GMs manage games including time flow, spell effects, combat, and more. Built with .NET 10, CSLA.NET 9.1 business objects, and Blazor Web App (SSR + InteractiveServer).

## Architecture

| Layer | Project | Purpose |
|-------|---------|---------|
| Domain/Business | `GameMechanics` | Game rules, CSLA business objects, resolvers |
| Data Access | `Threa.Dal` | Interface-only; defines `I*Dal` contracts |
| Data Implementation | `Threa.Dal.MockDb`, `Threa.Dal.Sqlite` | Pluggable DAL implementations |
| Presentation | `Threa.Client` | Blazor UI components, pages (SSR + InteractiveServer) |
| Server | `Threa` | Blazor Web App host, CSLA data portal |
| Messaging | `GameMechanics.Messaging.InMemory` | In-memory pub/sub using Rx.NET |
| Tests | `GameMechanics.Test` | MSTest unit tests |

### Key Design Patterns

- **CSLA.NET Business Objects**: All domain entities extend `BusinessBase<T>` or `BusinessListBase<T,C>`. Use CSLA's `[Create]`, `[Fetch]`, `[Insert]`, `[Update]`, `[Delete]` attributes for data operations
- **Repository Pattern**: DAL interfaces in `Threa.Dal`, implementations inject via DI
- **Resolver Pattern**: Combat, actions, spells use dedicated resolver classes (e.g., `AttackResolver`, `DefenseResolver`, `ActionResolver`) that accept `IDiceRoller` for testability
- **Deterministic Testing**: Use `DeterministicDiceRoller` for predictable test outcomes

## Core Game Mechanics

**Always reference** `design/` for authoritative rules:

- **Dice System**: 4dF+ (exploding Fudge dice) - see `Dice.cs`, `IDiceRoller.cs`
- **Ability Score**: `AS = Attribute + Skill Level - 5 + Modifiers`
- **Action Resolution**: `AS + 4dF+` vs Target Value (TV); Success Value (SV) = Roll - TV
- **Health Pools**: `Fatigue = (END × 2) - 5`, `Vitality = (STR × 2) - 5`
- **Seven Attributes**: STR (Physicality), DEX (Dodge), END (Drive), INT (Reasoning), ITT (Awareness), WIL (Focus), PHY (Bearing)

### Combat Flow

Attack/defense resolution follows this chain:
1. `AttackResolver.Resolve(AttackRequest)` → `AttackResult`
2. `DefenseResolver.Resolve(DefenseRequest)` → `DefenseResult`
3. `DamageResolver.Resolve(DamageRequest)` → `DamageResolutionResult`

Look up damage using `CombatResultTables.GetDamage(sv)`.

## Design Documentation

Core design documents in `design/`:

| Document | Description |
|----------|-------------|
| `GAME_RULES_SPECIFICATION.md` | **Primary reference** - Complete game mechanics |
| `ACTIONS.md` | Universal action framework and resolution flow |
| `COMBAT_SYSTEM.md` | Combat resolution, initiative, defense options |
| `ACTION_POINTS.md` | AP calculation, costs, recovery, Fatigue interaction |
| `TIME_SYSTEM.md` | Rounds, initiative, cooldowns, long-term events |
| `EFFECTS_SYSTEM.md` | Wounds, conditions, buffs, debuffs, spell effects |
| `EQUIPMENT_SYSTEM.md` | Equipment slots, item bonuses, stacking rules |
| `DATABASE_DESIGN.md` | Schema design for characters, skills, items, effects |
| `PLAY_PAGE_DESIGN.md` | Play page layout, combat workflows, UI patterns |

## Build & Test Commands

```bash
# Build solution
dotnet build Threa.sln

# Run all tests
dotnet test GameMechanics.Test/GameMechanics.Test.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~CombatTests"

# Run the web application
cd Threa/Threa
dotnet run
# Available at https://localhost:7133 or http://localhost:5181
```

## CSLA Business Object Conventions

```csharp
// Properties use static PropertyInfo pattern
public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
public string Name
{
    get => GetProperty(NameProperty);
    set => SetProperty(NameProperty, value);
}

// Child objects use LoadProperty (no dirty tracking)
private set => LoadProperty(AttributeListProperty, value);

// Data operations use attributes
[Fetch]
private async Task FetchAsync(int id, [Inject] ICharacterDal dal, ...) { ... }
```

**Use the CSLA MCP server** (`csla-framework-developer` agent) for guidance on CSLA-specific patterns and best practices. This project uses CSLA version 9.

## Code Conventions

- **Nullable enabled** project-wide
- **DTOs** in `Threa.Dal.Dto` are simple POCOs; business objects in `GameMechanics` contain rules
- **Test naming**: `MethodName_Scenario_ExpectedResult`
- **Use `[DataRow]`** for parameterized tests on result tables
- **Inject `IDiceRoller`** in any class needing randomness for testability

## Key Files Reference

| Purpose | Location |
|---------|----------|
| Game rules specification | `design/GAME_RULES_SPECIFICATION.md` |
| Action system | `design/ACTIONS.md`, `GameMechanics/Actions/ActionResolver.cs` |
| Combat system | `design/COMBAT_SYSTEM.md`, `GameMechanics/Combat/` |
| Character business object | `GameMechanics/CharacterEdit.cs` |
| DAL interfaces | `Threa.Dal/ICharacterDal.cs` |
| DTOs | `Threa.Dal/Dto/` |
| Test patterns | `GameMechanics.Test/CombatTests.cs` |

## Key Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `Csla` | 9.1.0 | Core business object framework |
| `Csla.AspNetCore` | 9.1.0 | ASP.NET Core integration |
| `Csla.Blazor` | 9.1.0 | Blazor integration |
| `Radzen.Blazor` | 8.4.2 | UI component library |

**Important**: When updating CSLA packages, update all CSLA packages together to maintain version compatibility.

## Common Tasks

### Adding a new skill check
1. Create request class (see `AttackRequest` pattern)
2. Create resolver with `IDiceRoller` injection
3. Use `CombatResultTables` or create skill-specific result table
4. Add tests using `DeterministicDiceRoller`

### Adding a CSLA property
Use the full PropertyInfo pattern with appropriate `GetProperty`/`SetProperty` or `LoadProperty` based on whether dirty tracking is needed.

### Modifying game rules
Update the corresponding `design/*.md` document first, then implement matching code changes.

## Security Considerations

- **Never commit secrets** to source control
- **Validate all user input** in business objects using CSLA validation rules
- **Use parameterized queries** in DAL implementations
- **Nullable reference types** enabled project-wide - handle null cases explicitly
- **CSLA authorization** rules should be applied to sensitive business operations
