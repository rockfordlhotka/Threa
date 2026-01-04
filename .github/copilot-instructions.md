# Threa - Copilot Instructions

## Project Overview

Threa is a **TTRPG (tabletop role-playing game) Character Sheet Assistant** built with .NET 10, CSLA.NET 9.1 business objects, and Blazor WebAssembly. It implements a skill-based RPG system using **4dF+ (exploding Fudge dice)** mechanics.

## Prerequisites

Before contributing to this project, ensure you have:

- **.NET 10 SDK** (version 10.0.101 or later) - [Download here](https://dotnet.microsoft.com/download/dotnet/10.0)
- **IDE**: Visual Studio 2022 (version 17.12+), Visual Studio Code, or JetBrains Rider
- **Git** for version control
- **Optional**: RabbitMQ for testing time event messaging (not required for core development)

### Verify Prerequisites

```bash
# Check .NET SDK version
dotnet --version  # Should be 10.0.101 or higher

# Check git
git --version
```

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

## Getting Started

### Initial Setup

```bash
# Clone the repository
git clone https://github.com/rockfordlhotka/Threa.git
cd Threa

# Restore dependencies
dotnet restore Threa.sln

# Build the solution
dotnet build Threa.sln
```

### Running the Application

The application is a Blazor WebAssembly app with an ASP.NET Core host:

```bash
# Run the web application (from repository root)
cd Threa/Threa
dotnet run

# The app will be available at:
# - HTTPS: https://localhost:7133
# - HTTP: http://localhost:5181
```

The application uses the **MockDb** data access layer by default for development (no database setup required).

### Build & Test

```bash
# Build solution
dotnet build Threa.sln

# Run all tests
dotnet test GameMechanics.Test/GameMechanics.Test.csproj

# Run specific test class
dotnet test --filter "FullyQualifiedName~CombatTests"

# Run tests with detailed output
dotnet test GameMechanics.Test/GameMechanics.Test.csproj --verbosity normal
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

### Dependency Management

Key dependencies and their purposes:

| Package | Version | Purpose |
|---------|---------|---------|
| `Csla` | 9.1.0 | Core business object framework |
| `Csla.AspNetCore` | 9.1.0 | ASP.NET Core integration |
| `Csla.Blazor.WebAssembly` | 9.1.0 | Blazor WASM data portal |
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

### CSLA questions?

Use the CSLA MCP server for guidance on CSLA-specific patterns and best practices. Remember we're using CSLA version 9.

### Modifying game rules

Update the corresponding `design/*.md` document first, then implement matching code changes.

## Debugging & Troubleshooting

### Common Issues

**Build Errors with .NET 10**
- Ensure you have .NET 10 SDK installed (check `global.json` for required version)
- Run `dotnet --list-sdks` to verify available SDKs
- The project uses `rollForward: latestFeature` in `global.json`

**CSLA Business Object Errors**
- Verify all CSLA packages are version 9.1.0
- Check that `[Inject]` attributes are used correctly in data portal methods
- Use CSLA MCP server for guidance on CSLA-specific patterns

**Test Failures**
- Tests use `DeterministicDiceRoller` for predictable outcomes
- Check that dice roll values match expected results in test setup
- Review `CombatResultTables` for correct damage values

### Debugging Blazor WASM

```bash
# Run with detailed logging
dotnet run --project Threa/Threa/Threa.csproj --verbosity detailed

# Debug in browser
# 1. Run the application
# 2. Press Shift+Alt+D in the browser
# 3. Follow the browser-specific debugging instructions
```

## Security Considerations

- **Never commit secrets** to source control (connection strings, API keys, passwords)
- **Validate all user input** in business objects using CSLA validation rules
- **Use parameterized queries** in DAL implementations to prevent SQL injection
- **Nullable reference types** are enabled project-wide - handle null cases explicitly
- **CSLA authorization** rules should be applied to sensitive business operations

### Security Testing

Before committing changes that affect:
- Data access (DAL implementations)
- User input handling
- Authentication/authorization

Run the test suite and manually verify input validation and error handling.
