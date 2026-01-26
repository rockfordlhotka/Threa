# Requirements: Threa TTRPG Assistant

**Defined:** 2026-01-26
**Core Value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.

## v1.1 Requirements

Requirements for v1.1 User Management & Authentication milestone. Each maps to roadmap phases.

### Registration & Authentication

- [ ] **AUTH-01**: New user can self-register with username (unique), password (min 6 chars), and secret question/answer
- [ ] **AUTH-02**: First registered user automatically receives Admin role
- [ ] **AUTH-03**: Subsequent registered users receive User role by default
- [ ] **AUTH-04**: User can initiate password reset by providing username
- [ ] **AUTH-05**: System validates user identity using secret question/answer (case-insensitive, trimmed)
- [ ] **AUTH-06**: User can set new password after successful secret answer validation

### User Management (Admin)

- [ ] **USER-01**: Admin can view list of all users with username, display name, roles, and enabled status
- [ ] **USER-02**: Admin can disable a user account (user locked out, data preserved)
- [ ] **USER-03**: Admin can enable a previously disabled user account
- [ ] **USER-04**: Admin can assign roles to users (User, GameMaster, Admin)
- [ ] **USER-05**: Admin can remove roles from users
- [ ] **USER-06**: Disabled users cannot log in

### User Profile

- [ ] **PROF-01**: User can set a display name (shown in UI, separate from login username)
- [ ] **PROF-02**: User can optionally provide an email address
- [ ] **PROF-03**: User can update their email address at any time
- [ ] **PROF-04**: User avatar displays via Gravatar when email is provided
- [ ] **PROF-05**: User avatar displays initials fallback when email is not provided
- [ ] **PROF-06**: User can view their own profile
- [ ] **PROF-07**: Users can view other users' public profiles (display name, avatar)

## Future Requirements

Deferred to future milestones. Tracked but not in current roadmap.

### Authentication Enhancements

- **AUTH-07**: Session timeout/expiration settings
- **AUTH-08**: Password strength requirements (complexity rules)
- **AUTH-09**: Two-factor authentication (2FA)
- **AUTH-10**: OAuth/SSO integration (Google, Discord, etc.)

### User Management Enhancements

- **USER-07**: Audit log (who logged in when, role changes)
- **USER-08**: Bulk user operations (import, export)
- **USER-09**: User activity tracking (last login, last action)

### Profile Enhancements

- **PROF-08**: Custom avatar upload (alternative to Gravatar)
- **PROF-09**: Bio/description field
- **PROF-10**: Profile privacy settings (public/private)

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Email verification for registration | No email sending capability, not required for semi-private deployment |
| Email-based password recovery | No email sending capability, using secret Q&A instead |
| Account deletion by user | Data retention preferred, admin can disable accounts |
| Multiple secret questions | One Q&A pair sufficient for semi-private deployment |
| CAPTCHA on registration | Semi-private deployment, known user group, low spam risk |
| Password history/reuse prevention | Unnecessary complexity for current scale |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| AUTH-01 | Phase 8 | Pending |
| AUTH-02 | Phase 8 | Pending |
| AUTH-03 | Phase 8 | Pending |
| AUTH-04 | Phase 9 | Pending |
| AUTH-05 | Phase 9 | Pending |
| AUTH-06 | Phase 9 | Pending |
| USER-01 | Phase 10 | Pending |
| USER-02 | Phase 10 | Pending |
| USER-03 | Phase 10 | Pending |
| USER-04 | Phase 10 | Pending |
| USER-05 | Phase 10 | Pending |
| USER-06 | Phase 10 | Pending |
| PROF-01 | Phase 11 | Pending |
| PROF-02 | Phase 11 | Pending |
| PROF-03 | Phase 11 | Pending |
| PROF-04 | Phase 11 | Pending |
| PROF-05 | Phase 11 | Pending |
| PROF-06 | Phase 11 | Pending |
| PROF-07 | Phase 11 | Pending |

**Coverage:**
- v1.1 requirements: 19 total
- Mapped to phases: 19
- Unmapped: 0

---
*Requirements defined: 2026-01-26*
*Last updated: 2026-01-26 after roadmap creation*
