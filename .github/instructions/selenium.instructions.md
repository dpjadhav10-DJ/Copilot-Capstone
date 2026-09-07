---
description: "Use when changing Selenium UI tests, browser workflows, selectors, or test fixtures in the Cafe Management solution."
applyTo: "tests/**/*.cs"
---
# Selenium Guidance

- Prefer stable semantic selectors and existing test conventions.
- Keep tests isolated, deterministic, and explicit about setup and cleanup.
- Cover the changed happy path plus relevant validation, failure, and boundary behavior.
- Do not weaken assertions to make a failing test pass.
- Record environment blocks such as unavailable SQL Server, Chrome, or ChromeDriver as blocked evidence.
