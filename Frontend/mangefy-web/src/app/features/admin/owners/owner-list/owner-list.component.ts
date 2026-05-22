import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { OwnerService, OwnerListItemDto } from '../owner.service';

const STATUS_LABEL: Record<string, string> = {
  Active: 'Ativo', PendingActivation: 'Aguardando ativação', Inactive: 'Inativo',
};

@Component({
  selector: 'app-owner-list',
  standalone: true,
  imports: [FormsModule, DatePipe],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Clientes</h1>
          <p class="page-subtitle">{{ total() }} cliente{{ total() !== 1 ? 's' : '' }} cadastrado{{ total() !== 1 ? 's' : '' }}</p>
        </div>
        <button class="btn btn-primary" (click)="openCreate()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Novo Cliente
        </button>
      </div>

      <!-- Filtro -->
      <div class="filters">
        <div class="search-wrap">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          <input class="search-input" [ngModel]="search()" (ngModelChange)="search.set($event)" placeholder="Buscar cliente por nome ou e-mail..." />
        </div>
      </div>

      <!-- Error -->
      @if (error()) {
        <div class="alert-error">{{ error() }}</div>
      }

      <!-- Table -->
      <div class="table-wrap">
        <table class="table">
          <thead>
            <tr>
              <th>Cliente</th>
              <th>E-mail</th>
              <th>Status</th>
              <th>Estabelecimentos</th>
              <th>Último acesso</th>
              <th>Cadastrado em</th>
            </tr>
          </thead>
          <tbody>
            @if (loading()) {
              @for (i of [1,2,3,4,5]; track i) {
                <tr class="skeleton-row">
                  <td><div class="skel" style="width:60%"></div></td>
                  <td><div class="skel" style="width:80%"></div></td>
                  <td><div class="skel" style="width:50px"></div></td>
                  <td><div class="skel" style="width:30px"></div></td>
                  <td><div class="skel" style="width:70px"></div></td>
                  <td><div class="skel" style="width:70px"></div></td>
                </tr>
              }
            } @else if (filtered().length === 0) {
              <tr><td colspan="6" class="empty-cell">Nenhum cliente encontrado.</td></tr>
            } @else {
              @for (o of filtered(); track o.id) {
                <tr class="table-row" (click)="goDetail(o.id)">
                  <td class="td-name">
                    <span class="name-avatar">{{ o.name.charAt(0).toUpperCase() }}</span>
                    <span class="name-text">{{ o.name }}</span>
                  </td>
                  <td class="td-email">{{ o.email }}</td>
                  <td><span class="badge badge-{{ o.status }}">{{ statusLabel(o.status) }}</span></td>
                  <td class="td-count">
                    <span class="tenant-count">{{ o.tenantCount }}</span>
                  </td>
                  <td class="td-muted">{{ o.lastLoginAt ? (o.lastLoginAt | date:'dd/MM/yyyy') : '—' }}</td>
                  <td class="td-muted">{{ o.createdAt | date:'dd/MM/yyyy' }}</td>
                </tr>
              }
            }
          </tbody>
        </table>
      </div>

      <!-- Paginação -->
      @if (totalPages() > 1) {
        <div class="pagination">
          <button class="pg-btn" (click)="goPage(page() - 1)" [disabled]="page() === 1">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>
          </button>
          @for (p of pageNumbers(); track p) {
            @if (p === -1) { <span class="pg-dots">…</span> }
            @else { <button class="pg-btn" [class.pg-active]="p === page()" (click)="goPage(p)">{{ p }}</button> }
          }
          <button class="pg-btn" (click)="goPage(page() + 1)" [disabled]="page() === totalPages()">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="9 18 15 12 9 6"/></svg>
          </button>
          <span class="pg-info">{{ (page()-1)*pageSize()+1 }}–{{ min(page()*pageSize(), total()) }} de {{ total() }}</span>
        </div>
      }
    </div>

    <!-- Drawer criar -->
    @if (drawerOpen()) {
      <div class="overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <h3 class="drawer-title">Novo Cliente</h3>
          <button class="btn-close" (click)="closeDrawer()">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>
        <div class="drawer-body">
          @if (drawerError()) {
            <div class="alert-error mb-16">{{ drawerError() }}</div>
          }
          <div class="field">
            <label class="field-label">Nome completo *</label>
            <input class="field-input" [(ngModel)]="form.name" placeholder="Ex: João Silva" maxlength="200" />
          </div>
          <div class="field">
            <label class="field-label">E-mail *</label>
            <input class="field-input" type="email" [(ngModel)]="form.email" placeholder="joao@email.com.br" />
          </div>
          <div class="info-box">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
            Um link de ativação de 48h será enviado ao cliente para definir sua senha.
          </div>
        </div>
        <div class="drawer-footer">
          <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
          <button class="btn btn-primary" (click)="submit()" [disabled]="saving()">
            {{ saving() ? 'Criando...' : 'Criar Cliente' }}
          </button>
        </div>
      </aside>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; }
    .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .page-title  { font-size: 20px; font-weight: 700; color: #111; }
    .page-subtitle { font-size: 12px; color: #aaa; margin-top: 2px; }

    .filters { display: flex; gap: 10px; margin-bottom: 12px; }
    .search-wrap {
      flex: 1; display: flex; align-items: center; gap: 8px;
      background: #fff; border: 1px solid #e8e8ec; border-radius: 8px; padding: 7px 12px;
      svg { color: #bbb; flex-shrink: 0; }
      &:focus-within { border-color: var(--color-brand); }
    }
    .search-input { flex: 1; border: none; outline: none; font-size: 13px; color: #111; &::placeholder { color: #ccc; } }

    .table-wrap { background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; overflow: hidden; }
    .table {
      width: 100%; border-collapse: collapse;
      th { padding: 9px 14px; text-align: left; font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .06em; color: #bbb; border-bottom: 1px solid #f0f0f3; background: #fafafa; }
      td { padding: 9px 14px; border-bottom: 1px solid #f4f4f6; font-size: 13px; }
      tr:last-child td { border-bottom: none; }
    }
    .table-row { cursor: pointer; transition: background .1s; &:hover td { background: #f9f9fb; } }
    .td-name { display: flex; align-items: center; gap: 9px; }
    .name-avatar {
      flex-shrink: 0; width: 28px; height: 28px; border-radius: 8px;
      background: var(--color-brand); color: #fff;
      display: flex; align-items: center; justify-content: center;
      font-size: 11px; font-weight: 800;
    }
    .name-text { font-weight: 600; color: #111; }
    .td-email { color: #555; font-size: 12px; }
    .td-muted { color: #999; font-size: 12px; }
    .td-count { text-align: center; }
    .tenant-count {
      display: inline-flex; align-items: center; justify-content: center;
      width: 24px; height: 24px; border-radius: 50%;
      background: #f0f0f3; font-size: 12px; font-weight: 700; color: #333;
    }
    .empty-cell { text-align: center; padding: 48px; color: #ccc; font-size: 13px; }

    /* Skeleton */
    .skel { height: 12px; border-radius: 6px; background: linear-gradient(90deg, #f0f0f3 25%, #e8e8ec 50%, #f0f0f3 75%); background-size: 200% 100%; animation: shimmer 1.4s infinite; }
    @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }

    /* Badges */
    .badge {
      display: inline-block; padding: 3px 10px; border-radius: 99px;
      font-size: 11px; font-weight: 700; white-space: nowrap;
      &-Active           { background: #dcfce7; color: #15803d; }
      &-PendingActivation{ background: #fef3c7; color: #b45309; }
      &-Inactive         { background: #f4f4f5; color: #71717a; }
    }

    /* Pagination */
    .pagination { display: flex; align-items: center; gap: 4px; padding: 14px 16px; border-top: 1px solid #f0f0f3; flex-wrap: wrap; }
    .pg-btn { min-width: 32px; height: 32px; padding: 0 8px; border-radius: 7px; border: 1px solid #e8e8ec; background: #fff; color: #555; font-size: 13px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all .12s; &:hover:not(:disabled) { background: #f4f4f6; } &:disabled { opacity: .35; cursor: not-allowed; } &.pg-active { background: var(--color-brand); color: #fff; border-color: transparent; } }
    .pg-dots { padding: 0 4px; color: #aaa; font-size: 13px; }
    .pg-info { margin-left: 8px; font-size: 12px; color: #aaa; white-space: nowrap; }

    /* Alert */
    .alert-error { display: flex; align-items: center; gap: 8px; background: #fef2f2; color: #b91c1c; border: 1px solid #fecaca; border-radius: 10px; padding: 12px 14px; font-size: 13px; margin-bottom: 20px; }
    .mb-16 { margin-bottom: 16px; }

    /* Buttons */
    .btn { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border-radius: 8px; font-size: 13px; font-weight: 600; cursor: pointer; border: none; transition: all .15s; }
    .btn-primary { background: var(--color-brand); color: #fff; &:hover { opacity: .9; } &:disabled { opacity: .5; cursor: not-allowed; } }
    .btn-ghost   { background: #f4f4f5; color: #555; border: 1px solid #e8e8ec; &:hover { background: #ebebef; } }

    /* Drawer */
    .overlay { position: fixed; inset: 0; background: rgba(0,0,0,.3); z-index: 100; backdrop-filter: blur(2px); }
    .drawer { position: fixed; top: 0; right: 0; bottom: 0; width: 420px; background: #fff; border-left: 1px solid #e8e8ec; z-index: 101; display: flex; flex-direction: column; box-shadow: -8px 0 32px rgba(0,0,0,.1); animation: slideIn .2s ease; }
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }
    .drawer-header { display: flex; align-items: center; justify-content: space-between; padding: 20px 24px; border-bottom: 1px solid #f0f0f3; flex-shrink: 0; }
    .drawer-title { font-size: 16px; font-weight: 700; color: #111; }
    .btn-close { width: 32px; height: 32px; border-radius: 8px; border: none; background: #f4f4f5; color: #666; display: flex; align-items: center; justify-content: center; cursor: pointer; &:hover { background: #ebebef; } }
    .drawer-body { flex: 1; overflow-y: auto; padding: 24px; display: flex; flex-direction: column; gap: 16px; }
    .drawer-footer { padding: 16px 24px; border-top: 1px solid #f0f0f3; display: flex; gap: 10px; justify-content: flex-end; flex-shrink: 0; }

    /* Form */
    .field { display: flex; flex-direction: column; gap: 6px; }
    .field-label { font-size: 12px; font-weight: 600; color: #555; }
    .field-input { padding: 9px 12px; border: 1px solid #e8e8ec; border-radius: 8px; font-size: 13px; color: #111; outline: none; transition: border-color .15s; &:focus { border-color: var(--color-brand); } &::placeholder { color: #ccc; } }

    /* Info box */
    .info-box { display: flex; align-items: flex-start; gap: 8px; background: #f0f9ff; border: 1px solid #bae6fd; border-radius: 8px; padding: 10px 12px; font-size: 12px; color: #0369a1; svg { flex-shrink: 0; margin-top: 1px; } }
  `],
})
export class OwnerListComponent implements OnInit {
  private svc = inject(OwnerService);
  private router = inject(Router);

  owners    = signal<OwnerListItemDto[]>([]);
  loading   = signal(true);
  error     = signal('');
  search    = signal('');
  page      = signal(1);
  pageSize  = signal(10);
  total     = signal(0);
  totalPages = computed(() => Math.ceil(this.total() / this.pageSize()));

  drawerOpen  = signal(false);
  drawerError = signal('');
  saving      = signal(false);
  form = { name: '', email: '' };

  filtered = computed(() => {
    const q = this.search().toLowerCase();
    return this.owners().filter(o =>
      !q || o.name.toLowerCase().includes(q) || o.email.toLowerCase().includes(q)
    );
  });

  pageNumbers = computed(() => {
    const total = this.totalPages(); const cur = this.page();
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    const pages: number[] = [1];
    if (cur > 3) pages.push(-1);
    for (let i = Math.max(2, cur-1); i <= Math.min(total-1, cur+1); i++) pages.push(i);
    if (cur < total-2) pages.push(-1);
    pages.push(total);
    return pages;
  });

  ngOnInit() { this.load(); }

  private load() {
    this.loading.set(true);
    this.svc.getAll(this.page(), this.pageSize()).subscribe({
      next: res => { this.owners.set(res.items); this.total.set(res.total); this.loading.set(false); },
      error: () => { this.error.set('Não foi possível carregar os donos.'); this.loading.set(false); },
    });
  }

  goPage(p: number) { if (p < 1 || p > this.totalPages()) return; this.page.set(p); this.load(); }
  min(a: number, b: number) { return Math.min(a, b); }
  goDetail(id: string) { this.router.navigate(['/admin/owners', id]); }
  statusLabel(s: string) { return STATUS_LABEL[s] ?? s; }

  openCreate() {
    this.form = { name: '', email: '' };
    this.drawerError.set('');
    this.drawerOpen.set(true);
  }

  closeDrawer() { if (!this.saving()) this.drawerOpen.set(false); }

  submit() {
    this.drawerError.set('');
    if (!this.form.name.trim()) { this.drawerError.set('Nome é obrigatório.'); return; }
    if (!this.form.email.trim()) { this.drawerError.set('E-mail é obrigatório.'); return; }

    this.saving.set(true);
    this.svc.create({ name: this.form.name.trim(), email: this.form.email.trim() }).subscribe({
      next: res => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.load();
        this.router.navigate(['/admin/owners', res.id]);
      },
      error: (err) => {
        this.drawerError.set(err?.error?.message ?? 'Erro ao criar cliente.');
        this.saving.set(false);
      },
    });
  }
}
