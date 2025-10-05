import { Job, Driver, Invoice, Customer, PagedResult, JobFilter, InvoiceFilter, PaginationParameters, InvoiceSummaryMetrics, InvoiceAgingBuckets } from '@/types';

const API_BASE_URL = (process.env.NEXT_PUBLIC_API_BASE_URL || '/api').trim();

class ApiService {
  private async request<T>(
    endpoint: string,
    options: RequestInit = {},
    query?: string
  ): Promise<T> {
    const baseUrl = API_BASE_URL.replace(/\/$/, '');
    const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
    const url = `${baseUrl}${normalizedEndpoint}${query ? `?${query}` : ''}`;

    const config: RequestInit = {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
    };

    // Add authentication token if available
    const token = this.getAuthToken();
    if (token) {
      config.headers = {
        ...config.headers,
        Authorization: `Bearer ${token}`,
      };
    }

    const response = await fetch(url, config);

    if (!response.ok) {
      const error = await response.text();
      throw new Error(`API Error: ${response.status} ${error}`);
    }

    return response.json();
  }

  private getAuthToken(): string | null {
    // This will be implemented when MSAL is set up
    return null;
  }

  // Job API methods
  async getJobs(
    filter?: JobFilter,
    pagination?: PaginationParameters
  ): Promise<PagedResult<Job>> {
    const params = new URLSearchParams();

    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          params.append(key, value.toString());
        }
      });
    }

    if (pagination) {
      params.append('pageNumber', pagination.pageNumber.toString());
      params.append('pageSize', pagination.pageSize.toString());
    }

    const query = params.toString();
    if (query) {
      return this.request<PagedResult<Job>>('/job', {}, query);
    } else {
      return this.request<PagedResult<Job>>('/job');
    }
  }

  async getJobById(id: string): Promise<Job> {
    return this.request<Job>(`/job/${id}`);
  }

  async createJob(job: Partial<Job>): Promise<Job> {
    return this.request<Job>('/job', {
      method: 'POST',
      body: JSON.stringify(job),
    });
  }

  async updateJob(id: string, job: Partial<Job>): Promise<Job> {
    return this.request<Job>(`/job/${id}`, {
      method: 'PUT',
      body: JSON.stringify(job),
    });
  }

  async deleteJob(id: string): Promise<void> {
    await this.request(`/job/${id}`, {
      method: 'DELETE',
    });
  }

  async assignDriver(jobId: string, driverId: number): Promise<Job> {
    return this.request<Job>(`/job/${jobId}/assign-driver`, {
      method: 'POST',
      body: JSON.stringify({ driverId }),
    });
  }

  async updateJobStatus(jobId: string, status: string): Promise<Job> {
    return this.request<Job>(`/job/${jobId}/status`, {
      method: 'PUT',
      body: JSON.stringify({ status }),
    });
  }

  // Driver API methods
  async getDrivers(): Promise<Driver[]> {
    return this.request<Driver[]>('/driver');
  }

  async getDriverById(id: number): Promise<Driver> {
    return this.request<Driver>(`/driver/${id}`);
  }

  async createDriver(driver: Partial<Driver>): Promise<Driver> {
    return this.request<Driver>('/driver', {
      method: 'POST',
      body: JSON.stringify(driver),
    });
  }

  async updateDriver(id: number, driver: Partial<Driver>): Promise<Driver> {
    return this.request<Driver>(`/driver/${id}`, {
      method: 'PUT',
      body: JSON.stringify(driver),
    });
  }

  async deleteDriver(id: number): Promise<void> {
    await this.request(`/driver/${id}`, {
      method: 'DELETE',
    });
  }

  // Invoice API methods
  async getInvoices(
    filter?: InvoiceFilter,
    pagination?: PaginationParameters
  ): Promise<PagedResult<Invoice>> {
    const params = new URLSearchParams();

    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          params.append(key, value.toString());
        }
      });
    }

    if (pagination) {
      params.append('pageNumber', pagination.pageNumber.toString());
      params.append('pageSize', pagination.pageSize.toString());
    }

    const query = params.toString();
    if (query) {
      return this.request<PagedResult<Invoice>>('/billing/invoices', {}, query);
    } else {
      return this.request<PagedResult<Invoice>>('/billing/invoices');
    }
  }

  async getInvoiceById(id: string): Promise<Invoice> {
    return this.request<Invoice>(`/billing/invoices/${id}`);
  }

  async createInvoice(invoice: Partial<Invoice>): Promise<Invoice> {
    return this.request<Invoice>('/billing/invoices', {
      method: 'POST',
      body: JSON.stringify(invoice),
    });
  }

  async updateInvoice(id: string, invoice: Partial<Invoice>): Promise<Invoice> {
    return this.request<Invoice>(`/billing/invoices/${id}`, {
      method: 'PUT',
      body: JSON.stringify(invoice),
    });
  }

  async deleteInvoice(id: string): Promise<void> {
    await this.request(`/billing/invoices/${id}`, {
      method: 'DELETE',
    });
  }

  async getInvoiceSummary(): Promise<InvoiceSummaryMetrics> {
    return this.request<InvoiceSummaryMetrics>('/invoices/summary');
  }

  async getInvoiceAging(): Promise<InvoiceAgingBuckets> {
    return this.request<InvoiceAgingBuckets>('/invoices/aging');
  }

  // Customer API methods
  async getCustomers(): Promise<Customer[]> {
    return this.request<Customer[]>('/customer');
  }

  async getCustomerById(id: string): Promise<Customer> {
    return this.request<Customer>(`/customer/${id}`);
  }

  async createCustomer(customer: Partial<Customer>): Promise<Customer> {
    return this.request<Customer>('/customer', {
      method: 'POST',
      body: JSON.stringify(customer),
    });
  }

  async updateCustomer(id: string, customer: Partial<Customer>): Promise<Customer> {
    return this.request<Customer>(`/customer/${id}`, {
      method: 'PUT',
      body: JSON.stringify(customer),
    });
  }

  async deleteCustomer(id: string): Promise<void> {
    await this.request(`/customer/${id}`, {
      method: 'DELETE',
    });
  }
}

export const apiService = new ApiService();