# System Architecture: US-004

## 1. Source and Summary
- User story reference: US-004, Implementation of Calculate Bill Module
- Source requirement analysis: `UserStories/US-004/US-004-RequirementAnalysis.md`
- Solution summary: Extend the existing ASP.NET Core minimal API and TypeScript single-page content region with a transient, database-priced Calculate Bill workflow and a final bill view.
- Actors and stakeholders: Cafe visitors or counter staff preparing a bill, cafe operators, and application maintainers.
- Architecture objective: Keep pricing authoritative on the server, keep bill editing responsive in the browser, preserve state through confirmation cancellation, and reuse the existing home-page, menu, SQL Server, and Selenium conventions.

## 2. Scope

### In scope
- Home-page navigation from the existing `Calculate Bill` link to a Calculate Bill view in the right content section.
- Menu item and portion selection from existing `dbo.MenuItem` data.
- Half/Full portion selection with Half as the default and a defined positive quantity dropdown.
- Server-side trusted-price lookup and decimal-safe line calculation.
- Transient browser bill state with add, edit-quantity, remove, total, empty, loading, and error states.
- Generate Bill and Discard Bill confirmation flows, including preservation on cancellation.
- Final bill view with cafe name, Print, and Generate New Bill actions.
- PDF download using the implementation-approved mechanism.
- Selenium coverage for navigation, calculations, mutations, confirmations, finalization, printing, failure cases, and responsive/accessibility behavior.

### Out of scope
- Persisting draft or final bills in SQL Server.
- Customer accounts, authentication, authorization, taxes, discounts, tips, payments, inventory, or receipts beyond the requested PDF.
- Changing the cafe story, menu-management workflow, existing database seed values, or application theme.
- Deriving prices from client-submitted values.

### Approved assumptions and constraints
- Bill state is transient in the browser and is owned by the Calculate Bill view. A browser refresh starts a new bill unless a later requirement explicitly adds persistence.
- A menu record is identified by `MenuItemId`; its `ItemName`, `Portion`, and `Price` are loaded from SQL Server.
- Existing `Half`, `Full`, and `NA` menu rows provide selectable database-backed options. `NA` rows are treated as non-portioned items, with Half/Full controls disabled for them.
- Duplicate additions create separate bill lines. This preserves each user action and avoids silently changing quantities; merging can be introduced later as a separate decision.
- Quantity values are a fixed dropdown from 1 through 10 until a product owner specifies another range.
- The current SQL seed price is authoritative for implementation and tests; Regular Coffee Half is 15 Rs in the seed data. The price displayed in a line is the selected portion's unit price. Amount is unit price multiplied by quantity, with two-decimal currency formatting.
- Empty bills cannot be generated; Generate Bill is disabled or returns an explicit validation message. Discard Bill on an empty bill is harmless and resets the view.
- The final bill captures the values already shown in the transient bill. Prices are read when a line is added or its quantity is edited and retained unchanged through generation. Editing a bill line changes quantity only; price and portion are not editable.
- Confirmation uses an accessible application dialog or native confirmation mechanism selected during implementation, with cancellation leaving the current state unchanged.
- The existing cafe name, `Musafir Cafe`, appears at the top of the final bill.

## 3. Component Architecture

```text
Browser
  |
  +--> Existing Home View / Right Content Region
  |       |
  |       +--> Calculate Bill View (TypeScript)
  |       |       |
  |       |       +--> BillState: lines, total, selection, pending/error state
  |       |       +--> Confirmation flow
  |       |       +--> Final Bill View / PDF action
  |       |
  |       +--> Existing Cafe Story View
  |
  +--> HTTP JSON requests
          |
          +--> GET /api/menu/bill-options
          +--> POST /api/bill/calculate
                    |
                    +--> BillService + validation
                              |
                              +--> MenuService price lookup
                                        |
                                        +--> SQL Server dbo.MenuItem

Selenium WebDriver --> Browser --> TypeScript --> Minimal API --> SQL Server
```

### Responsibilities
- **Home/content region:** Switch between story, menu management, and Calculate Bill views without replacing the application shell.
- **Calculate Bill view:** Own selection controls, transient lines, derived total, rendering, confirmation state, and navigation to the final view.
- **BillService:** Validate requested item/portion/quantity, resolve trusted pricing, calculate line amounts, and return stable DTOs.
- **MenuService:** Reuse or extend the existing SQL Server access boundary to retrieve selectable menu records and a specific item/portion price.
- **Minimal API endpoints:** Validate request shape, invoke services, map controlled errors to HTTP responses, and avoid exposing database exceptions.
- **SQL Server:** Remains the authoritative source for menu names, portions, and prices.
- **Selenium:** Verifies the integrated browser-visible workflow against a configured application and database.

