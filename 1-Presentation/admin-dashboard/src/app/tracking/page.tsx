import { DashboardLayout } from '@/components/layout/DashboardLayout';
import { TrackingDashboard } from '@/components/tracking/TrackingDashboard';

export default function TrackingPage() {
  return (
    <DashboardLayout>
      <TrackingDashboard />
    </DashboardLayout>
  );
}