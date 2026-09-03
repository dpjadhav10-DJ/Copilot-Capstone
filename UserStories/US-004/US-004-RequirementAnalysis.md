# Requirement Analysis: US-004

## 1. Source and Summary
- User Story Id: US-004
- User story reference: Implementation of Calculate Bill Module
- Source document: `UserStories/US-004/US-004-Description.txt`
- Story summary: Add an integrated Calculate Bill workflow that lets a cafe user select menu items, portions, and quantities; review, edit, and remove estimated bill lines; discard or generate a bill; and print the final bill as a PDF.
- Actors and stakeholders: Cafe visitors or counter staff preparing a bill; cafe operators; application maintainers.

## 2. Functional Requirements

### FR-001: Open the calculate-bill page
The application shall open a Calculate Bill page in the right content section when the user selects the existing `Calculate Bill` home-page link. The page shall retain the existing application theme, color scheme, and layout conventions.

### FR-002: Display calculate-bill sections
The page shall display the title `Generating Bill` and two vertical sections titled `Select Item:` and `Estimated Bill:`. The initial estimated-bill table shall contain no item rows and shall show a total of zero.

### FR-003: Select a menu item
The Select Item section shall provide a dropdown populated from the existing menu-item data. Each selectable item shall expose enough identity and pricing information for the selected portion to be priced from the database. The empty-menu state and menu-load failure state shall be handled visibly.

### FR-004: Select portion
The page shall provide radio buttons for `Half` and `Full`. `Half` shall be selected by default. The selected portion shall be included in the bill line and used to determine its price.

### FR-005: Select quantity
The page shall provide a dropdown containing numeric quantity values. The selected quantity shall be a positive integer and shall be used in the line amount calculation. The permitted minimum, maximum, and available quantity values are not specified by the source and must be confirmed during design.

### FR-006: Calculate line amount
The application shall retrieve the price for the selected menu item and portion from the database and calculate the line amount as:

`Amount = database price for selected item and portion x selected quantity`

The amount shall be calculated before adding the line and displayed or otherwise made clear in the Select Item section. Monetary calculations shall use a precise decimal representation in the backend and consistent currency formatting in the UI.

### FR-007: Add an item to the estimated bill
The `Add To Bill` button shall add a bill line containing Item, Portion, Quantity, Price, and Amount to the Estimated Bill table. For example, a Regular Coffee priced at 20 Rs for Half portion with quantity 5 shall produce an amount of 100 Rs. The total shall be recalculated after the addition.

### FR-008: Display estimated bill lines
The Estimated Bill section shall display a table with columns named `Item`, `Portion`, `Quantity`, `Price`, `Amount`, `Edit`, and `Remove`. Edit controls shall use a pencil icon and remove controls shall use a cross icon, with accessible names or labels for non-visual users.

### FR-009: Edit a bill line
Selecting the pencil control for a row shall allow the user to change the bill line quantity, or otherwise provide an equivalent quantity-edit interaction consistent with the application design. The row amount and total shall be recalculated from the current database price and updated quantity. For the specified example, changing quantity from 5 to 4 changes the amount from 100 Rs to 80 Rs.

### FR-010: Remove a bill line
Selecting the cross control for a row shall remove that line from the estimated bill. The table and total shall be recalculated immediately. Removing the specified 100 Rs line shall deduct 100 Rs from the prior total.

### FR-011: Calculate total bill
The total at the bottom of the Estimated Bill table shall equal the sum of all current line amounts. An empty bill shall always display zero. The total shall be recalculated after every add, edit, and remove operation rather than maintained only through client-side deltas.

### FR-012: Generate a bill with confirmation
The `Generate Bill` button shall request user confirmation. When the user confirms, the application shall display a final bill page containing the estimated-bill lines and total. When the user cancels, the application shall return to the Calculate Bill page without losing the current estimated-bill contents.

Generating an empty bill is not defined by the source and should be rejected or disabled with a user-facing explanation during design confirmation.

### FR-013: Print the final bill
The final bill page shall display the cafe name at the top and provide a `Print` button. Selecting Print shall produce a downloadable PDF containing the final bill table and total. The PDF filename, currency presentation, document metadata, and PDF-generation implementation are not specified and must be confirmed before implementation.

### FR-014: Generate a new bill
The final bill page shall provide a `Generate New Bill` button that navigates back to a fresh Calculate Bill page with an empty estimated bill and total zero.

### FR-015: Discard a bill with confirmation
The `Discard Bill` button shall request user confirmation. When confirmed, it shall remove all current estimated-bill lines, reset the total to zero, and leave the user on a fresh Calculate Bill page. When cancelled, it shall return to the Calculate Bill page with the existing estimated-bill contents preserved.

