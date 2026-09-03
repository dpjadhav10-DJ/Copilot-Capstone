# Requirement Analysis: US-002

## 1. Source and Summary
- User Story Id: US-002
- User story reference: Implementation of Add or Remove Cafe Menu Module
- Source document: `UserStories/US-002/US-002-Description.txt`
- Story summary: Add a menu-management page integrated into the home page so users can view, add, and remove cafe menu items backed by a database.
- Actors and stakeholders: Cafe staff or administrators managing menu items; cafe users viewing the current menu; application operators responsible for data integrity.

## 2. Functional Requirements

### FR-001: Open the cafe menu page
The application shall open an `Add/Remove Cafe Menu` page when the user selects the corresponding link from the home page. The page shall render in the right section where the cafe story is displayed on the home page.

### FR-002: Display the menu table
The page shall display menu items below the action buttons in a multipage table with these columns:
- Checkbox to select a menu item
- Name of Item
- Portion (Half/Full)
- Price

The table shall display 10 records per page and provide controls to navigate to the next and previous pages.

### FR-003: Control menu selection and removal
The `Remove Menu` button shall be disabled by default. It shall become enabled when at least one menu-item checkbox is selected and shall become disabled again when all selected items are deselected.

### FR-004: Open the add-menu form
When the user selects `Add Menu`, the application shall open a form in the same content area. The form shall contain:
- Item Name textbox
- Portion radio buttons with `Half` and `Full` options
- Price textbox
- Save button
- Cancel button

### FR-005: Apply add-menu validation
When the user selects `Save`, the application shall validate each field as follows:
- Item Name shall not be empty.
- Price shall not be negative.
- Portion shall default to `Half`.

The story does not specify whether zero price, non-numeric price, whitespace-only names, maximum lengths, or duplicate items are valid; these shall be resolved during design or recorded as implementation constraints before implementation.

### FR-006: Save a new menu item
When a valid menu item is saved, the application shall persist it in the database, navigate back to the `Add/Remove Cafe Menu` page, and display the new entry at the top of the menu table.

### FR-007: Cancel menu creation
When the user selects `Cancel` from the add-menu form, the application shall return to the menu page or prior menu view without saving changes.

### FR-008: Remove selected menu items
When one or more menu items are selected and the user selects `Remove Menu`, the application shall remove the selected entries from the table and database. The updated table shall no longer display the removed entries.

The story does not define whether removal is hard deletion or a soft-delete/status update; this is an open design decision.

### FR-009: Retrieve current menu items
Whenever the user navigates to the `Add/Remove Cafe Menu` page, the application shall retrieve and display the current menu list from the database rather than relying only on in-memory or hard-coded values.

### FR-010: Preserve existing application theme
The new page, form, table, controls, validation messages, and navigation integration shall follow the existing application theme, color scheme, and design conventions.

### FR-011: Seed the default menu
The database setup shall include these 10 default menu entries:

| Item Name | Portion | Price |
|---|---|---:|
| Regular Coffee | Half | 15 |
| Regular Coffee | Full | 25 |
| Regular Tea | Half | 12 |
| Regular Tea | Full | 20 |
| Veg Club Sandwitch | NA | 50 |
| French Fries - Regular | Half | 60 |
| French Fries - Regular | Full | 90 |
| Bun Butter | NA | 40 |
| Regular Maggie | NA | 50 |
| Veg Maggie | NA | 60 |

The source uses `Veg Club Sandwitch` and `NA`; those values shall be preserved unless the product owner approves corrections or a separate display rule.

### FR-012: Handle menu operation failures
If menu retrieval, creation, or removal fails, the application shall show a controlled user-facing error state and shall not expose raw database or exception details. Exact error messages, retry behavior, and partial-operation behavior require confirmation.

## 3. Business Rules and Validations

### Confirmed rules
- BR-001: The feature is named `Add/Remove Cafe Menu`.
- BR-002: The feature is accessed from the home page and occupies the right content section.
- BR-003: Menu rows contain a selection checkbox, item name, portion, and price.
- BR-004: Pagination displays 10 records per page with next and previous navigation.
- BR-005: `Remove Menu` starts disabled and is enabled only while at least one item is selected.
- BR-006: The add form provides item name, portion, price, Save, and Cancel controls.
- BR-007: Portion defaults to `Half`.
- BR-008: Item name cannot be empty and price cannot be negative.
- BR-009: A successful add persists the item and places it at the top of the table.
- BR-010: Cancel does not save changes.
- BR-011: Selected items are removed from both the table and database.
- BR-012: The menu is retrieved from the database whenever the menu page is opened.
- BR-013: The database is seeded with the 10 entries listed in the source story.

