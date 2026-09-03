# Implementation Plan: US-003

## 1. Source and Summary
- User story reference: US-003, Implementation of Contact Us Page
- Source architecture document: `UserStories/US-003/US-003-SystemArchitecture.md`
- Plan objective: Replace the placeholder Contact Us link with an in-application Contact Us view that renders the supplied cafe details in the existing right content section.
- Solution scope summary: Make the smallest frontend-only change needed for navigation, static contact content, secure social links, responsive presentation, and Selenium verification.

## 2. Implementation Strategy

### Delivery approach
Update the home-page anchor and TypeScript view state first, add only the CSS needed to keep the contact content aligned with the existing theme, update the emitted JavaScript through the project TypeScript build, then add focused Selenium coverage and run regression checks.

### Sequencing rationale
The HTML anchor and TypeScript handler establish the integration contract. The rendered semantic markup then defines the selectors and accessibility surface that styling and Selenium tests can consume. Build output verification ensures the application serves the changed source in its checked-in static bundle.

### Dependencies and prerequisites
- Existing .NET 8 application builds and serves the home page.
- Node.js/npm and the existing TypeScript toolchain are available for `npm run build`.
- Chrome and Selenium WebDriver are available for UI tests.
- No SQL Server or new backend endpoint is required for this story.

### Assumptions and constraints
- Contact details are static and remain exactly as supplied by US-003.
- The existing `storyPanel` remains the content-region owner.
- The existing `data-testid="nav-contact"` selector is preserved.
- Phone numbers remain visible text rather than `tel:` links unless separately approved.
- No new icon package is added; use recognizable text-based logo marks with accessible link names.

## 3. Step-by-Step Implementation Tasks

### T-001: Update Contact Us navigation anchor
- Primary layer: HTML
- Dependencies: None
- Expected outcome: The Contact Us anchor points to `#contact-us` while retaining its current visible label and `data-testid`.
- Files: `src/CafeManagement/wwwroot/index.html`
- Risks: A full external navigation must not remain, and existing navigation count/labels must be unchanged.

### T-002: Add the Contact Us view renderer
- Primary layer: TypeScript/UI
- Dependencies: T-001
- Expected outcome: Add `showContactUs()` that replaces only the right content panel with semantic title, reach-us, and connect-us sections.
- Files: `src/CafeManagement/src/main.ts`
- Details: Render exact address, phone numbers, Facebook URL, and Instagram URL. Add stable test ids and accessible names. Use `target="_blank"` and `rel="noopener noreferrer"` for both social links.
- Risks: Avoid user-controlled interpolation and avoid changing bill/menu/story state.

### T-003: Wire Contact Us click behavior
- Primary layer: TypeScript/UI
- Dependencies: T-001, T-002
- Expected outcome: Selecting Contact Us prevents full-page navigation and calls the new renderer, matching the existing menu and bill navigation pattern.
- Files: `src/CafeManagement/src/main.ts`
- Risks: Preserve the existing story, menu, bill, and location links and keep the page shell mounted.

### T-004: Align contact content with the existing theme
- Primary layer: CSS/UI
- Dependencies: T-002
- Expected outcome: Contact sections use existing typography, colors, spacing, focus styles, and responsive flow without overlap.
- Files: `src/CafeManagement/wwwroot/styles.css`
- Details: Prefer existing `.story-panel`, heading, kicker, and link conventions; add narrowly scoped rules only where existing styles do not provide adequate spacing/layout.
- Risks: Do not introduce a separate visual theme or break mobile layout.

### T-005: Build the frontend bundle
- Primary layer: TypeScript build
- Dependencies: T-002, T-003, T-004
- Expected outcome: `src/CafeManagement/wwwroot/main.js` reflects the TypeScript changes and the TypeScript compiler reports no errors.
- Command: Run `npm run build` from `src/CafeManagement`.
- Risks: Keep generated output consistent with the repository’s existing checked-in bundle workflow.

