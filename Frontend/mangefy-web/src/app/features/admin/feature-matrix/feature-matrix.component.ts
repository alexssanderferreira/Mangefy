import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { PlansService, PlanDto } from '../plans/plans.service';
import { BusinessTypeService, BusinessTypeDto } from '../business-types/business-type.service';
import { FeatureMatrixService, PlanFeatureSetDto } from './feature-matrix.service';
import { ToastService } from '../../../core/toast/toast.service';
import {
  LucideAngularModule, LoaderCircle, FileText, Menu, Monitor, Package,
  CreditCard, ChartNoAxesColumn, Calendar, Truck, Users, Check, Save, LayoutGrid
} from 'lucide-angular';

// ── Feature catalog mirroring FeatureCatalog.cs ──────────────────────────────
interface FeatureGroup {
  label: string;
  icon: string;
  features: { key: string; label: string; description: string }[];
}

const FEATURE_GROUPS: FeatureGroup[] = [
  {
    label: 'Comandas', icon: 'receipt',
    features: [
      { key: 'features.tabs', label: 'Gestão de Comandas', description: 'Abertura, pedidos e fechamento de comandas por cliente' },
    ],
  },
  {
    label: 'Cardápio', icon: 'menu',
    features: [
      { key: 'features.multi_menu', label: 'Múltiplos Cardápios', description: 'Café da manhã, almoço, jantar...' },
    ],
  },
  {
    label: 'Cozinha', icon: 'kds',
    features: [
      { key: 'features.kds', label: 'Kitchen Display System', description: 'Tela de produção para a cozinha' },
    ],
  },
  {
    label: 'Estoque', icon: 'stock',
    features: [
      { key: 'features.stock_basic', label: 'Estoque Básico', description: 'Controle simples de ingredientes' },
      { key: 'features.stock_advanced', label: 'Estoque Avançado', description: 'Alertas de nível mínimo, histórico e relatórios' },
    ],
  },
  {
    label: 'Caixa', icon: 'cash',
    features: [
      { key: 'features.daily_cash', label: 'Fechamento de Caixa', description: 'Abertura, sangrias e fechamento diário' },
    ],
  },
  {
    label: 'Relatórios', icon: 'reports',
    features: [
      { key: 'features.reports_basic', label: 'Relatórios Essenciais', description: 'Vendas, comandas, resumo do dia' },
      { key: 'features.reports_advanced', label: 'Analytics Avançado', description: 'Tendências, comparativos e exportação' },
    ],
  },
  {
    label: 'Reservas', icon: 'reservation',
    features: [
      { key: 'features.reservations', label: 'Gestão de Reservas', description: 'Agendamento e chegada de clientes' },
    ],
  },
  {
    label: 'Delivery', icon: 'delivery',
    features: [
      { key: 'features.delivery', label: 'Módulo de Delivery', description: 'Pedidos para entrega com endereço' },
    ],
  },
  {
    label: 'Equipe', icon: 'roles',
    features: [
      { key: 'features.custom_roles', label: 'Cargos Customizados', description: 'Criação de cargos além dos padrões do tipo de negócio' },
    ],
  },
];

