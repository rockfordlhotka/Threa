# Architecture

**Analysis Date:** 2026-01-24

## Pattern Overview

**Overall:** Layered CSLA.NET business object architecture with pluggable data access layer, combined with Blazor Web App (SSR + Interactive Server/WASM) for presentation.

**Key Characteristics:**
- **Domain-driven design** with CSLA.NET business objects as the central domain model
- **Repository pattern** with pluggable DAL implementations (MockDb, SQLite)
- **Resolver pattern** for stateless game rule resolution (attacks, defense, actions)
- **Pub/Sub messaging** via in-memory message bus for cross-cutting concerns
- **Hybrid Blazor rendering** with server-side business logic and client-side interactivity
- **Two-tier deployment**: Server hosts data portal; Client calls via HTTP proxy

## Layers

**Domain/Business Layer:**
- Purpose: Core game mechanics, CSLA business objects, rule resolvers
- Location: `GameMechanics/` project
- Contains: Business objects (CharacterEdit, SkillEdit, AttributeEdit), resolvers (ActionResolver, AttackResolver, DefenseResolver), game rules (AbilityScore, TargetValue, DamageResult)
- Depends on: `Threa.Dal` (interfaces only), BCrypt for password hashing
- Used by: Data portal, messaging layer, blazor components

**Data Access Layer (Interface):**
- Purpose: Defines contracts for data operations without implementation
- Location: `Threa.Dal/` project
- Contains: `I*Dal` interfaces (ICharacterDal, ISkillDal, IItemDal, IEffectDal, etc.), DTOs in `Dto/` folder
- Depends on: None (interface-only)
- Used by: Both DAL implementations and business objects

**Data Access Layer (Implementations):**
- Purpose: Pluggable implementations for different backends
- Locations:
  - `Threa.Dal.MockDb/` - In-memory mock database for development/testing
  - `Threa.Dal.SqlLite/` - SQLite-based persistence layer
- Contains: Implementation of I*Dal interfaces, configuration extensions
- Depends on: `Threa.Dal` interfaces
- Used by: Dependency injection container, configured at startup

**Messaging Layer:**
- Purpose: In-memory pub/sub for game events and time-based notifications
- Location: `GameMechanics.Messaging.InMemory/` project
- Contains: InMemoryMessageBus, InMemoryTimeEventPublisher/Subscriber, InMemoryActivityLogService
- Depends on: Rx.NET for reactive extensions
- Used by: Business objects that need to broadcast state changes

**Presentation Layer (Server):**
- Purpose: Blazor Web App host, ASP.NET Core services, data portal endpoint
- Location: `Threa/Threa/` project (server-side host)
- Contains: Program.cs (DI setup), Controllers (DataPortalController, CslaStateController), Services (ActiveCircuitHandler)
- Depends on: `GameMechanics`, all DAL projects, `Csla.AspNetCore`
- Used by: Client applications via HTTP

**Presentation Layer (Client):**
- Purpose: Blazor components, pages, client-side services
- Location: `Threa/Threa.Client/` project
- Contains: Razor components in `Components/Pages/*`, shared components in `Components/Shared/`, client services
- Depends on: `Csla.Blazor`, `Radzen.Blazor` UI components
- Used by: End users via browser (Server-Side Rendering + Interactive Server/WASM)

**Testing Layer:**
- Purpose: Unit tests for game mechanics
- Location: `GameMechanics.Test/` project
- Contains: MSTest test classes with deterministic dice roller
- Depends on: GameMechanics, mock implementations

## Data Flow

**Character Creation/Edit Flow:**

1. Blazor component calls `DataPortal.FetchAsync<CharacterEdit>(id)` or `CreateAsync()`
2. DataPortalController receives request and routes to CSLA data portal
3. CSLA invokes `[Fetch]`/`[Create]` method on CharacterEdit business object
4. CharacterEdit.Fetch calls injected `ICharacterDal` (resolved to MockDb or Sqlite implementation)
5. DAL returns DTO from database; business object populates from DTO
6. Business object returned to client via HTTP proxy serialization
7. Blazor component binds to business object properties
8. User modifies properties; component collects changes
9. Component calls `DataPortal.UpdateAsync(businessObject)`
10. CSLA invokes `[Update]` method, validates rules, persists changes via DAL

**Action Resolution Flow:**

1. Blazor page (PlayPage) constructs ActionRequest with skill, modifiers, target value
2. Calls `ActionResolver.Resolve(request)` (injected via DI)
3. ActionResolver builds AbilityScore, applies modifiers, rolls 4dF+
4. Returns ActionResult with ability score, roll, cost (AP/Fatigue), success value
5. Component displays result and updates character fatigue via business object

**Combat Resolution Flow:**

1. PlayPage constructs AttackRequest with attacker stats, defender dodge AS
2. Calls `AttackResolver.Resolve(request)` with injected IDiceRoller
3. AttackResolver calculates effective AS, rolls 4dF+, compares to passive defense TV
4. Determines hit location, rolls Physicality bonus, looks up damage via CombatResultTables
5. Returns AttackResult with all intermediate calculations
6. Component displays attack result and applies damage to defender via business object update

**State Management:**

