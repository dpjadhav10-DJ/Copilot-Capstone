# Implementation Plan: US-005

## 1. Source and Summary
- User story reference: US-005, Implementation of UI Changes on Home Page
- Source architecture document: `UserStories/US-005/US-005-SystemArchitecture.md`
- Plan objective: Implement the approved navigation changes and Home restoration behavior with minimal changes to the existing static shell, TypeScript view rendering, and Selenium coverage.
- Solution scope summary: Add a persistent Home link, rename Contact Us to Reach Us At, remove Locate Us, restore the initial cafe-story panel from all current client-rendered views, preserve the existing visual design, regenerate browser JavaScript, and verify retained flows at desktop and narrow viewports.

## 2. Implementation Strategy

### Delivery approach
Implement the static navigation contract first, then add view-scoped Home rendering in TypeScript, compile the browser artifact, and update focused Selenium tests. Do not modify C# or SQL because the existing active cafe-story endpoint and data remain sufficient.

### Sequencing rationale
1. Establish stable DOM selectors and labels before wiring behavior.
2. Add Home rendering and refactor story loading around the final DOM contract.
3. Apply CSS changes only if the new anchor does not inherit a suitable existing side-panel style.
4. Compile TypeScript so the served JavaScript matches the source.
5. Update and execute Selenium coverage against the built application.

### Dependencies and prerequisites
- Node.js and project dependencies required by the existing `npm run build` script.
- .NET 8 SDK for solution build and application hosting.
- SQL Server and deterministic active cafe-story data for successful Home loading.
- Chrome and compatible ChromeDriver for Selenium execution.
- A running application exposed through `CAFE_BASE_URL`, or the existing default `http://localhost:8080`.
- A deterministic active-story failure setup is required before the Selenium failure scenario can be executed validly.

### Assumptions and constraints
- `src/CafeManagement/src/main.ts` remains the source of browser behavior and `src/CafeManagement/wwwroot/main.js` remains generated output.
- Internal navigation continues to use native anchors with click handlers; no router or History API behavior is added.
- The side navigation remains persistent outside `.story-panel`.
- Existing C# endpoints, services, models, and SQL objects are unchanged.
- Existing desktop behavior and the 700-pixel responsive breakpoint define the required layout checks.

## 3. Step-by-Step Implementation Tasks

### TASK-001: Update persistent navigation markup
- Description: Replace the `Explore` paragraph with a native `Home` anchor using `href="#home"` and `data-testid="nav-home"`; change the contact anchor's visible text to `Reach Us At` while retaining `href="#contact-us"` and `data-testid="nav-contact"`; remove the Locate Us anchor entirely.
- Primary layer impacted: HTML presentation layer.
- Dependencies: Existing side-panel markup and selectors.
- Expected outcome: The navigation exposes exactly Home, Calculate Bill, Add/Remove Cafe Menu, and Reach Us At, with no locate element or obsolete labels.
- Notes or risks: Preserve semantic anchor behavior and do not change retained link destinations or ordering beyond replacing the former label position.

### TASK-002: Add view-scoped Home rendering
- Description: Add a `homeLink` reference and a `showHome` function in `src/main.ts`. Render the initial section kicker, `story-heading`, story-content loading state, and hidden story-error state into `.story-panel`, then load the active cafe story into those newly rendered elements.
- Primary layer impacted: TypeScript presentation logic.
- Dependencies: TASK-001 and existing `.story-panel` ownership.
- Expected outcome: Selecting Home from any current client-rendered view restores the initial home structure and story-loading behavior.
- Notes or risks: Keep navigation handlers attached only once to persistent anchors. The Home renderer must not reset bill state or alter unrelated view logic unless existing navigation already does so.

### TASK-003: Refactor story rendering and loading to use view-scoped elements
- Description: Change `renderStory` and `loadStory` to receive the current story-content and story-error elements rather than using module-level references to replaceable panel children. Before applying an asynchronous success or failure result, verify that the supplied elements remain connected to the active story panel.
- Primary layer impacted: TypeScript asynchronous UI logic.
- Dependencies: TASK-002.
- Expected outcome: Initial Home loading and restored Home loading share one code path, while late responses from abandoned Home renders cannot overwrite a later view.
- Notes or risks: Capture or resolve the initial Home elements before the initial `loadStory` call. Preserve paragraph splitting and the existing controlled error text.

