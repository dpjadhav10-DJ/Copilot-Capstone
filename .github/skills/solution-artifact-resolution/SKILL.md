---
name: solution-artifact-resolution
description: 'Use when an agent must resolve a UserStoryId, document name, or workflow artifact in the current solution. Defines exact matching, ambiguity handling, cancellation, naming, and same-folder rules.'
user-invocable: false
---

# Solution Artifact Resolution

Apply this procedure whenever a workflow input identifies a user story or document:

1. Accept only the supplied identifier or document name. Ask only for the missing required input and wait.
2. Search only within the current solution.
3. Prefer an exact folder or file-name match. A standard suffix such as `.md` may be treated as the same exact source document.
4. If no match exists, report `Aborted`, explain what was not found, and do not create or modify an artifact.
5. If multiple plausible matches exist, report `Aborted`, list the ambiguous locations, and do not guess. For a user-story source document, ask the user to identify the source.
6. If the user cancels or provides no usable identifier, report `Cancelled` with the reason.
7. Read the complete source document before analyzing or generating its dependent artifact.
8. Keep generated artifacts in the matching user-story folder and preserve exact names returned by prior workflow steps.

Use these artifact names:

- `US-{UserStoryId}-RequirementAnalysis.md`
- `US-{UserStoryId}-SystemArchitecture.md`
- `US-{UserStoryId}-ImplementationPlan.md`
- `US-{UserStoryId}-TestSummary.md`

When an expected artifact already exists, preserve it and report `Aborted` unless the user explicitly requested regeneration or an update. Do not modify source user-story documents unless the role explicitly permits it.
