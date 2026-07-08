import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Plus, Hexagon, House, User, X, LoaderCircle } from 'lucide-angular';
import { BusinessTypeService, BusinessTypeDto } from './business-type.service';
import { ToastService } from '../../../core/toast/toast.service';

@Component({
  selector: 'app-business-type-list',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Tipos de Negócio</h1>
          <p class="page-subtitle">{{ items().length }} tipo{{ items().length !== 1 ? 's' : '' }} cadastrado{{ items().length !== 1 ? 's' : '' }}</p>
        </div>
        <button class="btn btn-primary" (click)="drawerOpen.set(true)">
          <lucide-icon [img]="Plus" [size]="14" [strokeWidth]="2.5"></lucide-icon>
          Novo Tipo
        </button>
      </div>

      <!-- Loading -->
      @if (loading()) {
        <div class="loading-state"><div class="spinner"></div></div>
      }

      <!-- Empty -->
      @else if (items().length === 0) {
        <div class="empty-state">
          <lucide-icon [img]="Hexagon" [size]="40" [strokeWidth]="1.5"></lucide-icon>
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
                <span class="badge" [class.badge-success]="item.isActive" [class.badge-neutral]="!item.isActive">
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
                  <lucide-icon [img]="House" [size]="13" [strokeWidth]="2"></lucide-icon>
                  <span>{{ item.tenantCount }} tenant{{ item.tenantCount !== 1 ? 's' : '' }}</span>
                </div>
                <div class="bt-stat">
                  <lucide-icon [img]="User" [size]="13" [strokeWidth]="2"></lucide-icon>
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
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <lucide-icon [img]="Hexagon" [size]="16" [strokeWidth]="2"></lucide-icon>
            </div>
            <div>
              <h2 class="drawer-title">Novo Tipo de Negócio</h2>
              <p class="drawer-subtitle">Define o modelo de operação do tenant</p>
            </div>
          </div>
          <button class="btn-close" (click)="closeDrawer()">
            <lucide-icon [img]="X" [size]="16" [strokeWidth]="2"></lucide-icon>
          </button>
        </div>

        <div class="drawer-body">
          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="Hexagon" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Dados
            </div>
            <div class="field">
              <label class="field-label">Nome <span class="required">*</span></label>
              <input class="field-input" [(ngModel)]="form.name" placeholder="ex: Restaurante, Bar, Padaria" autofocus>
            </div>
            <div class="field">
              <label class="field-label">Descrição</label>
              <textarea class="field-input field-textarea" [(ngModel)]="form.description" placeholder="Descreva o tipo de negócio..." rows="3"></textarea>
            </div>
          </div>
        </div>

        <div class="drawer-footer">
          <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
          <button class="btn btn-primary" (click)="save()" [disabled]="saving()">
            @if (saving()) {
              <lucide-icon [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5" class="icon-spin"></lucide-icon>
              Salvando...
            } @else { Criar Tipo }
          </button>
        </div>
      </aside>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; max-width: 1200px; }

    .page-header { display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 24px; gap: 12px; }
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
      lucide-icon { color: #aaa; }
    }

    .form-section { display: flex; flex-direction: column; gap: 10px; padding: 14px; background: #fafafa; border: 1px solid #f0f0f3; border-radius: 10px; }
    .form-section-header { display: flex; align-items: center; gap: 6px; font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #999; lucide-icon { color: var(--color-brand); } }

    .field { display: flex; flex-direction: column; gap: 5px; }
    .field-label { font-size: 11px; font-weight: 600; color: #666; }
    .required { color: var(--color-brand); }
    .field-input {
      padding: 8px 10px; border: 1px solid #e8e8ec; border-radius: 7px;
      font-size: 13px; color: #111; outline: none; transition: border-color .15s, box-shadow .15s;
      background: #fff; width: 100%; box-sizing: border-box;
      &:focus { border-color: var(--color-brand); box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-brand) 12%, transparent); }
      &::placeholder { color: #ccc; }
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

  readonly Plus = Plus;
  readonly Hexagon = Hexagon;
  readonly House = House;
  readonly User = User;
  readonly X = X;
  readonly LoaderCircle = LoaderCircle;

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
