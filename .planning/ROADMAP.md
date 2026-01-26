# Roadmap: Threa TTRPG Assistant

## Milestones

- [x] **v1.0 Inventory & Equipment System** - Phases 1-7 (shipped 2026-01-26)
- [ ] **v1.1 User Management & Authentication** - Phases 8-11 (in progress)

## Phases

<details>
<summary>v1.0 Inventory & Equipment System (Phases 1-7) - SHIPPED 2026-01-26</summary>

See MILESTONES.md for v1.0 details.

**Key accomplishments:**
- Complete CSLA business object foundation with 38 passing unit tests
- GM item template management with full CRUD
- Player inventory system with equipment slot management
- Container system with nesting enforcement
- Item bonuses integrated with combat system
- Real-time GM item distribution
- 52 seed items across all categories

</details>

### v1.1 User Management & Authentication (In Progress)

**Milestone Goal:** Enable self-service user registration and admin user management without email dependency. Users can register, recover passwords via secret Q&A, and manage profiles with Gravatar integration.

- [x] **Phase 8: Registration Foundation** - Self-service registration with first-user-as-Admin (2026-01-26)
- [ ] **Phase 9: Password Recovery** - Secret Q&A password reset workflow
- [ ] **Phase 10: Admin User Management** - Admin panel for user control
- [ ] **Phase 11: User Profiles** - Enhanced profiles with Gravatar avatars

## Phase Details

### Phase 8: Registration Foundation
**Goal**: New users can self-register and the first user automatically becomes the system administrator
**Depends on**: Phase 7 (existing auth system)
**Requirements**: AUTH-01, AUTH-02, AUTH-03
**Success Criteria** (what must be TRUE):
  1. New user can access registration page and create account with username, password (min 6 chars), and secret question/answer
  2. First registered user in the system automatically has Admin role after registration completes
  3. Subsequent registered users have User role by default (not Admin, not GameMaster)
  4. Duplicate usernames are rejected with clear error message
**Plans:** 2 plans

Plans:
- [x] 08-01-PLAN.md - Data layer and UserRegistration business object with validation, first-user-admin logic
- [x] 08-02-PLAN.md - Registration UI page and login page link

### Phase 9: Password Recovery
**Goal**: Users who forget their password can reset it using their secret question/answer
**Depends on**: Phase 8
**Requirements**: AUTH-04, AUTH-05, AUTH-06
**Success Criteria** (what must be TRUE):
  1. User can initiate password reset by entering their username
  2. System displays the user's secret question (without revealing answer)
  3. User's answer is validated case-insensitively with trimmed whitespace
  4. After correct answer, user can set a new password and immediately log in
  5. Incorrect answer shows error but does not reveal correct answer
**Plans:** 2 plans

Plans:
- [x] 09-01-PLAN.md - Data layer extensions and PasswordRecovery business object with lockout logic
- [x] 09-02-PLAN.md - Password recovery UI wizard (3 steps)

### Phase 10: Admin User Management
**Goal**: Administrators can view, enable/disable, and assign roles to all users
**Depends on**: Phase 8
**Requirements**: USER-01, USER-02, USER-03, USER-04, USER-05, USER-06
**Success Criteria** (what must be TRUE):
  1. Admin can view list of all users showing username, display name, roles, and enabled status
  2. Admin can disable a user account (user cannot log in, but data preserved)
  3. Admin can enable a previously disabled user account
  4. Admin can assign and remove roles (User, GameMaster, Admin) to any user
  5. Disabled users are blocked at login with appropriate message
**Plans**: TBD

Plans:
- [ ] 10-01: User management data layer and business objects
- [ ] 10-02: Admin user management UI

### Phase 11: User Profiles
**Goal**: Users can customize their profiles with display names, email, and Gravatar-based avatars
**Depends on**: Phase 8
**Requirements**: PROF-01, PROF-02, PROF-03, PROF-04, PROF-05, PROF-06, PROF-07
**Success Criteria** (what must be TRUE):
  1. User can set a display name (shown throughout UI instead of username)
  2. User can optionally provide and update their email address
  3. When email is provided, user avatar displays via Gravatar (with retro fallback)
  4. When email is not provided, avatar displays initials from display name
  5. User can view their own profile page
  6. Users can view other users' public profiles showing display name and avatar
**Plans**: TBD

Plans:
- [ ] 11-01: Profile data layer and business objects
- [ ] 11-02: Profile UI and Gravatar integration

## Progress

**Execution Order:** Phases 9, 10, 11 can execute in parallel after Phase 8 completes.

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1-7 | v1.0 | 16/16 | Complete | 2026-01-26 |
| 8. Registration Foundation | v1.1 | 2/2 | Complete | 2026-01-26 |
| 9. Password Recovery | v1.1 | 2/2 | Complete | 2026-01-26 |
| 10. Admin User Management | v1.1 | 0/2 | Not started | - |
| 11. User Profiles | v1.1 | 0/2 | Not started | - |

---
*Roadmap created: 2026-01-26*
*Last updated: 2026-01-26*
