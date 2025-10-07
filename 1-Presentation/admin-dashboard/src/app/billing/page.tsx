'use client';

import { DashboardLayout } from '@/components/layout/DashboardLayout';
import { BillingManagement } from '@/components/billing/BillingManagement';

export default function BillingPage() {
  return (
    <DashboardLayout>
      <BillingManagement />
    </DashboardLayout>
  );
}