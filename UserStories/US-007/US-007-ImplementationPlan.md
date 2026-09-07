# Implementation Plan: US-007

## 1. Source and Summary
- User story reference: US-007, Addition of User Management Menu on the application pages.
- Source architecture document: `UserStories/US-007/US-007-SystemArchitecture.md`
- Plan objective: Add a persistent User Management navigation group with accessible button-driven expansion and client-rendered future-development placeholders while preserving the existing application shell and behavior.
- Approved interaction scope: User Management expands and collapses by click or native keyboard activation. Add User and Search User are activatable in-page, non-anchor controls that render the exact approved messages in `.story-panel`.
- Solution scope summary: Update the existing static shell, TypeScript source, synchronized compiled browser artifact, and responsive styles; add focused Selenium coverage. No C# backend, API, SQL, persistence, authentication, or authorization changes are planned.

## 2. Implementation Strategy

### Delivery approach
Extend the existing .NET-hosted static single-page shell in place. Add the User Management button and submenu to `wwwroot/index.html`, implement expansion and placeholder rendering in `src/main.ts`, run the existing TypeScript build so `wwwroot/main.js` remains synchronized, and make only focused responsive/theme adjustments in `wwwroot/styles.css`. Add stable Selenium coverage in the existing NUnit test project after the UI behavior is implemented.

### Sequencing rationale
1. Establish persistent semantic navigation markup and stable selectors without adding anchors or routes.
2. Implement the observable expansion state and keyboard-capable placeholder controls in the TypeScript source.
3. Build TypeScript and confirm the generated `wwwroot/main.js` matches the source output.
4. Preserve the existing theme, shell, footer, and responsive layout with focused CSS changes only.
5. Add and execute Selenium coverage for behavior, accessibility, no-navigation/no-request semantics, persistence, regression, and the narrow viewport.
6. Run focused build and UI verification, then inspect the diff for scope violations.

### Dependencies and prerequisites
- Existing `wwwroot/index.html`, `src/main.ts`, `wwwroot/main.js`, `wwwroot/styles.css`, and `.story-panel` rendering boundary.
- Existing `data-testid` convention and persistent navigation/footer shell.
- Node.js/npm and the TypeScript dependencies installed in `src/CafeManagement`.
- .NET 8 SDK, Chrome, compatible ChromeDriver, and the existing Selenium test project.
- Running application available through `CAFE_BASE_URL` or the existing local default for browser tests.

### Assumptions and constraints
- User Management is a persistent navigation group visible on all existing application views.
- The submenu is collapsed initially and exposes exactly Add User and Search User when expanded.
- The top-level control uses an observable accessible state such as `aria-expanded` and `aria-controls`; native button keyboard behavior is retained.
- Add User and Search User use in-page button or equivalent non-anchor controls and do not change the URL, browser history, or route.
- Placeholder activation replaces only `.story-panel` content and leaves the navigation, page shell, and footer present exactly once.
- The exact messages are `Add User is reserved for future development.` and `Search User is reserved for future development.`
- The current 700-pixel media-query breakpoint remains the narrow-layout boundary.
- No new router, URL/hash contract, runtime dependency, API, persistence, user-management model, authentication, or authorization behavior is introduced.

## 3. Step-by-Step Implementation Tasks

### TASK-001: Add persistent User Management markup
- Description: Add a persistent User Management navigation group to `wwwroot/index.html` with the exact `User Management` label, a button selector `data-testid="nav-user-management"`, an initially collapsed submenu selector `data-testid="user-management-submenu"`, and exactly the Add User and Search User controls with selectors `nav-add-user` and `nav-search-user`.
- Primary layer impacted: HTML presentation layer.
- Dependencies: Existing persistent navigation and page-shell structure.
- Expected outcome: User Management is present on initial load and remains outside the replaceable `.story-panel`; the four existing navigation anchors remain unchanged.
- Notes or risks: Do not implement the new controls as anchors, add unsupported hrefs, or introduce additional submenu entries. Preserve the existing footer position and shell structure.

