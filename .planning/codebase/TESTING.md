# Testing Patterns

**Analysis Date:** 2026-01-24

## Test Framework

**Runner:**
- MSTest (Microsoft.VisualStudio.TestTools.UnitTesting)
- Version: 4.0.2 (Microsoft.NET.Test.Sdk 18.0.1)
- Config: `GameMechanics.Test/GameMechanics.Test.csproj`
- Target Framework: net10.0

**Assertion Library:**
- MSTest built-in assertions (`Assert.AreEqual()`, `Assert.IsTrue()`, etc.)
- No external assertion library (Xunit, NUnit not used)

**Run Commands:**
```bash
# Run all tests
dotnet test GameMechanics.Test/GameMechanics.Test.csproj

# Run with coverage
dotnet test GameMechanics.Test/GameMechanics.Test.csproj /p:CollectCoverage=true

# Run specific test class
dotnet test --filter "FullyQualifiedName~CombatTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

**Coverage:**
- Tool: coverlet (version 6.0.4)
- No enforced coverage target
- Enabled via project-wide MSTest style suppressions: `MSTEST0037;MSTEST0032`

## Test File Organization

**Location:**
- Separate directory: Tests co-located in `GameMechanics.Test/` project
- Project reference: `GameMechanics.Test.csproj` depends on `GameMechanics.csproj` and `Threa.Dal.MockDb.csproj`

**Naming:**
- Pattern: `{Feature}Tests.cs`
- Examples:
  - `CombatTests.cs` (hit location, physicality bonus, combat results)
  - `ActionResolverTests.cs` (action resolution, modifiers, costs)
  - `ActionSystemTests.cs` (ability score, action costs)
  - `CharacterTests.cs` (character creation, damage, healing)
  - `SpellResolverTests.cs` (spell casting)
  - `TimeSystemTests.cs` (time management)

**Structure:**
```
GameMechanics.Test/
├── ActionResolverTests.cs
├── ActionSystemTests.cs
├── AttackEffectTests.cs
├── CharacterTests.cs
├── CombatTests.cs
├── [40+ more test files]
└── GameMechanics.Test.csproj
```

## Test Structure

**Suite Organization:**
```csharp
// From CombatTests.cs
[TestClass]
public class CombatTests
{
  #region HitLocation Tests

  [TestMethod]
  public void HitLocation_Roll1_ReturnsHead()
  {
    var location = HitLocationCalculator.MapRollToLocation(1);
    Assert.AreEqual(HitLocation.Head, location);
  }

  #endregion

  #region Physicality Bonus Tests
  // More tests grouped by feature
  #endregion
}
```

**Test Method Naming:**
- Pattern: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`
- Examples:
  - `Resolve_WithStandardAction_CalculatesCorrectly`
  - `Resolve_WithMultipleActionPenalty_AppliesNegative1`
  - `Resolve_WithWounds_AppliesPenalty`
  - `HitLocation_Roll1_ReturnsHead`
  - `PhysicalityBonus_VeryLowRV_GivesPenalty`

**Patterns:**

Setup (Initialize):
```csharp
[TestInitialize]
public void Setup()
{
  // Use deterministic dice roller for predictable outcomes
  _zeroRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
}
```

Teardown:
- Rarely used; tests are stateless and independent

Arrange-Act-Assert:
```csharp
[TestMethod]
public void Resolve_WithStandardAction_CalculatesCorrectly()
{
  // Arrange
  var request = CreateBasicRequest();
  var resolver = new ActionResolver(_zeroRoller);

  // Act
  var result = resolver.Resolve(request);

  // Assert
  Assert.AreEqual(11, result.AbilityScore.BaseAS);
  Assert.AreEqual(8, result.TargetValue.FinalTV);
  Assert.AreEqual(1, result.Cost.AP);
}
```

## Mocking

**Framework:** Manual mocking (no Moq, NSubstitute, etc.)

**Patterns:**

Deterministic Dice Roller:
```csharp
// From DeterministicDiceRoller.cs - implements IDiceRoller interface
_zeroRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);

// Queue specific results
var roller = new DeterministicDiceRoller()
  .Queue4dFPlusResults(3, -1, 2)
  .QueueFudgeRolls(1, 0, -1, 1);

// Set defaults when queue is empty
roller.SetDefaults(fudgeResult: 0, diceResult: 1);
```

Test Double Pattern - Mock DAL:
```csharp
// From CharacterTests.cs
[TestMethod]
public void CheckHealth()
{
  var provider = InitServices();
  var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
  var c = dp.Create(42);  // Uses MockDb implementation
  // ...
}
```

Service Initialization with DI:
```csharp
private ServiceProvider InitServices()
{
  IServiceCollection services = new ServiceCollection();
  services.AddCsla();  // CSLA data portal
  services.AddMockDb();  // Mock database implementations
  return services.BuildServiceProvider();
}
```

**What to Mock:**
- `IDiceRoller` - use `DeterministicDiceRoller` for all randomness
- Data access layer - use `Threa.Dal.MockDb` instead of real database

