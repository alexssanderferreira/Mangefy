import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface PlanFeatureSetDto {
  id: string;
  planId: string;
  businessTypeId: string;
  enabledFeatures: string[];
}

@Injectable({ providedIn: 'root' })
export class FeatureMatrixService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin`;

  getFeatureSets(planId: string) {
    return this.http.get<PlanFeatureSetDto[]>(`${this.base}/plans/${planId}/feature-sets`);
  }

  upsert(planId: string, businessTypeId: string, enabledFeatures: string[]) {
    return this.http.put<{ id: string }>(
      `${this.base}/plans/${planId}/feature-sets/${businessTypeId}`,
      { enabledFeatures }
    );
  }
}