### Assumptions requiring confirmation
- BR-014: Users permitted to add or remove menu items are authenticated cafe staff or administrators; the story does not define authentication or authorization.
- BR-015: A price is a non-negative decimal value with currency precision, but scale, currency, and maximum value are unspecified.
- BR-016: Multiple menu rows may share the same item name when their portions differ, as shown by the default data.
- BR-017: `NA` is a supported persisted/displayed portion for items without Half/Full sizing, despite the add form specifying only Half and Full.
- BR-018: New items are ordered newest-first using a persisted creation identifier or timestamp.
- BR-019: Removal is permanent unless the application’s existing data-retention conventions require soft deletion.
- BR-020: Removing multiple selected items is an atomic operation: either all selected rows are removed or none are.

## 4. C# Backend Requirements

### Endpoints and HTTP operations
The application shall provide backend operations equivalent to:
- A read operation for the current menu list, with pagination parameters and a page result containing rows, total count, current page, and page size.
- A create operation accepting item name, portion, and price and returning the created item or a controlled validation/error result.
- A delete operation accepting one or more menu-item identifiers and returning the updated operation result.

Exact route names, HTTP status codes, server-rendered versus API composition, and pagination parameter names are implementation decisions to be recorded in architecture.

### Request and response models
Models shall represent:
- Menu item identifier
- Item name
- Portion
- Price
- Creation ordering data when needed to place new items first
- Paginated menu rows, total record count, current page, and page size
- Field-level validation errors and operation-level errors

Create requests shall not accept a client-controlled identifier or creation timestamp.

### Service and domain logic
- Keep menu retrieval, creation, and removal behind a service boundary.
- Apply server-side validation even when equivalent client-side validation exists.
- Ensure a newly created item is returned or retrieved in newest-first order.
- Validate all selected identifiers before deletion.
- Ensure the remove operation cannot delete unrelated records based on malformed or missing identifiers.
- Return a fresh database-backed list after navigation or a successful mutation.

### Validation and error handling
- Reject missing or whitespace-only item names.
- Reject negative prices.
- Validate portion values against the approved domain values for the relevant operation, accounting for seeded `NA` rows.
- Reject malformed prices and invalid identifiers with controlled client errors.
- Return a controlled server error for database or persistence failures without exposing schema or exception details.
- Define behavior when a requested item was already removed or does not exist.

### Authentication and authorization implications
The source does not define authentication or authorization. Architecture shall explicitly decide whether menu mutations are public or restricted to authenticated/authorized staff. The UI and backend must enforce the same decision; authorization cannot rely only on hiding controls in the browser.

### Logging and integration considerations
- Log failed menu retrieval, creation, and removal operations with operation context.
- Avoid logging secrets or unnecessary user-entered data.
- Preserve compatibility with the existing home-page navigation and content-area integration.
- Use parameterized database access or the project’s established ORM/data-access pattern.

## 5. SQL Database Requirements

### Table and columns
Create a menu-item table with, at minimum:
- A primary-key menu-item identifier
- Required item name
- Required portion value
- Required non-negative price with appropriate decimal precision
- Creation timestamp or equivalent ordering field
- Optional update timestamp and active/status field if soft deletion or auditing is selected

The exact table and column names shall follow the project’s established SQL naming conventions.

### Constraints and data integrity
- Item name shall be non-null and should reject empty values at the application boundary; database enforcement should be considered where supported.
- Portion shall be constrained to approved values or represented through a controlled lookup/domain convention.
- Price shall be non-null and constrained to non-negative values.
- Seed data shall preserve the exact source values, including duplicate item names, `NA`, and the source spelling `Sandwitch`, unless approved changes are supplied.
- Deletes shall target primary-key identifiers and must not be based only on item name or display text.

### Indexing and query needs
- Index creation ordering to support newest-first results.
- Index active/status plus creation ordering if soft deletion is used.
- Support deterministic pagination ordering, including a tie-breaker such as the primary key.
- Indexes and constraints shall support lookup and deletion of selected identifiers.

### Transactions and concurrency
- Menu creation shall commit as one operation before the UI reports success.
- Multi-item removal should be transactional according to the approved atomicity rule.
- Handle concurrent additions and removals so pagination and newest-first ordering remain deterministic.
- Define behavior when a row selected by the UI no longer exists at deletion time.

