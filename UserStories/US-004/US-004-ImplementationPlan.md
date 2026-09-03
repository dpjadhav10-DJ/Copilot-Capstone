# Implementation Plan: US-004

## 1. Source and Summary
- User story reference: US-004, Implementation of Calculate Bill Module
- Source requirement analysis: `UserStories/US-004/US-004-RequirementAnalysis.md`
- Source architecture document: `UserStories/US-004/US-004-SystemArchitecture.md`
- Plan objective: Deliver a database-priced, transient Calculate Bill workflow integrated into the existing ASP.NET Core and TypeScript application, including final bill presentation, PDF download, and Selenium verification.
- Solution scope summary: Reuse existing `dbo.MenuItem` data and right-content-region navigation; add server-authoritative calculation APIs, client-owned bill state, confirmation flows, final bill rendering, approved PDF generation, and focused integration tests.

## 2. Implementation Strategy

### Delivery approach
Implement from the trusted pricing boundary outward: confirm product decisions, add C# contracts and service logic, expose minimal API endpoints, integrate the Calculate Bill and Final Bill views into the existing TypeScript content region, add focused styling and PDF support, then add and execute Selenium coverage.

### Sequencing rationale
The calculation contract must be stable before the UI can safely display or mutate bill lines. The client owns only transient line state and never supplies authoritative prices or amounts. PDF work is isolated behind an adapter so the selected library or browser-compatible mechanism can change without changing bill state or calculation rules.

### Dependencies and prerequisites
- Existing .NET 8 application and TypeScript client build successfully.
- SQL Server and configured `CafeDatabase` connection are available for API/UI verification.
- Existing `dbo.MenuItem` schema and current home-page content-region pattern remain available.
- Chrome/Selenium Manager or ChromeDriver is available for UI tests.
- Product/design decisions are recorded for quantity range, duplicate rows, Half/Full eligibility, empty-bill generation, price refresh, currency, and PDF behavior.

### Decisions to resolve before implementation
- Use the current SQL seed value of 15 Rs for Regular Coffee Half in implementation and tests. Do not change seed data silently.
- Use the architecture default quantity range of 1 through 10 unless the product owner changes it.
- Keep duplicate additions as separate rows.
- Restrict bill options to menu rows with Half or Full portions; do not expose NA rows.
- Disallow Generate Bill when the estimated bill is empty.
- Use browser print-to-PDF behavior through `window.print()`; do not add a PDF package or promise a fixed download filename.
- Read the current price when adding or quantity-editing a line, retain that price through generation, and make only quantity editable in the bill table.

## 3. Step-by-Step Implementation Tasks

### T-001: Confirm contracts and product decisions
- Primary layer: Cross-cutting
- Dependencies: None
- Expected outcome: Record the decisions listed above, including the seed/example price reconciliation and PDF strategy.
- Notes or risks: Do not invent a 20 Rs database price or claim PDF completion without a selected implementation and download/content check.

### T-002: Define bill models and result types
- Primary layer: C# backend
- Dependencies: T-001
- Expected outcome: Add request and response records for bill options, calculation requests, and calculated bill lines using `decimal` for prices and amounts.
- Notes or risks: Keep client line identifiers local-only; do not expose a fake persistence identity.

### T-003: Add menu-option and price lookup operations
- Primary layer: C# data access/service
- Dependencies: T-001, existing `MenuService`
- Expected outcome: Retrieve Half/Full menu options deterministically and retrieve a specific `MenuItemId` plus matching portion through parameterized SQL.
- Notes or risks: Return not-found for unknown IDs or mismatched portions; filter out existing NA rows.

### T-004: Implement BillService validation and calculation
- Primary layer: C# service/domain
- Dependencies: T-002, T-003
- Expected outcome: Validate item identity, Half/Full portion, quantity 1-10, resolve the trusted database price, and calculate `Price * Quantity` with `decimal` arithmetic.
- Notes or risks: Never accept client-supplied price, amount, item name, or total as authoritative input. Return typed validation/not-found outcomes.

### T-005: Expose bill API endpoints
- Primary layer: ASP.NET Core
- Dependencies: T-004
- Expected outcome: Add `GET /api/menu/bill-options` and `POST /api/bill/calculate` near existing menu endpoints with controlled status mapping, cancellation propagation, logging, and generic 503 errors.
- Notes or risks: Keep endpoint lambdas thin; follow existing `Results.ValidationProblem` behavior and do not leak SQL details.

### T-006: Add unit/API validation coverage
- Primary layer: C# tests or focused API verification
- Dependencies: T-004, T-005
- Expected outcome: Verify valid Half/Full calculations, quantity boundaries, unsupported portions, unknown IDs, mismatched portions, malformed requests, and database failures where a test seam exists.
- Notes or risks: Use the configured test approach; do not add a new test framework without need.

