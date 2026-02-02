# Phase 25 Plan 03: NPC Spawner Command Summary

## One-liner
CSLA CommandBase that clones NPC template to full playable character with table attachment.

## What Was Built

### NpcSpawner Command (GameMechanics/NpcSpawner.cs)

A CSLA `CommandBase<NpcSpawner>` that creates new NPC instances from templates:

**Input Properties:**
- `TemplateId` - ID of template character to spawn from
- `TableId` - Table to attach spawned NPC to
- `Name` - Name for the spawned NPC (auto-generated or custom)
- `Disposition` - Override template's default disposition
- `SessionNotes` - Optional session-specific notes

**Output Properties:**
- `SpawnedCharacterId` - ID of newly created NPC character
- `Success` - Whether spawn operation succeeded
- `ErrorMessage` - Error details if spawn failed

**Execute Flow:**
1. Fetch template character from DAL
2. Verify it's actually a template (IsTemplate=true)
3. Create new Character DTO copying all stats:
   - Identity (name, species, description, image)
   - Physical description (height, weight, skin, hair)
   - Health pools (FAT/VIT base values, starting at full)
   - Action points (starting at max)
   - Combat stats (damage class, difficulty rating)
   - XP and currency
   - All attributes (deep copy)
   - All skills (deep copy with all 30+ Skill properties)
4. Set NPC flags: IsNpc=true, IsTemplate=false, IsPlayable=true
5. Track source: SourceTemplateId, SourceTemplateName
6. Save via characterDal.SaveCharacterAsync()
7. Attach to table via tableDal.AddCharacterToTableAsync()

## Key Implementation Details

### NPC Flags
Spawned NPCs get:
- `IsNpc = true` - Marks as NPC (not player character)
- `IsTemplate = false` - Not a template (can be played)
- `IsPlayable = true` - Ready for play immediately (no activation)

### Health/AP Initialization
- FAT/VIT current values set to base values (full health)
- Pending damage/healing cleared to zero
- AP available set to max

### Template Tracking
- `SourceTemplateId` stores template's character ID
- `SourceTemplateName` stores template's name for display
- Template organization fields (Category, Tags, TemplateNotes) cleared

### Table Attachment
TableCharacter record created with:
- ConnectionStatus = Connected
- GmNotes = SessionNotes (if provided)
- JoinedAt = current UTC time

## Files Changed

| File | Change |
|------|--------|
| `GameMechanics/NpcSpawner.cs` | Created - CSLA command for NPC spawning |

## Commits

| Hash | Message |
|------|---------|
| 003d006 | feat(25-03): add NpcSpawner command for spawning NPCs from templates |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed CharacterSkill property names**
- **Found during:** Task 1 build verification
- **Issue:** Plan referenced non-existent properties (BaseLevel, TotalLevel, AssociatedAttributeName, DefaultLevel, IsAvailable)
- **Fix:** Used actual CharacterSkill properties (Level, XPBanked, XPSpent) plus inherited Skill properties
- **Files modified:** GameMechanics/NpcSpawner.cs

**2. [Rule 3 - Blocking] Used SaveCharacterAsync instead of InsertCharacterAsync**
- **Found during:** Task 1 implementation
- **Issue:** Plan referenced InsertCharacterAsync which doesn't exist
- **Fix:** Used SaveCharacterAsync which handles insert when Id=0
- **Files modified:** GameMechanics/NpcSpawner.cs

## Verification Results

All verification criteria passed:
- NpcSpawner.cs exists in GameMechanics/
- Command has all required input properties (TemplateId, TableId, Name, Disposition, SessionNotes)
- Command has all required output properties (SpawnedCharacterId, Success, ErrorMessage)
- Execute method clones template and sets NPC flags correctly
- Spawned NPC is attached to table via TableCharacter
- Full solution builds successfully (0 errors)

## Dependencies

**Requires:**
- 25-01 (Data Model Extensions) - SourceTemplateId/SourceTemplateName on Character DTO

**Provides:**
- Core business logic for NPC spawning from templates
- Foundation for 25-04 (Spawn NPC Modal) UI

## Duration

~3 minutes
