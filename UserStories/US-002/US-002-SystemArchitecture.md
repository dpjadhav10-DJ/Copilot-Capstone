# System Architecture: US-002

## 1. Source and Summary
- User story reference: US-002, Implementation of Add or Remove Cafe Menu Module
- Source requirement analysis: `UserStories/US-002/US-002-RequirementAnalysis.md`
- Solution summary: Extend the existing ASP.NET Core and TypeScript cafe application with a database-backed menu-management view embedded in the home page content area.
- Actors and stakeholders: Cafe staff or administrators managing menu data, cafe users viewing the menu, and application operators.
- Architecture objective: Keep menu presentation, validation, business operations, persistence, and browser tests separable while preserving the existing home-page and cafe-story workflow.

## 2. Scope

### In scope
- Home-page navigation to an `Add/Remove Cafe Menu` content view.
- Database-backed paginated menu retrieval.
- Add form with default Half portion, validation, save, and cancel behavior.
- Selection-based single or multiple menu removal.
- SQL schema migration/setup and idempotent seed data for the 10 specified entries.
- Selenium coverage for integration, CRUD behavior, validation, pagination, refresh, failures, and responsive layout.

### Out of scope
- Bill calculation, contact, and location workflows.
- Menu categories, images, inventory, tax, or availability management.
- Unspecified authentication/authorization policy until approved.
- Changing the existing cafe-story behavior or source story text.

### Assumptions and constraints
 - Implementation is gated on confirming authorization, price precision/currency, duplicate-item policy, NA creation behavior, deletion policy, and exact endpoint contracts wherever they affect public behavior or schema.

 - Clear or reconcile selection state whenever pagination or a data refresh changes the rendered rows, so selections from an old page cannot trigger unintended removal.
### Presentation layer
- Add a menu view state to the existing right content region.
 If the result is empty, the client renders an explicit empty-menu state. Pagination controls use the returned metadata and must not navigate to an invalid page.
- Render a page heading, Add Menu and Remove Menu controls, paginated table, and add form state.
- Use semantic table markup, labels, accessible button names, and stable data attributes or IDs for Selenium.
 The implementation plan shall include a security review item for the selected policy and verify that unauthorized mutation requests are rejected by the backend even when controls are manually invoked.
- Keep selection state in the UI only for the current rendered page; submit selected persistent identifiers to the backend.
 - Identify seed rows with a stable, duplicate-safe strategy and test the upgrade path against an existing database; do not identify them only by a mutable display value if that would create duplicates.
- Server-side validation is authoritative; client-side checks provide immediate feedback.
- Creation returns the persisted row or a validation/error result and the client refreshes the first page.
 Seed the 10 required entries for read tests. Use unique names or fixture cleanup for add tests. Removal tests must target known IDs and restore or recreate data so test order is independent; they must not permanently consume shared seed data. Provide a controlled failure configuration or test seam without exposing it in production behavior.
 - After deletion, verify that pagination metadata and controls adjust correctly when the current last page becomes invalid.

### Data access layer
- A repository exposes page retrieval, insert, and identifier-based delete/status operations.
- Queries use parameters or the project’s established ORM/data-access mechanism.
- The repository maps database records to domain/DTO models without leaking SQL details to the UI.

### Database layer
- Add a menu-item table with a primary key, required name, controlled portion, non-negative decimal price, and creation ordering metadata.
- Add indexes for newest-first paging and identifier/status lookup.
- Use an idempotent migration or setup script and duplicate-safe seed logic.

### External integrations
- None required. Selenium drives the browser and the application’s configured SQL Server is the persistence dependency.

## 4. Component Diagram

```text
Browser
  |
  v
TypeScript Menu View <----> Home Page Content Region
  |
  | HTTP JSON/page request
  v
ASP.NET Core Menu Endpoints
  |
  v
MenuService + Validation
  |
  v
MenuRepository / SQL Data Access
  |
  v
SQL Server MenuItem table

Selenium WebDriver --> Browser --> Menu View --> ASP.NET Core --> SQL Server
```

Responsibilities:
- UI: navigation, table rendering, selection state, form interaction, client feedback, and refresh.
- Endpoints: translate HTTP requests, enforce request shape, return consistent status/result payloads.
- Service: apply domain rules, coordinate transactions, and control operation outcomes.
- Validation: reject missing names, invalid portions, malformed/negative prices, and invalid identifiers.
- Repository: execute parameterized deterministic queries and mutations.
- SQL Server: enforce persistence integrity and supply seeded/default data.
- Selenium: verify user-visible workflow and integration behavior.

## 5. Data Flow

### Request flow
1. The user selects `Add/Remove Cafe Menu` from the home page.
2. The TypeScript client changes the right content region to the menu view.
3. The client requests page 1 with page size 10.
4. The endpoint invokes `MenuService`, which queries the repository.
5. The client renders the returned rows and pagination metadata.