### TASK-004: Wire Home navigation
- Description: Attach one click handler to the persistent Home anchor, prevent the default fragment-only action, and invoke `showHome`.
- Primary layer impacted: TypeScript event handling.
- Dependencies: TASK-001 through TASK-003.
- Expected outcome: Home is keyboard- and mouse-operable from every view without duplicated handlers.
- Notes or risks: Browser Back/Forward routing and URL synchronization remain out of scope.

### TASK-005: Verify and minimally adjust styling
- Description: Inspect Home under the existing `.side-panel a` rules at desktop and narrow viewports. Reuse existing styles; add a dedicated class or narrowly scoped rule only if needed to preserve the former label's visual hierarchy without weakening hover or focus-visible behavior.
- Primary layer impacted: CSS presentation layer, only if required.
- Dependencies: TASK-001.
- Expected outcome: The new link and removed locate item cause no clipping, overlap, incoherent spacing, or theme regression.
- Notes or risks: Do not redesign the side panel or modify unrelated colors, typography, or layout.

### TASK-006: Compile TypeScript
- Description: Run `npm run build` from `src/CafeManagement` after TypeScript changes.
- Primary layer impacted: Generated browser JavaScript.
- Dependencies: TASK-002 through TASK-004.
- Expected outcome: `wwwroot/main.js` is regenerated from the validated TypeScript source with no compiler errors.
- Notes or risks: Do not hand-edit generated JavaScript.

### TASK-007: Update focused Selenium tests
- Description: Update `HomePageTests.cs` to assert explicit navigation identities and labels, removal of old labels and locate selector, preserved Reach Us At behavior, Home restoration from top-level views, repeated transitions, and narrow-viewport geometry.
- Primary layer impacted: Selenium UI tests.
- Dependencies: TASK-001 through TASK-006 and running application data.
- Expected outcome: Automated coverage directly traces to US-005 acceptance criteria without relying on link order alone.
- Notes or risks: Keep existing safe social-link assertions. Use `FindElements` for absence checks and explicit waits for asynchronously loaded Home content.

### TASK-008: Add nested-view Home checks where deterministic
- Description: Exercise Home from the Add Menu form and final bill when existing deterministic setup can reach those states. If final-bill setup is unavailable, record that scenario as unexecuted instead of weakening or fabricating evidence.
- Primary layer impacted: Selenium UI tests and test setup.
- Dependencies: TASK-007, deterministic menu/bill data, and existing bill flow availability.
- Expected outcome: Evidence covers the requirement that Home works from nested application states where the environment can set them up reliably.
- Notes or risks: Do not add US-005 database seed changes or couple navigation tests to unstable data.

### TASK-009: Build and execute verification
- Description: Build the .NET solution, start the application using the established local configuration, run focused HomePageTests, and inspect failures against US-005 scope.
- Primary layer impacted: Integration and verification.
- Dependencies: TASK-001 through TASK-008 and environment prerequisites.
- Expected outcome: TypeScript compilation, .NET build, and executable Selenium evidence are recorded factually.
- Notes or risks: Report unavailable SQL Server, ChromeDriver, or deterministic failure setup as limitations; do not claim affected scenarios passed.

## 4. C# Backend Tasks
- No controller or endpoint changes.
- No service or domain-logic changes.
- No validation or error-handling changes.
- No DTO or model changes.
- No logging, authentication, or authorization changes.
- Build the existing C# solution to confirm the frontend and test updates introduce no project-level regression.

## 5. SQL Database Tasks
- No schema, table, relationship, key, constraint, or index changes.
- No migration or seed script changes.
- No data transformation, transaction, concurrency, or audit changes.
- Use existing active cafe-story data for successful Selenium execution.

## 6. Selenium UI Testing Tasks

### Automated scenarios
- Confirm Home is a native anchor with `data-testid="nav-home"`, visible text `Home`, and the approved internal target.
- Confirm Reach Us At retains `data-testid="nav-contact"`, the existing `#contact-us` target, and existing contact content and social-link behavior.
- Confirm `Explore`, `Contact Us`, and `data-testid="nav-locate"` are absent from the navigation.
- Confirm the four retained navigation identities explicitly rather than relying only on total count or DOM order.
- Open Calculate Bill, Add/Remove Cafe Menu, and Reach Us At; select Home from each and await the initial story heading and loaded content.
- Navigate repeatedly among retained views and confirm controls remain responsive and content is not duplicated.
- Open the Add Menu nested form and return Home.
- Open final bill and return Home only when deterministic existing menu and bill setup is available.
- At a viewport width of 700 pixels or below, confirm retained links are displayed, remain inside the viewport, have unclipped text, and do not overlap the story panel.
- Execute the controlled story-load failure scenario only when deterministic endpoint-failure setup exists; otherwise record it as unexecuted.

