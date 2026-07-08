import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { LucideAngularModule, Plus, X, Search, ChevronLeft, ChevronRight, User, CircleAlert, Mail, Phone, CreditCard, MapPin, LoaderCircle } from 'lucide-angular';
import { OwnerService, OwnerListItemDto } from '../owner.service';

const STATUS_LABEL: Record<string, string> = {
  Active: 'Ativo', PendingActivation: 'Aguardando ativação', Inactive: 'Inativo',
};

const STATUS_BADGE_CLASS: Record<string, string> = {
  Active: 'badge-success', PendingActivation: 'badge-warning', Inactive: 'badge-neutral',
};

@Component({
  selector: 'app-owner-list',
  standalone: true,
  imports: [FormsModule, DatePipe, LucideAngularModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Responsáveis</h1>
          <p class="page-subtitle">{{ total() }} responsável{{ total() !== 1 ? 'is' : '' }} cadastrado{{ total() !== 1 ? 's' : '' }}</p>
        </div>
        <button class="btn btn-primary" (click)="openCreate()">
          <lucide-icon [img]="Plus" [size]="14" [strokeWidth]="2.5"></lucide-icon>
          Novo Responsável
        </button>
      </div>

      <!-- Quick filter tabs -->
      <div class="filter-chips">
        <button class="filter-chip" [class.active]="filterStatus() === ''"                  (click)="filterStatus.set('')">Todos</button>
        <button class="filter-chip" [class.active]="filterStatus() === 'Active'"             (click)="filterStatus.set('Active')">Ativos</button>
        <button class="filter-chip" [class.active]="filterStatus() === 'PendingActivation'"  (click)="filterStatus.set('PendingActivation')">Aguardando ativação</button>
        <button class="filter-chip" [class.active]="filterStatus() === 'Inactive'"           (click)="filterStatus.set('Inactive')">Inativos</button>
        @if (search() || filterStatus()) {
          <button class="filter-chip-clear" (click)="search.set(''); filterStatus.set('')">
            <lucide-icon [img]="X" [size]="11" [strokeWidth]="2.5"></lucide-icon>
            Limpar filtros
          </button>
        }
      </div>

      <!-- Filtro -->
      <div class="filters">
        <div class="search-wrap">
          <lucide-icon [img]="Search" [size]="14" [strokeWidth]="2"></lucide-icon>
          <input class="search-input" [ngModel]="search()" (ngModelChange)="search.set($event)" placeholder="Buscar responsável por nome ou e-mail..." />
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
              <th>Responsável</th>
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
              <tr><td colspan="6" class="empty-cell">Nenhum responsável encontrado.</td></tr>
            } @else {
              @for (o of filtered(); track o.id) {
                <tr class="table-row" (click)="goDetail(o.id)">
                  <td class="td-name">
                    <span class="name-avatar">{{ o.name.charAt(0).toUpperCase() }}</span>
                    <span class="name-text">{{ o.name }}</span>
                  </td>
                  <td class="td-email">{{ o.email }}</td>
                  <td><span class="badge" [class]="statusBadgeClass(o.status)">{{ statusLabel(o.status) }}</span></td>
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
            <lucide-icon [img]="ChevronLeft" [size]="14" [strokeWidth]="2"></lucide-icon>
          </button>
          @for (p of pageNumbers(); track p) {
            @if (p === -1) { <span class="pg-dots">…</span> }
            @else { <button class="pg-btn" [class.pg-active]="p === page()" (click)="goPage(p)">{{ p }}</button> }
          }
          <button class="pg-btn" (click)="goPage(page() + 1)" [disabled]="page() === totalPages()">
            <lucide-icon [img]="ChevronRight" [size]="14" [strokeWidth]="2"></lucide-icon>
          </button>
          <span class="pg-info">{{ (page()-1)*pageSize()+1 }}–{{ min(page()*pageSize(), total()) }} de {{ total() }}</span>
        </div>
      }
    </div>

    <!-- Drawer criar -->
    @if (drawerOpen()) {
      <div class="drawer-overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <lucide-icon [img]="User" [size]="16" [strokeWidth]="2"></lucide-icon>
            </div>
            <div>
              <h3 class="drawer-title">Novo Responsável</h3>
              <p class="drawer-subtitle">Preencha os dados do responsável</p>
            </div>
          </div>
          <button class="btn-close" (click)="closeDrawer()">
            <lucide-icon [img]="X" [size]="16" [strokeWidth]="2"></lucide-icon>
          </button>
        </div>

        <div class="drawer-body">
          @if (drawerError()) {
            <div class="alert-error">
              <lucide-icon [img]="CircleAlert" [size]="14" [strokeWidth]="2"></lucide-icon>
              {{ drawerError() }}
            </div>
          }

          <!-- Seção: Identificação -->
          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="User" [size]="13" [strokeWidth]="2.5"></lucide-icon>
              Identificação
            </div>
            <div class="form-grid-1">
              <div class="field">
                <label class="field-label">Nome completo <span class="req">*</span></label>
                <div class="input-wrap">
                  <lucide-icon class="input-icon" [img]="User" [size]="14" [strokeWidth]="2"></lucide-icon>
                  <input class="field-input has-icon" [(ngModel)]="form.name" placeholder="Ex: João Silva" maxlength="200" />
                </div>
              </div>
            </div>
            <div class="form-grid-2">
              <div class="field">
                <label class="field-label">E-mail <span class="req">*</span></label>
                <div class="input-wrap">
                  <lucide-icon class="input-icon" [img]="Mail" [size]="14" [strokeWidth]="2"></lucide-icon>
                  <input class="field-input has-icon" type="email" [(ngModel)]="form.email" placeholder="joao@email.com.br" />
                </div>
              </div>
              <div class="field">
                <label class="field-label">Telefone</label>
                <div class="input-wrap">
                  <lucide-icon class="input-icon" [img]="Phone" [size]="14" [strokeWidth]="2"></lucide-icon>
                  <input class="field-input has-icon" [(ngModel)]="form.phone" placeholder="+55 11 99999-9999" maxlength="20" />
                </div>
              </div>
            </div>
          </div>

          <!-- Seção: Documento -->
          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="CreditCard" [size]="13" [strokeWidth]="2.5"></lucide-icon>
              Documento
            </div>
            <div class="form-grid-doc">
              <div class="field">
                <label class="field-label">Tipo</label>
                <select class="field-input field-select" [(ngModel)]="form.documentType" (ngModelChange)="form.documentNumber=''">
                  <option value="">Nenhum</option>
                  <option value="CPF">CPF</option>
                  <option value="CNPJ">CNPJ</option>
                </select>
              </div>
              <div class="field">
                <label class="field-label">Número</label>
                <input class="field-input" [(ngModel)]="form.documentNumber"
                  [placeholder]="form.documentType === 'CPF' ? '000.000.000-00' : form.documentType === 'CNPJ' ? '00.000.000/0001-00' : 'Selecione o tipo'"
                  [disabled]="!form.documentType" maxlength="18" />
              </div>
            </div>
          </div>

          <!-- Seção: Endereço -->
          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="MapPin" [size]="13" [strokeWidth]="2.5"></lucide-icon>
              Endereço
            </div>
            <div class="form-grid-cep">
              <div class="field">
                <label class="field-label">CEP</label>
                <div class="input-wrap">
                  @if (cepLoading()) {
                    <lucide-icon class="input-icon icon-spin" [img]="LoaderCircle" [size]="14" [strokeWidth]="2"></lucide-icon>
                  }
                  <input class="field-input" [class.has-icon]="cepLoading()" [(ngModel)]="form.cep"
                    placeholder="00000-000" maxlength="9"
                    (blur)="lookupCep()" (keydown.enter)="lookupCep()" />
                </div>
              </div>
              <div class="field" style="grid-column: span 2">
                <label class="field-label">Logradouro</label>
                <input class="field-input" [(ngModel)]="form.logradouro" placeholder="Rua, Av, Travessa..." maxlength="200" />
              </div>
            </div>
            <div class="form-grid-2">
              <div class="field">
                <label class="field-label">Número</label>
                <input class="field-input" [(ngModel)]="form.numero" placeholder="123" maxlength="20" />
              </div>
              <div class="field">
                <label class="field-label">Complemento</label>
                <input class="field-input" [(ngModel)]="form.complemento" placeholder="Apto, Sala..." maxlength="100" />
              </div>
            </div>
            <div class="form-grid-bairro">
              <div class="field" style="grid-column: span 2">
                <label class="field-label">Bairro</label>
                <input class="field-input" [(ngModel)]="form.bairro" placeholder="Bairro" maxlength="100" />
              </div>
              <div class="field" style="grid-column: span 2">
                <label class="field-label">Cidade</label>
                <input class="field-input" [(ngModel)]="form.cidade" placeholder="Cidade" maxlength="100" />
              </div>
              <div class="field">
                <label class="field-label">UF</label>
                <input class="field-input" [(ngModel)]="form.uf" placeholder="SP" maxlength="2" style="text-transform:uppercase" />
              </div>
            </div>
          </div>

          <div class="info-box">
            <lucide-icon [img]="CircleAlert" [size]="14" [strokeWidth]="2"></lucide-icon>
            Um link de ativação de 48h será enviado ao cliente para definir sua senha.
          </div>
        </div>

        <div class="drawer-footer">
          <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
          <button class="btn btn-primary" (click)="submit()" [disabled]="saving()">
            @if (saving()) {
              <lucide-icon class="icon-spin" [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5"></lucide-icon>
              Criando...
            } @else {
              Criar Cliente
            }
          </button>
        </div>
      </aside>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; }
    .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .page-subtitle { font-size: 12px; color: #aaa; margin-top: 2px; }

    .filters { display: flex; gap: 10px; margin-bottom: 12px; }
    .search-wrap {
      flex: 1; display: flex; align-items: center; gap: 8px;
      background: #fff; border: 1px solid #e8e8ec; border-radius: 8px; padding: 7px 12px;
      lucide-icon { color: #bbb; flex-shrink: 0; }
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
    /* Form sections */
    .form-section { display: flex; flex-direction: column; gap: 10px; padding: 16px; background: #fafafa; border: 1px solid #f0f0f3; border-radius: 10px; }
    .form-section-header { display: flex; align-items: center; gap: 6px; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #999; lucide-icon { color: var(--color-brand); } }
    .form-grid-1 { display: grid; gap: 10px; }
    .form-grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
    .form-grid-doc { display: grid; grid-template-columns: 100px 1fr; gap: 10px; }
    .form-grid-cep { display: grid; grid-template-columns: 110px 1fr 1fr; gap: 10px; }
    .form-grid-bairro { display: grid; grid-template-columns: 1fr 1fr 52px; gap: 10px; }

    /* Fields */
    .field { display: flex; flex-direction: column; gap: 5px; }
    .field-label { font-size: 11px; font-weight: 600; color: #666; }
    .req { color: var(--color-brand); }
    .input-wrap { position: relative; display: flex; align-items: center; }
    .input-icon { position: absolute; left: 10px; color: #bbb; pointer-events: none; flex-shrink: 0; }
    .field-input {
      width: 100%; padding: 8px 10px; border: 1px solid #e8e8ec; border-radius: 7px;
      font-size: 13px; color: #111; outline: none; transition: border-color .15s, box-shadow .15s; background: #fff; box-sizing: border-box;
      &.has-icon { padding-left: 30px; }
      &:focus { border-color: var(--color-brand); box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-brand) 12%, transparent); }
      &::placeholder { color: #ccc; }
      &:disabled { background: #f4f4f5; color: #bbb; cursor: not-allowed; border-color: #eee; }
    }
    .field-select {
      appearance: none; cursor: pointer;
      background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='11' height='11' viewBox='0 0 24 24' fill='none' stroke='%23aaa' stroke-width='2.5'%3E%3Cpolyline points='6 9 12 15 18 9'/%3E%3C/svg%3E");
      background-repeat: no-repeat; background-position: right 8px center; padding-right: 26px;
    }
    /* Info box */
    .info-box { display: flex; align-items: flex-start; gap: 8px; background: #f0f9ff; border: 1px solid #bae6fd; border-radius: 8px; padding: 10px 12px; font-size: 12px; color: #0369a1; lucide-icon { flex-shrink: 0; margin-top: 1px; } }
  `],
})
export class OwnerListComponent implements OnInit {
  private svc    = inject(OwnerService);
  private router = inject(Router);
  private route  = inject(ActivatedRoute);
  private http   = inject(HttpClient);

  readonly Plus = Plus;
  readonly X = X;
  readonly Search = Search;
  readonly ChevronLeft = ChevronLeft;
  readonly ChevronRight = ChevronRight;
  readonly User = User;
  readonly CircleAlert = CircleAlert;
  readonly Mail = Mail;
  readonly Phone = Phone;
  readonly CreditCard = CreditCard;
  readonly MapPin = MapPin;
  readonly LoaderCircle = LoaderCircle;

  owners       = signal<OwnerListItemDto[]>([]);
  loading      = signal(true);
  error        = signal('');
  search       = signal('');
  filterStatus = signal('');
  page       = signal(1);
  pageSize   = signal(10);
  total      = signal(0);
  totalPages = computed(() => Math.ceil(this.total() / this.pageSize()));

  drawerOpen  = signal(false);
  drawerError = signal('');
  saving      = signal(false);
  cepLoading  = signal(false);

  form = {
    name: '', email: '', phone: '',
    documentType: '' as '' | 'CPF' | 'CNPJ', documentNumber: '',
    cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidade: '', uf: '',
  };

  filtered = computed(() => {
    const q  = this.search().toLowerCase();
    const st = this.filterStatus();
    return this.owners().filter(o => {
      const matchSearch = !q || o.name.toLowerCase().includes(q) || o.email.toLowerCase().includes(q);
      const matchStatus = !st || o.status === st;
      return matchSearch && matchStatus;
    });
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

  ngOnInit() {
    const statusFromUrl = this.route.snapshot.queryParamMap.get('status');
    if (statusFromUrl) this.filterStatus.set(statusFromUrl);
    this.load();
  }

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
  statusBadgeClass(s: string) { return STATUS_BADGE_CLASS[s] ?? 'badge-neutral'; }

  openCreate() {
    this.form = { name: '', email: '', phone: '', documentType: '', documentNumber: '', cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidade: '', uf: '' };
    this.drawerError.set('');
    this.drawerOpen.set(true);
  }

  lookupCep() {
    const cep = this.form.cep.replace(/\D/g, '');
    if (cep.length !== 8) return;
    this.cepLoading.set(true);
    this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
      next: data => {
        if (!data.erro) {
          this.form.logradouro = data.logradouro ?? '';
          this.form.bairro     = data.bairro ?? '';
          this.form.cidade     = data.localidade ?? '';
          this.form.uf         = data.uf ?? '';
        }
        this.cepLoading.set(false);
      },
      error: () => this.cepLoading.set(false),
    });
  }

  closeDrawer() { if (!this.saving()) this.drawerOpen.set(false); }

  submit() {
    this.drawerError.set('');
    if (!this.form.name.trim()) { this.drawerError.set('Nome é obrigatório.'); return; }
    if (!this.form.email.trim()) { this.drawerError.set('E-mail é obrigatório.'); return; }

    this.saving.set(true);
    const f = this.form;
    const hasAddress = f.cep.trim() || f.logradouro.trim();
    this.svc.create({
      name:           f.name.trim(),
      email:          f.email.trim(),
      phone:          f.phone.trim() || null,
      documentType:   f.documentType || null,
      documentNumber: f.documentNumber.trim() || null,
      cep:            hasAddress ? f.cep.trim() : null,
      logradouro:     hasAddress ? f.logradouro.trim() : null,
      numero:         hasAddress ? f.numero.trim() : null,
      complemento:    hasAddress ? f.complemento.trim() || null : null,
      bairro:         hasAddress ? f.bairro.trim() : null,
      cidade:         hasAddress ? f.cidade.trim() : null,
      uf:             hasAddress ? f.uf.trim().toUpperCase() : null,
    }).subscribe({
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
