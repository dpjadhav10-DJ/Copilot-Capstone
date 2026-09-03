---
name: preview-approval-verification
description: 'Use when a Cafe Management workflow reviews, proposes, implements, updates, tests, or publishes changes. Enforces preview, explicit approval, scoped execution, and evidence-based completion reporting.'
user-invocable: false
---

# Preview, Approval, and Verification

Use the following control sequence for work that changes documents, code, tests, branches, or publication state:

1. Inspect the relevant source, current changes, and repository context before proposing work.
2. Prepare a concise preview covering findings or intent, proposed changes, impacted files or components, risks, dependencies, and unclear areas.
3. Do not modify anything before explicit user confirmation when the role requires approval. If rejected, report `Cancelled`; if revisions are requested, revise the preview and ask again.
4. After approval, apply only the approved scope. Preserve the original intent and do not introduce unsupported decisions or unrelated changes.
5. Verify the result with available checks, tests, build output, review evidence, or publication confirmation appropriate to the role.
6. Report status and evidence factually. If verification is incomplete, failed, skipped, or blocked, state that clearly and do not claim success.

Keep role-specific preview formats, approval gates, output fields, and workflow sequencing in the calling agent.
