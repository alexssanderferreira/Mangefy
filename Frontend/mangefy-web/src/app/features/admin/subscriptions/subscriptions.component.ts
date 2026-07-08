import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { LucideAngularModule, CircleAlert, Search, DollarSign, Check, X, TriangleAlert } from 'lucide-angular';
import { SubscriptionService, SubscriptionDto, InvoiceDto } from './subscription.service';
import { ToastService } from '../../../core/toast/toast.service';

type DrawerMode = 'invoice' | 'payment' | null;

@Component({
  selector: 'app-subscriptions',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <div class="page">

      <div class="page-header">
        <div>
          <h1 class="page-title">Assinaturas</h1>
          <p class="page-subtitle">{{ filtered().length }} assinatura{{ filtered().length !== 1 ? 's' : '' }}</p>
        </div>
      </div>

      @if (error()) {
        <div class="alert-error">
          <lucide-icon [img]="CircleAlert" [size]="15" [strokeWidth]="2"></lucide-icon>
          {{ error() }}
        </div>
      }

      @if (loading()) {
        <div class="skel-rows">@for (i of [1,2,3,4,5]; track i) { <div class="skeleton-row" style="height:48px"></div> }</div>
      }

      @if (!loading()) {
        <div class="filter-bar">
          <div class="search-wrap">
            <lucide-icon [img]="Search" [size]="14" [strokeWidth]="2" class="search-icon"></lucide-icon>
            <input class="search-input" type="text" placeholder="Buscar estabelecimento..." [value]="searchTerm()" (input)="searchTerm.set($any($event.target).value)" />
          </div>
          <select class="filter-select" [value]="filterStatus()" (change)="filterStatus.set($any($event.target).value)">
            <option value="">Todos os status</option>
            <option value="EmDia">Em dia</option>
            <option value="AguardandoPagamento">Aguardando pagamento</option>
            <option value="Inadimplente">Inadimplente</option>
            <option value="SemFaturas">Sem faturas</option>
          </select>
        </div>

        <div class="table-wrap">
          <table class="data-table">
            <thead><tr>
              <th>Estabelecimento</th>
              <th>Plano</th>
              <th>Status</th>
              <th>Fatura em aberto</th>
              <th>Próx. vencimento</th>
              <th>Último pagamento</th>
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
                  <td><span [class]="'badge ' + statusClass(s.status)">{{ statusLabel(s.status) }}</span></td>
                  <td>
                    @if (openInvoice(s); as inv) {
                      <span class="amount-pending">R$ {{ inv.amount.toFixed(2) }}</span>
                      <span class="due-label">· venc. {{ formatDate(inv.dueDate) }}</span>
                    } @else {
                      <span class="text-muted">–</span>
                    }
                  </td>
                  <td>{{ formatDate(s.nextDueDate) }}</td>
                  <td>
                    @if (lastPaid(s); as inv) {
                      <span class="amount-paid">R$ {{ inv.amount.toFixed(2) }}</span>
                      @if (inv.paidAt) {
                        <span class="due-label">· {{ formatDate(inv.paidAt) }}</span>
                      }
                    } @else {
                      <span class="text-muted">–</span>
                    }
                  </td>
                  <td>
                    <div class="row-actions" (click)="$event.stopPropagation()">
                      @if (!openInvoice(s)) {
                        <button class="btn-icon" title="Gerar fatura" (click)="openInvoiceDrawer(s)">
                          <lucide-icon [img]="DollarSign" [size]="14" [strokeWidth]="2"></lucide-icon>
                        </button>
                      }
                      @if (openInvoice(s)) {
                        <button class="btn-icon btn-icon-success" title="Confirmar pagamento" (click)="openPaymentDrawer(s)">
                          <lucide-icon [img]="Check" [size]="14" [strokeWidth]="2"></lucide-icon>
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

    <!-- Drawer: Gerar Fatura -->
    @if (drawer() === 'invoice' && selected()) {
      <div class="drawer-overlay" (click)="closeDrawer()">
        <div class="drawer" (click)="$event.stopPropagation()">
          <div class="drawer-header">
            <h2 class="drawer-title">Gerar Fatura</h2>
            <button class="btn-close" (click)="closeDrawer()">
              <lucide-icon [img]="X" [size]="16" [strokeWidth]="2"></lucide-icon>
            </button>
          </div>
          <div class="drawer-body">
            <div class="info-card">
              <div class="info-card-name">{{ selected()!.tenantName }}</div>
              <div class="info-card-sub">{{ selected()!.planName }} · próx. venc. {{ formatDate(selected()!.nextDueDate) }}</div>
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
              <label class="form-label">Data de vencimento</label>
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

    <!-- Drawer: Confirmar Pagamento -->
    @if (drawer() === 'payment' && selected()) {
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
              <div class="info-card-name">{{ selected()!.tenantName }}</div>
              <div class="info-card-sub">
                Fatura: R$ {{ openInvoice(selected()!)?.amount?.toFixed(2) }}
                · venc. {{ formatDate(openInvoice(selected()!)?.dueDate ?? '') }}
              </div>
            </div>
            @if (selected()!.overdueCount > 0) {
              <div class="alert-warn">
                <lucide-icon [img]="TriangleAlert" [size]="14" [strokeWidth]="2"></lucide-icon>
                Esta assinatura tem {{ selected()!.overdueCount }} fatura{{ selected()!.overdueCount !== 1 ? 's' : '' }} em atraso. O pagamento será aplicado à mais antiga.
              </div>
            }
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

    .filter-bar { display: flex; gap: 10px; margin-bottom: 16px; flex-wrap: wrap; }
    .search-wrap { position: relative; flex: 1; min-width: 200px; max-width: 320px; }
    .search-icon { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: var(--text-muted); pointer-events: none; }
    .search-input {
      width: 100%; padding: 8px 12px 8px 32px;
      border: 1px solid var(--surface-border); border-radius: 8px;
      background: var(--surface-bg); color: var(--text-primary); font-size: 13px; outline: none;
      &:focus { border-color: var(--color-brand); }
    }
    .filter-select {
      padding: 8px 12px; border: 1px solid var(--surface-border); border-radius: 8px;
      background: var(--surface-bg); color: var(--text-primary); font-size: 13px; outline: none; cursor: pointer;
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
    .amount-pending { font-weight: 600; color: var(--text-primary); }
    .amount-paid { font-weight: 500; color: #25a265; }
    .due-label { font-size: 12px; color: var(--text-muted); margin-left: 2px; }

    .row-actions { display: flex; gap: 4px; }
    .btn-icon-success { color: #25a265; border-color: rgba(37,162,101,.3); background: rgba(37,162,101,.06);
      &:hover { background: rgba(37,162,101,.12); } }

    .info-card {
      background: var(--surface-bg); border: 1px solid var(--surface-border);
      border-radius: 10px; padding: 14px 16px;
    }
    .info-card-name { font-size: 14px; font-weight: 600; color: var(--text-primary); }
    .info-card-sub { font-size: 12px; color: var(--text-muted); margin-top: 3px; }

    .input-group {
      display: flex; align-items: center;
      border: 1px solid var(--surface-border); border-radius: 8px;
      background: var(--surface-card); overflow: hidden; transition: border-color .15s;
      &:focus-within { border-color: var(--color-brand); }
    }
    .input-addon {
      padding: 0 12px; font-size: 13px; font-weight: 600; color: var(--text-muted);
      border-right: 1px solid var(--surface-border); background: var(--surface-bg);
      align-self: stretch; display: flex; align-items: center;
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
  `]
})
export class SubscriptionsComponent implements OnInit {
  private svc = inject(SubscriptionService);
  private toast = inject(ToastService);
  router = inject(Router);

  readonly CircleAlert = CircleAlert;
  readonly Search = Search;
  readonly DollarSign = DollarSign;
  readonly Check = Check;
  readonly X = X;
  readonly TriangleAlert = TriangleAlert;

  subscriptions = signal<SubscriptionDto[]>([]);
  loading = signal(true);
  error = signal('');
  acting = signal(false);

  drawer = signal<DrawerMode>(null);
  selected = signal<SubscriptionDto | null>(null);

  searchTerm = signal('');
  filterStatus = signal('');
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
    if (fs) list = list.filter(s => s.status === fs);
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

  // Retorna a fatura pendente/em atraso mais antiga (a que precisa de ação)
  openInvoice(s: SubscriptionDto): InvoiceDto | undefined {
    return s.invoices
      .filter(i => i.status === 'Pending' || i.status === 'Overdue')
      .sort((a, b) => a.dueDate.localeCompare(b.dueDate))[0];
  }

  // Retorna o último pagamento confirmado
  lastPaid(s: SubscriptionDto): InvoiceDto | undefined {
    return s.invoices
      .filter(i => i.status === 'Paid')
      .sort((a, b) => b.dueDate.localeCompare(a.dueDate))[0];
  }

  openInvoiceDrawer(s: SubscriptionDto) {
    this.selected.set(s);
    this.invoiceAmountStr = '';
    // nextDueDate vem como "YYYY-MM-DD" do DateOnly do .NET
    this.invoiceDueDate = s.nextDueDate.split('T')[0];
    this.drawer.set('invoice');
  }

  openPaymentDrawer(s: SubscriptionDto) {
    this.selected.set(s);
    const today = new Date();
    this.paymentPaidAt = today.toISOString().split('T')[0];
    // Avança 1 mês a partir do nextDueDate
    const nd = new Date(s.nextDueDate.split('T')[0] + 'T12:00:00');
    nd.setMonth(nd.getMonth() + 1);
    this.paymentNextDueDate = nd.toISOString().split('T')[0];
    this.paymentRef = '';
    this.paymentNotes = '';
    this.drawer.set('payment');
  }

  closeDrawer() { this.drawer.set(null); this.selected.set(null); }

  generateInvoice() {
    const parsed = parseFloat(this.invoiceAmountStr.replace(',', '.'));
    if (!parsed || isNaN(parsed) || !this.invoiceDueDate) return;
    this.acting.set(true);
    this.svc.generateInvoice(this.selected()!.id, { amount: parsed, dueDate: this.invoiceDueDate }).subscribe({
      next: () => { this.toast.show('Fatura gerada com sucesso.'); this.closeDrawer(); this.load(); this.acting.set(false); },
      error: () => { this.toast.show('Erro ao gerar fatura.', 'error'); this.acting.set(false); }
    });
  }

  confirmPayment() {
    const invoice = this.openInvoice(this.selected()!);
    if (!invoice || !this.paymentPaidAt || !this.paymentNextDueDate) return;
    this.acting.set(true);
    this.svc.confirmPayment(this.selected()!.id, invoice.id, {
      paidAt: this.paymentPaidAt,
      nextDueDate: this.paymentNextDueDate,
      paymentReference: this.paymentRef || null,
      notes: this.paymentNotes || null
    }).subscribe({
      next: () => { this.toast.show('Pagamento confirmado.'); this.closeDrawer(); this.load(); this.acting.set(false); },
      error: () => { this.toast.show('Erro ao confirmar pagamento.', 'error'); this.acting.set(false); }
    });
  }

  statusLabel(s: SubscriptionDto['status']) {
    return {
      EmDia: 'Em dia',
      AguardandoPagamento: 'Aguardando pagamento',
      Inadimplente: 'Inadimplente',
      SemFaturas: 'Sem faturas',
    }[s] ?? s;
  }

  statusClass(s: SubscriptionDto['status']) {
    return {
      EmDia: 'badge-success',
      AguardandoPagamento: 'badge-warning',
      Inadimplente: 'badge-danger',
      SemFaturas: 'badge-neutral',
    }[s] ?? 'badge-neutral';
  }

  formatDate(d: string | undefined | null) {
    if (!d) return '–';
    const [y, m, day] = d.split('T')[0].split('-');
    return `${day}/${m}/${y}`;
  }
}