### T-006: Add Contact Us Selenium coverage
- Primary layer: Selenium UI tests
- Dependencies: T-005
- Expected outcome: Tests verify navigation, exact content, social-link attributes, repeated navigation, and responsive/accessibility basics without requiring external network access.
- Files: `tests/CafeManagement.UiTests/HomePageTests.cs` or a focused new test file if local conventions favor separation.
- Details: Assert `Find us At`, both section titles, exact supplied values, exact hrefs, `_blank`, `noopener noreferrer`, accessible names, and preserved home shell. Use local DOM attributes rather than opening external sites.
- Risks: Tests must not become dependent on Facebook/Instagram availability; isolate any new-tab handle when testing browser-context behavior.

### T-007: Execute focused and regression verification
- Primary layer: Integration/QA
- Dependencies: T-005, T-006
- Expected outcome: TypeScript build, .NET build, focused UI tests, and existing home-page regression tests execute with results recorded factually.
- Details: Verify Contact Us repeatedly after navigating away, confirm story content still loads on a fresh home page, and check desktop/mobile layout where the test environment permits.
- Risks: If application startup, Chrome, or external dependencies are unavailable, record the blocked checks rather than claiming success.

## 4. File and Component Impact

### Files expected to change
- `src/CafeManagement/wwwroot/index.html`: Replace the placeholder Contact Us href.
- `src/CafeManagement/src/main.ts`: Add static Contact Us rendering and navigation handler.
- `src/CafeManagement/wwwroot/main.js`: Generated TypeScript output.
- `src/CafeManagement/wwwroot/styles.css`: Only if contact-specific layout/focus rules are necessary.
- `tests/CafeManagement.UiTests/HomePageTests.cs` or a focused Contact Us test file: Add browser integration assertions.

### Files not expected to change
- C# endpoints, models, services, and project dependencies.
- SQL scripts and database schema.
- Existing story, menu, and bill implementation except for shared navigation coexistence.

## 5. Test Plan

### Positive cases
- Home page contains four navigation links and Contact Us stays inside the application.
- Contact Us displays `Find us At`, `Reach us at:`, and `Connect us at:`.
- Exact address, both phone numbers, Facebook URL, and Instagram URL are present.
- Social links have accessible names, exact destinations, `_blank` targets, and safe rel attributes.
- Contact Us can be reopened after another supported view.

### Negative and corner cases
- Social-link verification does not require external network availability.
- Missing or changed content-region behavior should fail through stable title/content selectors rather than silently passing.
- Long contact text remains readable and does not overlap at mobile viewport width.
- Fresh home-page load still retrieves and displays the cafe story.

### Build and integration checks
- Run `npm run build` in `src/CafeManagement`.
- Run `dotnet build CafeManagement.sln` from the solution root.
- Start the application using the existing configuration and run focused Selenium tests.
- Run the complete UI test project when the focused checks pass.

## 6. Risks, Dependencies, and Rollback

### Risks
- Checked-in generated JavaScript can become stale if the TypeScript build is skipped.
- A selector or markup change could break existing Selenium assumptions.
- Using logo-only links without accessible labels would fail accessibility and testability requirements.
- New-tab behavior may be incorrectly tested through external network navigation.

### Mitigations
- Treat `npm run build` as a required implementation step and inspect its result.
- Preserve existing navigation test ids and page shell.
- Use semantic links with explicit accessible names and stable test ids.
- Assert local link attributes and browser target behavior only.

### Rollback boundary
Revert only the US-003 changes in the listed frontend/test files and restore the original placeholder anchor if implementation verification fails. Do not alter existing database or backend artifacts because they are outside this story.

## 7. Definition of Done

- Contact Us opens in the existing right content section from the home page.
- Required title, section headings, exact contact values, and social links are rendered.
- Social links open in a new tab with safe opener isolation.
- Existing application theme and responsive behavior are preserved.
- TypeScript output is rebuilt successfully.
- Selenium verifies integration, content accuracy, link configuration, repeat navigation, and relevant responsive/accessibility behavior.
- Existing home-page story behavior and other navigation workflows remain functional.
- No backend, SQL, or unrelated files are changed.