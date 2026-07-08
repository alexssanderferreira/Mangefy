import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { TenantService, TenantDto, PagedResult } from '../services/tenant.service';
import { PlansService, PlanDto } from '../../plans/plans.service';
import { OwnerService, OwnerListItemDto } from '../../owners/owner.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { LucideAngularModule, Plus, X, Search, ChevronLeft, ChevronRight, House, CircleAlert, User, Check, LoaderCircle } from 'lucide-angular';

interface BusinessTypeDto { id: string; name: string; }

interface CreateForm {
  ownerId: string;
  name: string;
  slug: string;
  planId: string;
  businessTypeId: string;
  timezone: string;
  trialDays: number;
}

function emptyForm(): CreateForm {
  return { ownerId: '', name: '', slug: '', planId: '', businessTypeId: '', timezone: 'America/Sao_Paulo', trialDays: 14 };
}

const STATUS_LABEL: Record<string, string> = {
  Active: 'Ativo', TrialPeriod: 'Trial', Suspended: 'Suspenso', Cancelled: 'Cancelado',
};
const OWNER_STATUS_LABEL: Record<string, string> = {
  Active: 'Ativo', PendingActivation: 'Aguardando ativação', Inactive: 'Inativo',
};
const STATUS_BADGE_CLASS: Record<string, string> = {
  Active: 'badge-success', TrialPeriod: 'badge-info', Suspended: 'badge-warning', Cancelled: 'badge-neutral',
};
const OWNER_STATUS_BADGE_CLASS: Record<string, string> = {
  Active: 'badge-success', PendingActivation: 'badge-warning', Inactive: 'badge-neutral',
};