### FR-016: Preserve bill state during the workflow
The current estimated bill shall remain intact while moving between the Calculate Bill page and confirmation dialogs, including cancellation of Generate Bill and Discard Bill. The persistence lifetime, browser-refresh behavior, and multi-tab behavior are not specified.

## 3. Business Rules and Validations

### Confirmed rules
- BR-001: The Calculate Bill workflow is reached through the existing home-page `Calculate Bill` link.
- BR-002: The page title is `Generating Bill`.
- BR-003: The page contains Select Item and Estimated Bill sections.
- BR-004: The default portion is Half, with Full as the other option.
- BR-005: A line amount is based on item, portion, database price, and quantity.
- BR-006: The estimated bill starts empty with total zero.
- BR-007: Adding, editing, and removing lines updates the table and total.
- BR-008: Generate Bill and Discard Bill both require confirmation.
- BR-009: A confirmed generated bill has Print and Generate New Bill actions.
- BR-010: A confirmed discarded bill is cleared and ready for a new bill.
- BR-011: The existing application theme and color scheme must be preserved.

### Rules requiring confirmation
- BR-012: Whether duplicate selections create separate rows or merge into an existing same-item/same-portion row.
- BR-013: The quantity dropdown range and whether it includes a special maximum or only fixed values.
- BR-014: Whether Full has its own stored MenuItem price, as the current menu model stores item/portion/price rows, or whether Full is derived from Half.
- BR-015: Whether the price shown in a bill is a unit price, a portion price, or another displayed value, and how rounding is applied.
- BR-016: Whether a menu item removed after selection can still be added, and whether existing bill lines retain their captured price.
- BR-017: Whether an empty bill may be generated or discarded.
- BR-018: Whether browser refresh, navigation away, or multiple tabs must preserve bill state.
- BR-019: Exact confirmation-dialog text and whether native browser confirmation is acceptable.
- BR-020: PDF filename, format, currency symbol, and print/download behavior.

### Validation requirements
- A menu item must be selected before Add To Bill is accepted.
- A valid portion must be selected; only Half and Full are accepted.
- Quantity must be a positive integer from the supported quantity list.
- Price must come from trusted server/database data; the client must not be able to submit an arbitrary price as authoritative bill data.
- Amount and total must be calculated with decimal-safe arithmetic and validated on the server for any persisted or final-bill operation.
- Menu-load, price-load, add, and finalization failures must produce controlled user-facing error states without losing the current bill where practical.
- Controls must prevent accidental duplicate submissions while an operation is pending.

## 4. C# Backend Requirements
- Expose a read operation for menu selections and their portion prices using the existing `MenuService` and API conventions.
- Provide a bill calculation boundary that accepts menu-item identity, portion, and quantity, then resolves the current trusted price from SQL Server and returns a calculated line or validation errors.
- Do not accept a client-supplied price or amount as authoritative input.
- Define bill-line and bill-summary response models containing item name, portion, quantity, unit/portion price, line amount, and total.
- Decide whether the bill is transient client-side state or is represented by server-side draft/final bill endpoints. US-004 does not explicitly require saving bills to the database.
- If final bills are persisted, define bill and bill-line tables, transactional finalization, identity/audit fields, and idempotency behavior before implementation.
- Return controlled errors for unknown or deleted menu items, unsupported portions, invalid quantities, unavailable database access, and empty-bill generation.
- Keep calculations in a shared service so UI rendering and any finalization endpoint use the same rules.
- Avoid exposing sensitive information in error messages or logs; the current bill contains no stated sensitive data.

## 5. SQL Database Requirements
- Use the existing `dbo.MenuItem` records as the source of item names, portions, and prices unless the architecture phase establishes a separate pricing model.
- Ensure the read query can retrieve all selectable menu items and their portion prices deterministically.
- Enforce or validate that menu item identity, portion, and price are present and valid for bill calculation.
- Do not introduce bill persistence tables unless the architecture and implementation plan explicitly approve server-side bill storage.
- If bill persistence is approved, use a transaction to create the bill and all lines, store the applied unit prices, and preserve the historical amount even if menu prices later change.
- Define behavior for deleted menu items and price changes between selection and finalization; this is an open design decision.

## 6. TypeScript and UI Requirements
- Extend the existing home-page link behavior to render or navigate to the Calculate Bill view in the right content section.
- Load menu options from the backend and render an accessible item dropdown.
- Render Half/Full radio controls with Half selected initially and a quantity dropdown with a defined supported range.
- Keep bill state in one owning view/model so add, edit, remove, total, confirmation cancellation, and reset operations cannot diverge.
- Render an empty state for a new bill and a clear error state for unavailable menu data.
- Use semantic table markup, accessible button names for pencil and cross controls, and stable selectors for Selenium tests.
- Use the existing application styling rather than introducing a separate visual theme.
- Handle responsive table behavior without overlapping controls or unreadable content.
- Use a browser-supported or approved PDF mechanism for Print after the architecture stage confirms the dependency and deployment implications.

