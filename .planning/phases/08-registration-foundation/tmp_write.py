with open("08-VERIFICATION.md", "w", encoding="utf-8") as f:
    lines = [
        "---",
        "phase: 08-registration-foundation",
        "verified: 2026-01-26T08:17:29Z",
        "status: human_needed",
        "score: 4/4 must-haves verified",
        "human_verification:",
        '  - test: "Register first user and verify admin role assignment"',
        '    expected: "First registered user can access admin features"',
        '    why_human: "Requires running app and verifying role-based UI behavior"',
        '  - test: "Register second user and verify Player role assignment"',
        '    expected: "Second registered user does NOT have admin access"',
        '    why_human: "Requires running app and verifying role-based UI behavior"',
        '  - test: "Attempt duplicate username registration"',
        '    expected: "Clear error message displayed on registration page"',
        '    why_human: "Requires running app to test UI error display"',
        '  - test: "Verify validation errors display correctly"',
        '    expected: "Short password/username shows validation messages inline"',
        '    why_human: "Requires running app to verify UI validation behavior"',
        "---",
        "",
        "# Phase 8: Registration Foundation Verification Report"
    ]
    for line in lines:
        f.write(line + "\n")
print("Frontmatter written")