@Component({
  selector: 'app-tenant-list',
  standalone: true,
  imports: [FormsModule, DatePipe, LucideAngularModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Estabelecimentos</h1>
          <p class="page-subtitle">{{ total() }} estabelecimento{{ total() !== 1 ? 's' : '' }}</p>
        </div>
        <button class="btn btn-primary" (click)="openCreate()">
          <lucide-icon [img]="Plus" [size]="14" [strokeWidth]="2.5"></lucide-icon>
          Novo Estabelecimento
        </button>
      </div>

      <!-- Quick filter tabs -->
      <div class="filter-chips">
        <button class="filter-chip" [class.active]="filterStatus() === ''"           (click)="filterStatus.set('')">Todos</button>
        <button class="filter-chip" [class.active]="filterStatus() === 'Active'"      (click)="filterStatus.set('Active')">Ativos</button>
        <button class="filter-chip" [class.active]="filterStatus() === 'TrialPeriod'" (click)="filterStatus.set('TrialPeriod')">Trial</button>
        <button class="filter-chip" [class.active]="filterStatus() === 'Suspended'"   (click)="filterStatus.set('Suspended')">Suspensos</button>
        <button class="filter-chip" [class.active]="filterStatus() === 'Cancelled'"   (click)="filterStatus.set('Cancelled')">Cancelados</button>
        @if (search() || filterStatus()) {
          <button class="filter-chip-clear" (click)="search.set(''); filterStatus.set('')">
            <lucide-icon [img]="X" [size]="11" [strokeWidth]="2.5"></lucide-icon>
            Limpar filtros
          </button>
        }
      </div>

      <!-- Filtros -->
      <div class="filters">
        <div class="search-wrap">
          <lucide-icon [img]="Search" [size]="14" [strokeWidth]="2"></lucide-icon>
          <input class="search-input" [ngModel]="search()" (ngModelChange)="search.set($event)" placeholder="Buscar por nome ou slug..." />
        </div>
        <select class="filter-select" [ngModel]="pageSize()" (ngModelChange)="onPageSizeChange($event)">
          <option [ngValue]="10">10 por página</option>
          <option [ngValue]="25">25 por página</option>
          <option [ngValue]="50">50 por página</option>
          <option [ngValue]="100">100 por página</option>
        </select>
      </div>

      <!-- Error -->
      @if (error()) {
        <div class="alert-error">{{ error() }}</div>
      }

      <!-- Table -->
      <div class="table-wrap">
        <table class="table">
          <colgroup>
            <col style="width:18%">
            <col style="width:13%">
            <col style="width:20%">
            <col style="width:9%">
            <col style="width:9%">
            <col style="width:11%">
            <col style="width:10%">
            <col style="width:10%">
          </colgroup>
          <thead>
            <tr>
              <th class="th-sort" (click)="setSort('name')">Estabelecimento <span class="sort-icon">{{ sortIcon('name') }}</span></th>
              <th class="th-sort" (click)="setSort('slug')">Slug <span class="sort-icon">{{ sortIcon('slug') }}</span></th>
              <th class="th-sort" (click)="setSort('email')">E-mail <span class="sort-icon">{{ sortIcon('email') }}</span></th>
              <th class="th-sort" (click)="setSort('status')">Status <span class="sort-icon">{{ sortIcon('status') }}</span></th>
              <th class="th-sort" (click)="setSort('planId')">Plano <span class="sort-icon">{{ sortIcon('planId') }}</span></th>
              <th class="th-sort" (click)="setSort('businessTypeId')">Tipo <span class="sort-icon">{{ sortIcon('businessTypeId') }}</span></th>
              <th class="th-sort" (click)="setSort('trialEndsAt')">Trial <span class="sort-icon">{{ sortIcon('trialEndsAt') }}</span></th>
              <th class="th-sort" (click)="setSort('createdAt')">Criado em <span class="sort-icon">{{ sortIcon('createdAt') }}</span></th>
            </tr>
          </thead>
          <tbody>
            @if (loading()) {
              @for (i of [1,2,3,4,5,6,7,8,9,10]; track i) {
                <tr class="skeleton-row">
                  <td><div class="skel" style="width:70%"></div></td>
                  <td><div class="skel" style="width:80%"></div></td>
                  <td><div class="skel" style="width:85%"></div></td>
                  <td><div class="skel" style="width:50px"></div></td>
                  <td><div class="skel" style="width:55px"></div></td>
                  <td><div class="skel" style="width:65%"></div></td>
                  <td><div class="skel" style="width:52px"></div></td>
                  <td><div class="skel" style="width:70px"></div></td>
                </tr>
              }
            } @else if (filtered().length === 0) {
              <tr>
                <td colspan="8" class="empty-cell">Nenhum estabelecimento encontrado.</td>
              </tr>
            } @else {
              @for (t of filtered(); track t.id) {
                <tr class="table-row" (click)="goDetail(t.id)">
                  <td class="td-name">
                    <span class="name-avatar">{{ t.name.charAt(0).toUpperCase() }}</span>
                    <span class="name-text" [title]="t.name">{{ t.name }}</span>
                  </td>
                  <td class="td-slug" [title]="t.slug">{{ t.slug }}</td>
                  <td class="td-email" [title]="t.email ?? ''">{{ t.email ?? '—' }}</td>
                  <td><span class="badge" [class]="statusBadgeClass(t.status)">{{ statusLabel(t.status) }}</span></td>
                  <td class="td-plan">{{ planName(t.planId) }}</td>
                  <td class="td-type" [title]="businessTypeName(t.businessTypeId)">{{ businessTypeName(t.businessTypeId) }}</td>
                  <td>
                    @if (t.status === 'TrialPeriod' && t.trialEndsAt) {
                      @let days = trialDaysLeft(t.trialEndsAt);
                      <span class="trial-days" [class.trial-expired]="days <= 0">
                        {{ days > 0 ? days + ' dias' : 'Expirado' }}
                      </span>
                    } @else { <span class="td-muted">—</span> }
                  </td>
                  <td class="td-muted">{{ t.createdAt | date:'dd/MM/yyyy' }}</td>
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
            @if (p === -1) {
              <span class="pg-dots">…</span>
            } @else {
              <button class="pg-btn" [class.pg-active]="p === page()" (click)="goPage(p)">{{ p }}</button>
            }
          }
          <button class="pg-btn" (click)="goPage(page() + 1)" [disabled]="page() === totalPages()">
            <lucide-icon [img]="ChevronRight" [size]="14" [strokeWidth]="2"></lucide-icon>
          </button>
          <span class="pg-info">{{ (page() - 1) * pageSize() + 1 }}–{{ min(page() * pageSize(), total()) }} de {{ total() }}</span>
        </div>
      }
    </div>

    <!-- ── Drawer criar ── -->
    @if (drawerOpen()) {
      <div class="drawer-overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <lucide-icon [img]="House" [size]="16" [strokeWidth]="2"></lucide-icon>
            </div>
            <div>
              <h3 class="drawer-title">Novo Estabelecimento</h3>
              <p class="drawer-subtitle">Preencha os dados do estabelecimento</p>
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
              <lucide-icon [img]="House" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Identificação
            </div>
            <div class="field">
              <label class="field-label">Nome do estabelecimento <span class="req">*</span></label>
              <input class="field-input" [(ngModel)]="form.name" (ngModelChange)="onNameChange($event)" placeholder="Ex: Pizzaria Bella Napoli" maxlength="100" />
            </div>
            <div class="field">
              <label class="field-label" style="display:flex;justify-content:space-between">
                Slug <span class="req">*</span>
                <span style="font-weight:400;color:#bbb;font-size:11px">Identificador único na URL</span>
              </label>
              <div class="slug-wrap" [class.slug-error]="slugTouched && !slugValid()">
                <span class="slug-prefix">mgf/</span>
                <input class="field-input slug-input" [(ngModel)]="form.slug" (ngModelChange)="onSlugChange($event)" placeholder="bella-napoli" maxlength="60" />
              </div>
              @if (slugTouched && !slugValid()) {
                <span class="field-error">Apenas letras minúsculas, números e hifens.</span>
              }
            </div>
            <div class="form-grid-2">
              <div class="field">
                <label class="field-label">Fuso horário</label>
                <select class="field-input field-select" [(ngModel)]="form.timezone">
                  <option value="America/Sao_Paulo">America/Sao_Paulo</option>
                  <option value="America/Manaus">America/Manaus</option>
                  <option value="America/Belem">America/Belem</option>
                  <option value="America/Fortaleza">America/Fortaleza</option>
                  <option value="America/Noronha">America/Noronha</option>
                </select>
              </div>
              <div class="field">
                <label class="field-label">Dias de trial</label>
                <input class="field-input" type="text" inputmode="numeric" [(ngModel)]="form.trialDays" (keydown)="onlyDigits($event)" />
              </div>
            </div>
          </div>

          <!-- Seção: Atribuição -->
          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="User" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Atribuição
            </div>
            <div class="field">
              <label class="field-label" style="display:flex;justify-content:space-between">
                Dono <span class="req">*</span>
                <span style="font-weight:400;color:#bbb;font-size:11px">Busque pelo nome ou e-mail</span>
              </label>
              <div class="owner-search-wrap" [class.focused]="ownerSearchFocused">
                <lucide-icon [img]="Search" [size]="13" [strokeWidth]="2"></lucide-icon>
                <input class="owner-search-input"
                  [ngModel]="ownerSearchText()"
                  (ngModelChange)="onOwnerSearch($event)"
                  (focus)="ownerSearchFocused = true"
                  (blur)="onOwnerBlur()"
                  placeholder="Buscar dono..." />
                @if (form.ownerId) {
                  <button class="owner-clear-btn" type="button" (mousedown)="clearOwner($event)">
                    <lucide-icon [img]="X" [size]="12" [strokeWidth]="2.5"></lucide-icon>
                  </button>
                }
              </div>
              @if (ownerSearchFocused && ownerResults().length > 0) {
                <div class="owner-dropdown">
                  @for (o of ownerResults(); track o.id) {
                    <button class="owner-option" type="button" (mousedown)="selectOwner(o, $event)">
                      <span class="owner-opt-avatar">{{ o.name.charAt(0).toUpperCase() }}</span>
                      <span class="owner-opt-info">
                        <span class="owner-opt-name">{{ o.name }}</span>
                        <span class="owner-opt-email">{{ o.email }}</span>
                      </span>
                      <span class="badge owner-opt-badge" [class]="ownerStatusBadgeClass(o.status)">{{ ownerStatusLabel(o.status) }}</span>
                    </button>
                  }
                </div>
              }
              @if (form.ownerId && selectedOwner()) {
                <div class="owner-selected">
                  <lucide-icon [img]="Check" [size]="12" [strokeWidth]="2.5"></lucide-icon>
                  {{ selectedOwner()!.name }} — {{ selectedOwner()!.email }}
                </div>
              }
            </div>
            <div class="form-grid-2">
              <div class="field">
                <label class="field-label">Plano <span class="req">*</span></label>
                <select class="field-input field-select" [(ngModel)]="form.planId">
                  <option value="">Selecione...</option>
                  @for (p of plans(); track p.id) {
                    @if (p.status === 'Active') {
                      <option [value]="p.id">{{ p.name }}</option>
                    }
                  }
                </select>
              </div>
              <div class="field">
                <label class="field-label">Tipo de negócio <span class="req">*</span></label>
                <select class="field-input field-select" [(ngModel)]="form.businessTypeId">
                  <option value="">Selecione...</option>
                  @for (bt of businessTypes(); track bt.id) {
                    <option [value]="bt.id">{{ bt.name }}</option>
                  }
                </select>
              </div>
            </div>
          </div>
        </div>

        <div class="drawer-footer">
          <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
          <button class="btn btn-primary" (click)="submit()" [disabled]="saving()">
            @if (saving()) {
              <lucide-icon class="icon-spin" [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5"></lucide-icon>
              Criando...
            } @else { Criar Estabelecimento }
          </button>
        </div>
      </aside>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; }

    .page-header {
      display: flex; align-items: center; justify-content: space-between;
      margin-bottom: 16px;
    }
    .page-subtitle { font-size: 12px; color: #aaa; margin-top: 2px; }

    /* Filtros */
    .filters { display: flex; gap: 10px; margin-bottom: 12px; }
    .search-wrap {
      flex: 1;
      display: flex; align-items: center; gap: 8px;
      background: #fff; border: 1px solid #e8e8ec; border-radius: 8px;
      padding: 7px 12px;
      lucide-icon { color: #bbb; flex-shrink: 0; }
      &:focus-within { border-color: var(--color-brand); }
    }
    .search-input {
      flex: 1; border: none; outline: none; font-size: 13px; color: #111;
      &::placeholder { color: #ccc; }
    }
    .filter-select {
      padding: 7px 12px; border: 1px solid #e8e8ec; border-radius: 8px;
      font-size: 13px; color: #555; background: #fff; outline: none; cursor: pointer;
      &:focus { border-color: var(--color-brand); }
    }

    /* Table */
    .table-wrap { background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; overflow: hidden; }
    .table {
      width: 100%; border-collapse: collapse; table-layout: fixed;
      th {
        padding: 9px 14px; text-align: left;
        font-size: 10px; font-weight: 700; text-transform: uppercase;
        letter-spacing: .06em; color: #bbb; white-space: nowrap;
        border-bottom: 1px solid #f0f0f3; background: #fafafa;
        overflow: hidden; text-overflow: ellipsis;
      }
      td {
        padding: 9px 14px; border-bottom: 1px solid #f4f4f6;
        font-size: 13px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
      }
      tr:last-child td { border-bottom: none; }
    }
    .table-row { cursor: pointer; transition: background .1s; &:hover td { background: #f9f9fb; } }
    .td-name { display: flex; align-items: center; gap: 9px; overflow: hidden; }
    .name-avatar {
      flex-shrink: 0; width: 26px; height: 26px; border-radius: 7px;
      background: var(--color-brand); color: #fff;
      display: flex; align-items: center; justify-content: center;
      font-size: 10px; font-weight: 800;
    }
    .name-text { font-weight: 600; color: #111; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .td-slug  { font-family: monospace; font-size: 11px; color: #999; }
    .td-email { color: #555; font-size: 12px; }
    .td-plan  { font-size: 12px; color: #333; font-weight: 600; }
    .td-type  { font-size: 12px; color: #777; }
    .td-muted { color: #999; font-size: 12px; }
    .th-sort { cursor: pointer; user-select: none; &:hover { color: #555; background: #f4f4f6; } }
    .sort-icon { font-size: 10px; opacity: .4; margin-left: 2px; }

    .trial-days {
      display: inline-block; padding: 2px 9px; border-radius: 99px;
      font-size: 11px; font-weight: 700;
      background: #dbeafe; color: #1d4ed8;
    }
    .trial-expired { background: #fee2e2; color: #b91c1c; }

    /* Owner search */
    .owner-search-wrap {
      display: flex; align-items: center; gap: 8px;
      border: 1px solid #e8e8ec; border-radius: 8px; padding: 8px 10px;
      background: #fff; transition: border-color .15s;
      lucide-icon { color: #bbb; flex-shrink: 0; }
      &.focused { border-color: var(--color-brand); }
    }
    .owner-search-input { flex: 1; border: none; outline: none; font-size: 13px; color: #111; &::placeholder { color: #ccc; } }
    .owner-clear-btn { background: none; border: none; color: #bbb; cursor: pointer; padding: 2px; display: flex; align-items: center; &:hover { color: #555; } }
    .owner-dropdown {
      background: #fff; border: 1px solid #e8e8ec; border-radius: 10px;
      box-shadow: 0 8px 24px rgba(0,0,0,.1); z-index: 10;
      overflow: hidden; max-height: 220px; overflow-y: auto; margin-top: 4px;
    }
    .owner-option {
      width: 100%; display: flex; align-items: center; gap: 10px;
      padding: 9px 12px; background: none; border: none; border-bottom: 1px solid #f4f4f6;
      cursor: pointer; text-align: left; transition: background .1s;
      &:last-child { border-bottom: none; }
      &:hover { background: #f9f9fb; }
    }
    .owner-opt-avatar {
      flex-shrink: 0; width: 26px; height: 26px; border-radius: 7px;
      background: var(--color-brand); color: #fff;
      display: flex; align-items: center; justify-content: center;
      font-size: 11px; font-weight: 800;
    }
    .owner-opt-info { flex: 1; display: flex; flex-direction: column; min-width: 0; }
    .owner-opt-name { font-size: 13px; font-weight: 600; color: #111; }
    .owner-opt-email { font-size: 11px; color: #888; }
    .owner-opt-badge { margin-left: auto; flex-shrink: 0; }
    .owner-selected {
      display: inline-flex; align-items: center; gap: 5px;
      font-size: 11px; color: #15803d; margin-top: 4px;
      lucide-icon { color: #16a34a; }
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
    .field-hint { font-weight: 400; color: #aaa; font-size: 11px; }
    .field-input {
      width: 100%; padding: 8px 10px; border: 1px solid #e8e8ec; border-radius: 7px;
      font-size: 13px; color: #111; outline: none; transition: border-color .15s, box-shadow .15s; background: #fff; box-sizing: border-box;
      &:focus { border-color: var(--color-brand); box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-brand) 12%, transparent); }
      &::placeholder { color: #ccc; }
    }
    .field-select { appearance: none; background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='11' height='11' viewBox='0 0 24 24' fill='none' stroke='%23aaa' stroke-width='2.5'%3E%3Cpolyline points='6 9 12 15 18 9'/%3E%3C/svg%3E"); background-repeat: no-repeat; background-position: right 8px center; padding-right: 26px; cursor: pointer; }
    .field-error { font-size: 11px; color: #dc2626; }

    /* Responsive */
    @media (max-width: 768px) {
      .page { padding: 14px; }
      .filters { flex-wrap: wrap; }
      .filter-select { flex: 1; min-width: 120px; }
      .table-wrap { overflow-x: auto; }
      .table { table-layout: auto; min-width: 560px; }
      /* Hide slug and business type columns on mobile */
      .table th:nth-child(2),
      .table td:nth-child(2),
      .table th:nth-child(7),
      .table td:nth-child(7) { display: none; }
      .drawer { width: 100%; border-left: none; border-top: 1px solid #e8e8ec; top: auto; border-radius: 16px 16px 0 0; }
      .field-group { grid-template-columns: 1fr; }
      .pagination { justify-content: center; }
      .pg-info { width: 100%; text-align: center; margin-left: 0; margin-top: 4px; }
    }


    /* Slug field */
    .slug-wrap {
      display: flex; align-items: center;
      border: 1px solid #e8e8ec; border-radius: 8px; overflow: hidden;
      transition: border-color .15s;
      &:focus-within { border-color: var(--color-brand); }
      &.slug-error { border-color: #fca5a5; }
    }
    .slug-prefix {
      padding: 0 10px; font-size: 12px; color: #aaa;
      background: #f7f7f9; border-right: 1px solid #e8e8ec;
      align-self: stretch; display: flex; align-items: center;
      white-space: nowrap; font-family: monospace;
    }
    .slug-input {
      border: none !important; border-radius: 0 !important;
      flex: 1; font-family: monospace; font-size: 13px;
    }
  `],
})
export class TenantListComponent implements OnInit {
  private svc = inject(TenantService);
  private plansSvc = inject(PlansService);
  private ownerSvc = inject(OwnerService);
  private http = inject(HttpClient);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  readonly Plus = Plus;
  readonly X = X;
  readonly Search = Search;
  readonly ChevronLeft = ChevronLeft;
  readonly ChevronRight = ChevronRight;
  readonly House = House;
  readonly CircleAlert = CircleAlert;
  readonly User = User;
  readonly Check = Check;
  readonly LoaderCircle = LoaderCircle;

  tenants       = signal<TenantDto[]>([]);
  loading       = signal(true);
  error         = signal('');
  search        = signal('');
  filterStatus  = signal('');
  sortCol       = signal('');
  sortAsc       = signal(true);

  page          = signal(1);
  pageSize      = signal(10);
  total         = signal(0);
  totalPages    = computed(() => Math.ceil(this.total() / this.pageSize()));

  drawerOpen  = signal(false);
  drawerError = signal('');
  saving      = signal(false);
  slugTouched = false;

  ownerSearchText  = signal('');
  ownerResults     = signal<OwnerListItemDto[]>([]);
  selectedOwner    = signal<OwnerListItemDto | null>(null);
  ownerSearchFocused = false;
  private ownerSearchTimer: any = null;

  plans         = signal<PlanDto[]>([]);
  businessTypes = signal<BusinessTypeDto[]>([]);

  form: CreateForm = emptyForm();

  filtered = computed(() => {
    const q = this.search().toLowerCase();
    const st = this.filterStatus();
    const col = this.sortCol();
    const asc = this.sortAsc();

    let list = this.tenants().filter(t => {
      const matchSearch = !q || t.name.toLowerCase().includes(q) || t.slug.toLowerCase().includes(q);
      const matchStatus = !st || t.status === st;
      return matchSearch && matchStatus;
    });

    if (col) {
      const datecols = new Set(['trialEndsAt', 'createdAt']);
      list = [...list].sort((a, b) => {
        const av = (a as any)[col];
        const bv = (b as any)[col];
        if (av == null && bv == null) return 0;
        if (av == null) return 1;
        if (bv == null) return -1;
        let cmp: number;
        if (datecols.has(col)) {
          cmp = new Date(av).getTime() - new Date(bv).getTime();
        } else {
          cmp = String(av).localeCompare(String(bv), 'pt-BR', { sensitivity: 'base' });
        }
        return asc ? cmp : -cmp;
      });
    }

    return list;
  });

  pageNumbers = computed(() => {
    const total = this.totalPages();
    const cur = this.page();
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    const pages: number[] = [1];
    if (cur > 3) pages.push(-1);
    for (let i = Math.max(2, cur - 1); i <= Math.min(total - 1, cur + 1); i++) pages.push(i);
    if (cur < total - 2) pages.push(-1);
    pages.push(total);
    return pages;
  });

  setSort(col: string) {
    if (this.sortCol() === col) this.sortAsc.update(v => !v);
    else { this.sortCol.set(col); this.sortAsc.set(true); }
  }

  sortIcon(col: string) {
    if (this.sortCol() !== col) return '↕';
    return this.sortAsc() ? '↑' : '↓';
  }

  goPage(p: number) {
    if (p < 1 || p > this.totalPages()) return;
    this.page.set(p);
    this.load();
  }

  onPageSizeChange(size: number) {
    this.pageSize.set(+size);
    this.page.set(1);
    this.load();
  }

  min(a: number, b: number) { return Math.min(a, b); }

  slugValid = computed(() => /^[a-z0-9-]+$/.test(this.form.slug));

  ngOnInit() {
    this.load();
    this.plansSvc.getAll().subscribe(p => this.plans.set(p));
    this.http.get<BusinessTypeDto[]>(`${environment.apiUrl}/admin/business-types`)
      .subscribe(bt => this.businessTypes.set(bt));

    // Filtro rápido via queryParam (ex: do dashboard)
    const statusFromUrl = this.route.snapshot.queryParamMap.get('status');
    if (statusFromUrl) this.filterStatus.set(statusFromUrl);

    // Atalho vindo de /admin/owners/:id → abre drawer de criação com owner pré-selecionado
    const ownerIdFromUrl = this.route.snapshot.queryParamMap.get('newWithOwner');
    if (ownerIdFromUrl) {
      this.ownerSvc.getById(ownerIdFromUrl).subscribe(o => {
        this.openCreate();
        this.form.ownerId = o.id;
        this.selectedOwner.set({ id: o.id, name: o.name, email: o.email, status: o.status, tenantCount: o.metrics?.totalEstablishments ?? 0, lastLoginAt: o.lastLoginAt, createdAt: o.createdAt });
        this.ownerSearchText.set(o.name);
      });
    }
  }

  planName(planId: string) {
    return this.plans().find(p => p.id === planId)?.name ?? '—';
  }

  businessTypeName(id: string) {
    return (this.businessTypes() as any[]).find((bt: any) => bt.id === id)?.name ?? '—';
  }

  private load() {
    this.loading.set(true);
    this.svc.getPaged(this.page(), this.pageSize()).subscribe({
      next: res => {
        // defensivo: API pode retornar array (formato antigo) ou PagedResult
        if (Array.isArray(res)) {
          this.tenants.set(res as any);
          this.total.set((res as any).length);
        } else {
          this.tenants.set(res.items ?? []);
          this.total.set(res.total ?? 0);
        }
        this.loading.set(false);
      },
      error: () => { this.error.set('Não foi possível carregar os estabelecimentos.'); this.loading.set(false); },
    });
  }

  goDetail(id: string) { this.router.navigate(['/admin/tenants', id]); }

  openCreate() {
    this.form = emptyForm();
    this.slugTouched = false;
    this.drawerError.set('');
    this.ownerSearchText.set('');
    this.ownerResults.set([]);
    this.selectedOwner.set(null);
    this.drawerOpen.set(true);
    this.loadDropdowns();
  }

  onOwnerSearch(text: string) {
    this.ownerSearchText.set(text);
    this.form.ownerId = '';
    this.selectedOwner.set(null);
    clearTimeout(this.ownerSearchTimer);
    if (!text.trim()) { this.ownerResults.set([]); return; }
    this.ownerSearchTimer = setTimeout(() => {
      this.ownerSvc.getAll(1, 50).subscribe(res => {
        const q = text.toLowerCase();
        this.ownerResults.set(
          res.items.filter(o => o.name.toLowerCase().includes(q) || o.email.toLowerCase().includes(q)).slice(0, 8)
        );
      });
    }, 200);
  }

  selectOwner(owner: OwnerListItemDto, event: MouseEvent) {
    event.preventDefault();
    this.form.ownerId = owner.id;
    this.selectedOwner.set(owner);
    this.ownerSearchText.set(owner.name);
    this.ownerResults.set([]);
    this.ownerSearchFocused = false;
  }

  clearOwner(event: MouseEvent) {
    event.preventDefault();
    this.form.ownerId = '';
    this.selectedOwner.set(null);
    this.ownerSearchText.set('');
    this.ownerResults.set([]);
  }

  onOwnerBlur() {
    setTimeout(() => { this.ownerSearchFocused = false; }, 150);
  }

  ownerStatusLabel(s: string) { return OWNER_STATUS_LABEL[s] ?? s; }
  statusBadgeClass(s: string) { return STATUS_BADGE_CLASS[s] ?? 'badge-neutral'; }
  ownerStatusBadgeClass(s: string) { return OWNER_STATUS_BADGE_CLASS[s] ?? 'badge-neutral'; }

  closeDrawer() { if (!this.saving()) this.drawerOpen.set(false); }

  private loadDropdowns() {
    if (this.plans().length === 0)
      this.plansSvc.getAll().subscribe(p => this.plans.set(p.filter(x => x.status === 'Active')));
    if (this.businessTypes().length === 0)
      this.http.get<BusinessTypeDto[]>(`${environment.apiUrl}/admin/business-types`)
        .subscribe(bt => this.businessTypes.set(bt.filter((x: any) => x.isActive)));
  }

  onNameChange(name: string) {
    if (!this.slugTouched) {
      this.form.slug = name.toLowerCase()
        .normalize('NFD').replace(/[̀-ͯ]/g, '')
        .replace(/[^a-z0-9\s-]/g, '')
        .trim().replace(/\s+/g, '-');
    }
  }

  onSlugChange(val: string) {
    this.slugTouched = true;
    this.form.slug = val.toLowerCase().replace(/[^a-z0-9-]/g, '');
  }

  onlyDigits(event: KeyboardEvent) {
    const allowed = ['Backspace', 'Delete', 'Tab', 'ArrowLeft', 'ArrowRight'];
    if (!allowed.includes(event.key) && !/^\d$/.test(event.key)) event.preventDefault();
  }

  submit() {
    this.drawerError.set('');
    if (!this.form.ownerId)        { this.drawerError.set('Selecione o dono.'); return; }
    if (!this.form.name.trim())    { this.drawerError.set('Nome é obrigatório.'); return; }
    if (!this.slugValid())          { this.drawerError.set('Slug inválido.'); return; }
    if (!this.form.planId)          { this.drawerError.set('Selecione um plano.'); return; }
    if (!this.form.businessTypeId)  { this.drawerError.set('Selecione o tipo de negócio.'); return; }

    this.saving.set(true);
    this.svc.create({ ...this.form }).subscribe({
      next: res => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.page.set(1);
        this.load();
        this.router.navigate(['/admin/tenants', res.id]);
      },
      error: (err) => { this.drawerError.set(err?.error?.message ?? 'Erro ao criar estabelecimento.'); this.saving.set(false); },
    });
  }

  statusLabel(s: string) { return STATUS_LABEL[s] ?? s; }

  trialDaysLeft(dateStr: string) {
    return Math.ceil((new Date(dateStr).getTime() - Date.now()) / 86_400_000);
  }
}
