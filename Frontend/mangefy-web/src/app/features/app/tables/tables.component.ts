import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule, Plus, Table, Users, Pencil, Ban, CircleCheck, X, CircleAlert, LoaderCircle,
} from 'lucide-angular';
import { TablesService, TableDto, TableStatus, SaveTableRequest } from './tables.service';
import { AuthService } from '../../../core/auth/auth.service';
import { ToastService } from '../../../core/toast/toast.service';

type DrawerMode = 'create' | 'edit';

interface TableForm {
  number: string;
  capacity: number | null;
  section: string;
}

function emptyForm(): TableForm {
  return { number: '', capacity: null, section: '' };
}

const STATUS_LABEL: Record<TableStatus, string> = {
  Available:   'Disponível',
  Occupied:    'Ocupada',
  Reserved:    'Reservada',
  Unavailable: 'Indisponível',
};

@Component({
  selector: 'app-tables',
  standalone: true,
  imports: [FormsModule, LucideAngularModule],
  template: `
    <div class="page-content">
      <div class="page-header">
        <div>
          <h1 class="page-title">Mesas</h1>
          <p class="page-subtitle">{{ tables().length }} mesa{{ tables().length !== 1 ? 's' : '' }} cadastrada{{ tables().length !== 1 ? 's' : '' }}</p>
        </div>
        @if (canManage()) {
          <div class="page-actions">
            <button class="btn btn-primary" (click)="openCreate()">
              <lucide-icon [img]="Plus" [size]="14" [strokeWidth]="2.5"></lucide-icon>
              Nova Mesa
            </button>
          </div>
        }
      </div>

      @if (error()) {
        <div class="alert-error">
          <lucide-icon [img]="CircleAlert" [size]="15" [strokeWidth]="2"></lucide-icon>
          {{ error() }}
        </div>
      }

      @if (loading()) {
        <div class="loading-state"><div class="spinner"></div></div>
      } @else if (tables().length === 0) {
        <div class="empty-state">
          <lucide-icon [img]="Table" [size]="40" [strokeWidth]="1.5"></lucide-icon>
          <p>Nenhuma mesa cadastrada</p>
          @if (canManage()) {
            <button class="btn btn-primary btn-sm" (click)="openCreate()">Criar primeira mesa</button>
          }
        </div>
      } @else {
        @for (group of groupedBySection(); track group.section) {
          <div class="section-label">{{ group.section }}</div>
          <div class="tables-grid">
            @for (t of group.tables; track t.id) {
              <div class="table-card" [class]="'status-' + t.status.toLowerCase()">
                @if (canManage()) {
                  <button class="tc-edit" (click)="openEdit(t)" title="Editar mesa">
                    <lucide-icon [img]="Pencil" [size]="11" [strokeWidth]="2"></lucide-icon>
                  </button>
                }
                <span class="tc-label">Mesa</span>
                <span class="tc-number">{{ t.number }}</span>
                <span class="tc-capacity">
                  <lucide-icon [img]="Users" [size]="11" [strokeWidth]="2"></lucide-icon>
                  {{ t.capacity }}
                </span>
                <span class="tc-status">{{ statusLabel(t.status) }}</span>

                @if (canManage() && t.status === 'Available') {
                  <button class="tc-toggle" (click)="toggleAvailability(t)" [disabled]="togglingId() === t.id" title="Indisponibilizar">
                    <lucide-icon [img]="Ban" [size]="12" [strokeWidth]="2"></lucide-icon>
                  </button>
                } @else if (canManage() && t.status === 'Unavailable') {
                  <button class="tc-toggle tc-toggle--on" (click)="toggleAvailability(t)" [disabled]="togglingId() === t.id" title="Disponibilizar">
                    <lucide-icon [img]="CircleCheck" [size]="12" [strokeWidth]="2"></lucide-icon>
                  </button>
                }
              </div>
            }
          </div>
        }
      }
    </div>

    <!-- ── Drawer criar/editar ── -->
    @if (drawerOpen()) {
      <div class="drawer-overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <lucide-icon [img]="Table" [size]="16" [strokeWidth]="2"></lucide-icon>
            </div>
            <div>
              <h3 class="drawer-title">{{ drawerMode() === 'create' ? 'Nova Mesa' : 'Editar Mesa' }}</h3>
              <p class="drawer-subtitle">{{ drawerMode() === 'create' ? 'Cadastre uma nova mesa' : 'Mesa ' + editingTable()?.number }}</p>
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

          <div class="form-group">
            <label>Número <span class="req">*</span></label>
            <input class="input" [(ngModel)]="form.number" maxlength="20" placeholder="Ex: 12" />
          </div>
          <div class="form-group">
            <label>Capacidade <span class="req">*</span></label>
            <input class="input" type="text" inputmode="numeric" [(ngModel)]="form.capacity" (keydown)="onlyDigits($event)" placeholder="Ex: 4" />
          </div>
          <div class="form-group">
            <label>Setor</label>
            <input class="input" [(ngModel)]="form.section" maxlength="60" placeholder="Ex: Salão, Varanda, Balcão" />
          </div>
        </div>

        <div class="drawer-footer">
          <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
          <button class="btn btn-primary" (click)="submit()" [disabled]="saving()">
            @if (saving()) {
              <lucide-icon class="icon-spin" [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5"></lucide-icon>
              Salvando...
            } @else {
              {{ drawerMode() === 'create' ? 'Criar Mesa' : 'Salvar Alterações' }}
            }
          </button>
        </div>
      </aside>
    }
  `,
  styles: [`
    .section-label {
      font-size: 10px; font-weight: 700; text-transform: uppercase;
      letter-spacing: .07em; color: var(--text-muted);
      margin-bottom: 12px; margin-top: 22px;
      &:first-of-type { margin-top: 0; }
    }

    .page-subtitle { font-size: 12px; color: var(--text-muted); margin-top: 2px; }

    .tables-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, 128px);
      gap: 12px;
    }

    .table-card {
      position: relative;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 3px;
      padding: 16px 10px 12px;
      border-radius: 14px;
      border: 1px solid var(--surface-border);
      background: var(--surface-card);
      transition: box-shadow .2s;
      &:hover { box-shadow: 0 4px 16px rgba(0,0,0,.07); }

      &.status-available   { background: #f0fdf4; border-color: #bbf7d0; }
      &.status-occupied    { background: #fef2f2; border-color: #fecaca; }
      &.status-reserved    { background: #eff6ff; border-color: #bfdbfe; }
      &.status-unavailable { background: #fafafa; border-color: var(--surface-border); }
    }

    .tc-edit {
      position: absolute; top: 7px; right: 7px;
      width: 20px; height: 20px; border-radius: 6px;
      display: flex; align-items: center; justify-content: center;
      border: none; background: rgba(255,255,255,.65); color: var(--text-muted);
      cursor: pointer; transition: background .15s, color .15s;
      &:hover:not(:disabled) { background: #fff; color: var(--text-primary); }
    }

    .tc-label {
      font-size: 9px; font-weight: 700; text-transform: uppercase;
      letter-spacing: .07em; color: var(--text-muted);
    }
    .tc-number { font-size: 24px; font-weight: 800; color: var(--text-primary); line-height: 1.1; letter-spacing: -.5px; }

    .tc-capacity {
      display: flex; align-items: center; gap: 3px;
      font-size: 11px; color: var(--text-secondary);
      lucide-icon { color: var(--text-muted); }
    }

    .tc-status {
      font-size: 9.5px; font-weight: 700; text-transform: uppercase; letter-spacing: .04em;
      margin-top: 3px;
    }
    .status-available   .tc-status { color: #15803d; }
    .status-occupied    .tc-status { color: #b91c1c; }
    .status-reserved    .tc-status { color: #1d4ed8; }
    .status-unavailable .tc-status { color: #71717a; }

    .tc-toggle {
      position: absolute; bottom: 7px; right: 7px;
      width: 22px; height: 22px; border-radius: 7px;
      display: flex; align-items: center; justify-content: center;
      border: none; cursor: pointer; transition: all .15s;
      background: #fef3c7; color: #b45309;
      &:hover:not(:disabled) { background: #fde68a; }
      &:disabled { opacity: .5; cursor: not-allowed; }
      &.tc-toggle--on { background: #dcfce7; color: #15803d; &:hover:not(:disabled) { background: #bbf7d0; } }
    }

    .req { color: var(--color-brand); }

    @media (max-width: 768px) {
      .tables-grid { grid-template-columns: repeat(auto-fill, 108px); gap: 10px; }
      .page-header { flex-wrap: wrap; gap: 10px; }
      .drawer { width: 100%; border-left: none; top: auto; border-radius: 16px 16px 0 0; }
    }
  `]
})
export class TablesComponent implements OnInit {
  private svc   = inject(TablesService);
  private auth  = inject(AuthService);
  private toast = inject(ToastService);

