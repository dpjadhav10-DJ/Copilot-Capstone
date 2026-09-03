# System Architecture: US-003

## 1. Source and Summary
- User story reference: US-003, Implementation of Contact Us Page
- Source requirement analysis: `UserStories/US-003/US-003-RequirementAnalysis.md`
- Solution summary: Extend the existing .NET 8 and TypeScript single-page cafe application with a static Contact Us view rendered in the existing right content panel.
- Architecture objective: Reuse the current home-page shell and styling while replacing the placeholder Contact Us URL with an in-application view that exposes exact contact information and safe new-tab social links.

## 2. Scope

### In scope
- Home-page navigation from `Contact Us` to the right content section.
- Static rendering of `Find us At`, `Reach us at:`, and `Connect us at:`.
- Exact supplied address, phone numbers, Facebook URL, and Instagram URL.
- Accessible Facebook and Instagram logo controls.
- New-tab external navigation with `noopener` protection.
- Responsive styling and Selenium integration coverage.

### Out of scope
- Contact information administration or database persistence.
- New C# endpoints, services, models, SQL tables, migrations, or seed data.
- User authentication, contact forms, messaging, analytics, or external API calls.
- Changes to the existing cafe-story, menu, bill, or location workflows beyond preserving their navigation integration.

### Constraints and decisions
- Use the existing `storyPanel` content-region ownership in `src/main.ts`.
- Follow the existing hash-link convention; `Contact Us` will use `#contact-us` and its click handler will render the view without a full-page reload.
- Keep contact values as source-controlled constants in the TypeScript view implementation so the exact story-supplied values are not transformed by a service or database mapping.
- Use text-based logo marks with accessible labels unless the project already introduces an approved icon library before implementation. The architecture does not require a new dependency for two simple external-link controls.
- Treat phone numbers as visible text. Clickable `tel:` behavior remains outside this story unless explicitly approved during design review.

## 3. Component Architecture

```text
Browser
  |
  v
Home page navigation (#contact-us)
  |
  v
TypeScript Contact View renderer
  |
  v
Existing storyPanel content region <----> Existing page shell and styles
  |
  +--> Facebook link (new tab, noopener/noreferrer)
  +--> Instagram link (new tab, noopener/noreferrer)

Selenium WebDriver --> Browser --> Contact View
```

Responsibilities:
- HTML shell: supplies the navigation anchor and existing content region.
- TypeScript: intercepts Contact Us navigation, renders semantic contact markup, and owns the exact static values.
- CSS: supplies layout and typography consistent with the current story panel; add only narrowly scoped Contact Us rules if needed.
- Browser: opens external social destinations in a separate browsing context according to the link target.
- Selenium: verifies navigation, exact content, link attributes/destinations, repeat navigation, accessibility names, and responsive layout.

## 4. Presentation and Navigation Design

### Home-page integration
- Change the current `Contact Us` anchor from the placeholder external URL to `href="#contact-us"`.
- Add an `id` and `data-testid` consistent with existing navigation selectors; preserve the existing `data-testid="nav-contact"` contract.
- Register a click handler beside the existing menu and calculate-bill handlers.
- Prevent the browser's default hash jump and invoke `showContactUs()` so the existing header/navigation remain visible and only the right panel changes.

### Contact view structure
The renderer will replace `storyPanel.innerHTML` with semantic markup equivalent to:
- A section kicker identifying contact information.
- An `h2` titled `Find us At`.
- A `section` titled `Reach us at:` containing address and phone values.
- A `section` titled `Connect us at:` containing two links with recognizable logo marks and accessible names.

Use stable hooks for browser tests, including a content-region test id, heading ids, reach/connect section ids, and explicit Facebook/Instagram link test ids. Exact class names may follow existing styles rather than becoming test contracts.

### External links
- Facebook href: `https://www.facebook.com/BeMusafir`.
- Instagram href: `https://www.instagram.com/BeMusafir`.
- Both links use `target="_blank"` and `rel="noopener noreferrer"`.
- Logo controls must expose accessible names such as `Facebook` and `Instagram`; visible text may supplement the logo to remain understandable if an icon does not load.
- Tests should inspect target and href configuration without requiring external network access.

