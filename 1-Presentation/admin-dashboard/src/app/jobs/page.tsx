import { DashboardLayout } from '@/components/layout/DashboardLayout';
import { JobsManagement } from '@/components/jobs/JobsManagement';

export default function JobsPage() {
  return (
    <DashboardLayout>
      <JobsManagement />
    </DashboardLayout>
  );
}