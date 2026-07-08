import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideAngularModule, Check, Search, X } from 'lucide-angular';
import { SubscriptionService, SubscriptionDto, InvoiceDto } from './subscription.service';
import { ToastService } from '../../../core/toast/toast.service';

@Component({
  selector: 'app-overdue',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <div class="page">

      <div class="page-header">
        <div>
          <h1 class="page-title">Inadimplências</h1>
          <p class="page-subtitle">
            @if (!loading()) {
              {{ overdueItems().length }} assinatura{{ overdueItems().length !== 1 ? 's' : '' }} com fatura{{ overdueItems().length !== 1 ? 's' : '' }} em atraso
            }
          </p>
        </div>
      </div>

      @if (error()) {
        <div class="alert-error">{{ error() }}</div>
      }

      @if (loading()) {
        <div class="skel-rows">@for (i of [1,2,3,4]; track i) { <div class="skeleton-row" style="height:60px"></div> }</div>
      }

      @if (!loading() && overdueItems().length === 0) {
        <div class="empty-state">
          <div class="empty-icon">
            <lucide-icon [img]="Check" [size]="36" [strokeWidth]="1.5"></lucide-icon>
          </div>
          <p class="empty-title">Tudo em dia!</p>
          <p class="empty-sub">Nenhuma assinatura com faturas em atraso.</p>
        </div>
      }

      @if (!loading() && overdueItems().length > 0) {
        <div class="summary-row">
          <div class="summary-card">
            <div class="summary-value">{{ overdueItems().length }}</div>
            <div class="summary-label">Assinaturas inadimplentes</div>
          </div>
          <div class="summary-card">
            <div class="summary-value">{{ totalOverdueInvoices() }}</div>
            <div class="summary-label">Faturas em atraso</div>
          </div>
          <div class="summary-card">
            <div class="summary-value">R$ {{ totalOverdueAmount().toFixed(2) }}</div>
            <div class="summary-label">Valor total em atraso</div>
          </div>
        </div>

        <div class="filter-bar">
          <div class="search-wrap">
            <lucide-icon [img]="Search" [size]="14" [strokeWidth]="2" class="search-icon"></lucide-icon>
            <input class="search-input" type="text" placeholder="Buscar estabelecimento..." [value]="searchTerm()" (input)="searchTerm.set($any($event.target).value)" />
          </div>
        </div>

        <div class="table-wrap">
          <table class="data-table">
            <thead><tr>
              <th>Estabelecimento</th>
              <th>Plano</th>
              <th>Faturas em atraso</th>
              <th>Valor total</th>
              <th>Fatura mais antiga</th>
              <th></th>
            </tr></thead>
            <tbody>
              @for (s of filtered(); track s.id) {
                <tr class="main-row clickable-row" (click)="router.navigate(['/admin/tenants', s.tenantId])">
                  <td>
                    <div class="cell-primary">{{ s.tenantName }}</div>
                    <div class="cell-sub">{{ s.tenantSlug }}</div>
                  </td>
                  <td>{{ s.planName }}</td>
                  <td>
                    <span class="badge badge-danger">{{ overdueInvoices(s).length }} fatura{{ overdueInvoices(s).length !== 1 ? 's' : '' }}</span>
                  </td>
                  <td class="amount-col">R$ {{ overdueTotal(s).toFixed(2) }}</td>
                  <td class="date-overdue">{{ oldestOverdue(s) }}</td>
                  <td>
                    <div class="row-actions" (click)="$event.stopPropagation()">
                      <button class="btn btn-sm btn-primary" (click)="openPayment(s)" [disabled]="acting()">
                        Confirmar Pagamento
                      </button>
                    </div>
                  </td>
                </tr>
                @for (inv of overdueInvoices(s); track inv.id) {
                  <tr class="sub-row">
                    <td colspan="2" class="sub-indent">
                      <span class="sub-label">Fatura venc. {{ formatDate(inv.dueDate) }}</span>
                    </td>
                    <td></td>
                    <td class="amount-col sub-amount">R$ {{ inv.amount.toFixed(2) }}</td>
                    <td colspan="2"></td>
                  </tr>
                }
              }
            </tbody>
          </table>
        </div>
      }

    </div>

    @if (showPayment() && selectedSub()) {
      <div class="drawer-overlay" (click)="closeDrawer()">
        <div class="drawer" (click)="$event.stopPropagation()">
          <div class="drawer-header">
            <h2 class="drawer-title">Confirmar Pagamento</h2>
            <button class="btn-close" (click)="closeDrawer()">
              <lucide-icon [img]="X" [size]="16" [strokeWidth]="2"></lucide-icon>
            </button>
          </div>
          <div class="drawer-body">
            <div class="info-card">
              <div class="info-card-name">{{ selectedSub()!.tenantName }}</div>
              <div class="info-card-sub">{{ selectedSub()!.planName }}</div>
            </div>

            <div class="invoice-summary">
              <div class="invoice-summary-row">
                <span>Fatura a pagar</span>
                <strong>R$ {{ overdueInvoices(selectedSub()!)[0]?.amount?.toFixed(2) }}</strong>
              </div>
              <div class="invoice-summary-row">
                <span>Vencimento</span>
                <strong class="text-danger">{{ formatDate(overdueInvoices(selectedSub()!)[0]?.dueDate ?? '') }}</strong>
              </div>
              @if (overdueInvoices(selectedSub()!).length > 1) {
                <div class="invoice-summary-row text-warn">
                  <span>Faturas restantes após confirmação</span>
                  <strong>{{ overdueInvoices(selectedSub()!).length - 1 }}</strong>
                </div>
              }
            </div>

            <div class="form-group">
              <label class="form-label">Data do pagamento</label>
              <input class="form-control" type="date" [(ngModel)]="paymentPaidAt" />
            </div>
            <div class="form-group">
              <label class="form-label">Próximo vencimento</label>
              <input class="form-control" type="date" [(ngModel)]="paymentNextDueDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Referência de pagamento</label>
              <input class="form-control" type="text" [(ngModel)]="paymentRef" placeholder="Número do boleto, código PIX..." />
            </div>
            <div class="form-group">
              <label class="form-label">Observações</label>
              <textarea class="form-control" rows="3" [(ngModel)]="paymentNotes"></textarea>
            </div>
          </div>
          <div class="drawer-footer">
            <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
            <button class="btn btn-primary" (click)="confirmPayment()" [disabled]="acting()">
              {{ acting() ? 'Aguarde...' : 'Confirmar Pagamento' }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .page { padding: 28px 32px; max-width: 1200px; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-subtitle { font-size: 13px; color: var(--text-muted); margin: 0; }

    .skel-rows { display: flex; flex-direction: column; gap: 8px; }

    .empty-icon {
      width: 64px; height: 64px; border-radius: 50%;
      background: rgba(37,162,101,.1); color: #25a265;
      display: flex; align-items: center; justify-content: center; margin: 0 auto 16px;
    }
    .empty-title { font-size: 18px; font-weight: 700; color: var(--text-primary); margin: 0 0 6px; }
    .empty-sub { font-size: 14px; color: var(--text-muted); margin: 0; }

    .summary-row { display: grid; grid-template-columns: repeat(3,1fr); gap: 16px; margin-bottom: 24px; }
    .summary-card {
      padding: 20px 24px; border-radius: 12px;
      background: rgba(var(--color-danger-rgb),.06); border: 1px solid rgba(var(--color-danger-rgb),.2);
    }
    .summary-value { font-size: 28px; font-weight: 800; color: var(--color-danger); line-height: 1; margin-bottom: 6px; }
    .summary-label { font-size: 12px; color: var(--text-muted); font-weight: 500; }

    .filter-bar { display: flex; gap: 10px; margin-bottom: 16px; }
    .search-wrap { position: relative; flex: 1; max-width: 320px; }
    .search-icon { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: var(--text-muted); pointer-events: none; }
    .search-input {
      width: 100%; padding: 8px 12px 8px 32px;
      border: 1px solid var(--surface-border); border-radius: 8px;
      background: var(--surface-bg); color: var(--text-primary); font-size: 13px; outline: none;
      &:focus { border-color: var(--color-brand); }
    }

    .table-wrap { border: 1px solid var(--surface-border); border-radius: 10px; overflow: hidden; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
    .data-table th {
      background: var(--surface-bg); color: var(--text-muted);
      font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: .5px;
      padding: 10px 14px; text-align: left; border-bottom: 1px solid var(--surface-border);
    }
    .data-table td { padding: 12px 14px; border-bottom: 1px solid var(--surface-border); color: var(--text-primary); vertical-align: middle; }
    .main-row td { border-bottom: none; }
    .clickable-row { cursor: pointer; transition: background .12s; &:hover { background: var(--surface-bg); } }
    .sub-row { background: rgba(var(--color-danger-rgb),.02); }
    .sub-row td { padding: 6px 14px; border-bottom: 1px solid rgba(var(--color-danger-rgb),.06); font-size: 12px; }
    .sub-row:last-of-type td { border-bottom: 1px solid var(--surface-border); }
    .sub-indent { padding-left: 28px !important; }
    .sub-label { color: var(--text-muted); }
    .sub-amount { color: var(--color-danger); font-weight: 600; }

    .cell-primary { font-weight: 500; }
    .cell-sub { font-size: 11px; color: var(--text-muted); margin-top: 2px; }
    .amount-col { font-weight: 600; color: var(--color-danger); }
    .date-overdue { color: var(--color-danger); font-size: 12px; font-weight: 500; }
    .row-actions { display: flex; gap: 4px; }

    .info-card {
      background: var(--surface-bg); border: 1px solid var(--surface-border);
      border-radius: 10px; padding: 14px 16px;
    }
    .info-card-name { font-size: 14px; font-weight: 600; color: var(--text-primary); }
    .info-card-sub { font-size: 12px; color: var(--text-muted); margin-top: 3px; }

    .invoice-summary {
      background: rgba(var(--color-danger-rgb),.05); border: 1px solid rgba(var(--color-danger-rgb),.15);
      border-radius: 10px; padding: 14px 16px; display: flex; flex-direction: column; gap: 8px;
    }
    .invoice-summary-row {
      display: flex; justify-content: space-between; align-items: center;
      font-size: 13px; color: var(--text-muted);
      strong { color: var(--text-primary); }
    }
    .text-danger { color: var(--color-danger) !important; }
    .text-warn { color: #d97706; strong { color: #d97706; } }

    .form-group { display: flex; flex-direction: column; gap: 6px; }
    .form-label { font-size: 12px; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: .5px; }
    .form-control {
      padding: 9px 12px; border: 1px solid var(--surface-border); border-radius: 8px;
      background: var(--surface-bg); color: var(--text-primary); font-size: 13.5px; outline: none;
      &:focus { border-color: var(--color-brand); }
    }
    textarea.form-control { resize: vertical; font-family: inherit; }
  `]
})
export class OverdueComponent implements OnInit {
  private svc = inject(SubscriptionService);
  private toast = inject(ToastService);
  router = inject(Router);

  readonly Check = Check;
  readonly Search = Search;
  readonly X = X;

  overdueItems = signal<SubscriptionDto[]>([]);
  loading = signal(true);
  error = signal('');
  acting = signal(false);
  showPayment = signal(false);
  selectedSub = signal<SubscriptionDto | null>(null);

  searchTerm = signal('');
  paymentPaidAt = '';
  paymentNextDueDate = '';
  paymentRef = '';
  paymentNotes = '';

  filtered = computed(() => {
    const q = this.searchTerm().toLowerCase();
    if (!q) return this.overdueItems();
    return this.overdueItems().filter(s => s.tenantName.toLowerCase().includes(q) || s.tenantSlug.toLowerCase().includes(q));
  });

  totalOverdueInvoices = computed(() =>
    this.overdueItems().reduce((acc, s) => acc + this.overdueInvoices(s).length, 0));

  totalOverdueAmount = computed(() =>
    this.overdueItems().reduce((acc, s) => acc + this.overdueTotal(s), 0));

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.svc.getOverdue().subscribe({
      next: data => { this.overdueItems.set(data); this.loading.set(false); },
      error: () => { this.error.set('Erro ao carregar inadimplências.'); this.loading.set(false); }
    });
  }

  overdueInvoices(s: SubscriptionDto): InvoiceDto[] {
    return s.invoices
      .filter(i => i.status === 'Overdue')
      .sort((a, b) => a.dueDate.localeCompare(b.dueDate));
  }

  overdueTotal(s: SubscriptionDto) {
    return this.overdueInvoices(s).reduce((acc, i) => acc + i.amount, 0);
  }

  oldestOverdue(s: SubscriptionDto) {
    const inv = this.overdueInvoices(s)[0];
    return inv ? this.formatDate(inv.dueDate) : '–';
  }

  openPayment(s: SubscriptionDto) {
    this.selectedSub.set(s);
    const today = new Date();
    this.paymentPaidAt = today.toISOString().split('T')[0];
    // Sugere o próximo vencimento 1 mês após o nextDueDate atual
    const nd = new Date(s.nextDueDate.split('T')[0] + 'T12:00:00');
    nd.setMonth(nd.getMonth() + 1);
    this.paymentNextDueDate = nd.toISOString().split('T')[0];
    this.paymentRef = '';
    this.paymentNotes = '';
    this.showPayment.set(true);
  }

  closeDrawer() { this.showPayment.set(false); this.selectedSub.set(null); }

  confirmPayment() {
    const sub = this.selectedSub()!;
    const invoice = this.overdueInvoices(sub)[0];
    if (!invoice || !this.paymentPaidAt || !this.paymentNextDueDate) return;
    this.acting.set(true);
    this.svc.confirmPayment(sub.id, invoice.id, {
      paidAt: this.paymentPaidAt,
      nextDueDate: this.paymentNextDueDate,
      paymentReference: this.paymentRef || null,
      notes: this.paymentNotes || null
    }).subscribe({
      next: () => { this.toast.show('Pagamento confirmado.'); this.closeDrawer(); this.load(); this.acting.set(false); },
      error: () => { this.toast.show('Erro ao confirmar pagamento.', 'error'); this.acting.set(false); }
    });
  }

  formatDate(d: string | undefined | null) {
    if (!d) return '–';
    const [y, m, day] = d.split('T')[0].split('-');
    return `${day}/${m}/${y}`;
  }
}
