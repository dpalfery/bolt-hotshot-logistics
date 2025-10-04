# Admin Dashboard UI - Follow-up Task Spec

## Context
Reviewed and aligned with: [requirements.md](6-Docs/specs/hotshot-logistics-platform/requirements.md:1), [design.md](6-Docs/specs/hotshot-logistics-platform/design.md:1), [architecture.md](.kilicode/rules/memory-bank/architecture.md:1), [tech.md](.kilocode/rules/memory-bank/tech.md:1). Tasks below use Clean Architecture terminology (Presentation layer) and adhere to security rules (no secrets, authenticated requests, parameterized queries on server) and testing requirements.

### Notes
- Presentation/UI work must not bypass Authorization; rely on MSAL and protected APIs per [User Authentication and Authorization requirements](6-Docs/specs/hotshot-logistics-platform/requirements.md:99) and [Security Design](6-Docs/specs/hotshot-logistics-platform/design.md:785).
- All UI behaviors require automated tests (Playwright E2E, unit tests) per [Testing standards](.kilicode/rules/memory-bank/tech.md:15) and [testing-general-rule.md](.kilocode/rules/testing-general-rule.md:1).

## 1. Align Playwright invoice mocks to `/billing/invoices`

#### Objective
Update Playwright network mocks/fixtures so tests intercept and respond to the billing invoices collection route consistently with API design and domain model.

#### Acceptance Criteria
1. WHEN Playwright tests run for the Invoices page THEN requests to GET /api/v1/billing/invoices SHALL be intercepted and mocked with a payload conforming to [IInvoice](6-Docs/specs/hotshot-logistics-platform/design.md:154).
2. IF tests filter by status query (e.g., ?status=overdue) THEN the mock SHALL return only invoices whose Status matches [InvoiceStatus states](6-Docs/specs/hotshot-logistics-platform/requirements.md:246).
3. WHEN requesting a single invoice GET /api/v1/billing/invoices/{id} THEN the mock SHALL return a single object with fields InvoiceNumber, CustomerId, JobId, Status, DueDate, TotalAmount matching [Invoices table fields](6-Docs/specs/hotshot-logistics-platform/design.md:444).
4. WHEN network mocking is active THEN no external network calls SHALL be performed by tests (offline deterministic tests).

#### Spec References
- API endpoints: [design.md /billing](6-Docs/specs/hotshot-logistics-platform/design.md:323)
- Domain model: [design.md IInvoice](6-Docs/specs/hotshot-logistics-platform/design.md:154)
- Billing and invoice management: [requirements.md §16](6-Docs/specs/hotshot-logistics-platform/requirements.md:236)

#### Dependencies and Open Questions
- Confirm list endpoint naming: design specifies POST /billing/invoices and GET /billing/invoices/{id}. For UI list mocks, adopt GET /billing/invoices with pagination and status query; confirm with backend or extend API spec accordingly.
- Ensure test data includes multiple statuses (Draft, Sent, Paid, Overdue, Cancelled) per [requirements](6-Docs/specs/hotshot-logistics-platform/requirements.md:246).

#### Testing Notes
- Implement Playwright route interception for /api/v1/billing/invoices*; place fixtures under a test-only path and avoid secrets. Assert no unexpected network requests.

## 2. Implement billing status filter UI

#### Objective
Create a status filter component on the Invoices page that queries and displays invoices by status with accessible controls and URL synchronization.

#### Acceptance Criteria
1. WHEN a user selects one or more statuses (Draft, Sent, Paid, Overdue, Cancelled) THEN the UI SHALL request /api/v1/billing/invoices?status=... and update the list.
2. IF no status is selected THEN the UI SHALL default to showing all statuses with server-side pagination.
3. WHEN the page is reloaded with status in the query string THEN the filter controls SHALL reflect the URL state and results SHALL be filtered accordingly.
4. WHEN the filter changes THEN the URL SHALL update without full-page reload to preserve shareable state.
5. IF the API returns 401/403 THEN the UI SHALL redirect to login or show an authorization message per RBAC; sensitive details SHALL NOT be disclosed.