### Migration and seed data
- Add repeatable database setup or migration support for the menu table.
- Seed the 10 default menu entries without creating duplicates when setup is rerun.
- Preserve existing US-001 database objects and seed data.
- The seed strategy shall define how an existing database is upgraded without destructive changes.

## 6. Selenium UI Test Requirements

### UI-001: Navigate to the menu page
- Setup: Start the application with the database available and the home page loaded.
- Actions: Select `Add/Remove Cafe Menu`.
- Expected: The right content section displays the menu page title and menu table, replacing or updating the cafe story content.
- Testability: Provide stable accessible names or semantic selectors for the navigation item, page title, content region, and menu table.

### UI-002: Display seeded menu data
- Setup: Use a database containing the required 10 seed rows.
- Actions: Open the menu page.
- Expected: All 10 default entries are displayed, with correct item names, portions, and prices.
- Testability: Rows and cells should have stable semantic structure or accessible selectors.

### UI-003: Verify default remove-button state
- Setup: Open the menu page with no selection.
- Actions: Inspect the controls.
- Expected: `Remove Menu` is disabled and `Add Menu` is available.

### UI-004: Enable and disable removal through selection
- Setup: Open the menu page.
- Actions: Select one row checkbox, inspect the remove control, then deselect it.
- Expected: `Remove Menu` becomes enabled after selection and disabled again after all selections are cleared.

### UI-005: Open the add form and verify defaults
- Setup: Open the menu page.
- Actions: Select `Add Menu`.
- Expected: The form opens in the same content area with the required fields and buttons; portion defaults to `Half`.

### UI-006: Validate an empty item name
- Setup: Open the add form.
- Actions: Leave item name empty, provide otherwise valid input, and select `Save`.
- Expected: A field-level validation message is displayed, no database row is created, and the user remains able to correct the form.

### UI-007: Validate a negative price
- Setup: Open the add form.
- Actions: Provide a valid item name, select a portion, enter a negative price, and select `Save`.
- Expected: A price validation message is displayed and no database row is created.

### UI-008: Add a valid menu item
- Setup: Open the add form.
- Actions: Enter a valid item name, choose a valid portion, enter a valid price, and select `Save`.
- Expected: The item is persisted, the menu page is shown, and the new row appears at the top with the submitted values.

### UI-009: Cancel menu creation
- Setup: Open the add form and enter unsaved values.
- Actions: Select `Cancel`, reopen or inspect the menu page.
- Expected: The menu page is shown and no row is created from the cancelled values.

### UI-010: Remove one menu item
- Setup: Open the menu page with a known seeded item.
- Actions: Select one row and select `Remove Menu`.
- Expected: The row disappears from the table and a subsequent page load confirms it is absent from the database-backed list.

### UI-011: Remove multiple menu items
- Setup: Open the menu page with at least two known items.
- Actions: Select multiple rows and select `Remove Menu`.
- Expected: All selected rows are removed and unselected rows remain.

### UI-012: Verify pagination
- Setup: Use more than 10 menu rows.
- Actions: Inspect the first page, select next, then previous.
- Expected: Each page contains no more than 10 rows; next and previous controls navigate to the correct pages and are appropriately disabled at the boundaries.

### UI-013: Verify database-backed refresh
- Setup: Change menu data through an approved database fixture or mutation, then navigate away and back to the menu page.
- Actions: Open the menu page again.
- Expected: The current database contents are displayed rather than stale client-side data.

### UI-014: Handle failed menu operations
- Setup: Make menu retrieval or mutation unavailable/fail through a controlled test configuration.
- Actions: Open the page or submit the operation.
- Expected: A controlled user-facing error is shown, raw exception details are not exposed, and the page remains usable where possible.

### UI-015: Verify layout and integration
- Setup: Use supported desktop and mobile viewport sizes.
- Actions: Navigate between the home page and menu page, open the form, and inspect the table.
- Expected: The existing theme is preserved, controls and text do not overlap, and the right content area remains usable. Exact viewport matrix is an open design decision.

## 7. Non-Functional Requirements

- NFR-001: Menu operations shall preserve data integrity under validation failures and concurrent requests.
- NFR-002: The page shall remain usable at the application’s supported desktop and mobile viewports without incoherent overlap.
- NFR-003: Menu controls, table headings, checkboxes, validation messages, and navigation shall expose semantic or accessible names sufficient for Selenium interaction.
- NFR-004: Database failures shall produce controlled responses without exposing implementation details.
- NFR-005: The application shall integrate the feature without regressing the existing home page and cafe-story workflow.
- NFR-006: Exact performance, browser support, accessibility conformance level, currency, and authorization targets are not specified and remain open questions.