## 5. Data Flow

### Contact navigation flow
1. The user selects `Contact Us` in the home-page navigation.
2. The click handler prevents full-page navigation and calls `showContactUs()`.
3. The renderer replaces the content of `storyPanel` with static semantic contact markup.
4. The existing page shell, header, navigation, and stylesheet remain mounted.
5. Selecting another supported navigation item replaces the same panel; selecting Contact Us again renders a fresh equivalent view.

### Failure behavior
- There is no network request or persistence operation for this static view.
- A missing content panel causes the renderer to return without disrupting the rest of the page, matching the defensive pattern used by existing view functions.
- No raw exception or implementation detail is shown to users.

## 6. Backend and Database Design

### C# backend
- No endpoint, controller/minimal API route, service, model, configuration, or dependency change is required.
- Existing static-file hosting and fallback routing continue to serve the application.

### SQL database
- No table, column, constraint, index, migration, or seed change is required.
- Existing cafe-story, menu, and bill database behavior remains untouched.

## 7. TypeScript Implementation Design
- Add a `showContactUs()` function near the existing content-view functions.
- Use DOM construction or a carefully escaped static template for the supplied constants; do not interpolate user-controlled data.
- Preserve existing `storyPanel` ownership and view replacement behavior.
- Add the Contact Us query and event registration at the bottom of the file alongside `menuLink` and `calculateBillLink`.
- Keep social hrefs literal and exact; no URL normalization or external fetch is needed.
- Do not alter bill state, menu state, or cafe-story loading state when Contact Us is opened.

## 8. Styling Design
- Reuse `.story-panel`, heading, kicker, and existing color variables.
- Add a small contact layout only if the existing flow needs spacing or responsive alignment; avoid introducing a new page-wide theme.
- Keep sections readable on narrow screens by using normal flow and wrapping long address/URL-related content where necessary.
- Ensure link focus states remain visible and align with the existing navigation focus treatment.
- Do not add decorative assets or a new font/dependency for this content view.

## 9. Selenium UI Testing Design

### Critical journeys
- Navigate from home to Contact Us and verify the right panel changes.
- Verify exact title, section headings, address, and both phone numbers.
- Verify Facebook and Instagram accessible names, exact hrefs, `_blank` targets, and safe `rel` values.
- Navigate to another existing view and back to Contact Us to verify repeatability.
- Check desktop and mobile layouts for readable, non-overlapping content and usable focusable links.

### Test isolation and external systems
- Tests run against the local application only.
- Do not depend on Facebook or Instagram availability; inspect link attributes and optionally observe the new window target.
- External tabs/windows must be closed or switched back in test cleanup so tests remain independent.

### Selector strategy
- Retain `data-testid="nav-contact"`.
- Add stable test ids for the Contact Us content region, title, reach-us section, connect-us section, Facebook link, and Instagram link.
- Prefer semantic heading and link queries where the Selenium test project supports them; avoid selectors based on CSS presentation details.

## 10. Risks, Dependencies, and Open Questions

### Risks and mitigations
- Risk: External-link tests become network-dependent. Mitigation: assert href, target, rel, and accessible names locally; treat platform availability as out of scope.
- Risk: A full-page link change breaks the existing content-panel workflow. Mitigation: retain the home shell and use the established click-handler pattern.
- Risk: Logo-only controls are unclear to assistive technology. Mitigation: provide explicit accessible names and stable semantic links.

### Dependencies
- Existing `index.html`, `src/main.ts`, and `styles.css` conventions.
- Existing TypeScript build process that emits `wwwroot/main.js`.
- Existing Selenium project and local application startup.

### Open questions carried forward
- Whether phone numbers should become `tel:` links.
- Whether the project has an approved icon library or specific logo assets to use.
- Whether contact details should eventually be configurable; that future change would require a new architecture review.