#### Spec References
- Invoice status states: [requirements.md §16.5](6-Docs/specs/hotshot-logistics-platform/requirements.md:246)
- API endpoints: [design.md /billing](6-Docs/specs/hotshot-logistics-platform/design.md:323)
- Frontend tech (React Query for server state): [tech.md Frontend](.kilicode/rules/memory-bank/tech.md:15)

#### Dependencies and Open Questions
- Requires backend list endpoint support for status, page, pageSize; if absent, rely on mocks until implemented.
- Requires authenticated HTTP client via MSAL per [tech.md](.kilicode/rules/memory-bank/tech.md:25).

#### Testing Notes
- Playwright: assert network request params; unit tests: component behavior, accessibility (keyboard focus, labels, ARIA).

## 3. Diagnose billing invoice data loading (API integration/auth)

#### Objective
Ensure the Invoices page integrates with the secured API using MSAL, handles token refresh, and renders data states (loading, empty, error) robustly.

#### Acceptance Criteria
1. WHEN an authenticated user opens Invoices THEN the UI SHALL call /api/v1/billing/invoices with Authorization header (Bearer {token}).
2. IF the token is expired THEN the client SHALL refresh/renew and retry once before surfacing an error.
3. WHEN the API responds 200 with a valid payload THEN the UI SHALL render rows with InvoiceNumber, Customer, Status, DueDate, Total.
4. IF the API responds 401/403 THEN the UI SHALL route to login or show access denied per role policies; no sensitive error details SHALL be shown.
5. WHEN the API returns an error (>=500) THEN the UI SHALL show a retry affordance and log a sanitized event to telemetry.

#### Spec References
- AuthN/AuthZ requirements: [requirements.md §7](6-Docs/specs/hotshot-logistics-platform/requirements.md:99)
- Security design and headers: [design.md Security](6-Docs/specs/hotshot-logistics-platform/design.md:785)
- API gateway and endpoints: [design.md API structure](6-Docs/specs/hotshot-logistics-platform/design.md:290)

#### Dependencies and Open Questions
- Confirm API base URL and CORS config (should be restrictive; no wildcard origins).
- Ensure telemetry uses correlation IDs and sanitized properties per security rules.

#### Testing Notes
- Playwright: simulate 401/403/500 responses; verify redirect, messaging, and retry. Unit tests for data-state components.

## 4. Align dashboard statistics mocks/selectors with actual endpoints

#### Objective
Update dashboard Playwright mocks and UI selectors to match GET /api/v1/analytics/dashboard response contract and verify KPIs render correctly.

#### Acceptance Criteria
1. WHEN dashboard loads THEN a mocked GET /api/v1/analytics/dashboard SHALL return metrics for active jobs, driver utilization, delivery performance.
2. WHEN the response arrives THEN the UI SHALL render metric cards with stable data-testid attributes used by tests.
3. IF metrics are unavailable THEN the UI SHALL show a non-blocking fallback with retry.
4. WHEN data updates in real time THEN SignalR-driven updates SHALL re-render metrics within 1 second (mocked for tests).

#### Spec References
- Analytics requirements: [requirements.md §6.1](6-Docs/specs/hotshot-logistics-platform/requirements.md:90)
- Analytics endpoint: [design.md /analytics/dashboard](6-Docs/specs/hotshot-logistics-platform/design.md:335)
- Cache key (analytics:dashboard): [design.md Redis schema](6-Docs/specs/hotshot-logistics-platform/design.md:566)

#### Dependencies and Open Questions
- Define the exact response schema for /analytics/dashboard (names/units). Align UI and mocks accordingly.
- Ensure selectors (data-testid) are agreed to and stable to avoid brittle tests.

#### Testing Notes
- Playwright: mock dashboard endpoint; assert text content and aria/role semantics on KPI cards.

## 5. Implement dashboard Recent Jobs section

#### Objective
Build a Recent Jobs widget showing the N most recent jobs with status, customer, and scheduled/delivery times; link to job details.