### TASK-002: Implement accessible expansion and placeholder rendering
- Description: In `src/main.ts`, wire the User Management button to toggle submenu visibility/state on click and native keyboard activation, keeping `aria-expanded` and `aria-controls` or an equivalent observable DOM state synchronized. Handle Add User and Search User as in-page controls that render the exact approved messages inside `.story-panel` using stable placeholder selectors.
- Primary layer impacted: TypeScript presentation behavior.
- Dependencies: TASK-001 and existing renderer/event-handling conventions.
- Expected outcome: The submenu expands and collapses without navigation; each placeholder renders only its corresponding message and remains client-side and transient.
- Notes or risks: Avoid duplicate listeners during repeated view transitions. Do not submit forms, change URL/history, call a user-management endpoint, persist data, or allow unsupported routes. Preserve existing Home, Calculate Bill, Add/Remove Cafe Menu, Reach Us At, and footer behavior.

### TASK-003: Build and synchronize the browser artifact
- Description: Run the existing TypeScript build from `src/CafeManagement` and verify the emitted `wwwroot/main.js` is synchronized with `src/main.ts` according to `tsconfig.json` output.
- Primary layer impacted: TypeScript build artifact.
- Dependencies: TASK-002 and installed npm dependencies.
- Expected outcome: The browser executes the implemented behavior from the generated JavaScript and no stale compiled implementation remains.
- Notes or risks: `wwwroot/main.js` is a generated deliverable for this application and must be updated only through the existing build process where possible. Do not introduce a separate bundler or runtime dependency.

### TASK-004: Preserve theme and responsive behavior
- Description: Update `wwwroot/styles.css` only as needed for submenu layout, focus treatment, placeholder presentation, and the existing responsive breakpoint, reusing current theme variables, typography, spacing, and layout conventions.
- Primary layer impacted: CSS presentation layer.
- Dependencies: TASK-001 and TASK-002.
- Expected outcome: User Management controls and placeholder content remain readable, keyboard-visible, within the viewport, and non-overlapping at desktop and 700-pixel-or-lower widths; the footer and existing shell remain intact.
- Notes or risks: Do not create a new theme, alter unrelated view styles, use CSS-only state that hides the accessibility state, or permit horizontal overflow and panel overlap.

### TASK-005: Add focused Selenium behavior coverage
- Description: Extend the existing NUnit/Selenium coverage with stable selectors for visibility, exact labels, expansion/collapse by click, native keyboard access, observable accessibility state, exact placeholder messages, non-anchor semantics, no URL navigation, no user-management API calls, and transient `.story-panel` rendering.
- Primary layer impacted: Selenium UI tests.
- Dependencies: TASK-001 through TASK-004.
- Expected outcome: The approved interaction rule is executable and regressions fail with focused diagnostics.
- Notes or risks: Assert DOM/accessibility state rather than timing or CSS-only assumptions. Use request inspection or the existing harness capability to demonstrate no unsupported user-management request; do not claim network evidence if the harness cannot observe it.

### TASK-006: Add persistence, regression, and narrow-layout coverage
- Description: Verify User Management remains visible after Home, Calculate Bill, Add/Remove Cafe Menu, and Reach Us At transitions; verify the shell and exactly one footer persist after placeholder activation; retain the existing four-anchor navigation regression; and check controls/messages at the existing narrow viewport boundary.
- Primary layer impacted: Selenium UI tests and integration verification.
- Dependencies: TASK-005 and existing deterministic view data.
- Expected outcome: Existing navigation remains usable, no duplicate shell/footer appears, placeholder state does not strand the user, and labels/messages remain inside the viewport without overlap.
- Notes or risks: Use existing deterministic cafe-story, menu, and bill data only for regression checks. Report unavailable error-state, final-bill, or network-observation scenarios as unexecuted rather than inferred as passing.

