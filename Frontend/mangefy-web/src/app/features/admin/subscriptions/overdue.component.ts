import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SubscriptionService, SubscriptionDto, InvoiceDto } from './subscription.service';
import { ToastService } from '../../../core/toast/toast.service';

@Component({
  selector: 'app-overdue',
  standalone: true,
  imports: [FormsModule],
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
        <div class="skel-rows">@for (i of [1,2,3,4]; track i) { <div class="skel-row"></div> }</div>
      }

      @if (!loading() && overdueItems().length === 0) {
        <div class="empty-state">
          <div class="empty-icon">
            <svg width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><polyline points="20 6 9 17 4 12"/></svg>
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
            <svg class="search-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
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
                      <button class="btn-sm btn-primary" (click)="openPayment(s)" [disabled]="acting()">
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
            <button class="drawer-close" (click)="closeDrawer()">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
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
    .page-title { font-size: 22px; font-weight: 700; color: var(--text-primary); margin: 0 0 4px; }
    .page-subtitle { font-size: 13px; color: var(--text-muted); margin: 0; }

    .alert-error {
      display: flex; align-items: center; gap: 8px;
      background: rgba(224,49,49,.1); border: 1px solid rgba(224,49,49,.3);
      color: var(--color-danger); border-radius: 8px; padding: 10px 14px;
      font-size: 13px; margin-bottom: 16px;
    }

    .skel-rows { display: flex; flex-direction: column; gap: 8px; }
    .skel-row { height: 60px; background: var(--surface-bg); border-radius: 8px; animation: pulse 1.5s infinite; }
    @keyframes pulse { 0%,100%{opacity:1} 50%{opacity:.5} }

    .empty-state { text-align: center; padding: 64px 20px; }
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
      background: rgba(224,49,49,.06); border: 1px solid rgba(224,49,49,.2);
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
    .sub-row { background: rgba(224,49,49,.02); }
    .sub-row td { padding: 6px 14px; border-bottom: 1px solid rgba(224,49,49,.06); font-size: 12px; }
    .sub-row:last-of-type td { border-bottom: 1px solid var(--surface-border); }
    .sub-indent { padding-left: 28px !important; }
    .sub-label { color: var(--text-muted); }
    .sub-amount { color: var(--color-danger); font-weight: 600; }

    .cell-primary { font-weight: 500; }
    .cell-sub { font-size: 11px; color: var(--text-muted); margin-top: 2px; }
    .amount-col { font-weight: 600; color: var(--color-danger); }
    .date-overdue { color: var(--color-danger); font-size: 12px; font-weight: 500; }
    .row-actions { display: flex; gap: 4px; }

    .badge { font-size: 11px; font-weight: 600; padding: 3px 9px; border-radius: 99px; }
    .badge-danger { background: rgba(224,49,49,.12); color: var(--color-danger); }

    .btn-sm {
      font-size: 12px; font-weight: 600; padding: 5px 12px; border-radius: 7px;
      cursor: pointer; border: none; transition: opacity .15s;
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
    .btn-primary { background: var(--color-brand); color: #fff; &:hover:not(:disabled){opacity:.88;} }

    /* Drawer */
    .drawer-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,.4);
      display: flex; justify-content: flex-end; z-index: 200; animation: fadeIn .15s ease;
    }
    @keyframes fadeIn { from{opacity:0} to{opacity:1} }
    .drawer {
      width: 420px; max-width: 95vw; background: var(--surface-card);
      border-left: 1px solid var(--surface-border);
      display: flex; flex-direction: column; height: 100vh; animation: slideIn .2s ease;
    }
    @keyframes slideIn { from{transform:translateX(40px);opacity:0} to{transform:translateX(0);opacity:1} }
    .drawer-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 20px 24px; border-bottom: 1px solid var(--surface-border);
    }
    .drawer-title { font-size: 16px; font-weight: 700; color: var(--text-primary); margin: 0; }
    .drawer-close {
      width: 28px; height: 28px; border-radius: 6px; border: none;
      background: transparent; color: var(--text-muted); cursor: pointer;
      display: flex; align-items: center; justify-content: center;
      &:hover { background: var(--surface-bg); }
    }
    .drawer-body { flex: 1; overflow-y: auto; padding: 24px; display: flex; flex-direction: column; gap: 16px; }
    .drawer-footer {
      padding: 16px 24px; border-top: 1px solid var(--surface-border);
      display: flex; justify-content: flex-end; gap: 8px;
    }

    .info-card {
      background: var(--surface-bg); border: 1px solid var(--surface-border);
      border-radius: 10px; padding: 14px 16px;
    }
    .info-card-name { font-size: 14px; font-weight: 600; color: var(--text-primary); }
    .info-card-sub { font-size: 12px; color: var(--text-muted); margin-top: 3px; }

    .invoice-summary {
      background: rgba(224,49,49,.05); border: 1px solid rgba(224,49,49,.15);
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

    .btn { display: inline-flex; align-items: center; gap: 7px; padding: 8px 16px; border-radius: 8px; font-size: 13px; font-weight: 600; cursor: pointer; border: none; transition: opacity .15s; &:disabled{opacity:.5;cursor:not-allowed;} }
    .btn-primary { background: var(--color-brand); color: #fff; &:hover:not(:disabled){opacity:.88;} }
    .btn-ghost { background: transparent; color: var(--text-primary); border: 1px solid var(--surface-border); &:hover{background:var(--surface-bg);} }
  `]
})
export class OverdueComponent implements OnInit {
  private svc = inject(SubscriptionService);
  private toast = inject(ToastService);
  router = inject(Router);

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
