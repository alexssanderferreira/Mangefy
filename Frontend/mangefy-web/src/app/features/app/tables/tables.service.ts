import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/auth/auth.service';

export type TableStatus = 'Available' | 'Occupied' | 'Reserved' | 'Unavailable';

export interface TableDto {
  id: string;
  number: string;
  capacity: number;
  status: TableStatus;
  section: string | null;
}

export interface SaveTableRequest {
  number: string;
  capacity: number;
  section?: string | null;
}

@Injectable({ providedIn: 'root' })
export class TablesService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  private get base(): string {
    return `${environment.apiUrl}/tenants/${this.auth.user()?.tenantId}/tables`;
  }

  getAll(): Observable<TableDto[]> {
    return this.http.get<TableDto[]>(this.base);
  }

  create(req: SaveTableRequest): Observable<void> {
    return this.http.post<{ id: string }>(this.base, req).pipe(map(() => undefined));
  }

  update(id: string, req: SaveTableRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  setStatus(id: string, status: 'Available' | 'Unavailable'): Observable<void> {
    return this.http.patch<void>(`${this.base}/${id}/status`, { status });
  }
}