**What NOT to Mock:**
- Business logic classes (`ActionResolver`, `AttackResolver`, etc.)
- Request/Result objects - create real instances
- CSLA business objects - create real instances via data portal
- Property validation rules - test them directly

## Fixtures and Factories

**Test Data:**
```csharp
// From ActionResolverTests.cs
private Skill CreateBasicSkill()
{
  return new Skill
  {
    Id = "swords",
    Name = "Swords",
    Category = SkillCategory.Combat,
    PrimaryAttribute = "STR",
    ActionType = ActionType.Attack,
    TargetValueType = TargetValueType.Fixed,
    DefaultTV = 8,
    ResultTable = ResultTableType.CombatDamage,
    AppliesPhysicalityBonus = true,
    RequiresTarget = true
  };
}

private ActionRequest CreateBasicRequest()
{
  return new ActionRequest
  {
    Skill = CreateBasicSkill(),
    SkillLevel = 4,
    AttributeName = "STR",
    AttributeValue = 12
  };
}
```

**Location:**
- Factories defined as private methods within test class
- No separate fixture library or builders (kept simple)
- DTOs (`Skill`, `ActionRequest`) created directly with object initializers

## Coverage

**Requirements:** Not enforced

**View Coverage:**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
# Coverage reports in generated output
```

**Notable Coverage:**
- Combat system: Extensive (40+ test methods across multiple files)
- Action resolution: Comprehensive (30+ test methods)
- Time system: Well-covered
- Effects system: Good coverage
- Character CSLA operations: Basic coverage

## Test Types

**Unit Tests:**
- Scope: Individual resolvers, calculators, utilities
- Approach: Pure function testing with mocked randomness
- Examples: `ActionResolverTests`, `CombatTests`, `ResultTablesTests`
- No database or network calls

**Integration Tests:**
- Scope: CSLA business objects with mock database
- Approach: Full data portal initialization with DI
- Examples: `CharacterTests.CreateCharacterWithSpecies_AppliesModifiers()`
- Uses `InitServices()` to bootstrap full DI container

**E2E Tests:**
- Framework: Not used
- Rationale: Blazor client-server testing covered by manual browser testing
- No Selenium or equivalent integration

## Common Patterns

**Async Testing:**
```csharp
// From CharacterTests.cs
[TestMethod]
public async Task CreateCharacterWithSpecies_AppliesModifiers()
{
  var provider = InitServices();

  var speciesListPortal = provider.GetRequiredService<IDataPortal<SpeciesList>>();
  var speciesList = await speciesListPortal.FetchAsync();
  var elfSpecies = speciesList.First(s => s.Id == "Elf");

  var characterPortal = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
  var c = characterPortal.Create(42, elfSpecies);

  Assert.AreEqual("Elf", c.Species);
}
```

**Parameterized Testing (DataRow):**
```csharp
// From CombatTests.cs
[TestMethod]
[DataRow(2)]
[DataRow(6)]
[DataRow(12)]
public void HitLocation_Roll2To12_ReturnsTorso(int roll)
{
  var location = HitLocationCalculator.MapRollToLocation(roll);
  Assert.AreEqual(HitLocation.Torso, location);
}
```

**Multiple Assertions with Comments:**
```csharp
// From ActionResolverTests.cs
var result = resolver.Resolve(request);

// BaseAS = 12 + 4 - 5 = 11
Assert.AreEqual(11, result.AbilityScore.BaseAS);
Assert.AreEqual(11, result.AbilityScore.FinalAS);
Assert.AreEqual(8, result.TargetValue.FinalTV);
Assert.AreEqual(1, result.Cost.AP);
Assert.AreEqual(1, result.Cost.FAT);
```

**Error Handling Testing:**
```csharp
// Test that insufficient resources are detected
[TestMethod]
public void CanAfford_WithInsufficientAP_ReturnsFalse()
{
  var request = CreateBasicRequest();
  var resolver = new ActionResolver(_zeroRoller);

  var cost = resolver.GetCost(request);
  var canAfford = resolver.CanAfford(request, 0, 100);  // AP = 0

  Assert.IsFalse(canAfford);
}
```

**State Mutation Testing:**
```csharp
// From CharacterTests.cs - test healing progression
while (c.Fatigue.Value < c.Fatigue.BaseValue)
  c.EndOfRound(effectPortal);
Assert.AreEqual(c.Fatigue.BaseValue, c.Fatigue.Value, "improper healing");
c.EndOfRound(effectPortal);
Assert.AreEqual(c.Fatigue.BaseValue, c.Fatigue.Value, "improper noop");
```

## Test Dependencies

**NuGet Packages:**
- `Csla` (9.1.0) - Business object framework with data portal
- `Microsoft.Extensions.DependencyInjection` (10.0.1) - Service container
- `Microsoft.NET.Test.Sdk` (18.0.1) - Test runner infrastructure
- `MSTest.TestFramework` (4.0.2) - Test framework
- `MSTest.TestAdapter` (4.0.2) - Test adapter
- `coverlet.collector` (6.0.4) - Code coverage

**Test Project References:**
- `GameMechanics.csproj` - Code under test
- `Threa.Dal.MockDb.csproj` - Mock database implementation

---

*Testing analysis: 2026-01-24*
