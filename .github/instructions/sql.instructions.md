---
description: "Use when changing SQL schema, migrations, seed data, queries, or database-related C# code in the Cafe Management solution."
applyTo: ["database/**/*.sql", "**/*.cs"]
---
# SQL Guidance

- Use parameterized queries and preserve existing connection and transaction patterns.
- Review keys, unique constraints, foreign keys, nullability, and concurrency implications.
- Treat drops, truncations, destructive updates, and production data changes as approval-required.
- Make schema changes traceable and idempotent where the repository pattern supports it.
- Never place passwords or secrets in scripts, examples, logs, or test output.
- Verify database changes with the narrowest available check before broader validation.