### T-007: Integrate Calculate Bill navigation and initial view
- Primary layer: TypeScript/UI
- Dependencies: T-005; existing `index.html` and `main.ts`
- Expected outcome: The existing Calculate Bill link stays inside the application and renders `Generating Bill`, Select Item, Estimated Bill, default Half, quantity dropdown, empty table state, and total zero in the right section.
- Notes or risks: Preserve story and menu navigation behavior and existing page shell selectors.

### T-008: Implement menu selection and add flow
- Primary layer: TypeScript/UI
- Dependencies: T-007, T-005
- Expected outcome: Load bill options, handle loading/empty/error states, submit item/portion/quantity, append a local bill line on success, and derive the total from current lines.
- Notes or risks: Preserve existing valid lines after a failed add; disable the action while pending; never post price or amount as trusted input.

### T-009: Implement estimated bill table and mutations
- Primary layer: TypeScript/UI
- Dependencies: T-008
- Expected outcome: Render Item, Portion, Quantity, Price, Amount, Edit, and Remove columns; support pencil quantity editing, cross removal, empty state, and total recalculation.
- Notes or risks: Use stable local line IDs and accessible names. Keep duplicate additions as separate rows. Avoid floating-point delta updates by using integer minor units or a documented rounding strategy for display totals.

### T-010: Implement confirmation and final bill workflow
- Primary layer: TypeScript/UI
- Dependencies: T-009, T-001
- Expected outcome: Generate Bill confirms before switching to a final view; cancellation preserves lines; confirmation shows cafe name, final table, total, Print, and Generate New Bill. Discard Bill confirms before clearing; cancellation preserves lines.
- Notes or risks: Disable Generate Bill for an empty bill. Ensure Generate New Bill creates a fresh empty state rather than reusing stale line data.

### T-011: Implement PDF adapter and download
- Primary layer: PDF integration
- Dependencies: T-010, T-001
- Expected outcome: Use the approved PDF mechanism to download a file containing cafe name, bill rows, amounts, and total without mutating bill state.
- Notes or risks: Verify print-ready content in the supported browser; Save as PDF filename and dialog behavior remain browser-controlled.

### T-012: Add focused styling and accessibility selectors
- Primary layer: CSS/UI
- Dependencies: T-007 through T-011
- Expected outcome: Match existing theme/color variables, maintain semantic labels and table markup, expose stable IDs/data attributes, provide accessible icon controls, and keep the table usable on narrow viewports.
- Notes or risks: Avoid overlapping columns and icon-only controls without accessible names. Do not refactor unrelated styles.

### T-013: Add Selenium fixtures and workflow tests
- Primary layer: Selenium tests
- Dependencies: T-006, T-007 through T-012
- Expected outcome: Cover navigation, initial state, menu options, add/calculation, multiple lines, edit, remove, confirmations, final view, new bill, discard, Print download, invalid inputs, failures, accessibility, and responsive layout.
- Notes or risks: The Regular Coffee price assertion must use the current seeded 15 Rs value. Keep test data deterministic and independent of menu-management mutation order.

### T-014: Execute functional and regression verification
- Primary layer: Integration/QA
- Dependencies: All prior tasks
- Expected outcome: Run TypeScript build, .NET build/tests, API checks, focused Selenium tests, full UI tests, and existing home-page regression checks; record evidence and skipped/blocked checks.
- Notes or risks: Do not claim PDF or Selenium success if required infrastructure is unavailable.

## 4. C# Backend Tasks

- Add `BillMenuOption`, `CalculateBillRequest`, and `CalculatedBillLine` records under the existing model conventions.
- Add a deterministic Half/Full menu-options query and identifier-plus-portion lookup using parameterized `SqlCommand` operations.
- Implement `BillService` with quantity bounds, portion validation, trusted price resolution, and decimal-safe amount calculation.
- Register the service using a lifetime compatible with per-operation SQL connections and existing project patterns.
- Add minimal API endpoints for bill options and single-line calculation.
- Map validation, not-found, database, and unexpected outcomes to stable safe responses.
- Log failures without exposing connection strings, SQL text with secrets, or unnecessary user-entered values.

## 5. SQL Database Tasks

- Make no schema change for the approved transient bill design.
- Reuse the existing `dbo.MenuItem` primary key, item name, portion, and decimal price.
- Confirm whether the current seed price of 15 Rs or the story example of 20 Rs is authoritative before test fixtures are finalized.
- Preserve the existing `NA` rows for menu management while excluding them from bill options.
- Verify the existing database setup supports deterministic lookup and that no bill data is persisted.
- If persistence becomes required, stop and submit a revised architecture/plan for bill and bill-line tables rather than extending this transient implementation implicitly.

## 6. TypeScript/UI Tasks

