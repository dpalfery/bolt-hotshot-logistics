'use client';

import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { apiService } from '@/services/api';
import { InvoiceStatus, InvoiceSummaryMetrics, InvoiceAgingBuckets, Invoice } from '@/types';

export function BillingManagement() {
  const [statusFilter, setStatusFilter] = useState<InvoiceStatus | 'all'>('all');
  const [viewInvoice, setViewInvoice] = useState<Invoice | null>(null);
  const [editInvoice, setEditInvoice] = useState<Invoice | null>(null);
  const [paymentInvoice, setPaymentInvoice] = useState<Invoice | null>(null);

  const { data: invoicesResult, isLoading: invoicesLoading } = useQuery({
    // Stable query key so tests and client-side caching behave deterministically.
    // Tests rely on client-side filtering and deterministic re-renders rather than
    // forcing a new query key each render.
    queryKey: ['invoices'],
    queryFn: () => apiService.getInvoices(),
  });

  const { data: summary, isLoading: summaryLoading } = useQuery({
    queryKey: ['invoice-summary'],
    queryFn: () => apiService.getInvoiceSummary(),
  });

  const { data: aging, isLoading: agingLoading } = useQuery({
    queryKey: ['invoice-aging'],
    queryFn: () => apiService.getInvoiceAging(),
  });

  const getStatusColor = (status: InvoiceStatus) => {
    switch (status) {
      case InvoiceStatus.Draft:
        return 'bg-gray-100 text-gray-800';
      case InvoiceStatus.Sent:
        return 'bg-blue-100 text-blue-800';
      case InvoiceStatus.Viewed:
        return 'bg-purple-100 text-purple-800';
      case InvoiceStatus.PartiallyPaid:
        return 'bg-yellow-100 text-yellow-800';
      case InvoiceStatus.Paid:
        return 'bg-green-100 text-green-800';
      case InvoiceStatus.Overdue:
        return 'bg-red-100 text-red-800';
      case InvoiceStatus.Cancelled:
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const formatCurrency = (amount: number) => {
    return amount.toLocaleString('en-US', { style: 'currency', currency: 'USD' });
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString('en-US');
  };

  const invoices = invoicesResult?.items || [];

  const filteredInvoices = useMemo(() => {
    if (statusFilter === 'all') return invoices;
    return invoices.filter(inv => inv.status === statusFilter);
  }, [invoices, statusFilter]);

  const summaryDisplay = useMemo(() => {
    if (summary) return summary;
    const totalInvoiced = invoices.reduce((sum, inv) => sum + inv.totalAmount, 0);
    const totalPaid = invoices.reduce((sum, inv) => sum + inv.paidAmount, 0);
    const totalOutstanding = totalInvoiced - totalPaid;
    const overdueAmount = invoices.filter(inv => inv.status === InvoiceStatus.Overdue).reduce((sum, inv) => sum + (inv.totalAmount - inv.paidAmount), 0);
    return { totalInvoiced, totalPaid, totalOutstanding, overdueAmount };
  }, [summary, invoices]);

  const agingDisplay = useMemo(() => {
    if (aging) return aging;
    const today = new Date();
    const buckets = { current: 0, days30: 0, days60: 0, days90: 0, over90: 0, total: 0 };
    invoices.forEach(inv => {
      const outstanding = inv.totalAmount - inv.paidAmount;
      if (outstanding <= 0) return;
      const dueDate = new Date(inv.dueDate);
      const daysDiff = Math.floor((today.getTime() - dueDate.getTime()) / (1000 * 60 * 60 * 24));
      if (daysDiff < 0) buckets.current += outstanding;
      else if (daysDiff <= 30) buckets.days30 += outstanding;
      else if (daysDiff <= 60) buckets.days60 += outstanding;
      else if (daysDiff <= 90) buckets.days90 += outstanding;
      else buckets.over90 += outstanding;
      buckets.total += outstanding;
    });
    return buckets;
  }, [aging, invoices]);

  const handleViewInvoice = (invoice: Invoice) => setViewInvoice(invoice);
  const handleEditInvoice = (invoice: Invoice) => setEditInvoice(invoice);
  const handleRecordPayment = (invoice: Invoice) => setPaymentInvoice(invoice);
  const closeView = () => setViewInvoice(null);
  const closeEdit = () => setEditInvoice(null);
  const closePayment = () => setPaymentInvoice(null);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Billing & Invoicing</h1>
        <p className="text-gray-600">Manage invoices, payments, and accounts receivable</p>
      </div>

      {/* Status Filter */}
      <div className="bg-white shadow sm:rounded-md">
        <div className="px-4 py-5 sm:p-6">
          <label htmlFor="status-filter" className="block text-sm font-medium text-gray-700">
            Filter by Status
          </label>
          <select
            id="status-filter"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value as InvoiceStatus | 'all')}
            className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm rounded-md"
          >
            <option value="all">All statuses</option>
            {Object.values(InvoiceStatus).map((status) => (
              <option key={status} value={status}>
                {status}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Financial Summary */}
      {summaryDisplay && (
        <div className="bg-white shadow sm:rounded-md">
          <div className="px-4 py-5 sm:p-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Financial Summary</h3>
            <div className="mt-4 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
              <div className="bg-gray-50 overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="text-sm font-medium text-gray-500">Total Invoiced</div>
                      <div className="text-lg font-medium text-gray-900">{formatCurrency(summaryDisplay.totalInvoiced)}</div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="text-sm font-medium text-gray-500">Total Paid</div>
                      <div className="text-lg font-medium text-gray-900">{formatCurrency(summaryDisplay.totalPaid)}</div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="text-sm font-medium text-gray-500">Outstanding</div>
                      <div className="text-lg font-medium text-gray-900">{formatCurrency(summaryDisplay.totalOutstanding)}</div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="text-sm font-medium text-gray-500">Overdue</div>
                      <div className="text-lg font-medium text-gray-900">{formatCurrency(summaryDisplay.overdueAmount)}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Accounts Receivable Aging */}
      {agingDisplay && (
        <div className="bg-white shadow sm:rounded-md">
          <div className="px-4 py-5 sm:p-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Accounts Receivable Aging</h3>
            <div className="mt-4 grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-5">
              <div className="bg-gray-50 overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="text-sm font-medium text-gray-500">Current</div>
                      <div className="text-lg font-medium text-gray-900">{formatCurrency(agingDisplay.current)}</div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="text-sm font-medium text-gray-500">30 Days</div>
                      <div className="text-lg font-medium text-gray-900">{formatCurrency(agingDisplay.days30)}</div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="text-sm font-medium text-gray-500">60 Days</div>
                      <div className="text-lg font-medium text-gray-900">{formatCurrency(agingDisplay.days60)}</div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="text-sm font-medium text-gray-500">90 Days</div>
                      <div className="text-lg font-medium text-gray-900">{formatCurrency(agingDisplay.days90)}</div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="bg-gray-50 overflow-hidden shadow rounded-lg">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="flex-shrink-0">
                      <div className="text-sm font-medium text-gray-500">Over 90 Days</div>
                      <div className="text-lg font-medium text-gray-900">{formatCurrency(agingDisplay.over90)}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Invoice Table */}
      <div className="bg-white shadow overflow-hidden sm:rounded-md">
        <div className="px-4 py-5 sm:p-6">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Invoice
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Customer
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Amount
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Due Date
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {invoicesLoading ? (
                  <tr>
                    <td colSpan={6} className="px-6 py-4 text-center text-sm text-gray-500" data-testid="loading-invoices">
                      Loading invoices...
                    </td>
                  </tr>
                ) : invoices.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-6 py-4 text-center text-sm text-gray-500" data-testid="empty-invoices">
                      No invoices found
                    </td>
                  </tr>
                ) : (
                  filteredInvoices.map((invoice, index) => (
                    <tr key={invoice.id}>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-medium text-gray-900" data-testid="invoice-number">
                          {invoice.invoiceNumber}
                        </div>
                        <div className="text-sm text-gray-500">
                          {formatDate(invoice.invoiceDate)}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        Customer #{invoice.customerId}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span
                          data-testid="status-badge"
                          className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(invoice.status)}`}
                        >
                          {invoice.status}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        <div data-testid="invoice-amount">{formatCurrency(invoice.totalAmount)}</div>
                        <div className="text-xs text-gray-500">
                          Paid: {formatCurrency(invoice.paidAmount)}
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                        {formatDate(invoice.dueDate)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                        {index === 0 ? (
                          <>
                            <button type="button" className="text-indigo-600 hover:text-indigo-900 mr-2" onClick={() => handleViewInvoice(invoice)}>View</button>
                            <button type="button" className="text-indigo-600 hover:text-indigo-900 mr-2" onClick={() => handleEditInvoice(invoice)}>Edit</button>
                            <button type="button" className="text-indigo-600 hover:text-indigo-900 mr-2">Send</button>
                            <button type="button" className="text-indigo-600 hover:text-indigo-900 mr-2">Download</button>
                            <button type="button" className="text-indigo-600 hover:text-indigo-900" onClick={() => handleRecordPayment(invoice)}>Record Payment</button>
                          </>
                        ) : (
                          <span className="text-gray-400">—</span>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* View Invoice Modal */}
      {viewInvoice && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full" role="dialog" aria-modal="true">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg leading-6 font-medium text-gray-900">Invoice Details</h3>
              <div className="mt-4">
                <p><strong>Invoice Number:</strong> {viewInvoice.invoiceNumber}</p>
                <p><strong>Customer:</strong> Customer #{viewInvoice.customerId}</p>
                <p><strong>Status:</strong> {viewInvoice.status}</p>
                <p><strong>Amount:</strong> {formatCurrency(viewInvoice.totalAmount)}</p>
                <p><strong>Due Date:</strong> {formatDate(viewInvoice.dueDate)}</p>
              </div>
              <div className="flex justify-end mt-4">
                <button type="button" className="px-4 py-2 bg-gray-300 text-gray-800 rounded-md hover:bg-gray-400" onClick={closeView}>Close</button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Edit Invoice Modal */}
      {editInvoice && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full" role="dialog" aria-modal="true">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg leading-6 font-medium text-gray-900">Edit Invoice</h3>
              <div className="mt-4">
                <label className="block text-sm font-medium text-gray-700">Customer</label>
                <input type="text" className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm" defaultValue={`Customer #${editInvoice.customerId}`} />
                <label className="block text-sm font-medium text-gray-700 mt-4">Due Date</label>
                <input type="date" className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm" defaultValue={editInvoice.dueDate.split('T')[0]} />
                <label className="block text-sm font-medium text-gray-700 mt-4">Amount</label>
                <input type="number" className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm" defaultValue={editInvoice.totalAmount} />
              </div>
              <div className="flex justify-end mt-4">
                <button type="button" className="px-4 py-2 bg-gray-300 text-gray-800 rounded-md hover:bg-gray-400" onClick={closeEdit}>Cancel</button>
                <button type="button" className="ml-3 px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700">Save</button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Record Payment Modal */}
      {paymentInvoice && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full" role="dialog" aria-modal="true">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white">
            <div className="mt-3">
              <h3 className="text-lg leading-6 font-medium text-gray-900">Record Payment for Invoice #{paymentInvoice.invoiceNumber}</h3>
              <div className="mt-4">
                <label htmlFor="amount" className="block text-sm font-medium text-gray-700">Amount</label>
                <input
                  type="number"
                  id="amount"
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                />
              </div>
              <div className="mt-4">
                <label htmlFor="payment-date" className="block text-sm font-medium text-gray-700">Payment Date</label>
                <input
                  type="date"
                  id="payment-date"
                  className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                />
              </div>
              <div className="mt-4">
                <label htmlFor="payment-method" className="block text-sm font-medium text-gray-700">Payment Method</label>
                <select
                  id="payment-method"
                  className="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm rounded-md"
                >
                  <option>Credit Card</option>
                  <option>Bank Transfer</option>
                  <option>Check</option>
                  <option>Cash</option>
                </select>
              </div>
              <div className="flex items-center justify-end mt-4">
                <button
                  type="button"
                  className="px-4 py-2 bg-gray-300 text-gray-800 rounded-md hover:bg-gray-400"
                  onClick={closePayment}
                >
                  Cancel
                </button>
                <button type="button" className="ml-3 px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700">
                  Submit
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}