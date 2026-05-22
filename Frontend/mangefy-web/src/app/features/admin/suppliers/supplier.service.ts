import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface SupplierCategoryDto {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
}

export interface PlatformSupplierDto {
  id: string;
  name: string;
  cnpj: string | null;
  supplierCategoryId: string;
  website: string | null;
  email: string | null;
  phone: string | null;
  description: string | null;
  isActive: boolean;
}

export interface CreateSupplierRequest {
  name: string;
  supplierCategoryId: string;
  cnpj?: string | null;
  website?: string | null;
  email?: string | null;
  phone?: string | null;
  description?: string | null;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string | null;
}

@Injectable({ providedIn: 'root' })
export class SupplierService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin`;

  // ─── Categories ────────────────────────────────────────────────────────────
  getCategories() {
    return this.http.get<SupplierCategoryDto[]>(`${this.base}/supplier-categories`);
  }

  createCategory(body: CreateCategoryRequest) {
    return this.http.post<{ id: string }>(`${this.base}/supplier-categories`, body);
  }

  updateCategory(id: string, body: { name: string; description: string | null }) {
    return this.http.put(`${this.base}/supplier-categories/${id}`, body);
  }

  activateCategory(id: string) {
    return this.http.patch(`${this.base}/supplier-categories/${id}/activate`, {});
  }

  deactivateCategory(id: string) {
    return this.http.patch(`${this.base}/supplier-categories/${id}/deactivate`, {});
  }

  deleteCategory(id: string) {
    return this.http.delete(`${this.base}/supplier-categories/${id}`);
  }

  // ─── Suppliers ─────────────────────────────────────────────────────────────
  getSuppliers(categoryId?: string) {
    const params = categoryId ? `?categoryId=${categoryId}` : '';
    return this.http.get<PlatformSupplierDto[]>(`${this.base}/platform-suppliers${params}`);
  }

  createSupplier(body: CreateSupplierRequest) {
    return this.http.post<{ id: string }>(`${this.base}/platform-suppliers`, body);
  }

  updateSupplier(id: string, body: CreateSupplierRequest) {
    return this.http.put(`${this.base}/platform-suppliers/${id}`, body);
  }

  activateSupplier(id: string) {
    return this.http.patch(`${this.base}/platform-suppliers/${id}/activate`, {});
  }

  deactivateSupplier(id: string) {
    return this.http.patch(`${this.base}/platform-suppliers/${id}/deactivate`, {});
  }

  deleteSupplier(id: string) {
    return this.http.delete(`${this.base}/platform-suppliers/${id}`);
  }
}
