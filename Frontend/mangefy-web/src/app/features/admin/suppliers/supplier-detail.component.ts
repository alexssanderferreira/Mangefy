import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { SupplierService, PlatformSupplierDto, SupplierCategoryDto } from './supplier.service';
import { ToastService } from '../../../core/toast/toast.service';
import { LucideAngularModule, ChevronLeft, Phone, MapPin, Trash2 } from 'lucide-angular';

type Tab = 'info' | 'edit';

@Component({
  selector: 'app-supplier-detail',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <div class="page">

      <!-- Breadcrumb -->
      <div class="breadcrumb">
        <button class="back-btn" (click)="goBack()">
          <lucide-icon [img]="ChevronLeft" [size]="14" [strokeWidth]="2"></lucide-icon>
          Fornecedores
        </button>
        @if (supplier()) { <span class="sep">/</span> <span class="bc-current">{{ supplier()!.name }}</span> }
      </div>

      @if (error()) {
        <div class="alert-error">{{ error() }}</div>
      }

      @if (loading()) {
        <div class="loading-state"><div class="spinner"></div></div>
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
            <span class="badge" [class.badge-success]="supplier()!.isActive" [class.badge-neutral]="!supplier()!.isActive">
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
                  <input class="input" [(ngModel)]="editForm.name" placeholder="Nome do fornecedor" />
                </div>
              </div>
              <div class="form-row">
                <div class="field">
                  <label class="field-label">Categoria <span class="required">*</span></label>
                  <select class="input field-select" [(ngModel)]="editForm.supplierCategoryId">
                    @for (c of categories(); track c.id) {
                      <option [value]="c.id">{{ c.name }}</option>
                    }
                  </select>
                </div>
                <div class="field">
                  <label class="field-label">CNPJ</label>
                  <input class="input" [(ngModel)]="editForm.cnpj" placeholder="00.000.000/0000-00" />
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Descrição</label>
                  <textarea class="input field-textarea" [(ngModel)]="editForm.description" rows="3" placeholder="Descrição opcional..."></textarea>
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Horário de Atendimento</label>
                  <input class="input" [(ngModel)]="editForm.businessHours" placeholder="ex: Seg–Sex 08h–18h" />
                </div>
              </div>
            </div>

            <div class="form-section">
              <div class="section-label">
                <lucide-icon [img]="Phone" [size]="13" [strokeWidth]="2"></lucide-icon>
                Contato
              </div>
              <div class="form-row">
                <div class="field">
                  <label class="field-label">E-mail</label>
                  <input class="input" type="email" [(ngModel)]="editForm.email" />
                </div>
                <div class="field">
                  <label class="field-label">Telefone</label>
                  <input class="input" [(ngModel)]="editForm.phone" placeholder="+55 41 99999-9999" />
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Website</label>
                  <input class="input" [(ngModel)]="editForm.website" placeholder="https://..." />
                </div>
              </div>
            </div>

            <div class="form-section">
              <div class="section-label">
                <lucide-icon [img]="MapPin" [size]="13" [strokeWidth]="2"></lucide-icon>
                Endereço
              </div>
              <div class="form-row">
                <div class="field">
                  <label class="field-label">CEP</label>
                  <input class="input" [(ngModel)]="editForm.cep" placeholder="00000-000" (blur)="lookupCep()" />
                </div>
                <div class="field">
                  <label class="field-label">Número</label>
                  <input class="input" [(ngModel)]="editForm.numero" />
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Logradouro</label>
                  <input class="input" [(ngModel)]="editForm.logradouro" />
                </div>
              </div>
              <div class="form-row">
                <div class="field">
                  <label class="field-label">Bairro</label>
                  <input class="input" [(ngModel)]="editForm.bairro" />
                </div>
                <div class="field">
                  <label class="field-label">Cidade</label>
                  <input class="input" [(ngModel)]="editForm.cidade" />
                </div>
                <div class="field field--sm">
                  <label class="field-label">UF</label>
                  <input class="input" [(ngModel)]="editForm.uf" maxlength="2" style="text-transform:uppercase" />
                </div>
              </div>
              <div class="form-row">
                <div class="field field--full">
                  <label class="field-label">Complemento</label>
                  <input class="input" [(ngModel)]="editForm.complemento" />
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
            <lucide-icon [img]="Trash2" [size]="24" [strokeWidth]="2"></lucide-icon>
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
      svg, lucide-icon { color: var(--color-brand); }
    }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .field { display: flex; flex-direction: column; gap: 5px; &.field--full { grid-column: 1 / -1; } &.field--sm { max-width: 80px; } }
    .field-label { font-size: 12px; font-weight: 600; color: #555; }
    .required { color: var(--color-brand); }
    .field-textarea { resize: vertical; min-height: 80px; }
    .field-select {
      appearance: none;
      background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='11' height='11' viewBox='0 0 24 24' fill='none' stroke='%23aaa' stroke-width='2.5'%3E%3Cpolyline points='6 9 12 15 18 9'/%3E%3C/svg%3E");
      background-repeat: no-repeat; background-position: right 10px center; padding-right: 28px; cursor: pointer;
    }
    .edit-actions { display: flex; gap: 8px; padding: 16px 20px; }

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

  readonly ChevronLeft = ChevronLeft;
  readonly Phone = Phone;
  readonly MapPin = MapPin;
  readonly Trash2 = Trash2;

  supplier   = signal<PlatformSupplierDto | null>(null);
  categories = signal<SupplierCategoryDto[]>([]);
  loading    = signal(true);
  error      = signal('');
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
      error: () => { this.loading.set(false); this.error.set('Não foi possível carregar o fornecedor.'); },
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