### Test data setup
- Reuse the existing active cafe story for successful loading assertions.
- Reuse existing menu and bill fixtures only for nested-view setup.
- Do not create US-005-specific SQL data or mutate production-like data for navigation tests.

### Page/object model considerations
- The current suite is small and directly uses stable `data-testid` selectors; do not introduce a page-object abstraction solely for US-005.
- Extract small private navigation or wait helpers only if they remove repeated setup across the new scenarios.

### Coverage classification
- Positive: Correct labels, destinations, content, Home restoration, and retained flows.
- Negative: Removed selectors and obsolete labels are absent; controlled story error when setup exists.
- Boundary: Repeated transitions, late asynchronous responses, and the existing 700-pixel responsive breakpoint.

## 7. Integration and Verification Tasks
- Run `npm run build` in `src/CafeManagement` and require a successful TypeScript compilation.
- Run `dotnet build CafeManagement.sln` and distinguish pre-existing failures from US-005 regressions.
- Start the application with its existing configuration and verify `/api/cafe-story/active` returns deterministic story content.
- Run focused `HomePageTests` through `dotnet test` with `CAFE_BASE_URL` set to the running instance when needed.
- Verify Reach Us At still renders contact details and safe external social links.
- Verify Home restoration after each supported top-level and deterministic nested view.
- Verify desktop `1440x900` and narrow 700-pixel-or-lower navigation geometry.
- Review generated `wwwroot/main.js` only as build output; source correctness is reviewed in `src/main.ts`.
- Record the story-failure and final-bill scenarios as unexecuted when deterministic prerequisites are unavailable.

## 8. Risks, Dependencies, and Open Questions

### Known risks
- Module-level references to initial story elements would become stale after another renderer replaces the panel.
- An asynchronous story response could target an abandoned Home render unless connection/current-view checks are enforced.
- Duplicating the Home markup can drift from the initial `index.html` structure; implementation and tests must keep IDs and loading/error content aligned.
- Existing tests that assert only navigation count could pass despite an incorrect set of links.
- Final-bill setup may make a navigation test dependent on menu data and unrelated bill behavior.

### External dependencies
- Node.js and TypeScript compiler dependencies.
- .NET 8 SDK and application runtime configuration.
- SQL Server with active cafe-story data.
- Chrome and compatible ChromeDriver.
- Deterministic endpoint-failure setup for the negative story-loading scenario.

### Unresolved questions
- Browser history, URL synchronization, and deep-link restoration remain future-scope decisions.
- Story caching versus requesting on every Home selection remains a future optimization.
- Cross-browser coverage beyond existing headless Chrome is not required by US-005.

### Items needing clarification
- No clarification blocks the core implementation.
- The controlled story-failure Selenium scenario cannot be claimed complete until a deterministic failure setup is available.
- Final-bill Home coverage remains conditional on deterministic existing test data; core Home behavior is still covered through other panel-replacing views.

## 9. Definition of Done
- `Explore` is replaced by an operable Home link with a stable selector.
- `Contact Us` is renamed to `Reach Us At` without changing its destination, contact content, or social links.
- Locate Us is absent from visible, focusable, and accessible navigation.
- Home restores the initial story-panel structure and existing success/error behavior from all tested client-rendered views.
- Story loading uses view-scoped elements and does not overwrite a later view with a stale asynchronous result.
- Existing Calculate Bill, Add/Remove Cafe Menu, Reach Us At, and Home flows remain operable after repeated navigation.
- Desktop and mandatory narrow-viewport checks show no clipping, overlap, or incoherent spacing caused by the navigation changes.
- `npm run build` completes and generated `wwwroot/main.js` matches the TypeScript source.
- `dotnet build CafeManagement.sln` completes without a US-005 regression.
- Focused Selenium tests pass for all executable scenarios.
- Any scenario blocked by unavailable deterministic setup is reported as unexecuted with the dependency identified.
- No C# backend or SQL database artifacts are changed.
- Implementation and review evidence remain traceable to the reviewed architecture and US-005 acceptance criteria.