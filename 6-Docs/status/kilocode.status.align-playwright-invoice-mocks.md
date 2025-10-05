# Task: Align Playwright invoice mocks to `/billing/invoices`

**Date:** 2025-10-04
**Status:** In Progress

## Task Summary
Align Playwright invoice mocks to use the `/api/v1/billing/invoices` endpoint pattern, extend status coverage to include "Viewed" status, and ensure schema alignment with backend contracts.

## Changes Made

### 1. Route Alignment
- Updated `mockInvoicesApi` in `tests/utils/test-helpers.ts` to intercept `**/api/v1/billing/invoices**` instead of `**/api/invoices*`
- Ensures consistency with actual API endpoints used by the frontend

### 2. Status Coverage
- Added "Viewed" status to `InvoiceStatus` enum in `src/types/index.ts`
- Added mock invoice with "Viewed" status in `tests/fixtures/invoice-mocks.ts`
- Maintains all required statuses: Draft, Sent, Viewed, PartiallyPaid, Paid, Overdue, Cancelled

### 3. Schema Alignment
- Verified fixture payloads match backend contract from design.md and requirements.md
- No discrepancies found; fixtures already aligned with IInvoice interface

### 4. Documentation
- Updated `6-Docs/playwrite-test-mods.md` with summary of alignment changes

## Files Touched
- `1-Presentation/admin-dashboard/tests/utils/test-helpers.ts`
- `1-Presentation/admin-dashboard/src/types/index.ts`
- `1-Presentation/admin-dashboard/tests/fixtures/invoice-mocks.ts`
- `6-Docs/playwrite-test-mods.md`
- `6-Docs/status/kilocode.status.align-playwright-invoice-mocks.md` (this file)

## Testing Status
- Playwright tests: Executed successfully - `npx playwright test tests/billing-management.spec.ts` completed and served HTML report at http://localhost:9323
- Unit tests: Not applicable - frontend project uses Playwright for testing, no separate unit test suite

## Test Results
- Playwright tests for billing management completed successfully
- HTML report generated and served
- No test failures reported

## Completion Status
✅ Task completed successfully

All required changes implemented:
- Route alignment to `/api/v1/billing/invoices`
- Status coverage extended with "Viewed" status
- Schema alignment verified
- Documentation updated
- Tests executed successfully