## 4. Data Model and State

### Server DTOs
- `BillMenuOption`: `MenuItemId`, `ItemName`, `Portion`, `Price`.
- `CalculateBillRequest`: `MenuItemId`, `Portion`, `Quantity`.
- `BillLine`: a stable client line identifier, `MenuItemId`, `ItemName`, `Portion`, `Quantity`, `Price`, and `Amount`.
- `CalculatedBillLine`: server result containing the trusted menu identity/name/portion, `Quantity`, `Price`, and `Amount`.

A server-generated line identifier is unnecessary for transient calculation; the client may use a monotonic local identifier for editing/removal. It must not be treated as a database identity.

### Client state
```text
BillState
  menuOptions: BillMenuOption[]
  lines: BillLine[]
  selectedMenuItemId: number | null
  selectedPortion: "Half" | "Full"
  selectedQuantity: number
  total: decimal-derived display value
  view: "calculate" | "final"
  loading: boolean
  error: string | null
```

The total is derived from `lines` after every mutation rather than incremented/decremented independently. The displayed total is formatted from a decimal-safe calculation result; the browser must not use floating-point deltas as the authoritative value.

### Database use
No new table is required for the approved transient design. Existing `dbo.MenuItem` supplies selectable rows and prices. The current schema allows `NA`; the bill-options query returns every row so all database menu items can enter the bill through the normal UI.

## 5. HTTP API Design

### `GET /api/menu/bill-options`
Returns every menu row, including `NA`, ordered deterministically by item name, portion, and menu-item identifier. The endpoint returns the existing item/portion rows rather than grouping them, because each row has an independent price.

Responses:
- `200 OK`: array of `BillMenuOption` records.
- `200 OK` with an empty array: no selectable menu rows.
- `503 Service Unavailable`: database/read failure with a generic user-safe message.

### `POST /api/bill/calculate`
Accepts one line request:

```json
{
  "menuItemId": 1,
  "portion": "Half",
  "quantity": 5
}
```

The service loads the row by `MenuItemId` and confirms that its stored portion matches the requested portion. It then calculates `Price * Quantity` using C# `decimal` arithmetic. The request never accepts price, amount, item name, or total as authoritative fields.

Responses:
- `200 OK`: `CalculatedBillLine` with trusted item data and calculated amount.
- `400 Bad Request`: malformed request, unsupported portion, or non-positive/out-of-range quantity.
- `404 Not Found`: menu item does not exist or does not have the requested portion.
- `503 Service Unavailable`: database/read failure with a generic user-safe message.

The client uses this endpoint when adding or changing a line. The client then derives the displayed bill total from the returned line values. Final bill generation does not create a persistence record.

### Error contract
Validation errors use stable field names and a user-safe message, following the existing `Results.ValidationProblem` style. Unexpected errors are logged at the API boundary and returned without SQL, connection, or stack-trace details.

## 6. Request and Interaction Flows

### Navigation and initial load
1. The user selects the home-page `Calculate Bill` link.
2. The client replaces the right content region with the Calculate Bill view.
3. The client renders `Generating Bill`, Select Item, Estimated Bill, an empty state, and total zero.
4. The client requests `/api/menu/bill-options`.
5. The client populates the item/portion selection controls or displays an empty/error state.

### Add flow
1. The user chooses an item, portion, and quantity.
2. The client validates that all controls have valid values and disables the submit control while pending.
3. The client posts identity, portion, and quantity to `/api/bill/calculate`.
4. The service resolves the SQL price and returns a calculated line.
5. The client appends a local line identifier, renders the table, and derives the total from all lines.
6. Existing valid lines remain when the new request fails.

### Edit flow
1. The user selects a row’s accessible pencil control.
2. The client presents a quantity editor for that row and preserves the current quantity as the default.
3. The client posts the row identity, portion, and new quantity to the calculation endpoint.
4. The returned trusted line replaces the old line and the client derives the total again.
5. Canceling the editor leaves the row unchanged.

### Remove flow
1. The user selects a row’s accessible cross control.
2. The client removes only that local line and rerenders the table and derived total.
3. No database mutation occurs because the bill is transient.

### Generate flow
1. Generate Bill is disabled when no lines exist.
2. The user selects Generate Bill and confirms.
3. The client switches to the final view using the current lines and total.
4. The final view displays `Musafir Cafe`, the bill table, total, Print, and Generate New Bill.
5. Canceling confirmation returns to the Calculate Bill view with the same state.