  readonly Plus         = Plus;
  readonly Table        = Table;
  readonly Users        = Users;
  readonly Pencil       = Pencil;
  readonly Ban          = Ban;
  readonly CircleCheck  = CircleCheck;
  readonly X            = X;
  readonly CircleAlert  = CircleAlert;
  readonly LoaderCircle = LoaderCircle;

  tables  = signal<TableDto[]>([]);
  loading = signal(true);
  error   = signal('');
  togglingId = signal('');

  canManage = computed(() => this.auth.hasPermission('tables.manage'));

  groupedBySection = computed(() => {
    const map = new Map<string, TableDto[]>();
    for (const t of this.tables()) {
      const key = t.section?.trim() || 'Sem setor';
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(t);
    }
    return Array.from(map.entries())
      .sort(([a], [b]) => a === 'Sem setor' ? 1 : b === 'Sem setor' ? -1 : a.localeCompare(b))
      .map(([section, list]) => ({
        section,
        tables: [...list].sort((x, y) => x.number.localeCompare(y.number, undefined, { numeric: true })),
      }));
  });

  drawerOpen  = signal(false);
  drawerMode  = signal<DrawerMode>('create');
  drawerError = signal('');
  saving      = signal(false);
  editingTable = signal<TableDto | null>(null);

  form: TableForm = emptyForm();

