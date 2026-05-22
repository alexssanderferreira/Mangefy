import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';

export interface AddressDto {
  cep: string;
  logradouro: string;
  numero: string;
  complemento?: string;
  bairro: string;
  cidade: string;
  uf: string;
}

export interface TenantDto {
  id: string;
  name: string;
  slug: string;
  email?: string | null;
  phone?: string | null;
  status: 'Active' | 'TrialPeriod' | 'Suspended' | 'Cancelled';
  planId: string;
  businessTypeId: string;
  timezone: string;
  trialEndsAt: string | null;
  createdAt: string;
  address?: AddressDto | null;
}

export interface CreateTenantRequest {
  ownerId: string;
  name: string;
  slug: string;
  planId: string;
  businessTypeId: string;
  timezone: string;
  trialDays: number;
  email?: string | null;
}

export interface EmployeeDto {
  id: string;
  name: string;
  email: string;
  tenantRoleId: string;
  status: string;
  lastLoginAt: string | null;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DashboardMetrics {
  total: number;
  active: number;
  trial: number;
  suspended: number;
  cancelled: number;
  trialsExpiringSoon: TenantDto[];
  recent: TenantDto[];
}

@Injectable({ providedIn: 'root' })
export class TenantService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/tenants`;

  getAll()              { return this.http.get<TenantDto[]>(this.base); }
  getPaged(page: number, pageSize = 10) {
    return this.http.get<PagedResult<TenantDto>>(this.base, { params: { page, pageSize } });
  }
  getById(id: string)   { return this.http.get<TenantDto>(`${this.base}/${id}`); }
  create(req: CreateTenantRequest) { return this.http.post<{ id: string }>(this.base, req); }
  suspend(id: string)   { return this.http.patch(`${this.base}/${id}/suspend`, null); }
  reactivate(id: string){ return this.http.patch(`${this.base}/${id}/reactivate`, null); }
  cancel(id: string)    { return this.http.patch(`${this.base}/${id}/cancel`, null); }
  getEmployees(id: string) { return this.http.get<EmployeeDto[]>(`${this.base}/${id}/employees`); }

  computeMetrics(tenants: TenantDto[]): DashboardMetrics {
    const now = new Date();
    const in14 = new Date(now.getTime() + 14 * 24 * 60 * 60 * 1000);
    return {
      total:     tenants.length,
      active:    tenants.filter(t => t.status === 'Active').length,
      trial:     tenants.filter(t => t.status === 'TrialPeriod').length,
      suspended: tenants.filter(t => t.status === 'Suspended').length,
      cancelled: tenants.filter(t => t.status === 'Cancelled').length,
      trialsExpiringSoon: tenants
        .filter(t => t.status === 'TrialPeriod' && t.trialEndsAt && new Date(t.trialEndsAt) <= in14)
        .sort((a, b) => new Date(a.trialEndsAt!).getTime() - new Date(b.trialEndsAt!).getTime()),
      recent: [...tenants]
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 6),
    };
  }

  daysUntil(dateStr: string): number {
    return Math.ceil((new Date(dateStr).getTime() - Date.now()) / (1000 * 60 * 60 * 24));
  }
}