## 7. Selenium UI Test Requirements

### UI-001: Navigate to Calculate Bill
- Setup: Start the application with menu data available.
- Actions: Open the home page and select `Calculate Bill`.
- Expected: The right content section displays `Generating Bill`, `Select Item:`, and `Estimated Bill:`.
- Testability: Provide stable accessible names or `data-testid` selectors for the link and page sections.

### UI-002: Verify initial bill state
- Setup: Open Calculate Bill.
- Actions: Inspect the Estimated Bill section.
- Expected: The table has no item rows and total is zero; Half is selected by default.

### UI-003: Add a bill line and calculate amount
- Setup: Menu contains Regular Coffee with Half price 20 Rs.
- Actions: Select Regular Coffee, leave Half selected, choose quantity 5, and select Add To Bill.
- Expected: A row shows Regular Coffee, Half, quantity 5, price 20 Rs, amount 100 Rs, and the total is 100 Rs.

### UI-004: Add multiple lines and recalculate total
- Setup: At least two valid menu selections are available.
- Actions: Add two or more lines.
- Expected: Every line is displayed and the total equals the sum of their amounts.

### UI-005: Edit quantity and recalculate
- Setup: The bill contains the specified Regular Coffee line with quantity 5 and amount 100 Rs.
- Actions: Select its pencil control and change quantity to 4.
- Expected: Quantity changes to 4, amount changes to 80 Rs, and the total reflects the new amount.

### UI-006: Remove a line and recalculate
- Setup: The bill contains the specified Regular Coffee line with amount 100 Rs.
- Actions: Select its cross control.
- Expected: The row is removed and the total deducts 100 Rs.

### UI-007: Cancel Generate Bill confirmation
- Setup: The bill contains one or more lines.
- Actions: Select Generate Bill and cancel confirmation.
- Expected: The Calculate Bill page remains visible and all lines and total are preserved.

### UI-008: Confirm Generate Bill and start new bill
- Setup: The bill contains one or more lines.
- Actions: Select Generate Bill, confirm, inspect final bill, then select Generate New Bill.
- Expected: The final page contains the same lines, total, cafe name, Print button, and Generate New Bill button. Generate New Bill returns to an empty Calculate Bill page with total zero.

### UI-009: Print final bill
- Setup: A bill has been generated.
- Actions: Select Print.
- Expected: A PDF download is initiated and contains the cafe name, bill rows, and total. Exact filename and PDF inspection approach require confirmation.

### UI-010: Cancel and confirm Discard Bill
- Setup: The bill contains one or more lines.
- Actions: Select Discard Bill and cancel; verify preservation. Repeat and confirm.
- Expected: Cancellation preserves the bill. Confirmation clears all lines, resets total to zero, and leaves a fresh Calculate Bill page.

### UI-011: Validate invalid and corner cases
- Setup: Use an empty menu, unavailable menu endpoint, invalid quantity, unknown item, repeated selection, and zero/negative quantity where the controls or API permit it.
- Expected: The application prevents invalid additions or displays controlled errors, does not produce negative/incorrect totals, and preserves valid existing bill lines where practical.

### UI-012: Verify responsive and accessible interaction
- Setup: Use supported desktop and mobile viewport sizes.
- Actions: Navigate through add, edit, remove, confirmation, and final-bill workflows using visible controls and accessible names.
- Expected: No incoherent overlap occurs, table content remains usable, and icon-only controls are discoverable by assistive technology.

## 8. Non-Functional Requirements
- NFR-001: The workflow shall be locally hostable within the existing .NET 8 application.
- NFR-002: Bill calculations shall be deterministic, decimal-safe, and consistent across UI, API, and any final PDF.
- NFR-003: Client input shall be validated at the API boundary; database values shall be authoritative for prices.
- NFR-004: The UI shall preserve the current application theme and remain readable at supported viewport sizes.
- NFR-005: Interactive controls shall have semantic labels or accessible names, especially pencil, cross, Print, and confirmation actions.
- NFR-006: The workflow shall provide controlled loading, empty, validation, and failure states.
- NFR-007: PDF generation shall not expose unrelated application data and shall be compatible with the supported local deployment environment.
- NFR-008: Selenium tests shall use stable selectors and avoid depending on presentation-only DOM details.

## 9. Traceability Matrix

