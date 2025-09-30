import { test, expect } from '@playwright/test';

test.describe('Dashboard Overview', () => {
  test.beforeEach(async ({ page }) => {
    // Mock dashboard statistics API
    await page.route('**/api/dashboard/stats', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalJobs: 156,
          activeDrivers: 23,
          revenue: 45678.90,
          pendingJobs: 12
        })
      });
    });

    // Mock recent jobs API
    await page.route('**/api/jobs*', async route => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          items: [
            {
              id: 'job-1',
              title: 'Urgent Delivery',
              status: 'InProgress',
              pickupAddress: '123 Main St',
              dropoffAddress: '456 Oak Ave',
              assignedDriverId: 1,
              scheduledPickupTime: '2024-12-01T10:00:00Z'
            },
            {
              id: 'job-2',
              title: 'Standard Delivery',
              status: 'Pending',
              pickupAddress: '789 Pine St',
              dropoffAddress: '321 Elm Ave',
              assignedDriverId: null,
              scheduledPickupTime: '2024-12-01T14:00:00Z'
            }
          ],
          totalCount: 2
        })
      });
    });

    await page.goto('/');
  });

  test.describe('Dashboard Statistics Cards', () => {
    test('should display dashboard stats cards with correct values', async ({ page }) => {
      // Check for stats cards with values
      await expect(page.getByText('Total Jobs')).toBeVisible();
      await expect(page.getByText('156')).toBeVisible();
      
      await expect(page.getByText('Active Drivers')).toBeVisible();
      await expect(page.getByText('23')).toBeVisible();
      
      await expect(page.getByText('Revenue')).toBeVisible();
      await expect(page.getByText('$45,678.90')).toBeVisible();
      
      await expect(page.getByText('Pending Jobs')).toBeVisible();
      await expect(page.getByText('12')).toBeVisible();
    });

    test('should display stats cards with proper styling', async ({ page }) => {
      // Check for card containers
      const statsCards = page.locator('[data-testid="stats-card"]');
      await expect(statsCards).toHaveCount(4);

      // Check for proper card styling
      const firstCard = statsCards.first();
      await expect(firstCard).toHaveClass(/bg-white.*rounded-lg.*shadow/);
    });

    test('should handle loading state for statistics', async ({ page }) => {
      // Mock delayed response
      await page.route('**/api/dashboard/stats', async route => {
        await new Promise(resolve => setTimeout(resolve, 1000));
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            totalJobs: 156,
            activeDrivers: 23,
            revenue: 45678.90,
            pendingJobs: 12
          })
        });
      });

      await page.reload();

      // Check for loading indicators
      const loadingIndicators = page.locator('[data-testid="loading-spinner"]');
      if (await loadingIndicators.first().isVisible()) {
        await expect(loadingIndicators).toHaveCount(4);
      }

      // Wait for data to load
      await expect(page.getByText('156')).toBeVisible();
    });

    test('should handle error state for statistics', async ({ page }) => {
      // Mock error response
      await page.route('**/api/dashboard/stats', async route => {
        await route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Internal server error' })
        });
      });

      await page.reload();

      // Check for error state
      await expect(page.getByText('Error loading statistics')).toBeVisible();
    });
  });

  test.describe('Navigation', () => {
    test('should navigate to different sections from sidebar', async ({ page }) => {
      // Test navigation to jobs
      await page.click('text=Jobs');
      await expect(page).toHaveURL('/jobs');

      // Navigate back to dashboard
      await page.goto('/');

      // Test navigation to drivers
      await page.click('text=Drivers');
      await expect(page).toHaveURL('/drivers');

      // Navigate back to dashboard
      await page.goto('/');

      // Test navigation to billing
      await page.click('text=Billing');
      await expect(page).toHaveURL('/billing');

      // Navigate back to dashboard
      await page.goto('/');

      // Test navigation to tracking
      await page.click('text=Tracking');
      await expect(page).toHaveURL('/tracking');
    });

    test('should highlight active navigation item', async ({ page }) => {
      // Check dashboard is active initially
      const dashboardLink = page.locator('nav a[href="/"]');
      await expect(dashboardLink).toHaveClass(/bg-blue-100.*text-blue-700/);

      // Navigate to jobs and check active state
      await page.click('text=Jobs');
      const jobsLink = page.locator('nav a[href="/jobs"]');
      await expect(jobsLink).toHaveClass(/bg-blue-100.*text-blue-700/);
    });

    test('should display user profile information', async ({ page }) => {
      // Check for user profile section
      await expect(page.getByText('Admin User')).toBeVisible();
      await expect(page.getByText('admin@hotshotlogistics.com')).toBeVisible();
    });
  });

  test.describe('Recent Activity Section', () => {
    test('should display recent jobs section', async ({ page }) => {
      // Check recent jobs section
      await expect(page.getByText('Recent Jobs')).toBeVisible();
      
      // Check job entries
      await expect(page.getByText('Urgent Delivery')).toBeVisible();
      await expect(page.getByText('Standard Delivery')).toBeVisible();
      
      // Check job details
      await expect(page.getByText('123 Main St → 456 Oak Ave')).toBeVisible();
      await expect(page.getByText('789 Pine St → 321 Elm Ave')).toBeVisible();
    });

    test('should display job status badges with correct styling', async ({ page }) => {
      // Check status badges
      const inProgressBadge = page.locator('text=InProgress').first();
      await expect(inProgressBadge).toBeVisible();
      await expect(inProgressBadge).toHaveClass(/bg-green-100.*text-green-800/);

      const pendingBadge = page.locator('text=Pending').first();
      await expect(pendingBadge).toBeVisible();
      await expect(pendingBadge).toHaveClass(/bg-yellow-100.*text-yellow-800/);
    });

    test('should handle empty recent jobs list', async ({ page }) => {
      // Mock empty jobs response
      await page.route('**/api/jobs*', async route => {
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

      // Check empty state
      await expect(page.getByText('No recent jobs')).toBeVisible();
    });

    test('should link to full jobs list', async ({ page }) => {
      // Check "View All Jobs" link
      const viewAllLink = page.locator('a[href="/jobs"]').filter({ hasText: 'View All Jobs' });
      await expect(viewAllLink).toBeVisible();
      
      // Click and verify navigation
      await viewAllLink.click();
      await expect(page).toHaveURL('/jobs');
    });
  });

  test.describe('Quick Actions', () => {
    test('should display quick action buttons', async ({ page }) => {
      // Check for quick action buttons
      await expect(page.getByText('Create New Job')).toBeVisible();
      await expect(page.getByText('Add Driver')).toBeVisible();
      await expect(page.getByText('Generate Invoice')).toBeVisible();
    });

    test('should navigate to create job form', async ({ page }) => {
      // Click create job button
      await page.click('text=Create New Job');
      await expect(page).toHaveURL('/jobs?action=create');
    });

    test('should navigate to add driver form', async ({ page }) => {
      // Click add driver button
      await page.click('text=Add Driver');
      await expect(page).toHaveURL('/drivers?action=add');
    });

    test('should navigate to invoice generation', async ({ page }) => {
      // Click generate invoice button
      await page.click('text=Generate Invoice');
      await expect(page).toHaveURL('/billing?action=generate');
    });
  });

  test.describe('Real-time Updates', () => {
    test('should handle real-time statistics updates', async ({ page }) => {
      // Initial load
      await expect(page.getByText('156')).toBeVisible();

      // Mock updated statistics
      await page.route('**/api/dashboard/stats', async route => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            totalJobs: 157,
            activeDrivers: 24,
            revenue: 46000.00,
            pendingJobs: 11
          })
        });
      });

      // Simulate real-time update (would normally come via SignalR)
      await page.evaluate(() => {
        // Trigger a refetch of statistics
        window.dispatchEvent(new CustomEvent('dashboard-update'));
      });

      // Check updated values
      await expect(page.getByText('157')).toBeVisible();
      await expect(page.getByText('24')).toBeVisible();
      await expect(page.getByText('$46,000.00')).toBeVisible();
      await expect(page.getByText('11')).toBeVisible();
    });
  });

  test.describe('Responsive Design', () => {
    test('should display correctly on mobile devices', async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 });
      await page.reload();

      // Check that stats cards stack vertically on mobile
      const statsGrid = page.locator('.grid').first();
      await expect(statsGrid).toHaveClass(/grid-cols-1.*sm:grid-cols-2.*lg:grid-cols-4/);

      // Check that sidebar is collapsed on mobile
      const sidebar = page.locator('nav');
      await expect(sidebar).toHaveClass(/hidden.*md:block/);
    });

    test('should display correctly on tablet devices', async ({ page }) => {
      // Set tablet viewport
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.reload();

      // Check that stats cards display in 2 columns on tablet
      const statsGrid = page.locator('.grid').first();
      await expect(statsGrid).toHaveClass(/sm:grid-cols-2/);
    });

    test('should display correctly on desktop', async ({ page }) => {
      // Set desktop viewport
      await page.setViewportSize({ width: 1200, height: 800 });
      await page.reload();

      // Check that stats cards display in 4 columns on desktop
      const statsGrid = page.locator('.grid').first();
      await expect(statsGrid).toHaveClass(/lg:grid-cols-4/);

      // Check that sidebar is visible on desktop
      const sidebar = page.locator('nav');
      await expect(sidebar).toBeVisible();
    });
  });

  test.describe('Performance and Loading', () => {
    test('should load dashboard within acceptable time', async ({ page }) => {
      const startTime = Date.now();
      await page.goto('/');
      
      // Wait for main content to be visible
      await expect(page.getByText('Dashboard Overview')).toBeVisible();
      
      const loadTime = Date.now() - startTime;
      expect(loadTime).toBeLessThan(3000); // Should load within 3 seconds
    });

    test('should handle concurrent API requests efficiently', async ({ page }) => {
      let statsRequestCount = 0;
      let jobsRequestCount = 0;

      // Count API requests
      await page.route('**/api/dashboard/stats', async route => {
        statsRequestCount++;
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            totalJobs: 156,
            activeDrivers: 23,
            revenue: 45678.90,
            pendingJobs: 12
          })
        });
      });

      await page.route('**/api/jobs*', async route => {
        jobsRequestCount++;
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
      await page.waitForTimeout(1000);

      // Should make only one request to each endpoint
      expect(statsRequestCount).toBe(1);
      expect(jobsRequestCount).toBe(1);
    });
  });
});