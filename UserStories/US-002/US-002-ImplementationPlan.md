# Implementation Plan: US-002

## 1. Source and Summary
- User story reference: US-002, Implementation of Add or Remove Cafe Menu Module
- Source architecture document: `UserStories/US-002/US-002-SystemArchitecture.md`
- Plan objective: Deliver the database-backed Add/Remove Cafe Menu workflow through the existing ASP.NET Core, TypeScript, SQL Server, and Selenium solution.
- Solution scope summary: Integrate a paginated menu view into the home page right content area, support validated add/cancel and single or multiple removal, seed the required menu, and verify the complete workflow.

## 2. Implementation Strategy

### Delivery approach
Implement from persistence upward: establish the idempotent schema and seed data, add C# contracts/services/endpoints, integrate the TypeScript view and navigation, then add and execute Selenium coverage. Keep mutations server-validated and database-backed.

### Sequencing rationale
The database contract controls the domain shape and seed behavior; backend contracts provide a stable integration boundary for the UI; UI work then enables browser validation; final verification confirms persistence, navigation, and regression behavior together.

### Dependencies and prerequisites
- Existing .NET 8 application and TypeScript client build successfully.
- SQL Server connection/configuration is available.
- Existing home-page content-region and navigation patterns are understood and preserved.
- Chrome/Selenium Manager or ChromeDriver is available for UI tests.

### Assumptions and constraints
- Reuse existing project conventions and avoid changing US-001 story behavior.
- Page size is fixed at 10 for the user-facing workflow unless the existing API convention requires a bounded parameter.
- New rows are newest-first with deterministic ordering.
- Authorization, price precision/currency, duplicate policy, NA creation behavior, deletion policy, and exact endpoint contracts must be resolved before implementation if they change schema or public behavior.

## 3. Step-by-Step Implementation Tasks

### T-001: Confirm solution conventions and policy gates
- Primary layer: Cross-cutting
- Dependencies: None
- Expected outcome: Confirm existing endpoint/data-access/client patterns and record decisions for authorization, price type/scale, duplicate handling, NA creation, deletion mode, error messages, and stale selections.
- Notes or risks: Do not begin schema or public contract work while a required decision remains ambiguous.

### T-002: Add the menu database schema
- Primary layer: SQL
- Dependencies: T-001; existing US-001 database setup
- Expected outcome: MenuItem table with primary key, required name/portion/price/creation fields, non-negative price integrity, and indexes for deterministic newest-first paging and lookup.
- Notes or risks: Preserve existing objects and use non-destructive upgrade logic.

### T-003: Add duplicate-safe default seed data
- Primary layer: SQL
- Dependencies: T-002
- Expected outcome: The exact 10 source rows are available after setup and rerunning setup does not duplicate them.
- Notes or risks: Preserve `Veg Club Sandwitch` and `NA` exactly unless approved corrections are provided. Seed identification must not depend only on mutable display values.

### T-004: Define backend menu contracts and models
- Primary layer: C# backend
- Dependencies: T-001, T-002
- Expected outcome: Request/response models for paginated reads, creation, and selected-ID removal, including safe validation-error representation.
- Notes or risks: Keep identifiers and timestamps server-controlled.

### T-005: Implement repository/data access
- Primary layer: C# data access
- Dependencies: T-002, T-004
- Expected outcome: Parameterized deterministic page query, insert operation, and identifier-based bulk delete/status operation using the existing data-access convention.
- Notes or risks: Handle empty results, stale IDs, transaction boundaries, and active/status filtering if soft deletion is selected.

### T-006: Implement service validation and mutation logic
- Primary layer: C# service/domain
- Dependencies: T-004, T-005, T-001
- Expected outcome: Server-authoritative name, portion, price, and identifier validation; newest-first creation; atomic selected-item removal; controlled operation results.
- Notes or risks: Do not use item name as a deletion key. Enforce authorization on mutation operations if required.

### T-007: Expose menu endpoints and error handling
- Primary layer: ASP.NET Core
- Dependencies: T-006
- Expected outcome: Existing application style exposes paginated GET, create POST, and selected-item removal operation with consistent status codes, safe errors, and logging.
- Notes or risks: Reject unauthorized requests at the backend when policy requires it; do not expose raw exceptions.

### T-008: Integrate menu navigation and view state
- Primary layer: TypeScript/UI
- Dependencies: T-007; existing home-page structure
- Expected outcome: Home-page `Add/Remove Cafe Menu` navigation switches the right content area to the menu view and supports return to the existing story view according to current conventions.
- Notes or risks: Avoid regressions in US-001 content retrieval and navigation.

### T-009: Implement menu table and pagination controls
- Primary layer: TypeScript/UI
- Dependencies: T-008, T-007
- Expected outcome: Semantic table with required columns, 10 rows per page, next/previous controls, loading/empty/error states, and deterministic row/checkbox selectors.
- Notes or risks: Clear or reconcile selection when page or data changes; disable controls at pagination boundaries, including after deletion shrinks the last page.

### T-010: Implement add form and client feedback
- Primary layer: TypeScript/UI
- Dependencies: T-008, T-007
- Expected outcome: Add Menu form with item name, Half/Full radio options, Half default, price, Save, Cancel, client validation, server-error rendering, and no mutation on cancel.
- Notes or risks: Client checks supplement but do not replace backend validation. Preserve source-supported behavior for NA rows.

### T-011: Implement selection and removal workflow
- Primary layer: TypeScript/UI
- Dependencies: T-009, T-007
- Expected outcome: Remove Menu is disabled with no selection, enabled with one or more current-row selections, submits persistent IDs, clears selection, and refreshes the valid page after success.
- Notes or risks: Prevent stale selections from causing unintended deletion; add confirmation only if approved.

