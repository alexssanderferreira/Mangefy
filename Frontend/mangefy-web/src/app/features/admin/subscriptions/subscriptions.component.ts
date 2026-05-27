import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SubscriptionService, SubscriptionDto, InvoiceDto } from './subscription.service';
import { ToastService } from '../../../core/toast/toast.service';

type DrawerMode = 'invoice' | 'payment' | null;

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Assinaturas</h1>
          <p class="page-subtitle">{{ filtered().length }} assinatura{{ filtered().length !== 1 ? 's' : '' }}</p>
        </div>
      </div>

      @if (error()) {
        <div class="alert-error">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
          {{ error() }}
        </div>
      }

      @if (loading()) {
        <div class="skel-rows">@for (i of [1,2,3,4,5]; track i) { <div class="skel-row"></div> }</div>
      }

      @if (!loading()) {
        <!-- Filter bar -->
        <div class="filter-bar">
          <div class="search-wrap">
            <svg class="search-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            <input class="search-input" type="text" placeholder="Buscar estabelecimento..." [value]="searchTerm()" (input)="searchTerm.set($any($event.target).value)" />
          </div>
          <select class="filter-select" [value]="filterStatus()" (change)="filterStatus.set($any($event.target).value)">
            <option value="">Todos os status</option>
            <option value="Pending">Pendente</option>
            <option value="Paid">Pago</option>
            <option value="Overdue">Atrasado</option>
            <option value="none">Sem faturas</option>
          </select>
        </div>

        <div class="table-wrap">
          <table class="data-table">
            <thead><tr>
              <th>Estabelecimento</th>
              <th>Plano</th>
              <th>Próx. Vencimento</th>
              <th>Último Valor</th>
              <th>Status</th>
              <th>Inadimplências</th>
              <th></th>
            </tr></thead>
            <tbody>
              @if (filtered().length === 0) {
                <tr><td colspan="7" class="empty-cell">Nenhuma assinatura encontrada.</td></tr>
              }
              @for (s of filtered(); track s.id) {
                <tr class="clickable-row" (click)="router.navigate(['/admin/tenants', s.tenantId])">
                  <td>
                    <div class="cell-primary">{{ s.tenantName }}</div>
                    <div class="cell-sub">{{ s.tenantSlug }}</div>
                  </td>
                  <td>{{ s.planName }}</td>
                  <td>{{ formatDate(s.nextDueDate) }}</td>
                  <td>{{ s.latestInvoiceAmount != null ? ('R$ ' + s.latestInvoiceAmount.toFixed(2)) : '–' }}</td>
                  <td>
                    @if (s.latestInvoiceStatus) {
                      <span [class]="'badge ' + statusClass(s.latestInvoiceStatus)">{{ statusLabel(s.latestInvoiceStatus) }}</span>
                    } @else {
                      <span class="badge badge-neutral">Sem faturas</span>
                    }
                  </td>
                  <td>
                    @if (s.overdueCount > 0) {
                      <span class="badge badge-danger">{{ s.overdueCount }} em atraso</span>
                    } @else {
                      <span class="text-muted">–</span>
                    }
                  </td>
                  <td>
                    <div class="row-actions" (click)="$event.stopPropagation()">
                      <button class="btn-icon" title="Gerar fatura" (click)="openInvoice(s)">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
                      </button>
                      @if (pendingInvoice(s)) {
                        <button class="btn-icon btn-icon-success" title="Confirmar pagamento" (click)="openPayment(s)">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="20 6 9 17 4 12"/></svg>
                        </button>
                      }
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }

    </div>

    <!-- Drawer: Generate Invoice -->
    @if (drawer() === 'invoice' && selected()) {
      <div class="drawer-overlay" (click)="closeDrawer()">
        <div class="drawer" (click)="$event.stopPropagation()">
          <div class="drawer-header">
            <h2 class="drawer-title">Gerar Fatura</h2>
            <button class="drawer-close" (click)="closeDrawer()">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="drawer-body">
            <div class="info-card">
              <div class="info-card-name">{{ selected()!.tenantName }}</div>
              <div class="info-card-sub">{{ selected()!.planName || 'Sem plano' }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Valor (R$)</label>
              <div class="input-group">
                <span class="input-addon">R$</span>
                <input class="form-control input-group-field" type="text" inputmode="decimal"
                  [value]="invoiceAmountStr"
                  (input)="invoiceAmountStr = $any($event.target).value"
                  placeholder="0,00" />
              </div>
            </div>
            <div class="form-group">
              <label class="form-label">Vencimento</label>
              <input class="form-control" type="date" [(ngModel)]="invoiceDueDate" />
            </div>
          </div>
          <div class="drawer-footer">
            <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
            <button class="btn btn-primary" (click)="generateInvoice()" [disabled]="acting()">
              {{ acting() ? 'Aguarde...' : 'Gerar Fatura' }}
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Drawer: Confirm Payment -->
    @if (drawer() === 'payment' && selected()) {
      <div class="drawer-overlay" (click)="closeDrawer()">
        <div class="drawer" (click)="$event.stopPropagation()">
          <div class="drawer-header">
            <h2 class="drawer-title">Confirmar Pagamento</h2>
            <button class="drawer-close" (click)="closeDrawer()">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="drawer-body">
            <p class="drawer-subtitle">{{ selected()!.tenantName }} · Fatura de R$ {{ pendingInvoice(selected()!)?.amount?.toFixed(2) }}</p>
            <div class="form-group">
              <label class="form-label">Data do Pagamento</label>
              <input class="form-control" type="date" [(ngModel)]="paymentPaidAt" />
            </div>
            <div class="form-group">
              <label class="form-label">Próximo Vencimento</label>
              <input class="form-control" type="date" [(ngModel)]="paymentNextDueDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Referência de Pagamento</label>
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

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
    }
    .page-title { font-size: 22px; font-weight: 700; color: var(--text-primary); margin: 0 0 4px; }
    .page-subtitle { font-size: 13px; color: var(--text-muted); margin: 0; }

    .alert-error {
      display: flex; align-items: center; gap: 8px;
      background: rgba(224,49,49,.1); border: 1px solid rgba(224,49,49,.3);
      color: var(--color-danger); border-radius: 8px; padding: 10px 14px;
      font-size: 13px; margin-bottom: 16px;
    }

    .skel-rows { display: flex; flex-direction: column; gap: 8px; }
    .skel-row { height: 48px; background: var(--surface-bg); border-radius: 8px; animation: pulse 1.5s infinite; }
    @keyframes pulse { 0%,100%{opacity:1} 50%{opacity:.5} }

    .filter-bar {
      display: flex; gap: 10px; margin-bottom: 16px; flex-wrap: wrap;
    }
    .search-wrap { position: relative; flex: 1; min-width: 200px; }
    .search-icon { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: var(--text-muted); pointer-events: none; }
    .search-input {
      width: 100%; padding: 8px 12px 8px 32px;
      border: 1px solid var(--surface-border); border-radius: 8px;
      background: var(--surface-bg); color: var(--text-primary);
      font-size: 13px; outline: none;
      &:focus { border-color: var(--color-brand); }
    }
    .filter-select {
      padding: 8px 12px; border: 1px solid var(--surface-border); border-radius: 8px;
      background: var(--surface-bg); color: var(--text-primary); font-size: 13px; outline: none;
      cursor: pointer;
    }

    .table-wrap { border: 1px solid var(--surface-border); border-radius: 10px; overflow: hidden; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 13px; }
    .data-table th {
      background: var(--surface-bg); color: var(--text-muted);
      font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: .5px;
      padding: 10px 14px; text-align: left; border-bottom: 1px solid var(--surface-border);
    }
    .data-table td { padding: 12px 14px; border-bottom: 1px solid var(--surface-border); color: var(--text-primary); vertical-align: middle; }
    .data-table tbody tr:last-child td { border-bottom: none; }
    .clickable-row { cursor: pointer; transition: background .12s; &:hover { background: var(--surface-bg); } }
    .empty-cell { text-align: center; color: var(--text-muted); padding: 32px !important; }

    .cell-primary { font-weight: 500; }
    .cell-sub { font-size: 11px; color: var(--text-muted); margin-top: 2px; }
    .text-muted { color: var(--text-muted); }

    .row-actions { display: flex; gap: 4px; }
    .btn-icon {
      width: 30px; height: 30px; border-radius: 6px; border: 1px solid var(--surface-border);
      background: transparent; color: var(--text-muted); cursor: pointer;
      display: flex; align-items: center; justify-content: center;
      transition: background .12s, color .12s;
      &:hover { background: var(--surface-bg); color: var(--text-primary); }
    }
    .btn-icon-success { &:hover { background: rgba(37,162,101,.1); color: #25a265; border-color: rgba(37,162,101,.3); } }

    .badge { font-size: 11px; font-weight: 600; padding: 2px 8px; border-radius: 99px; }
    .badge-pending { background: rgba(245,158,11,.12); color: #d97706; }
    .badge-paid { background: rgba(37,162,101,.12); color: #25a265; }
    .badge-overdue, .badge-danger { background: rgba(224,49,49,.12); color: var(--color-danger); }
    .badge-neutral { background: var(--surface-bg); color: var(--text-muted); }

    /* Drawer */
    .drawer-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,.4);
      display: flex; justify-content: flex-end; z-index: 200;
      animation: fadeIn .15s ease;
    }
    @keyframes fadeIn { from{opacity:0} to{opacity:1} }
    .drawer {
      width: 400px; max-width: 95vw; background: var(--surface-card);
      border-left: 1px solid var(--surface-border);
      display: flex; flex-direction: column; height: 100vh;
      animation: slideIn .2s ease;
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
    .drawer-subtitle { font-size: 13px; color: var(--text-muted); margin: 0 0 4px; }
    .drawer-footer {
      padding: 16px 24px; border-top: 1px solid var(--surface-border);
      display: flex; justify-content: flex-end; gap: 8px;
    }

    .info-card {
      background: var(--surface-bg); border: 1px solid var(--surface-border);
      border-radius: 10px; padding: 14px 16px; margin-bottom: 4px;
    }
    .info-card-name { font-size: 14px; font-weight: 600; color: var(--text-primary); }
    .info-card-sub { font-size: 12px; color: var(--text-muted); margin-top: 3px; }

    .input-group {
      display: flex; align-items: center;
      border: 1px solid var(--surface-border); border-radius: 8px;
      background: var(--surface-card); overflow: hidden;
      transition: border-color .15s;
      &:focus-within { border-color: var(--color-brand); }
    }
    .input-addon {
      padding: 0 12px; font-size: 13px; font-weight: 600;
      color: var(--text-muted); border-right: 1px solid var(--surface-border);
      background: var(--surface-bg); align-self: stretch;
      display: flex; align-items: center;
    }
    .input-group-field {
      flex: 1; min-width: 0; border: none !important; outline: none;
      background: transparent !important; padding: 10px 12px;
      font-size: 14px; color: var(--text-primary); border-radius: 0 !important;
    }

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
export class SubscriptionsComponent implements OnInit {
  private svc = inject(SubscriptionService);
  private toast = inject(ToastService);
  router = inject(Router);

  subscriptions = signal<SubscriptionDto[]>([]);
  loading = signal(true);
  error = signal('');
  acting = signal(false);

  drawer = signal<DrawerMode>(null);
  selected = signal<SubscriptionDto | null>(null);

  searchTerm = signal('');
  filterStatus = signal('');
  invoiceAmount: number | null = null;
  invoiceAmountStr = '';
  invoiceDueDate = '';
  paymentPaidAt = '';
  paymentNextDueDate = '';
  paymentRef = '';
  paymentNotes = '';

  filtered = computed(() => {
    let list = this.subscriptions();
    const q = this.searchTerm().toLowerCase();
    if (q) list = list.filter(s => s.tenantName.toLowerCase().includes(q) || s.tenantSlug.toLowerCase().includes(q));
    const fs = this.filterStatus();
    if (fs === 'none') list = list.filter(s => !s.latestInvoiceStatus);
    else if (fs) list = list.filter(s => s.latestInvoiceStatus === fs);
    return list;
  });

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.svc.getAll().subscribe({
      next: data => { this.subscriptions.set(data); this.loading.set(false); },
      error: () => { this.error.set('Erro ao carregar assinaturas.'); this.loading.set(false); }
    });
  }

  pendingInvoice(s: SubscriptionDto): InvoiceDto | undefined {
    return s.invoices.find(i => i.status === 'Pending' || i.status === 'Overdue');
  }

  openInvoice(s: SubscriptionDto) {
    this.selected.set(s);
    this.invoiceAmount = null;
    this.invoiceAmountStr = '';
    this.invoiceDueDate = s.nextDueDate;
    this.drawer.set('invoice');
  }

  openPayment(s: SubscriptionDto) {
    this.selected.set(s);
    const today = new Date();
    this.paymentPaidAt = `${today.getFullYear()}-${String(today.getMonth()+1).padStart(2,'0')}-${String(today.getDate()).padStart(2,'0')}`;
    const [y, m, day] = s.nextDueDate.split('T')[0].split('-').map(Number);
    const next = new Date(y, m, day); // month+1 because Date months are 0-indexed, m is already 1-indexed so m acts as m+1
    this.paymentNextDueDate = `${next.getFullYear()}-${String(next.getMonth()+1).padStart(2,'0')}-${String(next.getDate()).padStart(2,'0')}`;
    this.paymentRef = '';
    this.paymentNotes = '';
    this.drawer.set('payment');
  }

  closeDrawer() { this.drawer.set(null); this.selected.set(null); }

  generateInvoice() {
    const parsed = parseFloat(this.invoiceAmountStr.replace(',', '.'));
    if (!parsed || isNaN(parsed) || !this.invoiceDueDate) return;
    this.invoiceAmount = parsed;
    this.acting.set(true);
    this.svc.generateInvoice(this.selected()!.id, { amount: this.invoiceAmount, dueDate: this.invoiceDueDate }).subscribe({
      next: () => { this.toast.show('Fatura gerada com sucesso.'); this.closeDrawer(); this.load(); },
      error: () => { this.toast.show('Erro ao gerar fatura.', 'error'); this.acting.set(false); }
    });
  }

  confirmPayment() {
    const invoice = this.pendingInvoice(this.selected()!);
    if (!invoice || !this.paymentPaidAt || !this.paymentNextDueDate) return;
    this.acting.set(true);
    this.svc.confirmPayment(this.selected()!.id, invoice.id, {
      paidAt: this.paymentPaidAt,
      nextDueDate: this.paymentNextDueDate,
      paymentReference: this.paymentRef || null,
      notes: this.paymentNotes || null
    }).subscribe({
      next: () => { this.toast.show('Pagamento confirmado.'); this.closeDrawer(); this.load(); },
      error: () => { this.toast.show('Erro ao confirmar pagamento.', 'error'); this.acting.set(false); }
    });
  }

  statusLabel(s: string) {
    return { Pending: 'Pendente', Paid: 'Pago', Overdue: 'Atrasado' }[s] ?? s;
  }

  statusClass(s: string) {
    return { Pending: 'badge-pending', Paid: 'badge-paid', Overdue: 'badge-overdue' }[s] ?? 'badge-neutral';
  }

  formatDate(d: string) {
    if (!d) return '–';
    const [y, m, day] = d.split('T')[0].split('-');
    return `${day}/${m}/${y}`;
  }
}