### Add flow
1. The user opens Add Menu and enters name, portion, and price.
2. The client performs immediate checks and submits a create request.
3. The endpoint and service repeat validation on the server.
4. The repository inserts one row in a transaction and returns its identity/order data.
5. The client navigates to page 1 and retrieves the current list, showing the new row first.

### Remove flow
1. The user selects one or more row identifiers.
2. The client enables Remove Menu only when the selection is non-empty.
3. The endpoint validates the identifier collection.
4. The service deletes or marks all selected rows according to the approved deletion policy in one transaction.
5. The client refreshes the current valid page and clears selection state.

### Error/exception flow
- Client validation prevents avoidable requests but does not replace server validation.
- Validation failures return field-level or operation-level client errors without mutation.
- Missing/stale identifiers return a controlled conflict/not-found result according to the final API contract.
- SQL failures are logged at the application boundary and returned as a generic user-facing failure state.
- No raw exception, schema, connection, or secret details are sent to the browser.

### Approval/validation flow
The architecture is based on the approved requirement analysis. Authorization policy, price precision/currency, duplicate policy, NA creation support, and hard-delete versus soft-delete behavior remain explicit implementation/design decisions. They must be resolved before implementation if they affect contracts or schema.

## 6. C# Backend Design

### Controllers/endpoints
Use the existing ASP.NET Core endpoint style. The conceptual contract is:
- `GET /api/menu?page={page}&pageSize=10`: returns rows and pagination metadata.
- `POST /api/menu`: accepts item name, portion, and price; returns the created item or validation errors.
- `DELETE /api/menu`: accepts a body containing selected menu-item identifiers, or use the project’s established bulk-delete convention.

Exact routes, status codes, and server-rendered/client API composition shall follow existing application conventions.

### Service responsibilities
- Normalize and validate input without silently changing source values.
- Retrieve deterministic newest-first pages.
- Create one menu item and expose the resulting persisted identity.
- Remove only the requested identifiers, handling concurrent stale selections explicitly.
- Return a fresh paginated result after mutations when practical.

### Domain logic
- Name is required and whitespace-only values are invalid.
- Price must parse as the approved non-negative decimal type.
- Add-form portions are Half or Full; seeded NA is a permitted stored/display value until the add policy is resolved.
- New records sort before existing records using creation timestamp plus primary key.
- Deletion must never use item name alone because duplicate names exist in seed data.

### DTOs and models
- `MenuItemDto`: identifier, item name, portion, price, and any display-safe ordering metadata required by the client.
- `MenuPageDto`: rows, total count, current page, page size, and total pages.
- `CreateMenuItemRequest`: item name, portion, and price only.
- `RemoveMenuItemsRequest`: a non-empty collection of menu-item identifiers.
- Validation/error result: stable field names and a user-safe message.

### Validation and error handling
Use consistent 4xx responses for malformed requests, validation failures, missing rows, and authorization failures if authorization is introduced. Use a generic 5xx response for unexpected persistence failures. Ensure failed validation and failed transactions do not leave partial data.

### Authentication/authorization implications
The source does not establish whether menu mutation is public. The implementation plan must select and document the policy. If restricted, enforce it on POST and DELETE endpoints and expose controls only to authorized users as a usability measure, never as the sole security control.

### Logging and observability
Log operation type, outcome, and correlation context for failed reads/writes/deletes. Do not log credentials or unnecessary user-entered values. Preserve existing application logging conventions.

## 7. SQL Database Design

### Tables/entities
`MenuItem`:
- `MenuItemId`: integer or project-standard primary key.
- `ItemName`: required bounded text.
- `Portion`: required controlled text/domain value.
- `Price`: required non-negative decimal with approved precision/scale.
- `CreatedAt`: required UTC timestamp for newest-first ordering.
- Optional `UpdatedAt`, `IsActive`, or deletion metadata only if soft deletion/audit is approved.

### Keys and constraints
- Primary key on `MenuItemId`.
- Non-null constraints on name, portion, price, and creation timestamp.
- Database check constraints or equivalent validation for non-negative price and approved stored portions where compatible with seeded NA values.
- Deletion targets primary keys.
- Duplicate names are allowed; the duplicate combination policy for newly created rows remains open.

### Indexing considerations
- Composite index supporting active status, `CreatedAt DESC`, and `MenuItemId DESC` for deterministic pagination.
- Primary-key lookup supports selected-item removal.
- If hard deletion is chosen, omit status from the active-list index; if soft deletion is chosen, filter inactive rows consistently in every read.

### Transactions and concurrency
- Insert is atomic.
- Bulk removal is atomic unless the approved contract explicitly defines partial success.
- Use deterministic ordering so concurrent inserts do not duplicate or reorder a page unpredictably.
- Define stale-selection behavior when a selected row disappeared between read and delete.

### Seed and migration
- Add a repeatable SQL setup/migration for `MenuItem`.
- Insert the exact 10 source rows only when their seed identity/content is not already present; rerunning setup must not duplicate them.
- Preserve existing `CafeStory` schema and seed data.
- Upgrade an existing US-001 database without destructive replacement.

## 8. Selenium UI Testing Design

