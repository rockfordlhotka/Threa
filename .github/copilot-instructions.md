# Threa - Copilot Instructions

## Project Overview

Threa is a **TTRPG (tabletop role-playing game) Character Sheet Assistant** built with .NET 10, CSLA.NET 9.1 business objects, and Blazor WebAssembly. It implements a skill-based RPG system using **4dF+ (exploding Fudge dice)** mechanics.

## Architecture

| Layer | Project | Purpose |
|-------|---------|---------|
| Domain/Business | `GameMechanics` | Game rules, CSLA business objects, resolvers |
| Data Access | `Threa.Dal` | Interface-only; defines `I*Dal` contracts |
| Data Implementation | `Threa.Dal.MockDb`, `Threa.Dal.Sqlite` | Pluggable DAL implementations |
| Presentation | `Threa.Client` (Blazor WASM) | UI components, pages |
| Server | `Threa` | Blazor host, CSLA data portal server |
| Messaging | `GameMechanics.Messaging.RabbitMQ` | Time event pub/sub |
| Tests | `GameMechanics.Test` | MSTest unit tests |

### Key Design Patterns

- **CSLA.NET Business Objects**: All domain entities extend `BusinessBase<T>` or `BusinessListBase<T,C>`. Use CSLA's `[Create]`, `[Fetch]`, `[Insert]`, `[Update]`, `[Delete]` attributes for data operations
- **Repository Pattern**: DAL interfaces in `Threa.Dal`, implementations inject via DI
- **Resolver Pattern**: Combat, actions, spells use dedicated resolver classes (e.g., `AttackResolver`, `DefenseResolver`, `ActionResolver`) that accept `IDiceRoller` for testability
- **Deterministic Testing**: Use `DeterministicDiceRoller` for predictable test outcomes

## Core Game Mechanics

**Always reference** [`design/`](design/) for authoritative rules:

- **Dice System**: 4dF+ (exploding Fudge dice) - see `Dice.cs`, `IDiceRoller.cs`
- **Ability Score**: `AS = Attribute + Skill Level - 5 + Modifiers`
- **Action Resolution**: `AS + 4dF+` vs Target Value (TV); Success Value (SV) = Roll - TV
- **Health Pools**: `Fatigue = (END × 2) - 5`, `Vitality = (STR × 2) - 5`
- **Seven Attributes**: STR, DEX, END, INT, ITT, WIL, PHY

### Combat Flow

Attack/defense resolution follows this chain:
1. `AttackResolver.Resolve(AttackRequest)` → `AttackResult`
2. `DefenseResolver.Resolve(DefenseRequest)` → `DefenseResult`
3. `DamageResolver.Resolve(DamageRequest)` → `DamageResolutionResult`

Look up damage using `CombatResultTables.GetDamage(sv)`.

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

## Build & Test

```bash
# Build solution
dotnet build Threa.sln

# Run tests
dotnet test GameMechanics.Test/GameMechanics.Test.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~CombatTests"
```

## Key Files Reference

| Purpose | Location |
|---------|----------|
| Game rules specification | `design/GAME_RULES_SPECIFICATION.md` |
| Action system | `design/ACTIONS.md`, `GameMechanics/Actions/ActionResolver.cs` |
| Combat system | `design/COMBAT_SYSTEM.md`, `GameMechanics/Combat/` |
| Character business object | `GameMechanics/CharacterEdit.cs` |
| DAL interfaces | `Threa.Dal/ICharacterDal.cs`, etc. |
| DTOs | `Threa.Dal/Dto/` |
| Test patterns | `GameMechanics.Test/CombatTests.cs` |

## Code Conventions

- **Nullable enabled** project-wide
- **DTOs** in `Threa.Dal.Dto` are simple POCOs; business objects in `GameMechanics` contain rules
- **Test naming**: `MethodName_Scenario_ExpectedResult`
- **Use `[DataRow]`** for parameterized tests on result tables
- **Inject `IDiceRoller`** in any class needing randomness for testability

## Common Tasks

### Adding a new skill check

1. Create request class (see `AttackRequest` pattern)
2. Create resolver with `IDiceRoller` injection
3. Use `CombatResultTables` or create skill-specific result table
4. Add tests using `DeterministicDiceRoller`

### Adding a CSLA property

Use the full PropertyInfo pattern with appropriate `GetProperty`/`SetProperty` or `LoadProperty` based on whether dirty tracking is needed.

### CSLA questions?

Use the CSLA MCP server for guidance on CSLA-specific patterns and best practices. Remember we're using CSLA version 9.

### Modifying game rules

Update the corresponding `design/*.md` document first, then implement matching code changes.
