import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BusinessTypeService, BusinessTypeDto } from './business-type.service';
import { ToastService } from '../../../core/toast/toast.service';

@Component({
  selector: 'app-business-type-list',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Tipos de Negócio</h1>
          <p class="page-subtitle">{{ items().length }} tipo{{ items().length !== 1 ? 's' : '' }} cadastrado{{ items().length !== 1 ? 's' : '' }}</p>
        </div>
        <button class="btn btn-primary" (click)="drawerOpen.set(true)">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Novo Tipo
        </button>
      </div>

      <!-- Loading -->
      @if (loading()) {
        <div class="loading-state"><div class="spin"></div></div>
      }

      <!-- Empty -->
      @else if (items().length === 0) {
        <div class="empty-state">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>
          <p>Nenhum tipo de negócio cadastrado</p>
          <button class="btn btn-primary btn-sm" (click)="drawerOpen.set(true)">Criar primeiro tipo</button>
        </div>
      }

      <!-- Grid -->
      @else {
        <div class="bt-grid">
          @for (item of items(); track item.id) {
            <div class="bt-card" (click)="goToDetail(item.id)">
              <div class="bt-card-head">
                <div class="bt-avatar">{{ item.name.charAt(0).toUpperCase() }}</div>
                <span class="badge" [class.badge-active]="item.isActive" [class.badge-inactive]="!item.isActive">
                  {{ item.isActive ? 'Ativo' : 'Inativo' }}
                </span>
              </div>
              <h3 class="bt-name">{{ item.name }}</h3>
              @if (item.description) {
                <p class="bt-desc">{{ item.description }}</p>
              } @else {
                <p class="bt-desc bt-desc--empty">Sem descrição</p>
              }
              <div class="bt-footer">
                <div class="bt-stat">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
                  <span>{{ item.tenantCount }} tenant{{ item.tenantCount !== 1 ? 's' : '' }}</span>
                </div>
                <div class="bt-stat">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
                  <span>{{ item.roleTemplates.length }} template{{ item.roleTemplates.length !== 1 ? 's' : '' }}</span>
                </div>
              </div>
            </div>
          }
        </div>
      }
    </div>

    <!-- Drawer: Criar -->
    @if (drawerOpen()) {
      <div class="drawer-overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <h2 class="drawer-title">Novo Tipo de Negócio</h2>
          <button class="drawer-close" (click)="closeDrawer()">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>

        <div class="drawer-body">
          <div class="field">
            <label class="field-label">Nome <span class="required">*</span></label>
            <input class="field-input" [(ngModel)]="form.name" placeholder="ex: Restaurante, Bar, Padaria" autofocus>
          </div>
          <div class="field">
            <label class="field-label">Descrição</label>
            <textarea class="field-input field-textarea" [(ngModel)]="form.description" placeholder="Descrição opcional..." rows="3"></textarea>
          </div>
        </div>

        <div class="drawer-footer">
          <button class="btn btn-primary" (click)="save()" [disabled]="saving()">
            {{ saving() ? 'Salvando...' : 'Criar Tipo' }}
          </button>
          <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
        </div>
      </aside>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; max-width: 1200px; }

    .page-header { display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 24px; gap: 12px; }
    .page-title  { font-size: 22px; font-weight: 700; color: #111; }
    .page-subtitle { font-size: 13px; color: #aaa; margin-top: 3px; }

    /* Grid de cards */
    .bt-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 16px;
    }

    .bt-card {
      background: #fff; border: 1px solid #e8e8ec; border-radius: 14px;
      padding: 20px; cursor: pointer; transition: all .15s;
      display: flex; flex-direction: column; gap: 8px;
      &:hover { border-color: #d0d0d8; box-shadow: 0 4px 16px rgba(0,0,0,.06); transform: translateY(-1px); }
    }

    .bt-card-head {
      display: flex; align-items: center; justify-content: space-between; margin-bottom: 4px;
    }

    .bt-avatar {
      width: 40px; height: 40px; border-radius: 10px;
      background: var(--color-brand); color: #fff;
      display: flex; align-items: center; justify-content: center;
      font-size: 18px; font-weight: 800; flex-shrink: 0;
    }

    .bt-name { font-size: 15px; font-weight: 700; color: #111; margin: 0; }

    .bt-desc {
      font-size: 13px; color: #666; margin: 0;
      display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
      &--empty { color: #bbb; font-style: italic; }
    }

    .bt-footer {
      display: flex; align-items: center; justify-content: space-between;
      margin-top: 8px; padding-top: 12px; border-top: 1px solid #f0f0f3;
    }

    .bt-stat {
      display: flex; align-items: center; gap: 5px;
      font-size: 12px; color: #888; font-weight: 500;
      svg { color: #aaa; }
    }

    .badge {
      display: inline-block; padding: 3px 10px; border-radius: 99px;
      font-size: 11px; font-weight: 700;
      &-active   { background: #dcfce7; color: #15803d; }
      &-inactive { background: #f4f4f5; color: #71717a; }
    }

    .btn {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 8px 16px; border-radius: 8px;
      font-size: 13px; font-weight: 600; cursor: pointer;
      border: 1px solid transparent; transition: all .15s;
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
    .btn-primary { background: var(--color-brand); color: #fff; &:hover { opacity: .9; } }
    .btn-ghost   { background: #f4f4f5; color: #555; border-color: #e8e8ec; &:hover { background: #ebebef; } }
    .btn-sm      { padding: 6px 14px; font-size: 12px; }

    .loading-state { display: flex; justify-content: center; padding: 60px; }
    .spin {
      width: 32px; height: 32px; border-radius: 50%;
      border: 3px solid #f0f0f3; border-top-color: var(--color-brand);
      animation: spin .7s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }

    .empty-state {
      display: flex; flex-direction: column; align-items: center; justify-content: center;
      gap: 12px; padding: 80px 20px; color: #ccc;
      p { font-size: 14px; color: #999; }
      svg { color: #ddd; }
    }

    /* Drawer */
    .drawer-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,.35); z-index: 100;
      backdrop-filter: blur(2px); animation: fadeIn .15s ease;
    }
    .drawer {
      position: fixed; top: 0; right: 0; bottom: 0; width: 420px;
      background: #fff; z-index: 101; display: flex; flex-direction: column;
      box-shadow: -8px 0 40px rgba(0,0,0,.12);
      animation: slideIn .2s ease;
    }
    @keyframes fadeIn  { from { opacity: 0; } to { opacity: 1; } }
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }

    .drawer-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 20px 24px; border-bottom: 1px solid #f0f0f3;
    }
    .drawer-title { font-size: 16px; font-weight: 700; color: #111; }
    .drawer-close {
      display: flex; align-items: center; justify-content: center;
      width: 32px; height: 32px; border: none; background: #f4f4f5;
      border-radius: 8px; cursor: pointer; color: #666;
      transition: all .15s; &:hover { background: #ebebef; color: #111; }
    }
    .drawer-body   { flex: 1; overflow-y: auto; padding: 24px; display: flex; flex-direction: column; gap: 16px; }
    .drawer-footer { padding: 16px 24px; border-top: 1px solid #f0f0f3; display: flex; gap: 8px; }

    .field { display: flex; flex-direction: column; gap: 6px; }
    .field-label { font-size: 12px; font-weight: 600; color: #555; }
    .required { color: var(--color-brand); }
    .field-input {
      padding: 9px 12px; border: 1px solid #e8e8ec; border-radius: 8px;
      font-size: 13px; color: #111; outline: none; transition: border-color .15s;
      background: #fff; width: 100%; box-sizing: border-box;
      &:focus { border-color: var(--color-brand); }
    }
    .field-textarea { resize: vertical; min-height: 80px; font-family: inherit; }

    @media (max-width: 768px) {
      .page { padding: 14px; max-width: 100%; }
      .drawer { width: 100%; }
      .bt-grid { grid-template-columns: 1fr; }
    }
  `],
})
export class BusinessTypeListComponent implements OnInit {
  private svc    = inject(BusinessTypeService);
  private router = inject(Router);
  private toast  = inject(ToastService);

  items      = signal<BusinessTypeDto[]>([]);
  loading    = signal(true);
  drawerOpen = signal(false);
  saving     = signal(false);

  form = { name: '', description: '' };

  ngOnInit() {
    this.load();
  }

  private load() {
    this.loading.set(true);
    this.svc.getAll().subscribe({
      next:  items => { this.items.set(items); this.loading.set(false); },
      error: ()    => { this.loading.set(false); this.toast.error('Erro ao carregar tipos de negócio.'); },
    });
  }

  goToDetail(id: string) {
    this.router.navigate(['/admin/business-types', id]);
  }

  closeDrawer() {
    this.drawerOpen.set(false);
    this.form = { name: '', description: '' };
  }

  save() {
    if (!this.form.name.trim()) { this.toast.error('O nome é obrigatório.'); return; }
    this.saving.set(true);
    this.svc.create({ name: this.form.name, description: this.form.description || null }).subscribe({
      next: ({ id }) => {
        this.saving.set(false);
        this.closeDrawer();
        this.toast.success('Tipo de negócio criado com sucesso!');
        this.router.navigate(['/admin/business-types', id]);
      },
      error: (err: any) => {
        this.saving.set(false);
        this.toast.error(err?.error?.message ?? 'Erro ao criar tipo de negócio.');
      },
    });
  }
}
