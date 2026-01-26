# Phase 10: Admin User Management - Context

**Gathered:** 2026-01-26
**Status:** Ready for planning

<domain>
## Phase Boundary

Admin panel for viewing all users and controlling their access. Admins can view user list showing username, display name, roles, and enabled status. Admins can enable/disable accounts and assign/remove roles (User, GameMaster, Admin). Disabled users cannot log in but data is preserved.

</domain>

<decisions>
## Implementation Decisions

### Role assignment interface
- Edit button per user opens modal for editing
- Modal shows checkboxes for GameMaster and Admin roles
- User role is implicit/always present (not shown as checkbox)
- Modal includes editable profile fields (display name, email) alongside roles
- Inline success message appears in modal after save
- Admin manually closes modal after reviewing success message

### Enable/disable behavior
- Enabled/disabled status is controlled via checkbox in the edit modal (same modal as roles)
- No confirmation required when disabling (action is reversible)
- Disabled users shown with icon indicator (e.g., lock icon) next to username in list
- User list has sortable enabled/disabled status column (click header to group)

### Admin safety guardrails
- Admin disabling their own account: warn but allow ("You're about to disable your own account")
- Admin removing their own Admin role: warn but allow ("You'll lose admin access")
- Last remaining admin protection: block completely with error ("System must have at least one enabled admin")
- No audit logging in this phase (can be added later if needed)

### Claude's Discretion
- Exact modal layout and styling
- Warning message wording
- Icon choice for disabled status
- Table column order and default sort

</decisions>

<specifics>
## Specific Ideas

- Combined edit modal handles both role assignment and profile editing in one place
- System ensures at least one enabled admin exists at all times (hard constraint)

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 10-admin-user-management*
*Context gathered: 2026-01-26*
