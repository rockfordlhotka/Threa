# Phase 1: Foundation - Context

**Gathered:** 2026-01-24
**Status:** Ready for planning

<domain>
## Phase Boundary

Create CSLA business objects (ItemTemplateEdit, CharacterItemEdit) with full data portal operations (fetch, save, delete), plus populate database with seed test data covering example items for testing. This foundation supports all future item/equipment phases.

</domain>

<decisions>
## Implementation Decisions

### Business object structure
- **Edit + ReadOnly pairs**: Full CSLA pattern — ReadOnly objects for lists/display, Edit objects for mutation
- **Template reference**: CharacterItem stores ItemTemplateId only, load template when needed — lighter weight, requires separate fetch
- **Child collections**: Claude's discretion — decide whether to use child collections (ItemBonusList, ItemEffectList) or flatten based on CSLA best practices
- **Parent navigation**: Claude's discretion — decide whether CharacterItemEdit includes CharacterId property based on query needs

### Seed data composition
- **Real campaign mix**: Proportions match typical campaign inventory — more consumables/ammo than weapons
- **Mix of realistic + edge cases**: 80% realistic usable items, 20% edge cases for boundary testing
- **Sci-fi emphasis**: Firearms, energy weapons, armor matching RANGED_WEAPONS_SCIFI.md design doc
- **Basic weapons/armor/shields**: Cover both melee and ranged, no bonuses/effects in v1 seed data — foundation testing focused

### Validation rules
- **Required properties**: Name and Type only — minimal required fields
- **Slot compatibility**: Yes, validate in business object — ItemTemplateEdit enforces slot rules at save time (two-handed weapons, etc.)
- **Weight/Volume bounds**: Non-negative (>= 0) — allow zero for abstract items (quest items, keys)
- **Container capacity**: Warning, not error — flag suspicious values (container smaller than contents capacity) but allow for GM flexibility

### Data portal patterns
- **Fetch patterns**: Claude's discretion — decide between ID-only vs ID + criteria object based on Phase 2 query needs
- **Delete operations**: Both soft and hard delete supported — Deactivate() for soft (IsActive=false), Delete() for hard deletion
- **CharacterItem creation**: Data portal only — standard Create/Insert, no factory methods, caller sets properties manually
- **List objects**: Read-only lists — ItemTemplateList and CharacterItemList for display/selection only, edit items separately

### Claude's Discretion
- Child collection organization (bonuses/effects)
- Parent navigation properties
- Fetch method signatures (criteria support)
- Exact validation rule implementation details
- DAL method patterns

</decisions>

<specifics>
## Specific Ideas

- Seed data should include items that will work well for testing combat integration in Phase 6
- Sci-fi setting aligns with RANGED_WEAPONS_SCIFI.md — laser rifles, plasma pistols, combat armor, shields
- Container validation should warn but not block — allow "magic bag of holding" scenarios

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 01-foundation*
*Context gathered: 2026-01-24*
