# Cafe Management Workspace Instructions

## Workflow invariants
- Preserve the orchestrator sequence exactly: requirements, architecture, design review, implementation plan, implementation, code review, testing, publishing.
- Execute stages serially. Never skip, reorder, or parallelize stages.
- Stop on `Cancelled`, `Aborted`, or `Error` and report the failed stage.
- Preserve explicit approval gates before design updates, implementation, review fixes, commits, pushes, and pull requests.
- Do not claim completion without executable or repository evidence.

## Artifact invariants
- Resolve inputs only within this solution and prefer exact folder and file matches.
- Read complete source artifacts before analysis or generation.
- Keep generated artifacts in the matching `UserStories/US-{id}` folder with exact names.
- Do not overwrite an existing artifact unless the user explicitly requested an update or regeneration.
- Never modify source user-story documents unless the current role explicitly permits it.

## Scope and security
- Preserve user-story terminology, identifiers, and confirmed scope.
- Do not invent business rules, test results, credentials, or unsupported design decisions.
- Treat C#, SQL, and Selenium as relevant implementation lenses.
- Keep changes minimal and exclude unrelated files, especially `bin/`, `obj/`, `.vs/`, `TestResults/`, and `node_modules/`.
- Never print or commit passwords, tokens, private keys, or connection-string secrets.
- Do not use destructive Git commands or force-push unless explicitly approved for the current operation.
- Before committing, inspect staged files, run `git diff --cached --check`, and confirm generated output and secrets are excluded.
- Before pushing, confirm the approved branch and files, run the applicable build and tests, and report failures factually.
- Before creating a pull request, confirm the source and target branches, existing PR state, test evidence, and approved publication scope.
- These checks remain agent responsibilities and must be reported factually; configured hooks provide an additional deterministic guard and do not replace approval gates.

## Handoff format
Use this compact handoff when returning to the orchestrator:

```text
Status: Completed | Cancelled | Aborted | Error
UserStoryId: <value when known>
InputArtifact: <path when applicable>
OutputArtifact: <path when applicable>
Evidence: <commands, tests, or repository evidence>
NextStep: <next serial workflow stage or stop reason>
```

Reuse captured artifact paths from prior stages instead of searching again. Keep status, evidence, limitations, and missing prerequisites factual and concise.
