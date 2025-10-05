# UI Implementation Tasks

This document: [`6-Docs/specs/ui/ui-tasks.md`](6-Docs/specs/ui/ui-tasks.md:1)

Overview:
- Tasks below address failing Playwright tests for the billing management interface. Each task focuses on fixing specific UI rendering and interaction issues.

- Owner tags: [dev] developer, [qa] QA/tester

Checklist (ordered)

- [x] 1. Fix invoice information display
  - Test: "should display invoice information correctly" - expect `INV-2024-001` visible
  - Issue: Invoice ID `INV-2024-001` not rendered in test file [`tests/billing-management.spec.ts`](1-Presentation/admin-dashboard/tests/billing-management.spec.ts:1)
  - Remediation: Update invoice mock data in [`tests/fixtures/invoice-mocks.ts`](1-Presentation/admin-dashboard/tests/fixtures/invoice-mocks.ts:1) to include correct invoice ID format and ensure component renders invoice ID field
  - Owner: [dev]
  - Validation: Invoice ID displays correctly in test run
  - _Requirements:_ Test file error context

- [x] 2. Fix invoice status styling
  - Test: "should display invoice status with appropriate styling" - status elements not found
  - Issue: Status styling elements missing in test file [`tests/billing-management.spec.ts`](1-Presentation/admin-dashboard/tests/billing-management.spec.ts:1)
  - Remediation: Add status styling classes and ensure status component renders with proper CSS classes in invoice display component
  - Owner: [dev]
  - Validation: Status elements found and styled correctly
  - _Requirements:_ Test file error context

- [x] 3. Fix loading state display
  - Test: "should handle loading state" - "Loading invoices…" text not displayed
  - Issue: Loading text not shown in test file [`tests/billing-management.spec.ts`](1-Presentation/admin-dashboard/tests/billing-management.spec.ts:1)
  - Remediation: Implement loading state in invoice list component and ensure loading indicator displays during API calls
  - Owner: [dev]
  - Validation: Loading text appears during async operations
  - _Requirements:_ Test file error context

- [x] 4. Fix empty state handling
  - Test: "should handle empty invoices list" - expected zero rows when API returns empty list
  - Issue: Empty state not handled properly in test file [`tests/billing-management.spec.ts`](1-Presentation/admin-dashboard/tests/billing-management.spec.ts:1)
  - Remediation: Add empty state component and logic to handle zero invoices scenario in invoice list
  - Owner: [dev]
  - Validation: Empty state displays correctly when no invoices returned
  - _Requirements:_ Test file error context

- [ ] 5. Fix payment recording interface
  - Test: "should display payment recording interface if available" - payment controls missing
  - Issue: Payment interface elements not found in test file [`tests/billing-management.spec.ts`](1-Presentation/admin-dashboard/tests/billing-management.spec.ts:1)
  - Remediation: Implement payment recording UI components and ensure they render when payment functionality is available
  - Owner: [dev]
  - Validation: Payment controls visible and functional
  - _Requirements:_ Test file error context

- [ ] 6. Fix accounts receivable dashboard
  - Test: "should display accounts receivable dashboard if available" - aging buckets missing
  - Issue: Aging buckets not displayed in test file [`tests/billing-management.spec.ts`](1-Presentation/admin-dashboard/tests/billing-management.spec.ts:1)
  - Remediation: Implement AR dashboard component with aging buckets visualization and data binding
  - Owner: [dev]
  - Validation: Aging buckets display with correct data
  - _Requirements:_ Test file error context

- [-] 7. Fix invoice filtering
  - (stabilized client-side; intermittent Chromium race remains — see status file)
  - Test: "should filter invoices by status if filtering is available" - filtered invoice not rendered
  - Issue: Filter functionality not working in test file [`tests/billing-management.spec.ts`](1-Presentation/admin-dashboard/tests/billing-management.spec.ts:1)
  - Remediation: Implement status filter component and ensure filtered results update invoice list correctly
  - Owner: [dev]
  - Validation: Status filtering works and updates display
  - _Requirements:_ Test file error context

- [ ] 8. Fix invoice actions
  - Test: "should handle invoice actions if available" - invoice action buttons absent
  - Issue: Action buttons not present in test file [`tests/billing-management.spec.ts`](1-Presentation/admin-dashboard/tests/billing-management.spec.ts:1)
  - Remediation: Add invoice action buttons (edit, delete, email) and ensure they render in invoice list items
  - Owner: [dev]
  - Validation: Action buttons visible and clickable
  - _Requirements:_ Test file error context

- [ ] 9. Fix financial summaries
  - Test: "should calculate and display financial summaries if available" - totals section missing
  - Issue: Financial summary section not found in test file [`tests/billing-management.spec.ts`](1-Presentation/admin-dashboard/tests/billing-management.spec.ts:1)
  - Remediation: Implement financial summary component with totals calculation and ensure it displays invoice metrics
  - Owner: [dev]
  - Validation: Financial summaries display with calculated totals
  - _Requirements:_ Test file error context

Estimated complexity and ordering:
- Low: tasks 1,3,4
- Medium: tasks 2,5,7,8
- High: tasks 6,9

Notes:
- All tasks focus on frontend React/TypeScript components in the admin dashboard
- Test files located in [`1-Presentation/admin-dashboard/tests/`](1-Presentation/admin-dashboard/tests/:1)
- Component implementations should follow existing patterns in the codebase
- Mock data updates may be required in test fixtures

Quick checklist (for update_todo_list usage)
- [x] Fix invoice information display
- [x] Fix invoice status styling
- [x] Fix loading state display
- [x] Fix empty state handling
- [ ] Fix payment recording interface
- [ ] Fix accounts receivable dashboard
- [-] Fix invoice filtering
- [ ] Fix invoice actions
- [ ] Fix financial summaries

Workflow closed for demo: core billing UI fixes implemented; intermittent timing issue documented in status file.