# Phase 9: Password Recovery - Context

**Gathered:** 2026-01-26
**Status:** Ready for planning

<domain>
## Phase Boundary

Self-service password reset workflow for users who forget their password. Users enter their username, answer their secret question, and set a new password. Email-based recovery and two-factor authentication are separate phases.

</domain>

<decisions>
## Implementation Decisions

### Recovery flow steps
- Multi-step wizard (3 steps): Step 1: Enter username → Step 2: Answer secret question → Step 3: Set new password
- Allow back navigation between steps (users can click back to change username or retry)
- Progress indicator: Simple text showing "Step 1 of 3", "Step 2 of 3", "Step 3 of 3"
- Flow state: Restart from beginning on page navigation or refresh (no session state preservation)

### Security & validation
- Allow 3-5 incorrect answer attempts before lockout
- Lockout behavior: Time-based lockout (15-30 minutes) after max attempts
- Error display: Inline error message immediately when wrong answer submitted
- Error detail: Show remaining attempts (e.g., "Incorrect answer. You have 2 attempts remaining.")

### User feedback & messaging
- Unknown username: Generic success message ("If that username exists, the secret question will be shown.") to prevent username enumeration
- Success confirmation: Success page with manual login link ("Password reset successful! Click here to log in.")
- Message tone: Formal and professional throughout
- Message dismissal: Manual dismiss only (user must click X or OK to close messages)

### Password requirements
- Password rules: Same as registration (minimum 6 characters) for consistency
- Validation timing: Real-time validation as user types
- Confirmation field: Yes, require "Confirm Password" field (must match)
- Strength indicator: No password strength indicator

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 09-password-recovery*
*Context gathered: 2026-01-26*