  ngOnInit() { this.load(); }

  statusLabel(status: TableStatus) { return STATUS_LABEL[status]; }

  private load() {
    this.loading.set(true);
    this.error.set('');
    this.svc.getAll().subscribe({
      next:  tables => { this.tables.set(tables); this.loading.set(false); },
      error: ()      => { this.error.set('Não foi possível carregar as mesas.'); this.loading.set(false); },
    });
  }

  onlyDigits(event: KeyboardEvent) {
    const allowed = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight', 'Home', 'End'];
    if (!allowed.includes(event.key) && !/^\d$/.test(event.key)) {
      event.preventDefault();
    }
  }

  openCreate() {
    this.form = emptyForm();
    this.drawerMode.set('create');
    this.drawerError.set('');
    this.editingTable.set(null);
    this.drawerOpen.set(true);
  }

  openEdit(table: TableDto) {
    this.form = { number: table.number, capacity: table.capacity, section: table.section ?? '' };
    this.drawerMode.set('edit');
    this.drawerError.set('');
    this.editingTable.set(table);
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
  }

  submit() {
    if (!this.validate()) return;

    const req: SaveTableRequest = {
      number: this.form.number.trim(),
      capacity: Number(this.form.capacity),
      section: this.form.section.trim() || null,
    };

    this.saving.set(true);
    this.drawerError.set('');

    const request$ = this.drawerMode() === 'create'
      ? this.svc.create(req)
      : this.svc.update(this.editingTable()!.id, req);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.load();
        this.toast.show(
          this.drawerMode() === 'create' ? 'Mesa criada com sucesso.' : 'Mesa atualizada com sucesso.',
          'success'
        );
      },
      error: (err: { error?: { message?: string } }) => {
        this.saving.set(false);
        this.drawerError.set(err?.error?.message ?? 'Erro ao salvar mesa.');
      },
    });
  }

  toggleAvailability(table: TableDto) {
    const nextStatus: 'Available' | 'Unavailable' = table.status === 'Available' ? 'Unavailable' : 'Available';
    this.togglingId.set(table.id);
    this.svc.setStatus(table.id, nextStatus).subscribe({
      next:  () => { this.togglingId.set(''); this.load(); },
      error: () => { this.togglingId.set(''); this.error.set('Erro ao alterar status da mesa.'); },
    });
  }

  private validate(): boolean {
    if (!this.form.number?.trim()) {
      this.drawerError.set('Número da mesa é obrigatório.'); return false;
    }
    if (!this.form.capacity || this.form.capacity < 1) {
      this.drawerError.set('Capacidade deve ser pelo menos 1.'); return false;
    }
    return true;
  }
}
