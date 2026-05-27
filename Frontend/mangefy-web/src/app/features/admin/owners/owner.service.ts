import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface OwnerListItemDto {
  id: string;
  name: string;
  email: string;
  status: string;
  tenantCount: number;
  lastLoginAt: string | null;
  createdAt: string;
}

export interface OwnerAddressDto {
  cep: string;
  logradouro: string;
  numero: string;
  complemento?: string | null;
  bairro: string;
  cidade: string;
  uf: string;
}

export interface OwnerMetricsDto {
  totalEstablishments: number;
  activeEstablishments: number;
  trialEstablishments: number;
  suspendedEstablishments: number;
  plans: string[];
  estimatedMrr: number;
  daysAsClient: number;
}

export interface OwnerTenantDto {
  tenantId: string;
  tenantName: string;
  tenantSlug: string;
  status: string;
  planName: string | null;
  planPrice: number | null;
}

export interface OwnerDetailDto {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  documentType: 'CPF' | 'CNPJ' | null;
  documentNumber: string | null;
  notes: string | null;
  address: OwnerAddressDto | null;
  status: string;
  lastLoginAt: string | null;
  createdAt: string;
  metrics: OwnerMetricsDto;
  tenants: OwnerTenantDto[];
}

export interface ListOwnersResult {
  items: OwnerListItemDto[];
  total: number;
  page: number;
  pageSize: number;
}

export interface CreateOwnerRequest {
  name: string;
  email: string;
  phone?: string | null;
  documentType?: 'CPF' | 'CNPJ' | null;
  documentNumber?: string | null;
  cep?: string | null;
  logradouro?: string | null;
  numero?: string | null;
  complemento?: string | null;
  bairro?: string | null;
  cidade?: string | null;
  uf?: string | null;
}

export interface CreateOwnerResponse {
  id: string;
  activationToken: string;
  activationTokenExpiresAt: string;
}

export interface UpdateOwnerRequest {
  name: string;
  email: string;
  phone?: string | null;
  documentType?: 'CPF' | 'CNPJ' | null;
  documentNumber?: string | null;
  notes?: string | null;
  address?: OwnerAddressDto | null;
}

@Injectable({ providedIn: 'root' })
export class OwnerService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin/owners`;

  getAll(page = 1, pageSize = 10) {
    return this.http.get<ListOwnersResult>(`${this.base}?page=${page}&pageSize=${pageSize}`);
  }

  getById(id: string) {
    return this.http.get<OwnerDetailDto>(`${this.base}/${id}`);
  }

  create(request: CreateOwnerRequest) {
    return this.http.post<CreateOwnerResponse>(this.base, request);
  }

  update(id: string, request: UpdateOwnerRequest) {
    return this.http.put(`${this.base}/${id}`, request);
  }

  activate(id: string) {
    return this.http.patch(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string) {
    return this.http.patch(`${this.base}/${id}/deactivate`, {});
  }

  resendActivation(id: string) {
    return this.http.post<{ activationToken: string; expiresAt: string; emailSent: boolean }>(`${this.base}/${id}/resend-activation`, {});
  }
}
