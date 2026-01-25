# Codebase Structure

**Analysis Date:** 2026-01-24

## Directory Layout

```
S:\src\rdl\threa/
├── GameMechanics/                          # Domain/business logic layer (CSLA.NET business objects)
├── GameMechanics.Messaging.InMemory/       # In-memory pub/sub messaging service
├── GameMechanics.Test/                     # Unit tests for game mechanics
├── Threa.Dal/                              # Data access layer interfaces and DTOs
├── Threa.Dal.MockDb/                       # Mock in-memory DAL implementation
├── Threa.Dal.SqlLite/                      # SQLite DAL implementation
├── Threa/
│   ├── Threa/                              # Blazor Web App host (server-side)
│   └── Threa.Client/                       # Blazor client components (WASM + interactive)
├── Threa.Admin/                            # Admin utilities project
├── design/                                 # Game design documentation (GAME_RULES_SPECIFICATION.md, etc.)
├── data/                                   # Sample/seed data files
├── Sql/                                    # SQL migration/initialization scripts
├── .planning/                              # GSD planning documents
├── Threa.sln                               # Solution file
├── CLAUDE.md                               # Project instructions
└── README.md                               # Project overview
```

## Directory Purposes

**GameMechanics/:**
- Purpose: Core game mechanics and CSLA business objects
- Contains: Business objects (CharacterEdit, SkillEdit, AttributeEdit, etc.), action/combat resolvers, effect system, item system, magic system, time system, player/character management
- Key files: `CharacterEdit.cs`, `AttributeEdit.cs`, `SkillEdit.cs`, `ServiceCollectionExtensions.cs`

**GameMechanics/Actions/:**
- Purpose: Universal action resolution following ACTIONS.md specification
- Contains: ActionResolver, ActionRequest, ActionResult, AbilityScore, TargetValue, ActionCost, Encumbrance, Movement, Sprint resolver, result tables
- Pattern: Stateless resolvers injected with IDiceRoller

**GameMechanics/Combat/:**
- Purpose: Combat resolution, hit location, damage tables
- Contains: AttackResolver, DefenseResolver, DamageResolver, HitLocationCalculator, CombatResultTables, effect management, special actions
- Pattern: Resolver chain - attack → defense → damage

