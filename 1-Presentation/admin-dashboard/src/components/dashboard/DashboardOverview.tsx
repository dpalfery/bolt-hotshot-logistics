'use client';

import { useQuery } from '@tanstack/react-query';
import { apiService } from '@/services/api';
import { JobStatus } from '@/types';

export function DashboardOverview() {
  const { data: jobs, isLoading: jobsLoading } = useQuery({
    queryKey: ['jobs'],
    queryFn: () => apiService.getJobs(),
  });

  const { data: drivers, isLoading: driversLoading } = useQuery({
    queryKey: ['drivers'],
    queryFn: () => apiService.getDrivers(),
  });

  const { data: invoices, isLoading: invoicesLoading } = useQuery({
    queryKey: ['invoices'],
    queryFn: () => apiService.getInvoices(),
  });

  const stats = {
    totalJobs: jobs?.totalCount || 0,
    activeJobs: jobs?.items.filter(job => job.status === JobStatus.InProgress).length || 0,
    pendingJobs: jobs?.items.filter(job => job.status === JobStatus.Pending).length || 0,
    totalDrivers: drivers?.length || 0,
    activeDrivers: drivers?.filter(driver => driver.isActive).length || 0,
    totalInvoices: invoices?.totalCount || 0,
    overdueInvoices: invoices?.items.filter(invoice =>
      new Date(invoice.dueDate) < new Date() && invoice.balanceDue > 0
    ).length || 0,
  };

  if (jobsLoading || driversLoading || invoicesLoading) {
    return (
      <div className="space-y-6">
        <div className="animate-pulse">
          <div className="h-8 bg-gray-200 rounded w-1/4 mb-4"></div>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {[...Array(4)].map((_, i) => (
              <div key={i} className="bg-white p-6 rounded-lg shadow">
                <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                <div className="h-8 bg-gray-200 rounded w-1/2"></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard Overview</h1>
        <p className="text-gray-600">Welcome to Hotshot Logistics Admin Dashboard</p>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white p-6 rounded-lg shadow">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-blue-500 rounded-md flex items-center justify-center">
                <span className="text-white text-sm font-bold">J</span>
              </div>
            </div>
            <div className="ml-4">
              <dt className="text-sm font-medium text-gray-500 truncate">Total Jobs</dt>
              <dd className="text-2xl font-semibold text-gray-900">{stats.totalJobs}</dd>
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-green-500 rounded-md flex items-center justify-center">
                <span className="text-white text-sm font-bold">A</span>
              </div>
            </div>
            <div className="ml-4">
              <dt className="text-sm font-medium text-gray-500 truncate">Active Jobs</dt>
              <dd className="text-2xl font-semibold text-gray-900">{stats.activeJobs}</dd>
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-yellow-500 rounded-md flex items-center justify-center">
                <span className="text-white text-sm font-bold">D</span>
              </div>
            </div>
            <div className="ml-4">
              <dt className="text-sm font-medium text-gray-500 truncate">Active Drivers</dt>
              <dd className="text-2xl font-semibold text-gray-900">{stats.activeDrivers}</dd>
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg shadow">
          <div className="flex items-center">
            <div className="flex-shrink-0">
              <div className="w-8 h-8 bg-red-500 rounded-md flex items-center justify-center">
                <span className="text-white text-sm font-bold">O</span>
              </div>
            </div>
            <div className="ml-4">
              <dt className="text-sm font-medium text-gray-500 truncate">Overdue Invoices</dt>
              <dd className="text-2xl font-semibold text-gray-900">{stats.overdueInvoices}</dd>
            </div>
          </div>
        </div>
      </div>

      {/* Recent Activity */}
      <div className="bg-white shadow rounded-lg">
        <div className="px-4 py-5 sm:p-6">
          <h3 className="text-lg leading-6 font-medium text-gray-900">Recent Activity</h3>
          <div className="mt-5">
            <div className="text-sm text-gray-500">
              Activity feed will be implemented with real-time updates from SignalR
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}