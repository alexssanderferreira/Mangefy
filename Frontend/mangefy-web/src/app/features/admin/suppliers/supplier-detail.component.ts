import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { SupplierService, PlatformSupplierDto, SupplierCategoryDto } from './supplier.service';
import { ToastService } from '../../../core/toast/toast.service';

type Tab = 'info' | 'edit';

@Component({
  selector: 'app-supplier-detail',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">

      <!-- Breadcrumb -->
      <div class="breadcrumb">
        <button class="back-btn" (click)="goBack()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>
          Fornecedores
        </button>
        @if (supplier()) { <span class="sep">/</span> <span class="bc-current">{{ supplier()!.name }}</span> }
      </div>

      @if (loading()) {
        <div class="loading-state"><div class="spin"></div></div>
      } @else if (!supplier()) {
        <div class="empty-state">Fornecedor não encontrado.</div>
      } @else {

        <!-- Header -->
        <div class="detail-header">
          <div class="detail-meta">
            <div class="detail-avatar">{{ supplier()!.name.charAt(0).toUpperCase() }}</div>
            <div>
              <h1 class="detail-name">{{ supplier()!.name }}</h1>
              <div class="detail-desc">{{ categoryName(supplier()!.supplierCategoryId) }}</div>
            </div>
          </div>
          <div class="header-actions">
            <span class="badge" [class.badge-active]="supplier()!.isActive" [class.badge-inactive]="!supplier()!.isActive">
              {{ supplier()!.isActive ? 'Ativo' : 'Inativo' }}
            </span>
            @if (supplier()!.isActive) {
              <button class="btn btn-warn" (click)="toggleActive(false)" [disabled]="acting()">{{ acting() === 'toggle' ? '...' : 'Desativar' }}</button>
            } @else {
              <button class="btn btn-success" (click)="toggleActive(true)" [disabled]="acting()">{{ acting() === 'toggle' ? '...' : 'Ativar' }}</button>
            }
            <button class="btn btn-danger-outline" (click)="deleteModal.set(true)" [disabled]="acting()">Excluir</button>
          </div>
        </div>

        <!-- Tabs -->
        <div class="tabs">
          <button class="tab" [class.active]="activeTab() === 'info'" (click)="activeTab.set('info')">Informações</button>
          <button class="tab" [class.active]="activeTab() === 'edit'" (click)="openEdit()">Editar</button>
        </div>

        <!-- Tab: Informações -->
        @if (activeTab() === 'info') {
          <div class="cards-row">

            <div class="card">
              <div class="card-head"><h3 class="card-title">Dados Gerais</h3></div>
              <dl class="dl">
                <dt>Nome</dt>       <dd>{{ supplier()!.name }}</dd>
                <dt>Categoria</dt>  <dd>{{ categoryName(supplier()!.supplierCategoryId) }}</dd>
                <dt>CNPJ</dt>       <dd>{{ supplier()!.cnpj || '—' }}</dd>
                <dt>Status</dt>     <dd>{{ supplier()!.isActive ? 'Ativo' : 'Inativo' }}</dd>
                @if (supplier()!.description) {
                  <dt>Descrição</dt><dd>{{ supplier()!.description }}</dd>
                }
                @if (supplier()!.businessHours) {
                  <dt>Horário</dt>  <dd>{{ supplier()!.businessHours }}</dd>
                }
              </dl>
            </div>

            <div class="card">
              <div class="card-head"><h3 class="card-title">Contato</h3></div>
              <dl class="dl">
                @if (supplier()!.email) {
                  <dt>E-mail</dt><dd><a [href]="'mailto:' + supplier()!.email" class="link">{{ supplier()!.email }}</a></dd>
                }
                @if (supplier()!.phone) {
                  <dt>Telefone</dt><dd>{{ supplier()!.phone }}</dd>
                }
                @if (supplier()!.website) {
                  <dt>Website</dt><dd><a [href]="supplier()!.website!" target="_blank" class="link">{{ supplier()!.website }}</a></dd>
                }
                @if (!supplier()!.email && !supplier()!.phone && !supplier()!.website) {
                  <dt></dt><dd class="muted">Nenhum contato cadastrado</dd>
                }
              </dl>
            </div>

            @if (supplier()!.addressLogradouro || supplier()!.addressCep) {
              <div class="card">
                <div class="card-head"><h3 class="card-title">Endereço</h3></div>
                <dl class="dl">
                  <dt>Logradouro</dt>
                  <dd>{{ supplier()!.addressLogradouro }}{{ supplier()!.addressNumero ? ', ' + supplier()!.addressNumero : '' }}{{ supplier()!.addressComplemento ? ' — ' + supplier()!.addressComplemento : '' }}</dd>
                  <dt>Bairro</dt>    <dd>{{ supplier()!.addressBairro }}</dd>
                  <dt>Cidade / UF</dt><dd>{{ supplier()!.addressCidade }} / {{ supplier()!.addressUf }}</dd>
                  <dt>CEP</dt>       <dd>{{ supplier()!.addressCep }}</dd>
                </dl>
              </div>
            }

          </div>
        }

        <!-- Tab: Editar -->
        @if (activeTab() === 'edit') {
          <div class="card edit-card">

            <div class="form-section">
              <div class="section-label">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 7H4a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2z"/><circle cx="12" cy="12" r="2"/></svg>
                Dados Gerais
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Nome <span class="required">*</span></label>
                  <input class="field-input" [(ngModel)]="editForm.name" placeholder="Nome do fornecedor" />
                </div>
              </div>
              <div class="form-row">
                <div class="field">
                  <label class="field-label">Categoria <span class="required">*</span></label>
                  <select class="field-input field-select" [(ngModel)]="editForm.supplierCategoryId">
                    @for (c of categories(); track c.id) {
                      <option [value]="c.id">{{ c.name }}</option>
                    }
                  </select>
                </div>
                <div class="field">
                  <label class="field-label">CNPJ</label>
                  <input class="field-input" [(ngModel)]="editForm.cnpj" placeholder="00.000.000/0000-00" />
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Descrição</label>
                  <textarea class="field-input field-textarea" [(ngModel)]="editForm.description" rows="3" placeholder="Descrição opcional..."></textarea>
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Horário de Atendimento</label>
                  <input class="field-input" [(ngModel)]="editForm.businessHours" placeholder="ex: Seg–Sex 08h–18h" />
                </div>
              </div>
            </div>

            <div class="form-section">
              <div class="section-label">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07A19.5 19.5 0 0 1 4.69 12a19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 3.6 1.2h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L7.91 8.56a16 16 0 0 0 5.82 5.82l1.63-1.64a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                Contato
              </div>
              <div class="form-row">
                <div class="field">
                  <label class="field-label">E-mail</label>
                  <input class="field-input" type="email" [(ngModel)]="editForm.email" />
                </div>
                <div class="field">
                  <label class="field-label">Telefone</label>
                  <input class="field-input" [(ngModel)]="editForm.phone" placeholder="+55 41 99999-9999" />
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Website</label>
                  <input class="field-input" [(ngModel)]="editForm.website" placeholder="https://..." />
                </div>
              </div>
            </div>

            <div class="form-section">
              <div class="section-label">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
                Endereço
              </div>
              <div class="form-row">
                <div class="field">
                  <label class="field-label">CEP</label>
                  <input class="field-input" [(ngModel)]="editForm.cep" placeholder="00000-000" (blur)="lookupCep()" />
                </div>
                <div class="field">
                  <label class="field-label">Número</label>
                  <input class="field-input" [(ngModel)]="editForm.numero" />
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Logradouro</label>
                  <input class="field-input" [(ngModel)]="editForm.logradouro" />
                </div>
              </div>
              <div class="form-row">
                <div class="field">
                  <label class="field-label">Bairro</label>
                  <input class="field-input" [(ngModel)]="editForm.bairro" />
                </div>
                <div class="field">
                  <label class="field-label">Cidade</label>
                  <input class="field-input" [(ngModel)]="editForm.cidade" />
                </div>
                <div class="field field--sm">
                  <label class="field-label">UF</label>
                  <input class="field-input" [(ngModel)]="editForm.uf" maxlength="2" style="text-transform:uppercase" />
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Complemento</label>
                  <input class="field-input" [(ngModel)]="editForm.complemento" />
                </div>
              </div>
            </div>

            <div class="edit-actions">
              <button class="btn btn-primary" (click)="save()" [disabled]="!editForm.name.trim() || saving()">
                {{ saving() ? 'Salvando...' : 'Salvar alterações' }}
              </button>
              <button class="btn btn-ghost" (click)="activeTab.set('info')">Cancelar</button>
            </div>

          </div>
        }

      }
    </div>

    <!-- Modal: Excluir -->
    @if (deleteModal()) {
      <div class="modal-overlay" (click)="deleteModal.set(false)">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
          </div>
          <h3 class="modal-title">Excluir fornecedor</h3>
          <p class="modal-body">Tem certeza que quer excluir <strong>{{ supplier()?.name }}</strong>?<br>Esta ação não pode ser desfeita.</p>
          <div class="modal-actions">
            <button class="btn btn-ghost" (click)="deleteModal.set(false)">Cancelar</button>
            <button class="btn btn-danger" (click)="confirmDelete()" [disabled]="acting() === 'delete'">
              {{ acting() === 'delete' ? 'Excluindo...' : 'Excluir' }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; max-width: 900px; }

    /* Breadcrumb */
    .breadcrumb { display: flex; align-items: center; gap: 8px; margin-bottom: 20px; }
    .back-btn {
      display: flex; align-items: center; gap: 4px;
      background: none; border: none; cursor: pointer;
      font-size: 13px; color: #888; padding: 4px 6px; border-radius: 6px;
      &:hover { background: #f0f0f3; color: #333; }
    }
    .sep { color: #ccc; }
    .bc-current { font-size: 13px; color: #555; font-weight: 600; }

    /* Header */
    .detail-header {
      display: flex; align-items: center; justify-content: space-between;
      gap: 16px; margin-bottom: 20px; flex-wrap: wrap;
    }
    .detail-meta { display: flex; align-items: center; gap: 14px; }
    .detail-avatar {
      width: 48px; height: 48px; border-radius: 12px;
      background: var(--color-brand); color: #fff;
      display: flex; align-items: center; justify-content: center;
      font-size: 20px; font-weight: 800; flex-shrink: 0;
    }
    .detail-name { font-size: 20px; font-weight: 700; color: #111; }
    .detail-desc { font-size: 13px; color: #888; margin-top: 2px; }
    .header-actions { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }

    /* Tabs */
    .tabs { display: flex; border-bottom: 2px solid #f0f0f3; margin-bottom: 20px; }
    .tab {
      display: flex; align-items: center; gap: 6px;
      padding: 10px 18px; border: none; background: none;
      font-size: 13px; font-weight: 600; color: #aaa; cursor: pointer;
      border-bottom: 2px solid transparent; margin-bottom: -2px; transition: all .15s;
      &:hover { color: #555; }
      &.active { color: var(--color-brand); border-bottom-color: var(--color-brand); }
    }

    /* Cards */
    .cards-row { display: flex; flex-direction: column; gap: 12px; }
    .card { background: #fff; border: 1px solid #e8e8ec; border-radius: 14px; padding: 20px; }
    .card-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .card-title { font-size: 13px; font-weight: 700; color: #555; }

    /* DL */
    .dl {
      display: grid; grid-template-columns: auto 1fr; gap: 10px 20px; align-items: baseline;
      dt { font-size: 12px; color: #aaa; font-weight: 600; white-space: nowrap; }
      dd { font-size: 13px; color: #333; margin: 0; }
    }
    .link { color: var(--color-brand); text-decoration: none; &:hover { text-decoration: underline; } }
    .muted { color: #aaa; }

    /* Edit form */
    .edit-card { display: flex; flex-direction: column; gap: 0; padding: 0; overflow: hidden; }
    .form-section {
      padding: 20px; border-bottom: 1px solid #f0f0f3;
      display: flex; flex-direction: column; gap: 12px;
    }
    .section-label {
      display: flex; align-items: center; gap: 6px;
      font-size: 11px; font-weight: 700; text-transform: uppercase;
      letter-spacing: .06em; color: #aaa; margin-bottom: 4px;
      svg { color: var(--color-brand); }
    }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .field { display: flex; flex-direction: column; gap: 5px; &.field--full { grid-column: 1 / -1; } &.field--sm { max-width: 80px; } }
    .field-label { font-size: 12px; font-weight: 600; color: #555; }
    .required { color: var(--color-brand); }
    .field-input {
      padding: 9px 12px; border: 1px solid #e8e8ec; border-radius: 8px;
      font-size: 13px; color: #111; outline: none; transition: border-color .15s;
      background: #fff; width: 100%; box-sizing: border-box; font-family: inherit;
      &:focus { border-color: var(--color-brand); }
    }
    .field-textarea { resize: vertical; min-height: 80px; }
    .field-select {
      appearance: none;
      background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='11' height='11' viewBox='0 0 24 24' fill='none' stroke='%23aaa' stroke-width='2.5'%3E%3Cpolyline points='6 9 12 15 18 9'/%3E%3C/svg%3E");
      background-repeat: no-repeat; background-position: right 10px center; padding-right: 28px; cursor: pointer;
    }
    .edit-actions { display: flex; gap: 8px; padding: 16px 20px; }

    /* Badges */
    .badge {
      display: inline-block; padding: 3px 10px; border-radius: 99px; font-size: 11px; font-weight: 700;
      &-active   { background: #dcfce7; color: #15803d; }
      &-inactive { background: #f4f4f5; color: #71717a; }
    }

    /* Buttons */
    .btn {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 8px 16px; border-radius: 8px;
      font-size: 13px; font-weight: 600; cursor: pointer;
      border: 1px solid transparent; transition: all .15s;
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
    .btn-primary        { background: var(--color-brand); color: #fff; &:hover { opacity: .9; } }
    .btn-ghost          { background: #f4f4f5; color: #555; border-color: #e8e8ec; &:hover { background: #ebebef; } }
    .btn-warn           { background: #fffbeb; color: #b45309; border-color: #fde68a; &:hover { background: #fef3c7; } }
    .btn-success        { background: #f0fdf4; color: #15803d; border-color: #bbf7d0; &:hover { background: #dcfce7; } }
    .btn-danger-outline { background: #fef2f2; color: #b91c1c; border-color: #fecaca; &:hover { background: #fee2e2; } }
    .btn-danger         { background: #dc2626; color: #fff; &:hover { background: #b91c1c; } }

    /* Modal */
    .modal-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,.45); z-index: 200;
      display: flex; align-items: center; justify-content: center;
      backdrop-filter: blur(3px);
    }
    .modal {
      background: #fff; border-radius: 16px; padding: 32px 28px 24px;
      width: 100%; max-width: 400px;
      display: flex; flex-direction: column; align-items: center; gap: 12px;
      box-shadow: 0 20px 60px rgba(0,0,0,.2);
      animation: scaleIn .15s ease;
    }
    @keyframes scaleIn { from { transform: scale(.95); opacity: 0; } to { transform: scale(1); opacity: 1; } }
    .modal-icon {
      width: 48px; height: 48px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      background: #fef2f2; color: #b91c1c;
    }
    .modal-title { font-size: 17px; font-weight: 700; color: #111; }
    .modal-body  { font-size: 14px; color: #666; text-align: center; line-height: 1.6; strong { color: #111; } }
    .modal-actions { display: flex; gap: 10px; width: 100%; margin-top: 8px; button { flex: 1; justify-content: center; } }

    /* Loading / empty */
    .loading-state { display: flex; justify-content: center; padding: 60px; }
    .spin {
      width: 32px; height: 32px; border-radius: 50%;
      border: 3px solid #f0f0f3; border-top-color: var(--color-brand);
      animation: spin .7s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .empty-state { text-align: center; padding: 60px; color: #ccc; }

    @media (max-width: 768px) {
      .page { padding: 14px; max-width: 100%; }
      .form-row { grid-template-columns: 1fr; }
      .field--sm { max-width: 100%; }
    }
  `],
})
export class SupplierDetailComponent implements OnInit {
  private route  = inject(ActivatedRoute);
  private router = inject(Router);
  private http   = inject(HttpClient);
  private svc    = inject(SupplierService);
  private toast  = inject(ToastService);

  supplier   = signal<PlatformSupplierDto | null>(null);
  categories = signal<SupplierCategoryDto[]>([]);
  loading    = signal(true);
  saving     = signal(false);
  acting     = signal('');
  activeTab  = signal<Tab>('info');
  deleteModal = signal(false);

  editForm = {
    name: '', supplierCategoryId: '', cnpj: '', website: '', email: '',
    phone: '', description: '', businessHours: '',
    cep: '', logradouro: '', numero: '', bairro: '', cidade: '', uf: '', complemento: '',
  };

  private get id() { return this.route.snapshot.paramMap.get('id')!; }

  ngOnInit() {
    forkJoin({ supplier: this.svc.getSupplier(this.id), categories: this.svc.getCategories() }).subscribe({
      next: ({ supplier, categories }) => {
        this.supplier.set(supplier);
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.toast.error('Erro ao carregar fornecedor.'); },
    });
  }

  goBack() { this.router.navigate(['/admin/suppliers']); }

  categoryName(id: string) { return this.categories().find(c => c.id === id)?.name ?? '—'; }

  openEdit() {
    const s = this.supplier()!;
    Object.assign(this.editForm, {
      name: s.name, supplierCategoryId: s.supplierCategoryId,
      cnpj: s.cnpj ?? '', website: s.website ?? '', email: s.email ?? '',
      phone: s.phone ?? '', description: s.description ?? '', businessHours: s.businessHours ?? '',
      cep: s.addressCep ?? '', logradouro: s.addressLogradouro ?? '',
      numero: s.addressNumero ?? '', bairro: s.addressBairro ?? '',
      cidade: s.addressCidade ?? '', uf: s.addressUf ?? '', complemento: s.addressComplemento ?? '',
    });
    this.activeTab.set('edit');
  }

  lookupCep() {
    const cep = this.editForm.cep.replace(/\D/g, '');
    if (cep.length !== 8) return;
    this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
      next: (d: any) => {
        if (!d.erro) Object.assign(this.editForm, { logradouro: d.logradouro ?? '', bairro: d.bairro ?? '', cidade: d.localidade ?? '', uf: d.uf ?? '' });
      },
    });
  }

  save() {
    if (!this.editForm.name.trim()) return;
    const f = this.editForm;
    this.saving.set(true);
    this.svc.updateSupplier(this.id, {
      name: f.name, supplierCategoryId: f.supplierCategoryId,
      cnpj: f.cnpj || null, website: f.website || null, email: f.email || null,
      phone: f.phone || null, description: f.description || null, businessHours: f.businessHours || null,
      cep: f.cep || null, logradouro: f.logradouro || null, numero: f.numero || null,
      bairro: f.bairro || null, cidade: f.cidade || null, uf: f.uf || null, complemento: f.complemento || null,
    }).subscribe({
      next: () => {
        this.svc.getSupplier(this.id).subscribe(updated => {
          this.supplier.set(updated);
          this.saving.set(false);
          this.activeTab.set('info');
          this.toast.success('Fornecedor atualizado com sucesso!');
        });
      },
      error: (err: any) => { this.saving.set(false); this.toast.error(err?.error?.message ?? 'Erro ao salvar.'); },
    });
  }

  toggleActive(activate: boolean) {
    this.acting.set('toggle');
    const req = activate ? this.svc.activateSupplier(this.id) : this.svc.deactivateSupplier(this.id);
    req.subscribe({
      next: () => {
        this.supplier.update(s => s ? { ...s, isActive: activate } : s);
        this.acting.set('');
        this.toast.success(activate ? 'Fornecedor ativado com sucesso.' : 'Fornecedor desativado com sucesso.');
      },
      error: (err: any) => { this.acting.set(''); this.toast.error(err?.error?.message ?? 'Erro ao alterar status.'); },
    });
  }

  confirmDelete() {
    this.acting.set('delete');
    this.svc.deleteSupplier(this.id).subscribe({
      next: () => {
        this.acting.set('');
        this.deleteModal.set(false);
        this.toast.success('Fornecedor excluído.');
        this.router.navigate(['/admin/suppliers']);
      },
      error: (err: any) => { this.acting.set(''); this.toast.error(err?.error?.message ?? 'Erro ao excluir.'); },
    });
  }
}