### TASK-007: Execute focused verification and scope review
- Description: Run the TypeScript build, compile the solution, start the application for browser verification, run the focused US-007 Selenium tests plus existing navigation regression tests, inspect generated-artifact synchronization, and check the final diff for unintended C#, SQL, API, persistence, auth, authorization, or unrelated files.
- Primary layer impacted: Integration and verification.
- Dependencies: TASK-001 through TASK-006 and environment prerequisites.
- Expected outcome: Build, UI, accessibility, responsive, no-navigation/no-request, regression, and scope evidence is captured factually for handoff.
- Notes or risks: Do not claim tests or network checks were executed without a running application, Chrome/ChromeDriver, and applicable harness support.

## 4. C# Backend Tasks
- No C# controller, endpoint, service, domain-logic, model, DTO, validation, or error-handling changes.
- No user-management API contract or server-side placeholder rendering.
- No logging, authentication, authorization, role, permission, or account-rule changes.
- Run the existing solution build as a regression check only; do not add backend implementation for this presentation-only story.

## 5. SQL Database Tasks
- No database schema, table, column, relationship, key, constraint, index, migration, seed, transformation, transaction, concurrency, audit, or persistence changes.
- No user-record reads or writes are introduced.
- Existing data is needed only for regression navigation checks of existing views, not for User Management placeholders.

## 6. Selenium UI Testing Tasks

### Automated scenarios
- Verify `User Management` is visible on initial load and after each existing top-level view transition.
- Verify the submenu contains exactly `Add User` and `Search User` after expansion.
- Verify click expansion and collapse, with `aria-expanded`/`aria-controls` or equivalent observable DOM state changing correctly.
- Verify native keyboard activation of the User Management button and keyboard access to the submenu controls.
- Verify Add User and Search User are non-anchor in-page controls and do not expose unsupported navigation targets.
- Verify Add User renders `Add User is reserved for future development.` inside `.story-panel`, with `data-testid="add-user-placeholder"`.
- Verify Search User renders `Search User is reserved for future development.` inside `.story-panel`, with `data-testid="search-user-placeholder"`.
- Verify exact message text, stable selectors, and no stale opposite placeholder remain after switching controls.
- Verify placeholder activation does not change the URL/history, submit a form, navigate to a broken route, or issue a user-management API/network request when the test harness can observe requests.
- Verify the navigation, page shell, and exactly one footer remain present after placeholder activation.
- Preserve the existing assertion that `[data-testid='navigation'] a` contains exactly four anchors.
- Verify Home, Calculate Bill, Add/Remove Cafe Menu, and Reach Us At remain usable after placeholder views.
- Verify labels, controls, messages, and panel geometry remain within the viewport with no overlap or horizontal overflow at 700 pixels or below.

### Test data setup
- No new API or database test data is required for User Management placeholders.
- Reuse existing deterministic cafe-story, menu, and bill data only for existing-view regression checks.
- Use the configured `CAFE_BASE_URL` or the existing local application default.
- Use network/request instrumentation only if supported by the existing Selenium harness; otherwise report that evidence as unavailable rather than asserting it from page behavior alone.

### Page/object model considerations
- Continue the existing direct-selector NUnit/Selenium style unless repeated User Management assertions justify a small local helper.
- Use `data-testid` selectors listed in the architecture document and semantic selectors for button/anchor assertions.
- Prefer explicit waits for observable panel content and state attributes; avoid sleeps and CSS-only visibility assumptions.
- Use JavaScript geometry checks for the narrow viewport and verify the footer remains outside `.story-panel` exactly once.

### Coverage classification
- Positive: Persistent visibility, exact labels/messages, expansion, keyboard access, and return to existing views.
- Negative: No anchor semantics, no URL change, no unsupported route, no form submission, no API call, no persistence, no duplicate shell/footer, and no unhandled client error caused by placeholders.
- Accessibility: Native button semantics, keyboard operation, observable expansion state, focus treatment, and accessible control labeling.
- Boundary: Initial collapsed state, repeated expand/collapse and placeholder transitions, four-anchor regression, 700-pixel-or-lower geometry, viewport bounds, and horizontal overflow.
- Conditional: Network-request inspection and any error-state/final-bill coverage require deterministic harness setup and must be reported as executed or unexecuted with evidence.

