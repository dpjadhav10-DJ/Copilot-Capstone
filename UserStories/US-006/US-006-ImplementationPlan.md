# Implementation Plan: US-006

## 1. Source and Summary
- User story reference: US-006, Addition of Footer Message at the bottom of the Application Pages
- Source architecture document: `UserStories/US-006/US-006-SystemArchitecture.md`
- Plan objective: Add one persistent, centered, italic, non-clickable footer banner to the existing application shell without changing current application behavior.
- Solution scope summary: Update the static HTML shell and CSS, add stable Selenium coverage for footer content and presentation, and verify persistence across current dynamic views. No TypeScript, C#, SQL, API, or runtime dependency change is planned.

## 2. Implementation Strategy

### Delivery approach
Place a semantic `<footer>` as a sibling after the persistent `.content-grid` in `wwwroot/index.html`, style it with existing theme variables and the regular `DM Sans` font, then extend `HomePageTests.cs` with exact-content, semantic, presentation, cross-view, and responsive assertions.

### Sequencing rationale
1. Establish the persistent markup and stable selector.
2. Apply focused CSS for banner, typography, alignment, spacing, and responsive behavior.
3. Confirm dynamic renderers do not contain footer markup and therefore cannot duplicate it.
4. Add and run Selenium coverage for current views and layout.
5. Record print, future-page, and conditional final-bill limitations factually.

### Dependencies and prerequisites
- Existing HTML shell and CSS theme variables.
- Existing `DM Sans` font loading and responsive breakpoint.
- .NET 8 application running for Selenium.
- Chrome and compatible ChromeDriver.
- Existing menu and bill data only if final-bill footer coverage is executed.
- Deterministic error-state setup only if footer persistence during error states is claimed as executed evidence.

### Assumptions and constraints
- Document-flow placement is used; the footer is not fixed or sticky.
- The footer is static and does not call an API or access SQL.
- Existing dynamic renderers continue to replace only `.story-panel`.
- The current 700-pixel media-query breakpoint is the narrow-layout boundary.
- Print inclusion remains unresolved and is not implemented as a separate behavior.

## 3. Step-by-Step Implementation Tasks

### TASK-001: Add persistent footer markup
- Description: Add one semantic `<footer>` after `.content-grid` and before the end of `.page-shell`, with `data-testid="application-footer"` and the exact message `All the rights reserved for the cafe to the management.`.
- Primary layer impacted: HTML presentation layer.
- Dependencies: Existing page-shell structure.
- Expected outcome: One footer is present in the initial DOM and remains outside the replaceable story panel.
- Notes or risks: Use ordinary text content only; do not add an anchor, button, tabindex, or event handler.

### TASK-002: Style the footer banner
- Description: Add a focused `.application-footer` rule using existing theme variables, centered text alignment, italic `DM Sans` typography, banner spacing, and document-flow placement. Add narrow-viewport rules only where needed to prevent wrapping, clipping, or horizontal overflow.
- Primary layer impacted: CSS presentation layer.
- Dependencies: TASK-001.
- Expected outcome: The footer visually follows the existing theme and remains readable at desktop and 700-pixel-or-lower widths.
- Notes or risks: Do not introduce a new visual theme, fixed positioning, or decorative heading font.

### TASK-003: Confirm renderer boundary
- Description: Inspect all `storyPanel.innerHTML` assignments and ensure none include footer markup. Keep the footer in the persistent shell rather than duplicating it in Home, Menu, Calculate Bill, Contact, or final-bill renderer strings.
- Primary layer impacted: TypeScript integration boundary.
- Dependencies: TASK-001.
- Expected outcome: Dynamic view changes preserve exactly one footer without TypeScript behavior changes.
- Notes or risks: No source or generated JavaScript edit is expected unless inspection reveals a renderer that replaces the shell rather than the story panel.

### TASK-004: Add initial footer Selenium coverage
- Description: Extend `HomePageTests.cs` with assertions for the footer selector, exact text, semantic footer element, one-instance visibility, absence of interactive descendants, and non-focusability.
- Primary layer impacted: Selenium UI tests.
- Dependencies: TASK-001 and TASK-002.
- Expected outcome: Core Home footer requirements are directly testable with stable selectors.
- Notes or risks: Use DOM and computed-style assertions rather than brittle pixel-color assertions.

### TASK-005: Add presentation and responsive assertions
- Description: Assert italic computed style, regular font family, horizontal centering, footer placement at or after `.content-grid`, bounds within the viewport, and no horizontal overflow at the existing desktop and 700-pixel-or-lower mobile breakpoint.
- Primary layer impacted: Selenium UI tests.
- Dependencies: TASK-002 and TASK-004.
- Expected outcome: Layout and typography requirements have measurable automated checks.
- Notes or risks: Account for browser subpixel rounding when comparing center coordinates.

### TASK-006: Add cross-view persistence coverage
- Description: Navigate among Home, Menu, Calculate Bill, and Contact and assert the same single footer remains visible with the exact text after each transition. Cover loading/error states only when deterministic setup exists.
- Primary layer impacted: Selenium UI tests.
- Dependencies: TASK-003 through TASK-005.
- Expected outcome: Current client-rendered views cannot remove or duplicate the footer without test failure.
- Notes or risks: Final-bill coverage is conditional on existing deterministic bill setup and must be recorded as unexecuted otherwise.