### Discard flow
1. The user selects Discard Bill and confirms.
2. The client clears all lines, resets total to zero, and renders a fresh Calculate Bill view.
3. Canceling returns to Calculate Bill with the existing lines and total.

### Print flow
The final view invokes `window.print()` with print-specific CSS that hides application navigation/actions and presents the final bill as a print-ready document. The user can select the browser's Save as PDF destination. The print operation must not mutate bill state and does not add a PDF library dependency.

## 7. C# Backend Design

### Services
Extend `MenuService` with a deterministic bill-options query and a specific menu-item/portion lookup, or introduce a `BillService` that calls a narrowly scoped MenuService method. Prefer a separate `BillService` for validation and calculation so bill rules are not embedded in endpoint lambdas.

`BillService` responsibilities:
- Validate `MenuItemId`, the selected database portion including `NA`, and quantity 1-10.
- Retrieve the row by identifier and requested portion using parameterized SQL.
- Return trusted item name, portion, price, quantity, and amount.
- Use `decimal` for price and amount.
- Return typed success/not-found/validation outcomes suitable for minimal API mapping.

### Dependency registration
Register `BillService` with the existing singleton service pattern only if its dependencies remain configuration/stateless and the underlying connection is created per operation. Otherwise use the project’s selected lifetime consistently. Do not share an open `SqlConnection` between requests.

### Endpoint placement
Add the bill endpoints near the existing `/api/menu` endpoints in `Program.cs`. Keep endpoint lambdas thin: logging, cancellation-token propagation, status mapping, and service invocation belong there; calculation and validation belong in `BillService`.

### Security and integrity
- Ignore any client-supplied price, amount, item name, or total.
- Use parameterized SQL for item/portion lookup.
- Bound quantity server-side even though the UI uses a dropdown.
- Return generic failures and log exception details only on the server.
- No authentication/authorization is introduced because it is outside the confirmed story scope; this is an explicit product risk if the feature is later exposed to restricted staff workflows.

## 8. SQL Server Design

### Existing schema reuse
Use `dbo.MenuItem` without modifying the existing seed script for the transient bill feature. The current `MenuItemId` primary key and `ItemName`, `Portion`, and `Price` columns are sufficient.

### Query requirements
- Bill options: filter `Portion IN (N'Half', N'Full')` and order by `ItemName`, `Portion`, `MenuItemId`.
- Specific price: filter by `MenuItemId` and `Portion` and return one row.
- Use parameters for both identifier and portion.
- A missing row is a controlled not-found result.

### Concurrency and price changes
Because the bill is transient, no draft transaction is required. Each add/edit calculation reads the current database price. Once a line is displayed, its returned price is retained in the current bill until that line is edited; a future persisted-bill design must snapshot prices transactionally.

### Migration impact
No SQL migration is needed for the approved design. Existing database setup remains unchanged. If requirements later mandate bill persistence, that must be a separately reviewed architecture change with bill/header and bill-line tables, historical price snapshots, and transactional finalization.

## 9. Frontend Design

### View composition
Add `showCalculateBill()` and `showFinalBill()` functions alongside the existing story and menu view functions in `src/main.ts`. Keep the home-page shell and story-loading behavior intact. Use DOM construction or the project’s established rendering style, with text assigned through `textContent` for data values.

### Control behavior
- Item selection uses a labeled dropdown populated from bill options.
- Portion uses labeled Half/Full radios, with Half checked initially.
- Quantity uses a labeled numeric dropdown containing 1-10.
- Add To Bill, Generate Bill, Discard Bill, Print, and Generate New Bill have stable IDs or `data-testid` values.
- Pencil and cross controls use familiar icons with visible accessible labels/tooltips; icon glyph choice must match the existing project assets or approved icon dependency.
- Pending requests disable only the affected action and show a concise status/error state.

### Rendering and state safety
Render the table from `BillState.lines` after each mutation. Use a stable local line identifier for DOM row keys and test selectors. Calculate the display total by summing line amounts in a decimal-safe server result model; where TypeScript must sum display values, use integer minor units or a documented rounding strategy rather than unconstrained binary floating-point arithmetic.

### Responsive/accessibility behavior
Use semantic `section`, `form`, `fieldset`, `legend`, `label`, `table`, `thead`, `tbody`, and `button` elements. Keep table headers associated with cells. Ensure icon controls have accessible names and focus styles. At narrow widths, allow deliberate horizontal scrolling or a structured stacked representation; do not let columns overlap. Preserve the existing color and spacing variables.

## 10. Selenium Testing Architecture

