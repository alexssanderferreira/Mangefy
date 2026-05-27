import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface InvoiceDto {
  id: string;
  amount: number;
  currency: string;
  dueDate: string;
  paidAt: string | null;
  status: 'Pending' | 'Paid' | 'Overdue';
  paymentReference: string | null;
  notes: string | null;
}

export interface SubscriptionDto {
  id: string;
  tenantId: string;
  tenantName: string;
  tenantSlug: string;
  planId: string;
  planName: string;
  startDate: string;
  nextDueDate: string;
  latestInvoiceStatus: string | null;
  latestInvoiceAmount: number | null;
  latestInvoiceDueDate: string | null;
  overdueCount: number;
  invoices: InvoiceDto[];
}

export interface GenerateInvoiceRequest {
  amount: number;
  dueDate: string;
}

export interface ConfirmPaymentRequest {
  paidAt: string;
  nextDueDate: string;
  paymentReference: string | null;
  notes: string | null;
}

@Injectable({ providedIn: 'root' })
export class SubscriptionService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin/subscriptions`;

  getAll() { return this.http.get<SubscriptionDto[]>(this.base); }
  getOverdue() { return this.http.get<SubscriptionDto[]>(`${this.base}/overdue`); }

  generateInvoice(subscriptionId: string, req: GenerateInvoiceRequest) {
    return this.http.post<{ invoiceId: string }>(`${this.base}/${subscriptionId}/invoices`, req);
  }

  confirmPayment(subscriptionId: string, invoiceId: string, req: ConfirmPaymentRequest) {
    return this.http.patch(`${this.base}/${subscriptionId}/invoices/${invoiceId}/confirm`, req);
  }
}