## 8. Traceability Matrix

| Requirement | C# backend | SQL database | Selenium UI |
|---|---|---|---|
| FR-001 | Menu route/page composition | N/A | UI-001, UI-015 |
| FR-002 | Paginated read contract | Ordering and pagination indexes | UI-002, UI-012 |
| FR-003 | Selection/removal operation contract | Identifier-based deletion | UI-003, UI-004, UI-010, UI-011 |
| FR-004 | Form request model | N/A | UI-005 |
| FR-005 | Server-side validation | Price/portion/name integrity constraints | UI-006, UI-007 |
| FR-006 | Create operation and newest-first ordering | Insert transaction and creation ordering | UI-008 |
| FR-007 | Cancel requires no mutation | No insert on cancel | UI-009 |
| FR-008 | Multi-item removal service | Delete/status update and transaction | UI-010, UI-011 |
| FR-009 | Database-backed list service | Menu table query | UI-002, UI-013 |
| FR-010 | Existing page integration contract | N/A | UI-001, UI-015 |
| FR-011 | Seed/read model compatibility | Menu schema and 10 seed rows | UI-002 |
| FR-012 | Controlled errors and logging | Failure/transaction behavior | UI-014 |

## 9. Assumptions, Dependencies, and Open Questions

### Confirmed facts
- The source document is `UserStories/US-002/US-002-Description.txt`.
- The feature must be integrated through the home-page `Add/Remove Cafe Menu` link.
- The page must support database-backed retrieval, addition, and removal.
- The source supplies exactly 10 default menu entries and a 10-record page size.

### Assumptions
- The existing application’s C# backend, SQL database, frontend conventions, and Selenium harness will be reused.
- New menu items are displayed newest-first using a persisted ordering value.
- Menu mutations should be protected by server-side validation and, if required by architecture, authorization.
- Selected-row removal can support multiple rows because the UI explicitly allows selecting menu items plural.

### Dependencies
- Existing home-page route and right-section content integration.
- Existing database setup and connection configuration.
- Existing frontend theme and client-side navigation mechanism.
- Existing Selenium/WebDriver execution setup.
- A database migration or repeatable setup strategy compatible with the existing US-001 schema.

### Open questions
- Is the menu page public, or must add/remove operations require authenticated cafe staff or administrators?
- Which SQL engine/version and migration convention govern the new table?
- What are the exact API routes, HTTP status codes, and response shapes?
- Are zero prices, decimal precision, currency, maximum price, and non-numeric input rules required?
- Are whitespace-only names rejected, and what maximum name length applies?
- Are duplicate item name and portion combinations allowed for newly added entries?
- Should new menu entries support `NA` in the add form, or only Half and Full as stated?
- Is removal hard deletion or soft deletion, and is an audit trail required?
- What confirmation behavior, if any, is required before removal?
- What exact messages should be shown for validation, empty results, stale selections, and database failures?
- What happens if a multi-item deletion partially conflicts with concurrent changes?
- What viewport/browser/accessibility targets must Selenium cover?

## 10. Acceptance Criteria

- AC-001: Selecting `Add/Remove Cafe Menu` from the home page opens the menu page in the right content section.
- AC-002: The menu page displays `Add Menu`, `Remove Menu`, and a table with checkbox, item name, portion, and price columns.
- AC-003: The table displays 10 records per page and supports next/previous pagination.
- AC-004: `Remove Menu` is disabled with no selection, enabled with at least one selection, and disabled again when all selections are cleared.
- AC-005: Selecting `Add Menu` opens a same-area form containing item name, Half/Full portion options, price, Save, and Cancel; Half is selected by default.
- AC-006: Saving with an empty item name is rejected with validation and does not persist a row.
- AC-007: Saving with a negative price is rejected with validation and does not persist a row.
- AC-008: Saving valid values persists the row, returns to the menu page, and places the new row at the top.
- AC-009: Cancelling the add form does not persist entered values.
- AC-010: Removing selected items removes them from both the table and database.
- AC-011: Revisiting the menu page displays the current database-backed menu list.
- AC-012: The database contains the 10 default menu entries specified by the story after setup.
- AC-013: The feature follows the existing application theme and integrates without regressing the home-page workflow.
- AC-014: The definition of done includes database create/remove/retrieve support and integration, functional, positive, negative, and boundary testing.
- AC-015: Missing from the source and requiring confirmation: authorization, exact error and confirmation behavior, duplicate handling, price format/precision, `NA` entry behavior for newly added rows, deletion policy, supported viewport/browser matrix, and formal accessibility target.