#### Acceptance Criteria
1. WHEN dashboard loads THEN the UI SHALL call GET /api/v1/jobs?sort=createdAt&order=desc&pageSize=N and render latest jobs.
2. WHEN a JobStatusUpdated event is received via SignalR THEN the corresponding row SHALL update within 1 second.
3. IF no recent jobs exist THEN the widget SHALL show an empty state with a “Create Job” action.
4. WHEN a row is clicked THEN the UI SHALL navigate to the job details route.

#### Spec References
- Job management requirements (filters/search, real-time updates): [requirements.md §1.7–1.9, §1.5](6-Docs/specs/hotshot-logistics-platform/requirements.md:21)
- Jobs endpoint: [design.md /jobs](6-Docs/specs/hotshot-logistics-platform/design.md:298)
- Real-time event: [design.md IRealtimeHub.JobStatusUpdated](6-Docs/specs/hotshot-logistics-platform/design.md:346)

#### Dependencies and Open Questions
- Confirm job list query params and sorting. Establish DTO shape for widget (lightweight projection).
- Mock SignalR events in tests; ensure no external sockets during CI.

#### Testing Notes
- Playwright: mock list + SignalR; RTL unit tests for rendering logic and navigation.

## 6. Add “View All Jobs” navigation link

#### Objective
Add a “View All Jobs” link in the dashboard header or Recent Jobs widget that routes to the full Jobs list.

#### Acceptance Criteria
1. WHEN a user clicks “View All Jobs” THEN the UI SHALL navigate to the Jobs page route without full reload.
2. WHEN navigated THEN initial query params SHALL be preserved (e.g., date range defaults).
3. IF the user lacks permission THEN the UI SHALL show access denied per RBAC.

#### Spec References
- AuthZ roles: [requirements.md §7.4–7.5](6-Docs/specs/hotshot-logistics-platform/requirements.md:108)
- Jobs endpoint and navigation context: [design.md /jobs](6-Docs/specs/hotshot-logistics-platform/design.md:298)

#### Dependencies and Open Questions
- Confirm route path for Jobs page (e.g., /dashboard/jobs).

#### Testing Notes
- Playwright: click-through navigation test; verify URL and content render; negative test for forbidden role.

## 7. Implement dashboard Quick Actions buttons

#### Objective
Provide prominent Quick Actions on the dashboard: “Create Job”, “Assign Driver”, “Create Invoice”.

#### Acceptance Criteria
1. WHEN a user clicks “Create Job” THEN the UI SHALL navigate to the job creation form route.
2. WHEN a user clicks “Assign Driver” THEN the UI SHALL navigate to job list with pre-filter to unassigned or open assignment modal.
3. WHEN a user clicks “Create Invoice” THEN the UI SHALL navigate to invoice creation screen (custom or from completed job).
4. IF the user role lacks permission for any action THEN the button SHALL be hidden or disabled with tooltip explaining required role.

#### Spec References
- Job service capabilities: [design.md IJobService](6-Docs/specs/hotshot-logistics-platform/design.md:234)
- Billing service capabilities: [design.md IBillingService](6-Docs/specs/hotshot-logistics-platform/design.md:248)
- AuthZ roles: [requirements.md §7.4](6-Docs/specs/hotshot-logistics-platform/requirements.md:108)

#### Dependencies and Open Questions
- Confirm routes for create forms; validate RBAC mapping to buttons.

#### Testing Notes
- Playwright: click actions assert navigation and access control behavior; unit tests for conditional rendering by role.

---

## Compliance and Quality Notes
- Security: No secrets in code or tests; authenticated requests only; sanitize logs; restrictive CORS per [security-general-rule.md](.kilocode/rules/security-general-rule.md:1).
- Clean Architecture: UI contains no domain logic; relies on typed API DTOs/contracts and service boundaries per [architecture.md](.kilicode/rules/memory-bank/architecture.md:1).
- Testing: Add/maintain Playwright E2E and RTL unit tests; treat warnings as errors per [code-quality-general-rule.md](.kilicode/rules/code-quality-general-rule.md:1).