### Test coverage scope
Exercise the browser-visible integrated flow against the running ASP.NET Core application and configured SQL Server. Use isolated test data or reset fixtures for mutation tests.

### Critical user journeys
- Navigate from home page to the menu view.
- Inspect seeded rows and page controls.
- Add a valid item and see it at the top.
- Reject invalid name and price.
- Cancel without persistence.
- Select and remove one or multiple items.
- Revisit the page and see database-backed current data.
- Handle operation failures and narrow viewport layout.

### Selector/testability considerations
Provide stable accessible names for the navigation link, page heading, Add Menu, Remove Menu, form labels, Save/Cancel, pagination controls, and error state. Give each row and checkbox a stable identifier derived from the menu-item ID, not row position alone. Use semantic table headers and avoid presentation-only selectors.

### Test data and isolation
Seed the 10 required entries for read tests. Use unique names or fixture cleanup for add tests. Removal tests must target known IDs and restore or recreate data so test order is independent. Provide a controlled failure configuration or test seam without exposing it in production behavior.

### Positive, negative, and boundary scenarios
Cover seeded display, default disabled state, selection transitions, valid add, empty/whitespace name, negative/malformed price, cancel, single/multiple removal, stale selection, empty list, exactly 10 rows, 11+ rows, last-page navigation, operation failure, and desktop/mobile layout.

### Cross-browser/execution considerations
The existing Selenium WebDriver with Chrome is the baseline. Browser matrix, headless/CI configuration, and supported viewport dimensions remain implementation/test-plan decisions.

## 9. Non-Functional Considerations

- Preserve the existing home-page/cafe-story workflow while changing only the right content region when the menu feature is selected.
- Maintain readable and non-overlapping controls and table content at supported viewport sizes.
- Provide semantic/accessibly named controls and validation feedback sufficient for keyboard/browser automation.
- Keep database mutations transactional and server-validated.
- Avoid leaking database or exception details in user-facing errors.
- Performance, formal accessibility level, browser matrix, currency, and authorization targets are not specified and remain open questions.

## 10. Risks, Dependencies, and Open Questions

### Confirmed facts
- Existing baseline is ASP.NET Core .NET 8, TypeScript, SQL Server, and Selenium with Chrome.
- The feature must use the home page’s right content section.
- Ten exact default entries and a page size of 10 are required.

### Assumptions
- Existing frontend routing/content-region patterns can host the menu view.
- New entries are newest-first.
- Multiple deletion is supported and should be atomic.

### Dependencies
- Existing home page and client bundle.
- SQL Server connection and setup conventions.
- Existing database migration/seed mechanism.
- Selenium test project and browser driver availability.

### Unresolved questions
- Are menu mutations restricted to authenticated/authorized staff?
- What are the price precision, currency, maximum, and zero-price rules?
- Are duplicate name/portion combinations allowed?
- Can users add `NA` portions, or only Half/Full?
- Is removal hard delete or soft delete, and is an audit trail required?
- What confirmation, stale-selection, empty-list, and failure messages are required?
- What API routes/status codes and UI routing conventions should be used?
- What browser, viewport, accessibility, and CI targets are supported?

## 11. Traceability Matrix

| Requirement/acceptance | Backend components | Database objects | Selenium coverage |
|---|---|---|---|
| FR-001, AC-001 | Home/menu route and content composition | N/A | UI-001, UI-015 |
| FR-002, AC-002/003 | Paginated menu endpoint/service | MenuItem ordering index | UI-002, UI-012 |
| FR-003, AC-004 | Selection contract and bulk removal endpoint | Primary-key deletion/status update | UI-003, UI-004, UI-010, UI-011 |
| FR-004/005, AC-005/006/007 | Create request and validation | Name/portion/price constraints | UI-005, UI-006, UI-007 |
| FR-006, AC-008 | Create service and newest-first query | Insert transaction and CreatedAt index | UI-008 |
| FR-007, AC-009 | No mutation on cancel | No insert | UI-009 |
| FR-008, AC-010 | Bulk removal service | Transactional delete/status update | UI-010, UI-011 |
| FR-009, AC-011 | Database-backed GET and refresh | MenuItem table/query | UI-002, UI-013 |
| FR-010/definition of done, AC-013/014 | Existing app integration | Non-destructive migration | UI-001, UI-015 |
| FR-011, AC-012 | Read model compatibility | Idempotent 10-row seed | UI-002 |
| FR-012 | Controlled errors and logging | Transaction/failure handling | UI-014 |

## 12. Acceptance Mapping

The architecture supports the required home-page integration, menu table, ten-record pagination, add form, validation, persistence, newest-first display, cancellation, selected-item removal, database refresh, seed data, and Selenium verification through the existing application layers. It preserves the US-001 story workflow and makes identifiers, transactions, and server-side validation explicit for data integrity.

Before implementation begins, the unresolved authorization, price, duplicate, NA-entry, deletion, API-contract, and test-environment decisions must be confirmed wherever they affect the schema or public behavior. No unsupported business behavior is fixed by this architecture.
