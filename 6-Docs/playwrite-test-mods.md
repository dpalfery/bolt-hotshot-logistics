# Playwright Test Modification Proposals

## Billing Management Styling Test (`billing-management-Billing-0e7ff-us-with-appropriate-styling-chromium`)

### Observed Behavior
- Table headers render, but no invoice rows appear during the Playwright run.
- UI component `BillingManagement` calls `apiService.getInvoices()` which targets `/billing/invoices`.
- Playwright mock in `tests/billing-management.spec.ts` intercepts `**/api/invoices*`, so the real fetch `/billing/invoices` is never mocked.

### Proposed Test Update
1. Update the `page.route` pattern from `**/api/invoices*` to `**/billing/invoices*`.
2. Retain the mocked payload structure (`{ items, totalCount }`) since it already matches the UI expectations.

### Rationale
- Aligns the mock endpoint with the production API path, allowing invoice rows to render during the test.
- No UI changes required; the user interface functions correctly with actual data.

### Next Steps
- Await approval before modifying the test file.
- Once approved, update the mock route and re-run the targeted Playwright test to confirm rows render as expected.

## Invoice Mocks Alignment to `/billing/invoices` API

### Changes Made
1. **Route Alignment**: Updated `mockInvoicesApi` in `test-helpers.ts` to intercept `**/api/v1/billing/invoices**` instead of `**/api/invoices*`.
2. **Status Coverage**: Added "Viewed" status to `InvoiceStatus` enum and included a mock invoice with this status.
3. **Schema Alignment**: Verified fixture payloads match backend contract from design.md and requirements.md.

### Rationale
- Aligns Playwright mocks with the actual API endpoints used by the frontend.
- Ensures comprehensive status coverage for testing invoice workflows.
- Maintains consistency between test fixtures and production data structures.

### Files Modified
- `1-Presentation/admin-dashboard/tests/utils/test-helpers.ts`
- `1-Presentation/admin-dashboard/src/types/index.ts`
- `1-Presentation/admin-dashboard/tests/fixtures/invoice-mocks.ts`