## 7. Integration and Verification Tasks
- Run `npm run build` from `src/CafeManagement` and confirm successful TypeScript compilation plus synchronized `wwwroot/main.js` output.
- Run `dotnet build CafeManagement.sln` to verify the host and test projects compile without backend changes.
- Start the application using the existing launch configuration or `dotnet run --project src/CafeManagement/CafeManagement.csproj --urls http://localhost:8080` for Selenium execution when that is the configured local URL.
- Run the focused UI suite after implementation, for example `dotnet test tests/CafeManagement.UiTests/CafeManagement.UiTests.csproj --filter "FullyQualifiedName~UserManagement"`, together with the existing navigation regression tests. Use the actual test names once added; do not claim a filter passed if no matching tests exist.
- Verify exact User Management labels, submenu state, keyboard behavior, placeholder messages, non-anchor semantics, no URL/API behavior, shell/footer persistence, four-anchor count, existing-view restoration, and narrow viewport geometry.
- Inspect `git diff --check` and the final working tree to confirm only the approved UI, generated TypeScript artifact, Selenium coverage, and this plan are involved; no C#, SQL, persistence, auth, or authorization files are changed.

## 8. Risks, Dependencies, and Open Questions

### Known risks
- Placing User Management or placeholder content inside the dynamic panel could make it disappear after view transitions; persistent navigation must remain in the static shell.
- Implementing submenu items as anchors could violate the four-anchor regression and create unsupported routes.
- Failing to synchronize `wwwroot/main.js` after editing `main.ts` could leave browser behavior stale even when the source is correct.
- Repeated listener registration could cause duplicate rendering or inconsistent expansion state after repeated navigation.
- CSS changes could introduce overlap or horizontal overflow at the existing narrow breakpoint.
- A request assertion is only meaningful when the Selenium harness can observe browser requests; page-level behavior alone does not prove network absence.

### External dependencies
- Existing HTML shell, TypeScript renderer conventions, generated JavaScript output, and CSS theme tokens.
- Node.js/npm, TypeScript dependencies, .NET 8 SDK, Chrome, ChromeDriver, and a running application.
- Existing NUnit/Selenium project and `CAFE_BASE_URL` environment configuration.

### Unresolved questions
- Whether future pages beyond the current existing views are included in the phrase “all application pages”; this plan covers all currently supported views in the repository.
- Whether a minimal local fallback is needed for unexpected placeholder-renderer failure; no general error-boundary design is added by this plan.
- Whether the existing Selenium harness provides network interception; no new network-testing framework is introduced without separate approval.

### Items needing clarification
- None block the approved implementation-plan artifact. Any unresolved implementation detail must remain within the confirmed presentation-only scope and must not introduce backend, database, authentication, authorization, or routing behavior.

## 9. Definition of Done
- User Management is persistently visible with the exact label on all current application views.
- The User Management button expands and collapses by click and native keyboard activation, with observable accessible state.
- The submenu exposes exactly Add User and Search User as non-anchor in-page controls.
- Add User and Search User render their exact approved future-development messages in `.story-panel` using stable selectors.
- Placeholder interactions do not navigate, change URL/history, submit data, call unsupported APIs, or persist user data.
- The existing theme, typography, shell, footer, and responsive behavior remain intact, including the narrow viewport layout.
- `src/main.ts` and generated `wwwroot/main.js` are synchronized through `npm run build`.
- Selenium coverage verifies visibility, expansion/collapse, keyboard access, exact messages, non-anchor semantics, no navigation/API calls where observable, shell/footer persistence, the existing four-anchor regression, existing-view restoration, and narrow-layout geometry.
- `npm run build`, `dotnet build CafeManagement.sln`, and applicable focused Selenium tests have executable evidence recorded factually.
- No C# backend, API, SQL, persistence, authentication, or authorization changes are made for US-007.
- The implementation diff is limited to the approved UI, generated browser artifact, tests, and related workflow evidence; source story and architecture documents remain unchanged.