- **Character state**: Persisted in CharacterEdit CSLA business object, dirty-tracked for save detection
- **Game state**: TableEdit business object tracks active game, characters at table, time state
- **UI state**: Client-side services (RenderModeProvider, ActiveCircuitState) manage render modes and circuit state
- **Game events**: Published via InMemoryMessageBus for cross-cutting concerns (activity logging, time events)

## Key Abstractions

**Business Objects (CSLA.NET):**
- Purpose: Domain entities with dirty tracking, validation rules, authorization rules
- Examples: `CharacterEdit`, `SkillEdit`, `AttributeEdit`, `TableEdit`, `TableCharacterInfo`
- Pattern: Extend `BusinessBase<T>` or `BusinessListBase<T, C>`, use PropertyInfo pattern for strong typing
- See: `S:\src\rdl\threa\GameMechanics\CharacterEdit.cs` for property registration pattern

**Resolvers:**
- Purpose: Stateless computation of game mechanics (attacks, defenses, actions)
- Examples: `ActionResolver`, `AttackResolver`, `DefenseResolver`, `DamageResolver`, `SprintResolver`
- Pattern: Accept request object, use injected IDiceRoller, return immutable result object
- See: `S:\src\rdl\threa\GameMechanics\Combat\AttackResolver.cs` for resolver pattern

**Request/Result Objects:**
- Purpose: Strongly-typed containers for resolver inputs/outputs
- Examples: `ActionRequest`, `ActionResult`, `AttackRequest`, `AttackResult`, `DefenseRequest`, `DefenseResult`
- Pattern: Immutable (results), builder pattern for complex requests (see ActionRequestBuilder)

**Data Transfer Objects (DTOs):**
- Purpose: Simple POCOs for data layer, no validation or rules
- Location: `Threa.Dal/Dto/`
- Examples: `Character`, `CharacterAttribute`, `CharacterSkill`, `CharacterItem`, `CharacterEffect`
- Pattern: Auto-properties only, direct mapping from database

**DAL Interfaces:**
- Purpose: Contract between business layer and data implementations
- Location: `Threa.Dal/I*Dal.cs`
- Examples: `ICharacterDal`, `ISkillDal`, `IItemDal`, `IEffectDal`
- Pattern: Async methods returning DTOs, injectable via DI container

**Effect Behaviors:**
- Purpose: Pluggable implementations for different effect types (wounds, buffs, poisons, etc.)
- Location: `GameMechanics/Effects/Behaviors/`
- Examples: `WoundBehavior`, `SpellBuffBehavior`, `DebuffBehavior`, `PoisonBehavior`
- Pattern: Implement effect-specific logic, used by EffectBehaviorFactory

## Entry Points

**Server Entry Point:**
- Location: `S:\src\rdl\threa\Threa\Threa\Program.cs`
- Triggers: `dotnet run` in Threa project directory
- Responsibilities:
  - Configure ASP.NET Core dependency injection
  - Register CSLA services with AspNetCore and ServerSideBlazor
  - Register DAL implementation (MockDb or Sqlite)
  - Register GameMechanics services
  - Register in-memory messaging
  - Configure authentication/authorization
  - Map Razor components and data portal controller

**Client Entry Point:**
- Location: `S:\src\rdl\threa\Threa\Threa.Client\Program.cs`
- Triggers: WebAssembly host bootstrapping
- Responsibilities:
  - Configure HTTP client
  - Register CSLA Blazor WASM integration
  - Configure client-side data portal with HTTP proxy
  - Register cascade authentication state

**App Component:**
- Location: `S:\src\rdl\threa\Threa\Threa\Components\App.razor`
- Triggers: First page load
- Responsibilities: Root component, defines routing, layout, auth boundary

**Pages:**
- Location: `S:\src\rdl\threa\Threa\Threa.Client\Components\Pages\`
- Examples: Characters.razor, CharacterEdit.razor, GameMaster/Characters.razor, GamePlay/Play.razor
- Pattern: Route-mapped Razor components, async data loading in OnInitializedAsync, form binding to business objects

## Error Handling

**Strategy:** Exceptions propagate from resolvers up through business objects to Blazor components; components catch and display error messages.

**Patterns:**
- **Domain exceptions**: OperationFailedException, DuplicateKeyException, NotFoundException in `Threa.Dal/`
- **Validation**: CSLA business rules checked via `BusinessRules.CheckRules()`, validation errors added to `BrokenRules` collection
- **Authorization**: CSLA authorization rules enforce field/method access; unauthorized operations throw AuthorizationException
- **Data portal errors**: Wrapped by CSLA and returned as BusinessLayerException on client
- **Component error handling**: Try-catch in OnInitializedAsync, display error toast/modal via Radzen components

## Cross-Cutting Concerns

**Logging:** No dedicated logger found; could use CSLA's built-in logging or ILogger from ASP.NET Core

**Validation:**
- Business objects use CSLA `AddRule()` for property validation
- DTOs validated implicitly via business object rules
- Client-side Blazor provides form validation via data annotations

**Authentication:**
- Cookie-based via ASP.NET Core authentication
- Cascading auth state to Blazor components
- CSLA authorization rules tied to principal claims

**Dependency Injection:**
- Centralized in Program.cs via builder.Services
- CSLA business objects can use [Inject] attribute on parameters
- DAL implementations registered as Singleton or Scoped per interface
- Resolvers injected with IDiceRoller for testability

---

*Architecture analysis: 2026-01-24*