| Requirement | C# backend | SQL database | TypeScript/UI | Selenium UI |
|---|---|---|---|---|
| FR-001, FR-002 | Route/view integration | N/A | Calculate Bill view | UI-001, UI-002 |
| FR-003, FR-004, FR-005 | Menu/price read and validation | `dbo.MenuItem` source | Select controls | UI-002, UI-003, UI-011 |
| FR-006, FR-007, FR-011 | Shared calculation service | Trusted price lookup | Line and total rendering | UI-003, UI-004 |
| FR-008, FR-009, FR-010 | Bill state or calculation endpoint | Optional persistence only | Table/edit/remove controls | UI-005, UI-006 |
| FR-012, FR-015, FR-016 | Optional finalization/state endpoint | Optional draft/final tables | Confirmation and state preservation | UI-007, UI-010 |
| FR-013, FR-014 | Final-bill/PDF support as approved | Historical prices if persisted | Final view and actions | UI-008, UI-009 |
| NFR-001, NFR-002, NFR-003 | .NET hosting and validation | Data integrity | Consistent calculation display | UI-003, UI-011 |
| NFR-004, NFR-005, NFR-006, NFR-008 | Error contracts | Availability | Theme, responsive, accessible states | UI-011, UI-012 |

## 10. Assumptions, Dependencies, and Open Questions

### Confirmed facts
- US-004 is the source of the Calculate Bill requirements.
- The current application is a .NET 8 C# application with a TypeScript frontend.
- The current menu model contains item name, portion, price, and creation metadata.
- Existing menu records are read from SQL Server through `MenuService`.
- The home page already exposes a `Calculate Bill` link that is currently a placeholder workflow.

### Assumptions
- A bill is initially transient and need not be persisted unless a later design decision requires it.
- The cafe name displayed on the final bill is the existing application cafe name, `Musafir Cafe`.
- Existing menu rows with Half and Full portions are the source of portion-specific prices.
- Bill lines capture the selected item and price for the current bill; price-change behavior requires confirmation.
- The browser session is the initial scope for preserving a bill while the user uses the workflow.

### Dependencies
- Existing `MenuService` and menu API behavior.
- SQL Server availability and seeded `dbo.MenuItem` data.
- The existing home-page DOM and stylesheet conventions.
- A selected PDF-generation strategy compatible with .NET 8 and local hosting.
- Selenium test environment and a deterministic test database/menu fixture.

### Open questions
- What exact quantity values and maximum quantity should be available?
- Should duplicate item/portion additions merge, replace, or create additional rows?
- Does every portion have an independent database price, and should the user see the unit price or portion price?
- Is bill state client-only, server-side draft state, or persisted as a final bill?
- What should happen when a selected menu row is removed or repriced before finalization?
- What are the exact confirmation messages and empty-bill rules?
- What PDF library or browser print-to-PDF contract is approved, and what filename/currency format is required?
- Should browser refresh preserve the current bill?

## 11. Acceptance Criteria
- AC-001: Selecting `Calculate Bill` from the home page opens a right-section page titled `Generating Bill` with `Select Item:` and `Estimated Bill:` sections.
- AC-002: A new Calculate Bill page starts with an empty table, total zero, an item dropdown sourced from the existing menu, Half selected, and a quantity dropdown.
- AC-003: Adding a valid item creates a row with Item, Portion, Quantity, Price, and Amount, and updates the total using database pricing and quantity.
- AC-004: The specified Regular Coffee example produces amount 100 Rs for Half at price 20 Rs and quantity 5.
- AC-005: Editing the specified line from quantity 5 to 4 changes its amount from 100 Rs to 80 Rs and updates the total.
- AC-006: Removing a line removes it from the table and updates the total accordingly.
- AC-007: The table contains accessible pencil and cross controls for editing and removing each line.
- AC-008: Generate Bill asks for confirmation; cancellation preserves the current Calculate Bill contents, and confirmation displays the final bill.
- AC-009: The final bill displays the cafe name, the estimated bill data, a Print action that downloads the approved PDF format, and a Generate New Bill action.
- AC-010: Generate New Bill returns to an empty Calculate Bill page with total zero.
- AC-011: Discard Bill asks for confirmation; cancellation preserves the current bill, and confirmation clears it and returns to a fresh Calculate Bill page.
- AC-012: Invalid selections, invalid quantities, unavailable menu data, and calculation failures are handled with controlled user-facing feedback and do not silently create incorrect totals.
- AC-013: The workflow preserves the existing application theme and remains usable at supported desktop and mobile viewport sizes.
- AC-014: Selenium integration and functional tests cover positive, negative, and corner cases required by the definition of done.