### Environment
Run against the existing ASP.NET Core application with SQL Server seeded through the current database script. Tests require deterministic menu fixtures and must not depend on menu-management mutation order.

### Stable selectors
Provide selectors for:
- `Calculate Bill` navigation link.
- `Generating Bill` heading and `Select Item:`/`Estimated Bill:` sections.
- Item, portion, and quantity controls.
- Add To Bill, Generate Bill, Discard Bill, confirmation actions, final bill, Print, and Generate New Bill.
- Bill table, each row by local line identifier, pencil, cross, amount, and total.
- Empty, loading, validation, and failure states.

### Test groups
- Navigation and initial empty/default state.
- Menu options loaded from database, including empty and unavailable states.
- Regular Coffee Half at 20 Rs and quantity 5 producing 100 Rs, plus quantity 4 producing 80 Rs where the fixture supplies that price.
- Multiple lines and total recomputation.
- Edit quantity, cancel edit, remove one line, and empty-table reset.
- Generate confirmation cancellation and confirmation-to-final-view.
- Final bill content, cafe name, Generate New Bill reset, and PDF download initiation/content validation.
- Discard confirmation cancellation and confirmation reset.
- Invalid item identity, unsupported portion, quantities outside 1-10, failed calculation, duplicate additions, and rapid repeated actions.
- Keyboard/accessibility interaction and desktop/mobile layout.

## 11. Risks, Dependencies, and Decisions

### Risks
- The source example says Half price 20 Rs, while the current seed script has Regular Coffee Half at 15 Rs. The confirmed implementation decision is to use the current SQL seed value of 15 Rs for implementation and tests.
- The current menu schema permits `NA`; all portions are included in bill selection and `NA` is handled as non-portioned.
- Transient browser state is lost on refresh and is not shared across tabs.
- Browser print-to-PDF depends on the user's browser print dialog and does not provide a direct file-download API.
- Without authorization, any user who can access the app can calculate a bill; no bill data is persisted.

### Dependencies
- Existing `MenuItem` table and SQL Server connection.
- Existing `MenuService`, minimal API startup, static frontend, and stylesheet conventions.
- Approved quantity range, duplicate behavior, empty-bill rules, price-change behavior, and currency/PDF format.
- Selenium WebDriver and a deterministic test fixture.

### Architecture decisions requiring confirmation before implementation
- Confirm fixed quantity range 1-10.
- Confirm separate duplicate rows.
- Confirm Half/Full-only selection and independent stored portion prices.
- Confirm transient-only bill state.
- Confirm empty-bill generation is disallowed.
- Select PDF library or browser-compatible implementation and exact filename/currency format.
- Reconcile the Regular Coffee example price of 20 Rs with the current seeded Half price of 15 Rs.

## 12. Traceability

| Requirement | Component(s) | Data/API | UI/Test |
|---|---|---|---|
| FR-001, FR-002 | Home content region, Calculate Bill view | N/A | Navigation and initial-state tests |
| FR-003, FR-004, FR-005 | Calculate Bill controls, BillService validation | `GET /api/menu/bill-options` | Selection/default/invalid-input tests |
| FR-006, FR-007, FR-011 | BillService, bill state renderer | `POST /api/bill/calculate`, `dbo.MenuItem` | Calculation/add/multiple-line tests |
| FR-008, FR-009, FR-010 | Bill table renderer and local state | No bill persistence | Edit/remove/total tests |
| FR-012, FR-015, FR-016 | Confirmation and view state | Transient browser state | Cancel/confirm preservation tests |
| FR-013, FR-014 | Final Bill view and PDF adapter | No persistence; approved PDF mechanism | Print/new-bill tests |
| NFR-001, NFR-002, NFR-003 | .NET service/API, SQL lookup | Parameterized decimal calculation | Calculation/error tests |
| NFR-004, NFR-005, NFR-006, NFR-008 | Existing styles, semantic UI, selectors | Controlled API errors | Responsive/accessibility/Selenium tests |

## 13. Implementation Handoff

The implementation plan should sequence work as follows:
1. Confirm the architecture decisions listed above, especially the seed/example price mismatch and PDF strategy.
2. Add bill DTOs and BillService with unit-level validation/calculation coverage.
3. Add bill-options and calculate endpoints with API error mapping.
4. Add Calculate Bill and Final Bill TypeScript views while preserving existing story/menu navigation.
5. Add focused styling and stable accessibility/test selectors.
6. Add the PDF adapter and verify downloaded content.
7. Add Selenium integration tests with isolated/deterministic fixtures.
8. Run build, unit/API checks, Selenium tests, and review evidence before publication.
