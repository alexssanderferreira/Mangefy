import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface RoleTemplateDto {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  permissions: string[];
  usageCount: number;
}

export interface BusinessTypeDto {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  roleTemplates: RoleTemplateDto[];
  tenantCount: number;
}

export interface CreateBusinessTypeRequest {
  name: string;
  description?: string | null;
}

export interface UpdateBusinessTypeRequest {
  name: string;
  description: string | null;
}

export interface RoleTemplateRequest {
  name: string;
  description: string | null;
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class BusinessTypeService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin/business-types`;

  getAll(): Observable<BusinessTypeDto[]> {
    return this.http.get<BusinessTypeDto[]>(this.base);
  }

  create(req: CreateBusinessTypeRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.base, req);
  }

  update(id: string, req: UpdateBusinessTypeRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  addTemplate(id: string, req: RoleTemplateRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.base}/${id}/role-templates`, req);
  }

  updateTemplate(id: string, templateId: string, req: RoleTemplateRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/role-templates/${templateId}`, req);
  }

  activateTemplate(id: string, templateId: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/role-templates/${templateId}/activate`, {});
  }

  deactivateTemplate(id: string, templateId: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/role-templates/${templateId}/deactivate`, {});
  }

  activate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/activate`, {});
  }

  deactivate(id: string): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/deactivate`, {});
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  deleteTemplate(id: string, templateId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}/role-templates/${templateId}`);
  }
}
