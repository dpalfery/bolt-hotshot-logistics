import { Job, Driver, Invoice, Customer, PagedResult, JobFilter, InvoiceFilter, PaginationParameters } from '@/types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:7071/api';

class ApiService {
  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`;

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

    return this.request<PagedResult<Job>>(`/job?${params}`);
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

    return this.request<PagedResult<Invoice>>(`/billing/invoices?${params}`);
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