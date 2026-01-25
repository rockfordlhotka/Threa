# External Integrations

**Analysis Date:** 2026-01-24

## APIs & External Services

**Not detected** - This is a self-contained TTRPG character sheet and game management system with no external API integrations for third-party services like payment processing, analytics, or cloud AI.

## Data Storage

**Databases:**
- SQLite
  - Connection: `ConnectionStrings:Sqlite` in `appsettings.json` points to `S:/src/rdl/threa/threa.db`
  - Client: `Microsoft.Data.Sqlite` 10.0.1
  - DAL Implementation: `Threa.Dal.SqlLite/` with individual `*Dal.cs` classes:
    - `CharacterDal.cs` - Character records
    - `CharacterItemDal.cs` - Character inventory
    - `CharacterEffectDal.cs` - Effect states on characters
    - `ItemTemplateDal.cs` - Item definitions
    - `ItemEffectDal.cs` - Item effect templates
    - `EffectDefinitionDal.cs` - Game effect definitions
    - `PlayerDal.cs` - Player accounts
    - `SkillDal.cs` - Skill definitions
    - `MagicSchoolDal.cs` - Magic school definitions
    - `SpeciesDal.cs` - Species/race definitions
    - `TableDal.cs` - Lookup tables
    - `ImageDal.cs` - Character images

**File Storage:**
- Local filesystem only
  - SQLite database: `S:/src/rdl/threa/threa.db`
  - Character images stored via `IImageDal` in `Threa.Dal.SqlLite/ImageDal.cs`
  - CSV/TSV import/export files via CLI in `Threa.Admin/`

**Caching:**
- Not detected - No caching service (Redis, MemoryCache) integrated

## Authentication & Identity

**Auth Provider:**
- Custom BCrypt-based authentication
  - Implementation: Cookie-based session authentication via `Microsoft.AspNetCore.Authentication.Cookies`
  - Password hashing: `BCrypt.Net-Next` 4.0.3
  - Salt generation: `BCrypt.GenerateSalt(12)` - 12-round cost factor
  - Storage: `HashedPassword` and `Salt` fields in `Threa.Dal.Dto.Player`
  - Roles: Three-role system (Administrator, GameMaster, Player) stored as comma-separated string

**Authorization:**
- CSLA Business Rules - Declarative authorization via CSLA business objects in `GameMechanics/`
- Built-in ASP.NET Core authorization middleware
- Programmatic role checks via role string parsing in `Threa.Admin/Program.cs` (lines 353-378)

**Session Management:**
- ASP.NET Core session state for Server-side Blazor
- Cookie authentication scheme: `CookieAuthenticationDefaults.AuthenticationScheme`
- Configured in `Threa/Threa/Program.cs` (lines 20-23)
- Cascading authentication state for Blazor components

## Monitoring & Observability

**Error Tracking:**
- Not detected - No third-party error tracking (Sentry, Application Insights, etc.)
- Built-in exception handling in middleware: `.UseExceptionHandler("/Error", createScopeForErrors: true)` in `Threa/Threa/Program.cs`

**Logs:**
- ASP.NET Core built-in logging to console
- Configuration in `appsettings.json`:
  - Default level: Information
  - Microsoft.AspNetCore: Warning (reduced noise)
- Logging abstractions injected via `Microsoft.Extensions.Logging.Abstractions`

## CI/CD & Deployment

**Hosting:**
- Not yet deployed - Development/local deployment only
- Deployment target: Any ASP.NET Core host (IIS, Docker, Linux with .NET Runtime)

**CI Pipeline:**
- Not detected - No CI/CD configured (GitHub Actions, Azure Pipelines, etc.)

**Build Commands:**
```bash
dotnet build Threa.sln
dotnet run -p Threa/Threa/Threa.csproj
```

## Environment Configuration

**Required env vars:**
- `ASPNETCORE_ENVIRONMENT` - Set to "Development" or "Production" for `Program.cs` middleware behavior
- SQLite connection string via `appsettings.json` (file-based, not env var)

**Secrets location:**
- Hardcoded in `appsettings.json` (Development): SQLite path
- Password hashing happens client-side in admin CLI, not transmitted
- No API keys, credentials, or secrets detected in repository
- Git: Ensure `.gitignore` excludes `*.db` files and `appsettings.*.json` with sensitive data

## Webhooks & Callbacks

**Incoming:**
- Not detected - No webhook endpoints

**Outgoing:**
- Not detected - No outbound webhooks or event notifications

## Messaging & Event Distribution

**In-Process Messaging:**
- In-Memory Pub/Sub (Rx.NET)
  - Implementation: `GameMechanics.Messaging.InMemory/`
  - Services:
    - `ITimeEventPublisher` - Publishes game time events
    - `ITimeEventSubscriber` - Subscribes to time events
    - `IActivityLogService` - Logs game activities
  - Tech: `System.Reactive` 6.0.1
  - Registration: `services.AddInMemoryMessaging()` in `Threa/Threa/Program.cs`
  - Note: Single-server only (not distributed); consider replacing with service bus for multi-server deployments

**Data Access Patterns:**
- Data Portal pattern via CSLA - All data operations flow through `DataPortal<T>` factories
- No direct HTTP API endpoints detected - UI communicates directly to Blazor components

## Pluggable DAL

**MockDb (Testing):**
- Implementation: `Threa.Dal.MockDb/ConfigurationExtensions.cs`
- Used in all test projects via `services.AddMockDb()`
- In-memory implementations of all DAL interfaces

**SQLite (Production):**
- Implementation: `Threa.Dal.SqlLite/ConfigurationExtensions.cs`
- Registered via `services.AddSqlite()` in `Threa/Threa/Program.cs` and `Threa.Admin/Program.cs`
- All CRUD operations via parameterized `SqliteConnection` queries

---

*Integration audit: 2026-01-24*
