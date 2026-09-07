---
description: "Use when changing C# backend, services, models, APIs, or .NET tests in the Cafe Management solution."
applyTo: "**/*.cs"
---
# C# Guidance

- Match existing ASP.NET Core and service patterns before adding abstractions.
- Validate inputs at the boundary and preserve clear error behavior.
- Do not log secrets or connection-string credentials.
- Keep database access parameterized and cancellation-aware where the surrounding code supports it.
- Add or update focused tests for changed behavior; report unexecuted checks honestly.
- Keep changes limited to the approved user-story scope.