### TASK-007: Execute verification
- Description: Run TypeScript/build checks as applicable, build the .NET solution, start the application, run focused footer and existing navigation tests, and inspect the working tree for unintended generated changes.
- Primary layer impacted: Integration and verification.
- Dependencies: TASK-001 through TASK-006 and environment prerequisites.
- Expected outcome: Source diagnostics, build results, Selenium evidence, and limitations are captured in the test summary.
- Notes or risks: Do not claim print, future-page, error-state, or final-bill coverage without corresponding setup and evidence.

## 4. C# Backend Tasks
- No controller or endpoint changes.
- No service or domain-logic changes.
- No validation or error-handling changes.
- No DTO or model changes.
- No logging, authentication, or authorization changes.
- Build the existing backend to confirm the static footer does not introduce a project regression.

## 5. SQL Database Tasks
- No schema, table, relationship, key, constraint, index, migration, seed, or data transformation changes.
- No transaction, concurrency, or audit work.
- Existing SQL data is needed only to support whichever dynamic-view Selenium setup is executed.

## 6. Selenium UI Testing Tasks

### Automated scenarios
- Verify exactly one `[data-testid='application-footer']` exists and is visible on Home.
- Verify the footer text equals `All the rights reserved for the cafe to the management.` exactly.
- Verify the element is a semantic `FOOTER` and has no anchor, button, input, select, textarea, or form descendants.
- Verify the footer is not keyboard-focusable and does not trigger navigation or state changes.
- Verify computed `font-style` is `italic` and computed `font-family` includes the regular `DM Sans` font.
- Verify text is horizontally centered within the footer banner.
- Verify footer placement is at or after the `.content-grid` bottom and does not overlap it.
- Verify footer bounds and text remain within the viewport with no horizontal overflow at the desktop and 700-pixel-or-lower layouts.
- Navigate through Home, Menu, Calculate Bill, and Contact and assert one unchanged footer remains after each transition.
- Execute final-bill and dynamic error-state footer checks only when deterministic setup exists.

### Test data setup
- No new test data is required for the static footer or Home, Menu, and Contact navigation checks.
- Reuse existing menu and bill data only for optional final-bill coverage.
- Use deterministic endpoint/error setup before claiming error-state coverage.

### Page/object model considerations
- Keep the current direct-selector test style; no page-object abstraction is needed for one persistent component.
- Use a small helper only if it removes repeated footer lookup and exact-text assertions.

### Coverage classification
- Positive: Exact message, visibility, styling, centering, and cross-view persistence.
- Negative: No interactive descendants, no focusability, and no duplicate footer.
- Boundary: Short/expanded views, 700-pixel breakpoint, viewport bounds, and horizontal overflow.
- Conditional: Final bill, error states, and print output only with confirmed setup and requirements.

## 7. Integration and Verification Tasks
- Run editor diagnostics for changed HTML, CSS, and Selenium files.
- Run the existing TypeScript build if the implementation leaves TypeScript unchanged only as a regression check; no generated JavaScript change is expected.
- Run `dotnet build CafeManagement.sln`.
- Start the application using the existing launch configuration.
- Run focused Selenium tests covering US-006 and existing navigation regression behavior.
- Verify footer presence after dynamic `.story-panel` replacement.
- Verify desktop and narrow viewport geometry.
- Record print behavior as unresolved unless explicitly tested under an approved requirement.
- Inspect `git diff --check` and ensure no unrelated files or generated `bin/obj` artifacts are included.

## 8. Risks, Dependencies, and Open Questions

### Known risks
- Placing the footer inside `.story-panel` would remove it during dynamic navigation; the implementation must keep it in the persistent shell.
- Adding footer markup to multiple renderer strings would create duplicates.
- Fixed positioning could cover bill controls or create mobile overlap; document flow is required by this plan.
- Exact centering can be affected by text wrapping and browser subpixel rounding at narrow widths.
- Font assertions may vary if the external font has not loaded before the test runs.

### External dependencies
- Existing page shell and CSS theme variables.
- Google Fonts availability or an equivalent loaded regular font for computed-style verification.
- Running .NET application, SQL availability for applicable views, Chrome, and ChromeDriver.

### Unresolved questions
- Whether the footer belongs in browser print output or final bill print output.
- Whether “bottom” requires viewport pinning on short pages rather than document-flow placement.
- Exact banner background, border, height, and minimum spacing beyond existing theme conventions.
- Whether future routes and all error pages are included in “all application pages.”

### Items needing clarification
- No clarification blocks the core static footer implementation.
- Print, future-page, final-bill, and deterministic error-state coverage must remain explicitly conditional until their prerequisites or requirements are confirmed.

## 9. Definition of Done
- One semantic footer exists in the persistent application shell with the exact required message.
- The footer is visible after Home and all currently supported dynamic view transitions.
- The message is centered, italic, and rendered in the regular application text font.
- The footer follows the existing theme, color scheme, spacing, and responsive design.
- The footer contains no interactive semantics, focus target, or click behavior.
- The footer is not duplicated in dynamic renderer strings.
- Desktop and 700-pixel-or-lower layout checks show no clipping, overflow, overlap, or content obstruction.
- Existing navigation and dynamic views remain functional.
- Focused Selenium tests pass for all executable scenarios.
- Print, error-state, final-bill, and future-page scenarios are reported as tested or unexecuted with evidence-based reasons.
- No C# backend or SQL database artifacts are changed.
- Build, test, and working-tree evidence is recorded factually before handoff.