### T-012: Add Selenium fixtures and selectors
- Primary layer: Selenium tests
- Dependencies: T-003, T-008, T-009, T-010, T-011
- Expected outcome: Isolated setup/cleanup supports stable seeded display and mutation scenarios; selectors use accessible names and persistent IDs.
- Notes or risks: Tests must restore/recreate removed data and must not permanently consume shared seed rows.

### T-013: Execute functional and regression verification
- Primary layer: Integration/QA
- Dependencies: T-012 and all implementation tasks
- Expected outcome: Backend, database, UI, and Selenium checks pass for positive, negative, boundary, failure, and existing-home-page workflows.
- Notes or risks: Record unavailable infrastructure, skipped cases, and failures factually.

## 4. C# Backend Tasks

- Add menu item, paginated result, create request, removal request, and validation/error models.
- Implement repository methods for deterministic page retrieval, insert, and primary-key-based bulk removal.
- Implement service-level validation for required/whitespace names, approved portion values, malformed/negative prices, and valid non-empty identifiers.
- Expose menu operations through the project’s existing ASP.NET Core endpoint pattern.
- Ensure insert and bulk removal have explicit transaction behavior.
- Return safe 4xx validation/conflict/not-found results and generic 5xx persistence failures.
- Apply authorization to POST and removal operations if the approved policy restricts mutations.
- Log failed operations without secrets or unnecessary user-entered data.

## 5. SQL Database Tasks

- Add a non-destructive MenuItem schema migration/setup script.
- Define primary key, required columns, approved portion representation, non-negative price constraint, and UTC creation ordering.
- Add indexes for newest-first pagination and identifier/status lookup.
- Add duplicate-safe, repeatable insertion of the 10 exact default rows.
- Preserve the existing CafeStory table/index/seed.
- Implement approved hard-delete or soft-delete behavior consistently in reads and removal.
- Verify atomic insert/removal behavior and define stale-row handling under concurrency.

## 6. Selenium UI Testing Tasks

- Add page navigation and menu-title/content-region coverage.
- Verify all 10 seeded rows, exact portions/prices, and required table headings.
- Verify default Remove Menu disabled state and selection enable/disable transitions.
- Verify add form controls and default Half selection.
- Verify empty-name and negative/malformed-price validation without persistence.
- Verify valid add persists and appears at the top.
- Verify Cancel does not create a row.
- Verify single and multiple removal, database-backed refresh, and stale/empty outcomes.
- Verify exactly 10 rows, 11+ rows, first/last page controls, and last-page adjustment after deletion.
- Verify controlled retrieval/mutation failures without raw exception details.
- Verify desktop/mobile layout and no regression to home-page cafe-story rendering.
- Use isolated fixtures and cleanup for every mutation test.

## 7. Integration and Verification Tasks

- Run the existing application build before and after the feature changes.
- Apply database setup against a clean database and an existing US-001 database; verify idempotency and preservation of cafe-story data.
- Exercise GET, POST, and removal operations with valid, invalid, empty, malformed, duplicate, stale, and unauthorized inputs as applicable.
- Start the application and verify navigation from home page to menu page and back.
- Run focused Selenium tests, then the complete UI test project.
- Confirm the new row is newest-first and the menu is reread from SQL after navigation/refresh.
- Confirm no unhandled errors, schema details, or connection information reach the browser.
- Recheck US-001 home-page story retrieval and existing navigation after menu integration.

## 8. Risks, Dependencies, and Open Questions

### Known risks
- Unresolved mutation authorization can create a security gap or force late contract changes.
- Ambiguous price precision/currency can cause validation and persistence inconsistencies.
- Seeded `NA` portions conflict with the add form’s Half/Full-only wording.
- Hard versus soft deletion affects indexes, refresh behavior, and data retention.
- Shared database state can make Selenium mutation tests order-dependent.
- Page deletion on the last page can produce invalid pagination if metadata is not refreshed.

### External dependencies
- SQL Server availability and configured connection string.
- Node/npm availability for TypeScript build, unless checked-in bundle workflow is used.
- Chrome and Selenium driver availability.
- Existing endpoint, database, and UI conventions.

### Unresolved questions
- Are menu mutations restricted to authenticated staff or administrators?
- What price type, precision, scale, currency, maximum, and zero-price rules apply?
- Are duplicate name/portion pairs allowed?
- Can newly added items use NA?
- Is removal permanent or soft-deleted, and is an audit history required?
- Is a confirmation prompt required before removal?
- What exact API routes/status codes and user-facing messages are expected?
- What browsers, viewport sizes, accessibility level, and CI execution are supported?

## 9. Definition of Done

- The reviewed architecture decisions and policy gates are resolved or explicitly accepted as implementation assumptions.
- The menu schema and idempotent 10-row seed setup work on clean and existing databases without damaging US-001 data.
- Users can navigate to the menu view, view paginated current data, add valid items, cancel without saving, and remove selected items.
- Required validation prevents empty names and negative prices, with server-side enforcement.
- New items appear at the top and revisiting the page retrieves current database contents.
- Pagination, empty states, selection state, errors, and last-page behavior are correct.
- Existing home-page cafe-story behavior remains functional and themed consistently.
- Selenium covers integration, positive, negative, failure, and boundary cases with isolated test data.
- Build, database verification, and focused/full tests are executed and their evidence is recorded.
- Code review confirms maintainability, security, data integrity, and absence of unrelated changes.
