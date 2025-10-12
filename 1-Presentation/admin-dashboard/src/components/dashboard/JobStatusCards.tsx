'use client';

import { useQuery } from '@tanstack/react-query';
import { Hourglass, ClipboardList, Truck, Inbox } from 'lucide-react';
import { apiService } from '@/services/api';

interface StatusCardProps {
  label: string;
  count: number;
  color: string;
  icon: React.ReactNode;
}

function StatusCard({ label, count, color, icon }: StatusCardProps) {
  return (
    <div className="bg-white p-6 rounded-lg shadow">
      <div className="flex items-center mb-4">
        <div
          className="w-8 h-8 rounded-md flex items-center justify-center mr-3"
          style={{ backgroundColor: color }}
        >
          {icon}
        </div>
        <h3 className="text-sm font-semibold text-gray-900">{label}</h3>
      </div>
      <div className="text-center">
        <p className="text-2xl font-bold text-gray-900">{count}</p>
      </div>
    </div>
  );
}

export function JobStatusCards() {
  const { data: statusSummary, isLoading, error } = useQuery({
    queryKey: ['jobStatusSummary'],
    queryFn: () => apiService.getJobStatusSummary(),
    retry: false,
  });

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="bg-white p-6 rounded-lg shadow animate-pulse">
            <div className="flex items-center mb-4">
              <div className="w-8 h-8 bg-gray-200 rounded-md mr-3"></div>
              <div className="h-4 bg-gray-200 rounded w-16"></div>
            </div>
            <div className="text-center">
              <div className="h-8 bg-gray-200 rounded w-12 mx-auto"></div>
            </div>
          </div>
        ))}
      </div>
    );
  }

  if (error || !statusSummary) {
    return (
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="bg-white p-6 rounded-lg shadow">
            <div className="flex items-center mb-4">
              <div className="w-8 h-8 bg-gray-300 rounded-md mr-3 flex items-center justify-center">
                <span className="text-gray-500 text-xs">!</span>
              </div>
              <h3 className="text-sm font-semibold text-gray-500">Error</h3>
            </div>
            <div className="text-center">
              <p className="text-2xl font-bold text-gray-500">-</p>
            </div>
          </div>
        ))}
      </div>
    );
  }

  const statusCards = [
    {
      label: 'Pending',
      count: statusSummary.pendingCount,
      color: '#3B82F6',
      icon: <Hourglass className="w-4 h-4 text-white" />,
    },
    {
      label: 'Assigned',
      count: statusSummary.assignedCount,
      color: '#0EA5E9',
      icon: <ClipboardList className="w-4 h-4 text-white" />,
    },
    {
      label: 'EnRoute',
      count: statusSummary.enRouteCount,
      color: '#0284C7',
      icon: <Truck className="w-4 h-4 text-white" />,
    },
    {
      label: 'Received',
      count: statusSummary.receivedCount,
      color: '#0369A1',
      icon: <Inbox className="w-4 h-4 text-white" />,
    },
  ];

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      {statusCards.map((card) => (
        <StatusCard
          key={card.label}
          label={card.label}
          count={card.count}
          color={card.color}
          icon={card.icon}
        />
      ))}
    </div>
  );
}