**GameMechanics/Effects/:**
- Purpose: Effect management and application
- Contains: EffectManager, EffectCalculator, EffectBehaviorFactory, effect behavior implementations (wound, buff, poison, etc.)
- Key files: `S:\src\rdl\threa\GameMechanics\Effects\Behaviors\` - pluggable behavior classes

**GameMechanics/Items/:**
- Purpose: Item management and item effect resolution
- Contains: Item properties, item effects, equipment, item bonuses and stacking logic

**GameMechanics/Magic/:**
- Purpose: Spell casting and magic system
- Contains: SpellCastingService, spell effects, mana pools, magic school definitions, spell resolvers

**GameMechanics/Time/:**
- Purpose: Time tracking and cooldown management
- Contains: Round management, initiative, cooldown calculation, long-term time events

**GameMechanics/GamePlay/:**
- Purpose: Game session management for GMs
- Contains: TableEdit (active game session), TableList, TableCharacterList, table character management (attach/detach)

**GameMechanics.Messaging.InMemory/:**
- Purpose: In-memory event pub/sub for cross-cutting concerns
- Contains: InMemoryMessageBus (generic publish/subscribe), InMemoryTimeEventPublisher/Subscriber, InMemoryActivityLogService
- Used by: Time events, effect application, logging

**GameMechanics.Test/:**
- Purpose: Unit tests for game mechanics
- Contains: MSTest test classes (CombatTests, ActionTests, etc.), DeterministicDiceRoller for predictable test outcomes
- Pattern: Test naming convention: `MethodName_Scenario_ExpectedResult`

**Threa.Dal/:**
- Purpose: Data access layer contracts
- Contains: `I*Dal` interfaces (ICharacterDal, ISkillDal, IItemDal, IEffectDal, IMagicSchoolDal, IPlayerDal, etc.), custom exceptions (NotFoundException, DuplicateKeyException, OperationFailedException)
- Key folder: `Dto/` - Data transfer objects (Character, CharacterAttribute, CharacterSkill, CharacterItem, CharacterEffect, EffectDefinition, etc.)
- Pattern: Interface-only, no implementation

**Threa.Dal.MockDb/:**
- Purpose: In-memory mock DAL for development/testing without database
- Contains: MockDb (in-memory data store), implementations of all I*Dal interfaces
- Key file: `S:\src\rdl\threa\Threa.Dal.MockDb\MockDb.cs` - Central in-memory data store
- Used by: Default development configuration in Program.cs

**Threa.Dal.SqlLite/:**
- Purpose: SQLite-based persistent DAL
- Contains: SQLite implementations of all I*Dal interfaces, database context, migrations
- Used by: Production or when persistence is required

**Threa/Threa/ (Server Host):**
- Purpose: ASP.NET Core Blazor Web App host
- Contains: Program.cs (DI setup), Razor components in Components/, Controllers/, Services/
- Key files:
  - `Program.cs` - Dependency injection configuration, CSLA setup, DAL registration
  - `Controllers/DataPortalController.cs` - CSLA data portal HTTP endpoint
  - `Controllers/CslaStateController.cs` - CSLA state management endpoint
  - `Services/ActiveCircuitHandler.cs` - Blazor circuit lifetime management

**Threa/Threa.Client/ (Client Components):**
- Purpose: Blazor interactive components and pages
- Contains: Pages and shared components in `Components/`, client services
- Key directories:
  - `Components/Pages/Admin/` - User and admin management pages (Users.razor, UserEdit.razor)
  - `Components/Pages/Character/` - Character management (Characters.razor, CharacterEdit.razor, ActivateCharacter.razor, delete, skill/attribute/item/magic tabs)
  - `Components/Pages/GameMaster/` - GM tools (Characters.razor, CharacterXPEdit.razor, ItemDelete.razor)
  - `Components/Pages/GamePlay/` - Active play page (Play.razor, RangedWeaponInfo.cs, DefenseHitData.cs, AmmoSourceInfo.cs)
  - `Components/Pages/Player/` - Player dashboard
  - `Components/Shared/` - Reusable components (ActionCostSelector.razor, BoostSelector.razor, EffectIcon.razor, etc.)
- Key files:
  - `Program.cs` - WASM app setup, HTTP client, CSLA WASM configuration
  - `Services/ActiveCircuitState.cs` - Shared circuit state across components
  - `Services/RenderModeProvider.cs` - Determine render mode (Server vs WASM)

**design/:**
- Purpose: Authoritative game design documentation
- Contains: Game rules specification, action system, combat system, effects, items, magic, time, play page layout, equipment, implants, ranged weapons
- Key files:
  - `GAME_RULES_SPECIFICATION.md` - Master reference for all game mechanics
  - `COMBAT_SYSTEM.md` - Combat resolution, defense options, hit locations
  - `ACTIONS.md` - Universal action framework
  - `EFFECTS_SYSTEM.md` - Effect types, duration, application
  - `ITEM_EFFECTS_SYSTEM.md` - Item-triggered effects, curses, equipment
  - `TIME_SYSTEM.md` - Rounds, initiative, cooldowns
  - `PLAY_PAGE_DESIGN.md` - UI layout and workflows
  - `IMPLANTS_SYSTEM.md` - Cybernetic enhancements

**data/:**
- Purpose: Sample/seed data for database initialization
- Contains: JSON or other seed data files for reference items, spells, effects

**Sql/:**
- Purpose: Database schema and migrations
- Contains: SQL scripts for database initialization (if using SQLite backend)

**.planning/:**
- Purpose: GSD planning documents (generated by map-codebase commands)
- Contains: ARCHITECTURE.md, STRUCTURE.md, CONVENTIONS.md, TESTING.md, STACK.md, INTEGRATIONS.md, CONCERNS.md

## Key File Locations

**Entry Points:**
- `S:\src\rdl\threa\Threa\Threa\Program.cs` - Server-side ASP.NET Core setup
- `S:\src\rdl\threa\Threa\Threa.Client\Program.cs` - Client-side WebAssembly setup
- `S:\src\rdl\threa\Threa\Threa\Components\App.razor` - Root Blazor component
- `S:\src\rdl\threa\Threa\Threa.Client\Components\Pages\Character\Characters.razor` - Main character list page

**Configuration:**
- `S:\src\rdl\threa\GameMechanics\ServiceCollectionExtensions.cs` - GameMechanics DI registration
- `S:\src\rdl\threa\Threa.Dal.MockDb\ConfigurationExtensions.cs` - MockDb DI registration
- `S:\src\rdl\threa\GameMechanics.Messaging.InMemory\ServiceCollectionExtensions.cs` - Messaging DI registration

**Core Logic:**
- `S:\src\rdl\threa\GameMechanics\CharacterEdit.cs` - Character business object
- `S:\src\rdl\threa\GameMechanics\Actions\ActionResolver.cs` - Action resolution
- `S:\src\rdl\threa\GameMechanics\Combat\AttackResolver.cs` - Attack resolution
- `S:\src\rdl\threa\GameMechanics\Combat\DefenseResolver.cs` - Defense resolution
- `S:\src\rdl\threa\GameMechanics\Combat\CombatResultTables.cs` - Damage and result lookups

**Testing:**
- `S:\src\rdl\threa\GameMechanics.Test\GameMechanics.Test.csproj` - Test project
- `S:\src\rdl\threa\GameMechanics.Test\CombatTests.cs` - Combat resolver tests
- `S:\src\rdl\threa\GameMechanics.Test\DeterministicDiceRoller.cs` - Predictable dice for testing

## Naming Conventions

**Files:**
- Business objects: `{EntityName}Edit.cs` (e.g., CharacterEdit.cs, SkillEdit.cs)
- Lists: `{EntityName}List.cs` (e.g., CharacterList.cs, SkillEditList.cs)
- Resolvers: `{Action}Resolver.cs` (e.g., ActionResolver.cs, AttackResolver.cs)
- Request/Result: `{Action}Request.cs`, `{Action}Result.cs` (e.g., ActionRequest.cs, AttackResult.cs)
- DAL interfaces: `I{Entity}Dal.cs` (e.g., ICharacterDal.cs, ISkillDal.cs)
- DAL implementations: `{Entity}Dal.cs` in implementation project (e.g., CharacterDal.cs in Threa.Dal.MockDb)
- DTOs: `{Entity}.cs` in `Dto/` folder (e.g., Character.cs, CharacterAttribute.cs)
- Behaviors: `{EffectType}Behavior.cs` (e.g., WoundBehavior.cs, SpellBuffBehavior.cs)
- Pages: `{PageName}.razor` (e.g., Characters.razor, CharacterEdit.razor)
- Shared components: `{ComponentName}.razor` in Components/Shared/ (e.g., ActionCostSelector.razor)

**Directories:**
- Feature folders: Lowercase plural (e.g., `Actions/`, `Combat/`, `Effects/`, `Items/`, `Magic/`)
- Pages: Feature-based (e.g., `Pages/Character/`, `Pages/GameMaster/`, `Pages/GamePlay/`)
- Behaviors: `Behaviors/` subfolder of Effects
- Test classes: Match source file names (e.g., ActionResolverTests.cs for ActionResolver.cs)

## Where to Add New Code

**New Game Mechanic (Resolver):**
- Primary code: `GameMechanics/{FeatureName}/{MechanicName}Resolver.cs`
- Request object: `GameMechanics/{FeatureName}/{MechanicName}Request.cs`
- Result object: `GameMechanics/{FeatureName}/{MechanicName}Result.cs`
- Tests: `GameMechanics.Test/{MechanicName}ResolverTests.cs`
- Pattern: Inject IDiceRoller, use DeterministicDiceRoller in tests

**New Business Object:**
- Location: `GameMechanics/{EntityName}Edit.cs` for single entities
- Location: `GameMechanics/{EntityName}EditList.cs` for collections (extends BusinessListBase)
- Pattern: Extend BusinessBase<T>, use PropertyInfo registration, add [Fetch]/[Update]/[Delete] methods with [Inject] attributes

**New DAL Interface:**
- Location: `Threa.Dal/I{Entity}Dal.cs`
- Pattern: Async methods returning Task<Dto or List<Dto>>, no implementation

**New DAL Implementation:**
- Location: `Threa.Dal.MockDb/{Entity}Dal.cs` for mock implementation
- Location: `Threa.Dal.SqlLite/{Entity}Dal.cs` for SQLite implementation
- Pattern: Implement I{Entity}Dal interface, return DTOs, handle exceptions

**New Component/Page:**
- Server-side page: `Threa/Threa/Components/Pages/{Feature}/{PageName}.razor`
- Client-side component: `Threa/Threa.Client/Components/{Type}/{ComponentName}.razor`
- Pattern: Route via @page, async load data in OnInitializedAsync, bind to business objects

**New Effect Behavior:**
- Location: `GameMechanics/Effects/Behaviors/{EffectType}Behavior.cs`
- Pattern: Implement behavior interface, handle effect application logic, register in EffectBehaviorFactory

**Utilities/Helpers:**
- Shared helpers: `GameMechanics/{UtilityName}.cs` or create `Utilities/` subfolder
- UI utilities: `Threa.Client/Services/{UtilityName}.cs`

## Special Directories

**bin/ and obj/:**
- Purpose: Build output (compiled assemblies)
- Generated: Yes (by dotnet build)
- Committed: No (git-ignored)

**.vs/:**
- Purpose: Visual Studio cache and snapshots
- Generated: Yes (by Visual Studio)
- Committed: No (git-ignored)

**TestResults/:**
- Purpose: Test run output and reports
- Generated: Yes (by dotnet test)
- Committed: No

**design/:**
- Purpose: Authoritative game rules documentation (NOT code-generated, manually maintained)
- Generated: No
- Committed: Yes - essential for understanding game mechanics

**.planning/codebase/:**
- Purpose: GSD codebase analysis documents
- Generated: Yes (by /gsd:map-codebase command)
- Committed: Yes - used by /gsd:plan-phase and /gsd:execute-phase

---

*Structure analysis: 2026-01-24*
