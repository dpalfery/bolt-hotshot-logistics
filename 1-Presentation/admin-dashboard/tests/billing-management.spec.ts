import { test, expect } from '@playwright/test';

test.describe('Billing Management', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to billing page
    await page.goto('/billing');
  });

  test.describe('Billing and Invoicing Interface (14.3)', () => {
    test('should display billing management page with header', async ({ page }) => {
      // Check page title and description
      await expect(page.getByText('Billing & Invoicing')).toBeVisible();
      await expect(page.getByText('Manage invoices, payments, and accounts receivable')).toBeVisible();
    });

    test('should display invoices table with proper columns', async ({ page }) => {
      // Check table headers
      await expect(page.getByText('Invoice')).toBeVisible();
      await expect(page.getByText('Customer')).toBeVisible();
      await expect(page.getByText('Status')).toBeVisible();
      await expect(page.getByText('Amount')).toBeVisible();
      await expect(page.getByText('Due Date')).toBeVisible();
    });

    test('should display invoice information correctly', async ({ page }) => {
      // Mock invoices data
      await page.route('**/api/invoices*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              {
                id: 'inv-001',
                invoiceNumber: 'INV-2024-001',
                customerId: 'CUST-001',
                invoiceDate: '2024-01-15T00:00:00Z',
                dueDate: '2024-02-15T00:00:00Z',
                totalAmount: 1500.00,
                paidAmount: 1500.00,
                balanceDue: 0.00,
                status: 'Paid'
              },
              {
                id: 'inv-002',
                invoiceNumber: 'INV-2024-002',
                customerId: 'CUST-002',
                invoiceDate: '2024-01-20T00:00:00Z',
                dueDate: '2024-02-20T00:00:00Z',
                totalAmount: 2500.00,
                paidAmount: 1000.00,
                balanceDue: 1500.00,
                status: 'PartiallyPaid'
              },
              {
                id: 'inv-003',
                invoiceNumber: 'INV-2024-003',
                customerId: 'CUST-003',
                invoiceDate: '2024-01-25T00:00:00Z',
                dueDate: '2024-01-25T00:00:00Z', // Past due
                totalAmount: 800.00,
                paidAmount: 0.00,
                balanceDue: 800.00,
                status: 'Overdue'
              }
            ],
            totalCount: 3
          })
        });
      });

      await page.reload();

      // Check invoice numbers
      await expect(page.getByText('INV-2024-001')).toBeVisible();
      await expect(page.getByText('INV-2024-002')).toBeVisible();
      await expect(page.getByText('INV-2024-003')).toBeVisible();

      // Check customer IDs
      await expect(page.getByText('Customer #CUST-001')).toBeVisible();
      await expect(page.getByText('Customer #CUST-002')).toBeVisible();
      await expect(page.getByText('Customer #CUST-003')).toBeVisible();

      // Check invoice dates are formatted correctly
      await expect(page.getByText('1/15/2024')).toBeVisible();
      await expect(page.getByText('1/20/2024')).toBeVisible();
      await expect(page.getByText('1/25/2024')).toBeVisible();

      // Check amounts
      await expect(page.getByText('$1500.00')).toBeVisible();
      await expect(page.getByText('$2500.00')).toBeVisible();
      await expect(page.getByText('$800.00')).toBeVisible();

      // Check paid amounts
      await expect(page.getByText('Paid: $1500.00')).toBeVisible();
      await expect(page.getByText('Paid: $1000.00')).toBeVisible();
      await expect(page.getByText('Paid: $0.00')).toBeVisible();

      // Check due dates
      await expect(page.getByText('2/15/2024')).toBeVisible();
      await expect(page.getByText('2/20/2024')).toBeVisible();
      await expect(page.getByText('1/25/2024')).toBeVisible();
    });

    test('should display invoice status with appropriate styling', async ({ page }) => {
      // Mock invoices with different statuses
      await page.route('**/api/invoices*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              {
                id: 'inv-draft',
                invoiceNumber: 'INV-DRAFT-001',
                customerId: 'CUST-001',
                invoiceDate: '2024-01-15T00:00:00Z',
                dueDate: '2024-02-15T00:00:00Z',
                totalAmount: 1000.00,
                paidAmount: 0.00,
                balanceDue: 1000.00,
                status: 'Draft'
              },
              {
                id: 'inv-sent',
                invoiceNumber: 'INV-SENT-001',
                customerId: 'CUST-002',
                invoiceDate: '2024-01-16T00:00:00Z',
                dueDate: '2024-02-16T00:00:00Z',
                totalAmount: 1200.00,
                paidAmount: 0.00,
                balanceDue: 1200.00,
                status: 'Sent'
              },
              {
                id: 'inv-partial',
                invoiceNumber: 'INV-PARTIAL-001',
                customerId: 'CUST-003',
                invoiceDate: '2024-01-17T00:00:00Z',
                dueDate: '2024-02-17T00:00:00Z',
                totalAmount: 1500.00,
                paidAmount: 500.00,
                balanceDue: 1000.00,
                status: 'PartiallyPaid'
              },
              {
                id: 'inv-paid',
                invoiceNumber: 'INV-PAID-001',
                customerId: 'CUST-004',
                invoiceDate: '2024-01-18T00:00:00Z',
                dueDate: '2024-02-18T00:00:00Z',
                totalAmount: 800.00,
                paidAmount: 800.00,
                balanceDue: 0.00,
                status: 'Paid'
              },
              {
                id: 'inv-overdue',
                invoiceNumber: 'INV-OVERDUE-001',
                customerId: 'CUST-005',
                invoiceDate: '2024-01-19T00:00:00Z',
                dueDate: '2024-01-20T00:00:00Z',
                totalAmount: 2000.00,
                paidAmount: 0.00,
                balanceDue: 2000.00,
                status: 'Overdue'
              },
              {
                id: 'inv-cancelled',
                invoiceNumber: 'INV-CANCELLED-001',
                customerId: 'CUST-006',
                invoiceDate: '2024-01-20T00:00:00Z',
                dueDate: '2024-02-20T00:00:00Z',
                totalAmount: 600.00,
                paidAmount: 0.00,
                balanceDue: 0.00,
                status: 'Cancelled'
              }
            ],
            totalCount: 6
          })
        });
      });

      await page.reload();

      // Check status styling for each status type
      const draftStatus = page.locator('text=Draft').first();
      await expect(draftStatus).toBeVisible();
      await expect(draftStatus).toHaveClass(/bg-gray-100.*text-gray-800/);

      const sentStatus = page.locator('text=Sent').first();
      await expect(sentStatus).toBeVisible();
      await expect(sentStatus).toHaveClass(/bg-blue-100.*text-blue-800/);

      const partiallyPaidStatus = page.locator('text=PartiallyPaid').first();
      await expect(partiallyPaidStatus).toBeVisible();
      await expect(partiallyPaidStatus).toHaveClass(/bg-yellow-100.*text-yellow-800/);

      const paidStatus = page.locator('text=Paid').first();
      await expect(paidStatus).toBeVisible();
      await expect(paidStatus).toHaveClass(/bg-green-100.*text-green-800/);

      const overdueStatus = page.locator('text=Overdue').first();
      await expect(overdueStatus).toBeVisible();
      await expect(overdueStatus).toHaveClass(/bg-red-100.*text-red-800/);

      const cancelledStatus = page.locator('text=Cancelled').first();
      await expect(cancelledStatus).toBeVisible();
      await expect(cancelledStatus).toHaveClass(/bg-gray-100.*text-gray-800/);
    });

    test('should handle loading state', async ({ page }) => {
      // Mock slow API response
      await page.route('**/api/invoices*', async route => {
        await new Promise(resolve => setTimeout(resolve, 1000));
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [],
            totalCount: 0
          })
        });
      });

      await page.reload();

      // Check loading state
      await expect(page.getByText('Loading invoices...')).toBeVisible();
      
      // Wait for loading to complete
      await expect(page.getByText('Loading invoices...')).not.toBeVisible({ timeout: 2000 });
    });

    test('should handle empty invoices list', async ({ page }) => {
      // Mock empty response
      await page.route('**/api/invoices*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [],
            totalCount: 0
          })
        });
      });

      await page.reload();

      // Verify table structure is still present but no invoice rows
      await expect(page.getByText('Invoice')).toBeVisible();
      await expect(page.getByText('Customer')).toBeVisible();
      await expect(page.getByText('Status')).toBeVisible();
      await expect(page.getByText('Amount')).toBeVisible();
      await expect(page.getByText('Due Date')).toBeVisible();
      
      // Check that no invoice data is displayed
      const tableRows = page.locator('tbody tr');
      await expect(tableRows).toHaveCount(0);
    });

    test('should display invoice generation form if available', async ({ page }) => {
      // Check if there's an invoice generation button
      const generateButton = page.getByRole('button', { name: /generate|create.*invoice/i });
      
      if (await generateButton.isVisible()) {
        await generateButton.click();
        
        // Check if invoice generation form opens
        await expect(page.getByText(/generate.*invoice|create.*invoice/i)).toBeVisible();
        
        // Check for expected form fields
        await expect(page.getByLabel(/customer/i)).toBeVisible();
        await expect(page.getByLabel(/due.*date/i)).toBeVisible();
        await expect(page.getByLabel(/amount|total/i)).toBeVisible();
      }
    });

    test('should display payment recording interface if available', async ({ page }) => {
      // Mock invoice data first
      await page.route('**/api/invoices*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              {
                id: 'inv-001',
                invoiceNumber: 'INV-2024-001',
                customerId: 'CUST-001',
                invoiceDate: '2024-01-15T00:00:00Z',
                dueDate: '2024-02-15T00:00:00Z',
                totalAmount: 1500.00,
                paidAmount: 0.00,
                balanceDue: 1500.00,
                status: 'Sent'
              }
            ],
            totalCount: 1
          })
        });
      });

      await page.reload();

      // Check if there's a payment recording button or interface
      const paymentButton = page.getByRole('button', { name: /record.*payment|add.*payment/i });
      
      if (await paymentButton.isVisible()) {
        await paymentButton.click();
        
        // Check if payment recording form opens
        await expect(page.getByText(/record.*payment|add.*payment/i)).toBeVisible();
        
        // Check for expected form fields
        await expect(page.getByLabel(/amount/i)).toBeVisible();
        await expect(page.getByLabel(/payment.*date/i)).toBeVisible();
        await expect(page.getByLabel(/payment.*method/i)).toBeVisible();
      }
    });

    test('should display accounts receivable dashboard if available', async ({ page }) => {
      // Mock aging report data
      await page.route('**/api/invoices/aging*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            current: 5000.00,
            days30: 2000.00,
            days60: 1000.00,
            days90: 500.00,
            over90: 200.00,
            total: 8700.00
          })
        });
      });

      // Check if accounts receivable section exists
      const arSection = page.getByText(/accounts.*receivable|aging.*report/i);
      
      if (await arSection.isVisible()) {
        // Check for aging buckets
        await expect(page.getByText(/current/i)).toBeVisible();
        await expect(page.getByText(/30.*days/i)).toBeVisible();
        await expect(page.getByText(/60.*days/i)).toBeVisible();
        await expect(page.getByText(/90.*days/i)).toBeVisible();
      }
    });

    test('should filter invoices by status if filtering is available', async ({ page }) => {
      // Mock invoices with filtering support
      await page.route('**/api/invoices*', async route => {
        const url = new URL(route.request().url());
        const status = url.searchParams.get('status');
        
        let items = [
          {
            id: 'inv-paid',
            invoiceNumber: 'INV-PAID-001',
            customerId: 'CUST-001',
            invoiceDate: '2024-01-15T00:00:00Z',
            dueDate: '2024-02-15T00:00:00Z',
            totalAmount: 1000.00,
            paidAmount: 1000.00,
            balanceDue: 0.00,
            status: 'Paid'
          },
          {
            id: 'inv-overdue',
            invoiceNumber: 'INV-OVERDUE-001',
            customerId: 'CUST-002',
            invoiceDate: '2024-01-16T00:00:00Z',
            dueDate: '2024-01-20T00:00:00Z',
            totalAmount: 1500.00,
            paidAmount: 0.00,
            balanceDue: 1500.00,
            status: 'Overdue'
          }
        ];

        // Apply filtering if status parameter exists
        if (status) {
          items = items.filter(invoice => invoice.status === status);
        }

        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items,
            totalCount: items.length
          })
        });
      });

      await page.reload();

      // Verify initial state shows all invoices
      await expect(page.getByText('INV-PAID-001')).toBeVisible();
      await expect(page.getByText('INV-OVERDUE-001')).toBeVisible();

      // Check if filter controls exist
      const statusFilter = page.getByRole('combobox', { name: /status|filter/i });
      
      if (await statusFilter.isVisible()) {
        // Test filtering by paid status
        await statusFilter.selectOption('Paid');
        await expect(page.getByText('INV-PAID-001')).toBeVisible();
        await expect(page.getByText('INV-OVERDUE-001')).not.toBeVisible();
        
        // Test filtering by overdue status
        await statusFilter.selectOption('Overdue');
        await expect(page.getByText('INV-OVERDUE-001')).toBeVisible();
        await expect(page.getByText('INV-PAID-001')).not.toBeVisible();
      }
    });

    test('should handle invoice actions if available', async ({ page }) => {
      // Mock invoice data
      await page.route('**/api/invoices*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              {
                id: 'inv-001',
                invoiceNumber: 'INV-2024-001',
                customerId: 'CUST-001',
                invoiceDate: '2024-01-15T00:00:00Z',
                dueDate: '2024-02-15T00:00:00Z',
                totalAmount: 1500.00,
                paidAmount: 0.00,
                balanceDue: 1500.00,
                status: 'Sent'
              }
            ],
            totalCount: 1
          })
        });
      });

      await page.reload();

      // Check for action buttons (view, edit, send, etc.)
      const viewButton = page.getByRole('button', { name: /view/i });
      const editButton = page.getByRole('button', { name: /edit/i });
      const sendButton = page.getByRole('button', { name: /send/i });
      const downloadButton = page.getByRole('button', { name: /download/i });

      // Test view action if available
      if (await viewButton.isVisible()) {
        await viewButton.click();
        await expect(page.getByText(/invoice.*details|view.*invoice/i)).toBeVisible();
      }

      // Test edit action if available
      if (await editButton.isVisible()) {
        await editButton.click();
        await expect(page.getByText(/edit.*invoice/i)).toBeVisible();
      }
    });

    test('should handle API errors gracefully', async ({ page }) => {
      // Mock API error
      await page.route('**/api/invoices*', async route => {
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Internal server error' })
        });
      });

      await page.reload();

      // Check that error is handled gracefully
      await expect(page.getByText('Billing & Invoicing')).toBeVisible();
    });

    test('should calculate and display financial summaries if available', async ({ page }) => {
      // Mock financial summary data
      await page.route('**/api/invoices/summary*', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            totalInvoiced: 50000.00,
            totalPaid: 35000.00,
            totalOutstanding: 15000.00,
            overdueAmount: 5000.00
          })
        });
      });

      // Check if financial summary section exists
      const summarySection = page.getByText(/financial.*summary|invoice.*summary/i);
      
      if (await summarySection.isVisible()) {
        // Check for summary metrics
        await expect(page.getByText(/total.*invoiced/i)).toBeVisible();
        await expect(page.getByText(/total.*paid/i)).toBeVisible();
        await expect(page.getByText(/outstanding/i)).toBeVisible();
        await expect(page.getByText(/overdue/i)).toBeVisible();
      }
    });
  });
});