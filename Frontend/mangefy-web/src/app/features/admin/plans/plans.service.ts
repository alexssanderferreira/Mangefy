import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface PlanDto {
  id: string;
  name: string;
  description: string | null;
  monthlyPrice: number;
  maxTables: number;
  maxMenuItems: number;
  maxUsers: number;
  maxCustomRoles: number;
  status: 'Active' | 'Inactive';
}

export interface CreatePlanRequest {
  name: string;
  description: string | null;
  monthlyPrice: number;
  maxTables: number;
  maxMenuItems: number;
  maxUsers: number;
  maxCustomRoles: number;
}

export interface UpdatePlanRequest {
  monthlyPrice: number;
  maxTables: number;
  maxMenuItems: number;
  maxUsers: number;
  maxCustomRoles: number;
  description: string | null;
}

@Injectable({ providedIn: 'root' })
export class PlansService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin/plans`;

  getAll() { return this.http.get<PlanDto[]>(this.base); }

  create(req: CreatePlanRequest) { return this.http.post<{ id: string }>(this.base, req); }

  update(id: string, req: UpdatePlanRequest) { return this.http.put(`${this.base}/${id}`, req); }

  activate(id: string)   { return this.http.patch(`${this.base}/${id}/activate`, null); }
  deactivate(id: string) { return this.http.patch(`${this.base}/${id}/deactivate`, null); }
  delete(id: string)     { return this.http.delete(`${this.base}/${id}`); }
  changeTenantPlan(tenantId: string, newPlanId: string) {
    return this.http.patch(`${environment.apiUrl}/tenants/${tenantId}/plan`, { newPlanId });
  }
}
