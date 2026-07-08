import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { PlansService, PlanDto, CreatePlanRequest, UpdatePlanRequest } from './plans.service';
import { ToastService } from '../../../core/toast/toast.service';
import { LucideAngularModule, Plus, CircleAlert, Star, LayoutGrid, Navigation, Users, UserRound, Pencil, CircleMinus, Trash2, CircleCheck, X, DollarSign, LoaderCircle } from 'lucide-angular';

type DrawerMode = 'create' | 'edit';

interface PlanForm {
  name: string;
  description: string;
  monthlyPrice: number | null;
  maxTables: number | null;
  maxMenuItems: number | null;
  maxUsers: number | null;
  maxCustomRoles: number | null;
}

function emptyForm(): PlanForm {
  return { name: '', description: '', monthlyPrice: null, maxTables: null, maxMenuItems: null, maxUsers: null, maxCustomRoles: 0 };
}

@Component({
  selector: 'app-plans',
  standalone: true,
  imports: [FormsModule, CurrencyPipe, LucideAngularModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Planos</h1>
          <p class="page-subtitle">{{ plans().length }} plano{{ plans().length !== 1 ? 's' : '' }} cadastrado{{ plans().length !== 1 ? 's' : '' }}</p>
        </div>
        <button class="btn btn-primary" (click)="openCreate()">
          <lucide-icon [img]="Plus" [size]="14" [strokeWidth]="2.5"></lucide-icon>
          Novo Plano
        </button>
      </div>

      <!-- Error -->
      @if (error()) {
        <div class="alert-error">
          <lucide-icon [img]="CircleAlert" [size]="15" [strokeWidth]="2"></lucide-icon>
          {{ error() }}
        </div>
      }

      <!-- Loading -->
      @if (loading()) {
        <div class="plans-grid">
          @for (i of [1,2,3]; track i) { <div class="plan-card skeleton-card"></div> }
        </div>
      }

      <!-- Empty -->
      @else if (plans().length === 0) {
        <div class="empty-state">
          <lucide-icon [img]="Star" [size]="40" [strokeWidth]="1.5"></lucide-icon>
          <p>Nenhum plano cadastrado</p>
          <button class="btn btn-primary btn-sm" (click)="openCreate()">Criar primeiro plano</button>
        </div>
      }

      <!-- Plans grid -->
      @else {

        <!-- Ativos -->
        @if (activePlans().length > 0) {
          <div class="section-label">Ativos</div>
          <div class="plans-grid">
          @for (plan of activePlans(); track plan.id) {
            <div class="plan-card">
              <div class="plan-header">
                <div class="plan-name-row">
                  <h2 class="plan-name">{{ plan.name }}</h2>
                  <span class="status-badge active">Ativo</span>
                </div>
                @if (plan.description) {
                  <p class="plan-desc">{{ plan.description }}</p>
                }
              </div>
              <div class="plan-price">
                {{ plan.monthlyPrice | currency:'BRL':'symbol':'1.2-2' }}
                <span class="plan-price-period">/mês</span>
              </div>
              <div class="plan-limits">
                <div class="limit-row">
                  <lucide-icon [img]="LayoutGrid" [size]="13" [strokeWidth]="2"></lucide-icon>
                  <span>{{ plan.maxTables }} mesas</span>
                </div>
                <div class="limit-row">
                  <lucide-icon [img]="Navigation" [size]="13" [strokeWidth]="2"></lucide-icon>
                  <span>{{ plan.maxMenuItems }} itens de menu</span>
                </div>
                <div class="limit-row">
                  <lucide-icon [img]="Users" [size]="13" [strokeWidth]="2"></lucide-icon>
                  <span>{{ plan.maxUsers }} utilizadores</span>
                </div>
                @if (plan.maxCustomRoles > 0) {
                  <div class="limit-row">
                    <lucide-icon [img]="UserRound" [size]="13" [strokeWidth]="2"></lucide-icon>
                    <span>{{ plan.maxCustomRoles }} cargos personalizados</span>
                  </div>
                }
              </div>
              <div class="plan-actions">
                <button class="btn-action" (click)="openEdit(plan)">
                  <lucide-icon [img]="Pencil" [size]="14" [strokeWidth]="2"></lucide-icon>
                  Editar
                </button>
                <button class="btn-action btn-action--warn" (click)="toggleStatus(plan)" [disabled]="toggling() === plan.id">
                  <lucide-icon [img]="CircleMinus" [size]="14" [strokeWidth]="2"></lucide-icon>
                  {{ toggling() === plan.id ? '...' : 'Desativar' }}
                </button>
                <button class="btn-action btn-action--danger" (click)="deletePlan(plan)" [disabled]="deleting() === plan.id">
                  <lucide-icon [img]="Trash2" [size]="14" [strokeWidth]="2"></lucide-icon>
                  {{ deleting() === plan.id ? '...' : 'Excluir' }}
                </button>
              </div>
            </div>
          }
          </div>
        }

        <!-- Inativos -->
        @if (inactivePlans().length > 0) {
          <div class="section-label section-label--inactive">Inativos</div>
          <div class="plans-grid">
          @for (plan of inactivePlans(); track plan.id) {
            <div class="plan-card inactive">
              <div class="plan-header">
                <div class="plan-name-row">
                  <h2 class="plan-name">{{ plan.name }}</h2>
                  <span class="status-badge inactive">Inativo</span>
                </div>
                @if (plan.description) {
                  <p class="plan-desc">{{ plan.description }}</p>
                }
              </div>
              <div class="plan-price">
                {{ plan.monthlyPrice | currency:'BRL':'symbol':'1.2-2' }}
                <span class="plan-price-period">/mês</span>
              </div>
              <div class="plan-limits">
                <div class="limit-row">
                  <lucide-icon [img]="LayoutGrid" [size]="13" [strokeWidth]="2"></lucide-icon>
                  <span>{{ plan.maxTables }} mesas</span>
                </div>
                <div class="limit-row">
                  <lucide-icon [img]="Navigation" [size]="13" [strokeWidth]="2"></lucide-icon>
                  <span>{{ plan.maxMenuItems }} itens de menu</span>
                </div>
                <div class="limit-row">
                  <lucide-icon [img]="Users" [size]="13" [strokeWidth]="2"></lucide-icon>
                  <span>{{ plan.maxUsers }} utilizadores</span>
                </div>
                @if (plan.maxCustomRoles > 0) {
                  <div class="limit-row">
                    <lucide-icon [img]="UserRound" [size]="13" [strokeWidth]="2"></lucide-icon>
                    <span>{{ plan.maxCustomRoles }} cargos personalizados</span>
                  </div>
                }
              </div>
              <div class="plan-actions">
                <button class="btn-action" (click)="openEdit(plan)">
                  <lucide-icon [img]="Pencil" [size]="14" [strokeWidth]="2"></lucide-icon>
                  Editar
                </button>
                <button class="btn-action btn-action--success" (click)="toggleStatus(plan)" [disabled]="toggling() === plan.id">
                  <lucide-icon [img]="CircleCheck" [size]="14" [strokeWidth]="2"></lucide-icon>
                  {{ toggling() === plan.id ? '...' : 'Ativar' }}
                </button>
                <button class="btn-action btn-action--danger" (click)="deletePlan(plan)" [disabled]="deleting() === plan.id">
                  <lucide-icon [img]="Trash2" [size]="14" [strokeWidth]="2"></lucide-icon>
                  {{ deleting() === plan.id ? '...' : 'Excluir' }}
                </button>
              </div>
            </div>
          }
          </div>
        }
      }
    </div>

    <!-- ── Modal de confirmação de exclusão ── -->
    @if (deleteTarget()) {
      <div class="modal-overlay" (click)="cancelDelete()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-icon">
            <lucide-icon [img]="Trash2" [size]="24" [strokeWidth]="2"></lucide-icon>
          </div>
          <h3 class="modal-title">Excluir plano</h3>
          <p class="modal-body">
            Tem a certeza que quer excluir o plano <strong>{{ deleteTarget()!.name }}</strong>?<br>
            Esta ação não pode ser desfeita.
          </p>
          <div class="modal-actions">
            <button class="btn btn-ghost" (click)="cancelDelete()">Cancelar</button>
            <button class="btn btn-danger" (click)="confirmDelete()">Excluir</button>
          </div>
        </div>
      </div>
    }

    <!-- ── Drawer overlay ── -->
    @if (drawerOpen()) {
      <div class="drawer-overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <lucide-icon [img]="Star" [size]="16" [strokeWidth]="2"></lucide-icon>
            </div>
            <div>
              <h3 class="drawer-title">{{ drawerMode() === 'create' ? 'Novo Plano' : 'Editar Plano' }}</h3>
              <p class="drawer-subtitle">{{ drawerMode() === 'create' ? 'Configure o novo plano de assinatura' : editingPlan()?.name }}</p>
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

          <!-- Identificação -->
          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="Star" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Identificação
            </div>
            @if (drawerMode() === 'create') {
              <div class="field">
                <label class="field-label">Nome do plano <span class="req">*</span></label>
                <input class="field-input" [(ngModel)]="form.name" placeholder="Ex: Profissional" maxlength="100" />
              </div>
            } @else {
              <div class="field">
                <label class="field-label">Nome do plano</label>
                <div class="field-static">{{ editingPlan()?.name }}</div>
              </div>
            }
            <div class="field">
              <label class="field-label">Descrição</label>
              <input class="field-input" [(ngModel)]="form.description" placeholder="Breve descrição do plano" maxlength="300" />
            </div>
          </div>

          <!-- Preço -->
          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="DollarSign" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Preço
            </div>
            <div class="field">
              <label class="field-label">Preço mensal <span class="req">*</span></label>
              <div class="price-input-wrap">
                <span class="price-prefix">R$</span>
                <input class="field-input price-input" type="text" inputmode="numeric" [value]="priceDisplay()" (input)="onPriceInput($event)" placeholder="0,00" />
              </div>
            </div>
          </div>

          <!-- Limites -->
          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="LayoutGrid" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Limites do plano
            </div>
            <div class="form-grid-2">
              <div class="field">
                <label class="field-label">Máx. mesas <span class="req">*</span></label>
                <input class="field-input" type="text" inputmode="numeric" [(ngModel)]="form.maxTables" placeholder="0" (keydown)="onlyDigits($event)" />
              </div>
              <div class="field">
                <label class="field-label">Máx. itens menu <span class="req">*</span></label>
                <input class="field-input" type="text" inputmode="numeric" [(ngModel)]="form.maxMenuItems" placeholder="0" (keydown)="onlyDigits($event)" />
              </div>
              <div class="field">
                <label class="field-label">Máx. usuários <span class="req">*</span></label>
                <input class="field-input" type="text" inputmode="numeric" [(ngModel)]="form.maxUsers" placeholder="0" (keydown)="onlyDigits($event)" />
              </div>
              <div class="field">
                <label class="field-label">Cargos personalizados</label>
                <input class="field-input" type="text" inputmode="numeric" [(ngModel)]="form.maxCustomRoles" placeholder="0" (keydown)="onlyDigits($event)" />
              </div>
            </div>
          </div>
        </div>

        <div class="drawer-footer">
          <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
          <button class="btn btn-primary" (click)="submit()" [disabled]="saving()">
            @if (saving()) {
              <lucide-icon class="icon-spin" [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5"></lucide-icon>
              Salvando...
            } @else {
              {{ drawerMode() === 'create' ? 'Criar Plano' : 'Salvar Alterações' }}
            }
          </button>
        </div>
      </aside>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; }

    .section-label {
      font-size: 10px; font-weight: 700; text-transform: uppercase;
      letter-spacing: .07em; color: #bbb;
      margin-bottom: 12px; margin-top: 6px;
      &--inactive { margin-top: 28px; }
    }

    .page-header {
      display: flex; align-items: center; justify-content: space-between;
      margin-bottom: 20px;
    }
    .page-subtitle { font-size: 12px; color: #aaa; margin-top: 2px; }

    /* ── Plans grid ── */
    .plans-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 16px;
    }

    .plan-card {
      background: #fff;
      border: 1px solid #e8e8ec;
      border-radius: 16px;
      padding: 22px;
      display: flex;
      flex-direction: column;
      gap: 0;
      transition: box-shadow .2s;
      &:hover { box-shadow: 0 4px 20px rgba(0,0,0,.07); }
      &.inactive { opacity: .6; }
    }

    .skeleton-card {
      height: 280px;
      background: linear-gradient(90deg, #f5f5f7 25%, #ebebef 50%, #f5f5f7 75%);
      background-size: 200% 100%;
      animation: shimmer 1.4s infinite;
      border-radius: 16px;
    }

    .plan-header { margin-bottom: 16px; }

    .plan-name-row {
      display: flex; align-items: center; justify-content: space-between;
      gap: 10px; margin-bottom: 6px;
    }

    .plan-name { font-size: 16px; font-weight: 700; color: #111; }

    .plan-desc { font-size: 12px; color: #999; line-height: 1.5; }

    .status-badge {
      font-size: 10px; font-weight: 700;
      padding: 2px 8px; border-radius: 99px;
      white-space: nowrap; flex-shrink: 0;
      &.active   { background: #dcfce7; color: #15803d; }
      &.inactive { background: #f4f4f5; color: #71717a; }
    }

    .plan-price {
      font-size: 32px; font-weight: 800; color: #111;
      letter-spacing: -1px; margin-bottom: 18px;
    }
    .plan-price-period { font-size: 14px; font-weight: 400; color: #aaa; }

    .plan-limits {
      display: flex; flex-direction: column; gap: 8px;
      padding: 16px 0; border-top: 1px solid #f0f0f3;
      margin-bottom: 18px; flex: 1;
    }

    .limit-row {
      display: flex; align-items: center; gap: 8px;
      font-size: 13px; color: #555;
      lucide-icon { color: #aaa; flex-shrink: 0; }
    }

    .plan-actions {
      display: flex; gap: 8px;
      padding-top: 16px; border-top: 1px solid #f0f0f3;
    }

    .btn-action {
      flex: 1;
      display: flex; align-items: center; justify-content: center; gap: 5px;
      padding: 7px 12px;
      border: 1px solid #e8e8ec;
      border-radius: 8px;
      background: #fff;
      font-size: 12px; font-weight: 600; color: #555;
      cursor: pointer; transition: all .15s;
      &:hover { background: #f7f7f9; border-color: #d0d0d8; color: #333; }
      &:disabled { opacity: .5; cursor: not-allowed; }
      &.btn-action--warn   { color: #b45309; border-color: #fde68a; background: #fffbeb; &:hover { background: #fef3c7; } }
      &.btn-action--success { color: #15803d; border-color: #bbf7d0; background: #f0fdf4; &:hover { background: #dcfce7; } }
      &.btn-action--danger  { color: #b91c1c; border-color: #fecaca; background: #fef2f2; &:hover { background: #fee2e2; } }
    }

    /* Form sections */
    .form-section { display: flex; flex-direction: column; gap: 10px; padding: 14px; background: #fafafa; border: 1px solid #f0f0f3; border-radius: 10px; }
    .form-section-header { display: flex; align-items: center; gap: 6px; font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #999; lucide-icon { color: var(--color-brand); } }
    .form-grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }

    /* Form fields */
    .field { display: flex; flex-direction: column; gap: 5px; }
    .field-group { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .field-label { font-size: 11px; font-weight: 600; color: #666; }
    .req { color: var(--color-brand); }
    .field-input {
      width: 100%; padding: 8px 10px; border: 1px solid #e8e8ec; border-radius: 7px;
      font-size: 13px; color: #111; outline: none; transition: border-color .15s, box-shadow .15s; background: #fff; box-sizing: border-box;
      &:focus { border-color: var(--color-brand); box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-brand) 12%, transparent); }
      &::placeholder { color: #ccc; }
    }
    .price-input-wrap {
      display: flex; align-items: center;
      border: 1px solid #e8e8ec; border-radius: 8px;
      overflow: hidden; transition: border-color .15s;
      &:focus-within { border-color: var(--color-brand); }
    }
    .price-prefix {
      padding: 0 10px; font-size: 13px; font-weight: 600;
      color: #888; background: #f7f7f9;
      border-right: 1px solid #e8e8ec; white-space: nowrap;
      align-self: stretch; display: flex; align-items: center;
    }
    .price-input {
      border: none !important; border-radius: 0 !important;
      flex: 1; outline: none;
      &:focus { border-color: transparent; }
    }

    .field-static {
      padding: 9px 12px; background: #f7f7f9;
      border: 1px solid #e8e8ec; border-radius: 8px;
      font-size: 13px; color: #555; font-weight: 600;
    }

    @media (max-width: 768px) {
      .page { padding: 14px; }
      .plans-grid { grid-template-columns: 1fr; }
      .field-group { grid-template-columns: 1fr; }
      .drawer { width: 100%; border-left: none; top: auto; border-radius: 16px 16px 0 0; }
      .page-header { flex-wrap: wrap; gap: 10px; }
    }
  `]
})
export class PlansComponent implements OnInit {
  private svc = inject(PlansService);
  private toast = inject(ToastService);

  readonly Plus = Plus;
  readonly CircleAlert = CircleAlert;
  readonly Star = Star;
  readonly LayoutGrid = LayoutGrid;
  readonly Navigation = Navigation;
  readonly Users = Users;
  readonly UserRound = UserRound;
  readonly Pencil = Pencil;
  readonly CircleMinus = CircleMinus;
  readonly Trash2 = Trash2;
  readonly CircleCheck = CircleCheck;
  readonly X = X;
  readonly DollarSign = DollarSign;
  readonly LoaderCircle = LoaderCircle;

  plans        = signal<PlanDto[]>([]);
  loading      = signal(true);
  error        = signal('');
  toggling     = signal('');
  deleting     = signal('');
  deleteTarget  = signal<PlanDto | null>(null);
  priceDisplay  = signal('');

  activePlans   = computed(() => this.plans().filter(p => p.status === 'Active'));
  inactivePlans = computed(() => this.plans().filter(p => p.status === 'Inactive'));

  drawerOpen  = signal(false);
  drawerMode  = signal<DrawerMode>('create');
  drawerError = signal('');
  saving      = signal(false);
  editingPlan = signal<PlanDto | null>(null);

  form: PlanForm = emptyForm();

  ngOnInit() { this.load(); }

  private load() {
    this.loading.set(true);
    this.svc.getAll().subscribe({
      next:  plans => { this.plans.set(plans); this.loading.set(false); },
      error: ()    => { this.error.set('Não foi possível carregar os planos.'); this.loading.set(false); },
    });
  }

  onlyDigits(event: KeyboardEvent) {
    const allowed = ['Backspace','Delete','Tab','ArrowLeft','ArrowRight','Home','End'];
    if (!allowed.includes(event.key) && !/^\d$/.test(event.key)) {
      event.preventDefault();
    }
  }

  onPriceInput(event: Event) {
    const raw = (event.target as HTMLInputElement).value.replace(/\D/g, '');
    const cents = parseInt(raw || '0', 10);
    const value = cents / 100;
    this.form.monthlyPrice = value;
    this.priceDisplay.set(value > 0
      ? value.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
      : '');
  }

  openCreate() {
    this.form = emptyForm();
    this.priceDisplay.set('');
    this.drawerMode.set('create');
    this.drawerError.set('');
    this.editingPlan.set(null);
    this.drawerOpen.set(true);
  }

  openEdit(plan: PlanDto) {
    this.form = {
      name: plan.name,
      description: plan.description ?? '',
      monthlyPrice: plan.monthlyPrice,
      maxTables: plan.maxTables,
      maxMenuItems: plan.maxMenuItems,
      maxUsers: plan.maxUsers,
      maxCustomRoles: plan.maxCustomRoles,
    };
    this.priceDisplay.set(plan.monthlyPrice.toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }));
    this.drawerMode.set('edit');
    this.drawerError.set('');
    this.editingPlan.set(plan);
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
  }

  submit() {
    if (!this.validate()) return;
    this.saving.set(true);
    this.drawerError.set('');

    if (this.drawerMode() === 'create') {
      const req: any = {
        name: this.form.name.trim(),
        description: this.form.description?.trim() || null,
        monthlyPrice: this.form.monthlyPrice,
        maxTables: this.form.maxTables,
        maxMenuItems: this.form.maxMenuItems,
        maxUsers: this.form.maxUsers,
        maxCustomRoles: this.form.maxCustomRoles ?? 0,
      };
      this.svc.create(req).subscribe({
        next: () => { this.saving.set(false); this.drawerOpen.set(false); this.load(); },
        error: () => { this.drawerError.set('Erro ao criar plano.'); this.saving.set(false); },
      });
    } else {
      const plan = this.editingPlan()!;
      const req: UpdatePlanRequest = {
        monthlyPrice: this.form.monthlyPrice!,
        maxTables: this.form.maxTables!,
        maxMenuItems: this.form.maxMenuItems!,
        maxUsers: this.form.maxUsers!,
        maxCustomRoles: this.form.maxCustomRoles ?? 0,
        description: this.form.description?.trim() || null,
      };
      this.svc.update(plan.id, req).subscribe({
        next: () => { this.saving.set(false); this.drawerOpen.set(false); this.load(); this.toast.show(`Plano "${plan.name}" atualizado com sucesso.`, 'success'); },
        error: () => { this.drawerError.set('Erro ao atualizar plano.'); this.saving.set(false); },
      });
    }
  }

  deletePlan(plan: PlanDto) {
    this.deleteTarget.set(plan);
  }

  confirmDelete() {
    const plan = this.deleteTarget()!;
    this.deleting.set(plan.id);
    this.deleteTarget.set(null);
    this.svc.delete(plan.id).subscribe({
      next:  () => { this.deleting.set(''); this.load(); this.toast.show(`Plano "${plan.name}" excluído com sucesso.`, 'success'); },
      error: () => { this.deleting.set(''); this.error.set('Erro ao excluir plano.'); },
    });
  }

  cancelDelete() {
    this.deleteTarget.set(null);
  }

  toggleStatus(plan: PlanDto) {
    this.toggling.set(plan.id);
    const action = plan.status === 'Active'
      ? this.svc.deactivate(plan.id)
      : this.svc.activate(plan.id);

    action.subscribe({
      next:  () => { this.toggling.set(''); this.load(); },
      error: () => { this.toggling.set(''); this.error.set('Erro ao alterar status do plano.'); },
    });
  }

  private validate(): boolean {
    if (this.drawerMode() === 'create' && !this.form.name?.trim()) {
      this.drawerError.set('Nome é obrigatório.'); return false;
    }
    if (!this.form.monthlyPrice || this.form.monthlyPrice < 0) {
      this.drawerError.set('Preço mensal inválido.'); return false;
    }
    if (!this.form.maxTables || this.form.maxTables < 1) {
      this.drawerError.set('Máximo de mesas deve ser pelo menos 1.'); return false;
    }
    if (!this.form.maxMenuItems || this.form.maxMenuItems < 1) {
      this.drawerError.set('Máximo de itens de menu deve ser pelo menos 1.'); return false;
    }
    if (!this.form.maxUsers || this.form.maxUsers < 1) {
      this.drawerError.set('Máximo de utilizadores deve ser pelo menos 1.'); return false;
    }
    return true;
  }
}