- Attach the Calculate Bill navigation handler and keep it within the existing right content region.
- Load `/api/menu/bill-options` and render item/portion choices without trusting client price values.
- Render Half/Full radios with Half default and quantity options 1-10.
- Maintain one bill state owner for lines, total, view, loading, and errors.
- Add lines through `/api/bill/calculate`, use returned trusted prices/amounts, and preserve valid state after failures.
- Render semantic table markup and accessible pencil/cross buttons with stable test selectors.
- Implement quantity editing without allowing invalid values or stale rows to corrupt totals.
- Remove only the local transient line and recompute total from remaining lines.
- Implement accessible Generate/Discard confirmation, cancellation state preservation, final bill, Generate New Bill reset, and Print.
- Add the PDF adapter only after the dependency and output format are approved.
- Extend existing styles minimally for forms, tables, states, actions, final bill, focus treatment, and narrow viewport behavior.

## 7. Selenium UI Testing Tasks

- Navigate from the home page through the `Calculate Bill` link and verify the right content section.
- Verify title, section headings, empty estimated table, zero total, Half default, item dropdown, and quantity choices.
- Verify options are sourced from database menu rows and NA rows are not selectable.
- Verify valid add and the approved Regular Coffee price/quantity example.
- Verify multiple-line totals and separate duplicate rows.
- Verify edit quantity, cancel edit, remove line, and empty-table total reset.
- Verify Generate Bill cancellation preserves state and confirmation opens the final bill.
- Verify final bill cafe name, rows, total, Print action, and Generate New Bill reset.
- Verify the final bill is print-ready and `window.print()` is invoked; browser Save as PDF behavior is environment-dependent and has no fixed filename contract.
- Verify Discard Bill cancellation preserves state and confirmation clears all lines.
- Verify invalid item/portion/quantity, unknown item, unavailable menu/calculation endpoint, rapid repeat action, and empty-bill generation handling.
- Verify accessible names for icon actions and usable desktop/mobile layout.
- Keep fixtures isolated and ensure tests do not permanently consume shared menu seed data.

## 8. Integration and Verification Tasks

- Run `npm run build` in `src/CafeManagement` before and after TypeScript changes.
- Run `dotnet build CafeManagement.sln` after backend and UI bundle changes.
- Verify bill-options and calculation endpoints with valid, invalid, boundary, unknown, mismatched, and failure inputs.
- Start the application against the configured SQL Server and exercise the complete browser workflow.
- Run focused US-004 Selenium tests, then the full UI test project.
- Re-run the existing home-page story retrieval and menu-navigation tests to detect regressions.
- Verify generated PDF output and confirm Print does not alter transient bill state.
- Record the seed/example price decision, PDF dependency, executed commands, results, and any unavailable infrastructure in the test summary.

## 9. Risks, Dependencies, and Open Questions

### Known risks
- The source example’s 20 Rs conflicts with the current Regular Coffee Half seed price of 15 Rs; the confirmed implementation value is 15 Rs.
- Browser print-to-PDF depends on browser behavior and cannot guarantee a direct download filename.
- A transient bill disappears on refresh and is not shared across browser tabs.
- Existing `NA` menu rows require explicit filtering from bill options.
- Client-side line totals can drift if binary floating-point arithmetic is used carelessly.
- Without authentication/authorization, the calculation workflow is public; this is outside current story scope but should remain visible.

### External dependencies
- SQL Server availability and valid `CafeDatabase` connection string.
- Node/npm and TypeScript compiler for rebuilding `main.js`.
- .NET 8 SDK.
- Chrome and Selenium WebDriver/driver availability.
- Approved PDF package or browser-compatible download mechanism.

### Unresolved questions
- The current 15 Rs seed value is authoritative for this implementation.
- Prices are retained from add/edit calculation through generation; only quantity can be edited.
- What exact currency symbol, decimal format, PDF filename, and PDF content layout are required?
- Should browser refresh preserve transient bill state?
- Are separate duplicate rows definitely preferred over merging?

## 10. Definition of Done

- Approved decisions are recorded and no implementation assumption contradicts the story or architecture.
- Users can navigate from home to Calculate Bill in the right content section.
- Menu options and portion pricing come from trusted SQL Server data through the backend.
- Valid add, edit, remove, total, empty, loading, and failure behavior works.
- Generate Bill and Discard Bill confirmation cancellation preserves state; confirmation performs the requested transition.
- Final bill includes cafe name, all lines, total, Print, and Generate New Bill.
- Print downloads the approved PDF format and does not mutate bill state.
- Existing home-page story and menu behavior remain functional and visually consistent.
- Selenium covers positive, negative, boundary, failure, integration, accessibility, and responsive cases.
- TypeScript build, .NET build, focused tests, full UI tests, and PDF evidence are executed and reported factually.
- Code review confirms security of price handling, maintainability, data integrity, testability, and no unrelated changes.