@Component({
  selector: 'app-feature-matrix',
  standalone: true,
  imports: [LucideAngularModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Matriz de Features</h1>
          <p class="page-subtitle">Configure quais funcionalidades estão disponíveis por Plano × Tipo de Negócio</p>
        </div>
      </div>

      <!-- Plan selector -->
      <div class="plan-bar">
        <span class="plan-label">Plano</span>
        <div class="plan-tabs">
          @for (p of plans(); track p.id) {
            <button class="plan-tab" [class.active]="selectedPlanId() === p.id" (click)="selectPlan(p.id)">
              {{ p.name }}
              @if (p.status === 'Inactive') { <span class="plan-inactive-dot"></span> }
            </button>
          }
        </div>
        @if (loadingMatrix()) {
          <lucide-icon class="spin-icon" [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5"></lucide-icon>
        }
      </div>

      @if (loading()) {
        <div class="skel-rows">@for (i of [1,2,3,4,5,6]; track i) { <div class="skel-row"></div> }</div>
      }

      @if (!loading() && selectedPlanId()) {
        <!-- Matrix grid -->
        <div class="matrix-wrap">
          <table class="matrix-table">
            <thead>
              <tr>
                <th class="feature-col">Feature</th>
                @for (bt of activeBusinessTypes(); track bt.id) {
                  <th class="bt-col">
                    <div class="bt-header">
                      <div class="bt-avatar">{{ bt.name[0] }}</div>
                      <span class="bt-name">{{ bt.name }}</span>
                    </div>
                  </th>
                }
              </tr>
            </thead>
            <tbody>
              @for (group of featureGroups; track group.label) {
                <!-- Group header row -->
                <tr class="group-row">
                  <td [attr.colspan]="activeBusinessTypes().length + 1">
                    <div class="group-header">
                      @switch (group.icon) {
                          @case ('receipt') {
                            <lucide-icon [img]="FileText" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                          }
                          @case ('menu') {
                            <lucide-icon [img]="Menu" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                          }
                          @case ('kds') {
                            <lucide-icon [img]="Monitor" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                          }
                          @case ('stock') {
                            <lucide-icon [img]="Package" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                          }
                          @case ('cash') {
                            <lucide-icon [img]="CreditCard" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                          }
                          @case ('reports') {
                            <lucide-icon [img]="ChartNoAxesColumn" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                          }
                          @case ('reservation') {
                            <lucide-icon [img]="Calendar" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                          }
                          @case ('delivery') {
                            <lucide-icon [img]="Truck" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                          }
                          @case ('roles') {
                            <lucide-icon [img]="Users" [size]="11" [strokeWidth]="2.5"></lucide-icon>
                          }
                        }
                      {{ group.label }}
                    </div>
                  </td>
                </tr>

                <!-- Feature rows -->
                @for (feat of group.features; track feat.key) {
                  <tr class="feature-row">
                    <td class="feature-cell">
                      <div class="feature-name">{{ feat.label }}</div>
                      <div class="feature-desc">{{ feat.description }}</div>
                    </td>
                    @for (bt of activeBusinessTypes(); track bt.id) {
                      <td class="check-cell">
                        <label class="check-wrap" [class.checked]="isEnabled(bt.id, feat.key)"
                          (click)="$event.preventDefault(); toggle(bt.id, feat.key)">
                          <input
                            type="checkbox"
                            class="sr-only"
                            [checked]="isEnabled(bt.id, feat.key)"
                            (click)="$event.stopPropagation()"
                          />
                          <div class="check-box" [class.checked]="isEnabled(bt.id, feat.key)">
                            @if (isEnabled(bt.id, feat.key)) {
                              <lucide-icon [img]="Check" [size]="10" [strokeWidth]="3"></lucide-icon>
                            }
                          </div>
                        </label>
                      </td>
                    }
                  </tr>
                }
              }
            </tbody>
          </table>
        </div>

        <!-- Footer actions -->
        <div class="matrix-footer">
          <div class="footer-info">
            @if (dirtyCount() > 0) {
              <span class="dirty-badge">{{ dirtyCount() }} coluna{{ dirtyCount() !== 1 ? 's' : '' }} com alterações não salvas</span>
            } @else {
              <span class="saved-label">Tudo salvo</span>
            }
          </div>
          <div class="footer-actions">
            @if (dirtyCount() > 0) {
              <button class="btn-ghost" (click)="discardChanges()">Descartar</button>
            }
            <button class="btn-primary" [disabled]="dirtyCount() === 0 || saving()" (click)="saveAll()">
              @if (saving()) {
                <lucide-icon class="spin-icon" [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5"></lucide-icon>
                Salvando...
              } @else {
                <lucide-icon [img]="Save" [size]="14" [strokeWidth]="2.5"></lucide-icon>
                Salvar Alterações
              }
            </button>
          </div>
        </div>
      }

      @if (!loading() && !selectedPlanId()) {
        <div class="empty-state">
          <lucide-icon [img]="LayoutGrid" [size]="40" [strokeWidth]="1.5"></lucide-icon>
          <p>Nenhum plano cadastrado. Crie um plano primeiro.</p>
        </div>
      }

    </div>
  `,
  styles: [`
    .page { padding: 24px 28px; }
    .page-header { margin-bottom: 20px; }
    .page-title { font-size: 22px; font-weight: 700; color: #111; }
    .page-subtitle { font-size: 12px; color: #aaa; margin-top: 2px; }

    /* Plan selector */
    .plan-bar { display: flex; align-items: center; gap: 12px; margin-bottom: 20px; flex-wrap: wrap; }
    .plan-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #bbb; }
    .plan-tabs { display: flex; gap: 4px; flex-wrap: wrap; }
    .plan-tab {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 7px 14px; border-radius: 8px;
      border: 1.5px solid #e8e8ec; background: #fff;
      font-size: 13px; font-weight: 600; color: #777; cursor: pointer;
      transition: all .15s;
      &:hover { border-color: #bbb; color: #333; }
      &.active { border-color: var(--color-brand); background: color-mix(in srgb, var(--color-brand) 8%, transparent); color: var(--color-brand); }
    }
    .plan-inactive-dot { width: 6px; height: 6px; border-radius: 50%; background: #f59e0b; }

    /* Matrix table */
    .matrix-wrap { background: #fff; border: 1px solid #e8e8ec; border-radius: 14px; overflow: auto; margin-bottom: 16px; max-height: calc(100vh - 260px); }
    .matrix-table { width: 100%; border-collapse: collapse; min-width: 600px; }

    .feature-col { width: 260px; min-width: 220px; }
    .bt-col { width: 110px; min-width: 90px; text-align: center; }

    .matrix-table th {
      padding: 12px 14px; background: #fafafa;
      border-bottom: 1px solid #f0f0f3;
      font-size: 10px; font-weight: 700; text-transform: uppercase;
      letter-spacing: .05em; color: #bbb;
      position: sticky; top: 0; z-index: 2;
    }

    .bt-header { display: flex; flex-direction: column; align-items: center; gap: 6px; }
    .bt-avatar {
      width: 30px; height: 30px; border-radius: 8px; flex-shrink: 0;
      background: var(--color-brand); color: #fff; font-size: 13px; font-weight: 700;
      display: flex; align-items: center; justify-content: center;
    }
    .bt-name { font-size: 11px; font-weight: 700; color: #555; text-transform: none; letter-spacing: 0; }

    /* Group row */
    .group-row td {
      padding: 8px 14px; background: #f7f7f9;
      border-top: 1px solid #f0f0f3; border-bottom: 1px solid #f0f0f3;
    }
    .group-header {
      display: flex; align-items: center; gap: 6px;
      font-size: 10px; font-weight: 700; text-transform: uppercase;
      letter-spacing: .07em; color: #999;
      lucide-icon { color: var(--color-brand); }
    }

    /* Feature row */
    .feature-row td { border-bottom: 1px solid #f7f7f9; }
    .feature-row:last-child td { border-bottom: none; }
    .feature-row:hover td { background: #fafafa; }

    .feature-cell { padding: 12px 14px; }
    .feature-name { font-size: 13px; font-weight: 600; color: #111; }
    .feature-desc { font-size: 11px; color: #aaa; margin-top: 2px; line-height: 1.35; }

    /* Checkbox cell */
    .check-cell { text-align: center; padding: 12px; vertical-align: middle; }
    .check-wrap { display: inline-flex; align-items: center; justify-content: center; cursor: pointer; }
    .sr-only { position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0,0,0,0); }
    .check-box {
      width: 20px; height: 20px; border-radius: 5px;
      border: 2px solid #d1d5db;
      display: flex; align-items: center; justify-content: center;
      transition: all .15s; background: #fff;
      &.checked { background: var(--color-brand); border-color: var(--color-brand); color: #fff; }
    }
    .check-wrap:hover .check-box:not(.checked) { border-color: var(--color-brand); background: color-mix(in srgb, var(--color-brand) 6%, transparent); }

    /* Footer */
    .matrix-footer {
      display: flex; align-items: center; justify-content: space-between;
      padding: 14px 18px; background: #fff;
      border: 1px solid #e8e8ec; border-radius: 12px;
      gap: 12px; flex-wrap: wrap;
    }
    .footer-info { display: flex; align-items: center; gap: 8px; }
    .dirty-badge {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 4px 10px; border-radius: 99px;
      background: color-mix(in srgb, #f59e0b 15%, transparent); color: #92400e;
      font-size: 12px; font-weight: 600;
    }
    .saved-label { font-size: 12px; color: #bbb; }
    .footer-actions { display: flex; gap: 8px; }

    /* Buttons */
    .btn-primary {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 9px 16px; background: #0a0a0a; color: #fff;
      border: none; border-radius: 8px; font-size: 13px; font-weight: 600;
      font-family: inherit; cursor: pointer; transition: background .15s;
      &:hover:not(:disabled) { background: #222; }
      &:disabled { opacity: .4; cursor: not-allowed; }
    }
    .btn-ghost {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 9px 16px; background: #fff; color: #555;
      border: 1.5px solid #e8e8ec; border-radius: 8px; font-size: 13px; font-weight: 600;
      font-family: inherit; cursor: pointer; transition: all .15s;
      &:hover { border-color: #bbb; }
    }

    @keyframes spinAnim { to { transform: rotate(360deg); } }
    .spin-icon { animation: spinAnim .8s linear infinite; transform-origin: center; color: #aaa; }

    /* Skeleton */
    .skel-rows { display: flex; flex-direction: column; gap: 8px; }
    .skel-row { height: 52px; background: #f5f5f7; border-radius: 10px; animation: pulse 1.4s infinite; }
    @keyframes pulse { 0%,100% { opacity:1; } 50% { opacity:.5; } }

    .empty-state {
      display: flex; flex-direction: column; align-items: center; gap: 12px;
      padding: 60px 0; color: #ccc;
      p { font-size: 14px; color: #aaa; }
    }

    @media (max-width: 768px) {
      .page { padding: 12px; }
    }
  `]
})
export class FeatureMatrixComponent implements OnInit {
  private plansSvc  = inject(PlansService);
  private btSvc     = inject(BusinessTypeService);
  private matrixSvc = inject(FeatureMatrixService);
  private toast     = inject(ToastService);

  readonly featureGroups = FEATURE_GROUPS;

  readonly LoaderCircle = LoaderCircle;
  readonly FileText = FileText;
  readonly Menu = Menu;
  readonly Monitor = Monitor;
  readonly Package = Package;
  readonly CreditCard = CreditCard;
  readonly ChartNoAxesColumn = ChartNoAxesColumn;
  readonly Calendar = Calendar;
  readonly Truck = Truck;
  readonly Users = Users;
  readonly Check = Check;
  readonly Save = Save;
  readonly LayoutGrid = LayoutGrid;

  loading       = signal(true);
  loadingMatrix = signal(false);
  saving        = signal(false);

  plans         = signal<PlanDto[]>([]);
  businessTypes = signal<BusinessTypeDto[]>([]);
  featureSets   = signal<PlanFeatureSetDto[]>([]);

  selectedPlanId = signal<string>('');

  // local editable state: businessTypeId → Set<featureKey>
  private localState = signal<Map<string, Set<string>>>(new Map());

  // track which business types have been modified
  private dirtyBts = signal<Set<string>>(new Set());

  activeBusinessTypes = computed(() => this.businessTypes().filter(bt => bt.isActive));

  dirtyCount = computed(() => this.dirtyBts().size);

  ngOnInit() {
    forkJoin({
      plans: this.plansSvc.getAll(),
      bts:   this.btSvc.getAll(),
    }).subscribe({
      next: ({ plans, bts }) => {
        this.plans.set(plans);
        this.businessTypes.set(bts);
        this.loading.set(false);
        if (plans.length > 0) {
          this.selectPlan(plans[0].id);
        }
      },
      error: () => { this.toast.error('Erro ao carregar dados.'); this.loading.set(false); }
    });
  }

  selectPlan(planId: string) {
    if (this.selectedPlanId() === planId) return;
    this.selectedPlanId.set(planId);
    this.dirtyBts.set(new Set());
    this.loadMatrix(planId);
  }

  private loadMatrix(planId: string) {
    this.loadingMatrix.set(true);
    this.matrixSvc.getFeatureSets(planId).subscribe({
      next: (sets) => {
        this.featureSets.set(sets);
        const map = new Map<string, Set<string>>();
        for (const s of sets) {
          map.set(s.businessTypeId, new Set(s.enabledFeatures));
        }
        // ensure all active business types have an entry
        for (const bt of this.activeBusinessTypes()) {
          if (!map.has(bt.id)) map.set(bt.id, new Set());
        }
        this.localState.set(map);
        this.loadingMatrix.set(false);
      },
      error: () => { this.toast.error('Erro ao carregar matriz.'); this.loadingMatrix.set(false); }
    });
  }

  isEnabled(btId: string, featureKey: string): boolean {
    return this.localState().get(btId)?.has(featureKey) ?? false;
  }

  toggle(btId: string, featureKey: string) {
    const map = new Map(this.localState());
    const set = new Set(map.get(btId) ?? []);
    if (set.has(featureKey)) set.delete(featureKey);
    else set.add(featureKey);
    map.set(btId, set);
    this.localState.set(map);

    const dirty = new Set(this.dirtyBts());
    dirty.add(btId);
    this.dirtyBts.set(dirty);
  }

  discardChanges() {
    this.loadMatrix(this.selectedPlanId());
    this.dirtyBts.set(new Set());
  }

  saveAll() {
    const planId = this.selectedPlanId();
    const dirty = [...this.dirtyBts()];
    if (dirty.length === 0) return;

    this.saving.set(true);
    const requests = dirty.map(btId =>
      this.matrixSvc.upsert(planId, btId, [...(this.localState().get(btId) ?? [])])
    );

    forkJoin(requests).subscribe({
      next: () => {
        this.toast.success('Matriz atualizada com sucesso.');
        this.dirtyBts.set(new Set());
        this.saving.set(false);
      },
      error: (err) => {
        this.toast.error(err?.error?.message ?? 'Erro ao salvar.');
        this.saving.set(false);
      }
    });
  }
}
