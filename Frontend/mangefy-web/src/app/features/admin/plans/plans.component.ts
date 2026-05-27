import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe } from '@angular/common';
import { PlansService, PlanDto, CreatePlanRequest, UpdatePlanRequest } from './plans.service';

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
  imports: [FormsModule, CurrencyPipe],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Planos</h1>
          <p class="page-subtitle">{{ plans().length }} plano{{ plans().length !== 1 ? 's' : '' }} cadastrado{{ plans().length !== 1 ? 's' : '' }}</p>
        </div>
        <button class="btn btn-primary" (click)="openCreate()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Novo Plano
        </button>
      </div>

      <!-- Error -->
      @if (error()) {
        <div class="alert-error">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
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
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
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
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                  <span>{{ plan.maxTables }} mesas</span>
                </div>
                <div class="limit-row">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 11l19-9-9 19-2-8-8-2z"/></svg>
                  <span>{{ plan.maxMenuItems }} itens de menu</span>
                </div>
                <div class="limit-row">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                  <span>{{ plan.maxUsers }} utilizadores</span>
                </div>
                @if (plan.maxCustomRoles > 0) {
                  <div class="limit-row">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="8" r="4"/><path d="M20 21a8 8 0 1 0-16 0"/></svg>
                    <span>{{ plan.maxCustomRoles }} cargos personalizados</span>
                  </div>
                }
              </div>
              <div class="plan-actions">
                <button class="btn-action" (click)="openEdit(plan)">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                  Editar
                </button>
                <button class="btn-action btn-action--warn" (click)="toggleStatus(plan)" [disabled]="toggling() === plan.id">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="8" y1="12" x2="16" y2="12"/></svg>
                  {{ toggling() === plan.id ? '...' : 'Desativar' }}
                </button>
                <button class="btn-action btn-action--danger" (click)="deletePlan(plan)" [disabled]="deleting() === plan.id">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4h6v2"/></svg>
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
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
                  <span>{{ plan.maxTables }} mesas</span>
                </div>
                <div class="limit-row">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 11l19-9-9 19-2-8-8-2z"/></svg>
                  <span>{{ plan.maxMenuItems }} itens de menu</span>
                </div>
                <div class="limit-row">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                  <span>{{ plan.maxUsers }} utilizadores</span>
                </div>
                @if (plan.maxCustomRoles > 0) {
                  <div class="limit-row">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="8" r="4"/><path d="M20 21a8 8 0 1 0-16 0"/></svg>
                    <span>{{ plan.maxCustomRoles }} cargos personalizados</span>
                  </div>
                }
              </div>
              <div class="plan-actions">
                <button class="btn-action" (click)="openEdit(plan)">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                  Editar
                </button>
                <button class="btn-action btn-action--success" (click)="toggleStatus(plan)" [disabled]="toggling() === plan.id">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="9 12 11 14 15 10"/></svg>
                  {{ toggling() === plan.id ? '...' : 'Ativar' }}
                </button>
                <button class="btn-action btn-action--danger" (click)="deletePlan(plan)" [disabled]="deleting() === plan.id">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4h6v2"/></svg>
                  {{ deleting() === plan.id ? '...' : 'Excluir' }}
                </button>
              </div>
            </div>
          }
          </div>
        }
      }
    </div>

    <!-- ── Toast ── -->
    @if (toast()) {
      <div class="toast">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="9 12 11 14 15 10"/></svg>
        {{ toast() }}
      </div>
    }

    <!-- ── Modal de confirmação de exclusão ── -->
    @if (deleteTarget()) {
      <div class="modal-overlay" (click)="cancelDelete()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14H6L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4h6v2"/></svg>
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
      <div class="overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
            </div>
            <div>
              <h3 class="drawer-title">{{ drawerMode() === 'create' ? 'Novo Plano' : 'Editar Plano' }}</h3>
              <p class="drawer-subtitle">{{ drawerMode() === 'create' ? 'Configure o novo plano de assinatura' : editingPlan()?.name }}</p>
            </div>
          </div>
          <button class="btn-close" (click)="closeDrawer()">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>

        <div class="drawer-body">
          @if (drawerError()) {
            <div class="alert-error">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              {{ drawerError() }}
            </div>
          }

          <!-- Identificação -->
          <div class="form-section">
            <div class="form-section-header">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
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
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
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
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
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
              <svg class="spin-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 12a9 9 0 1 1-6.219-8.56"/></svg>
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
    .page-title   { font-size: 20px; font-weight: 700; color: #111; }
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

    @keyframes shimmer {
      0%   { background-position: 200% 0; }
      100% { background-position: -200% 0; }
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
      svg { color: #aaa; flex-shrink: 0; }
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

    /* ── Empty ── */
    .empty-state {
      display: flex; flex-direction: column; align-items: center;
      padding: 60px 0; gap: 12px; color: #ccc;
      p { font-size: 14px; color: #bbb; }
    }

    /* ── Alert ── */
    .alert-error {
      display: flex; align-items: center; gap: 8px;
      background: #fef2f2; color: #b91c1c;
      border: 1px solid #fecaca; border-radius: 10px;
      padding: 12px 14px; font-size: 13px; margin-bottom: 20px;
    }
    .mb-16 { margin-bottom: 16px; }

    /* ── Buttons ── */
    .btn {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 8px 16px; border-radius: 8px;
      font-size: 13px; font-weight: 600; cursor: pointer;
      border: none; transition: all .15s;
    }
    .btn-primary { background: var(--color-brand); color: #fff; &:hover { opacity: .9; } &:disabled { opacity: .5; cursor: not-allowed; } }
    .btn-ghost   { background: #f4f4f5; color: #555; border: 1px solid #e8e8ec; &:hover { background: #ebebef; } }
    .btn-danger  { background: #dc2626; color: #fff; &:hover { background: #b91c1c; } }
    .btn-sm { padding: 6px 12px; font-size: 12px; }

    /* ── Toast ── */
    .toast {
      position: fixed; bottom: 28px; right: 28px;
      display: flex; align-items: center; gap: 10px;
      background: #18181b; color: #fff;
      padding: 12px 18px; border-radius: 10px;
      font-size: 13px; font-weight: 500;
      box-shadow: 0 8px 24px rgba(0,0,0,.25);
      z-index: 300;
      animation: toastIn .2s ease, toastOut .3s ease 3.2s forwards;
      svg { color: #4ade80; flex-shrink: 0; }
    }

    @keyframes toastIn {
      from { opacity: 0; transform: translateY(12px); }
      to   { opacity: 1; transform: translateY(0); }
    }

    @keyframes toastOut {
      from { opacity: 1; transform: translateY(0); }
      to   { opacity: 0; transform: translateY(8px); }
    }

    /* ── Modal ── */
    .modal-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,.45);
      z-index: 200; display: flex; align-items: center; justify-content: center;
      backdrop-filter: blur(3px); animation: fadeIn .15s ease;
    }

    @keyframes fadeIn {
      from { opacity: 0; }
      to   { opacity: 1; }
    }

    .modal {
      background: #fff; border-radius: 16px;
      padding: 32px 28px 24px;
      width: 100%; max-width: 380px;
      display: flex; flex-direction: column; align-items: center; gap: 12px;
      box-shadow: 0 20px 60px rgba(0,0,0,.2);
      animation: scaleIn .15s ease;
    }

    @keyframes scaleIn {
      from { transform: scale(.95); opacity: 0; }
      to   { transform: scale(1);   opacity: 1; }
    }

    .modal-icon {
      width: 48px; height: 48px; border-radius: 50%;
      background: #fef2f2; color: #dc2626;
      display: flex; align-items: center; justify-content: center;
    }

    .modal-title { font-size: 17px; font-weight: 700; color: #111; }

    .modal-body {
      font-size: 14px; color: #666; text-align: center; line-height: 1.6;
      strong { color: #111; }
    }

    .modal-actions {
      display: flex; gap: 10px; width: 100%; margin-top: 8px;
      button { flex: 1; justify-content: center; }
    }

    /* ── Overlay + Drawer ── */
    .overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,.3);
      z-index: 100; backdrop-filter: blur(2px);
    }

    .drawer {
      position: fixed; top: 0; right: 0; bottom: 0;
      width: 420px; background: #fff;
      border-left: 1px solid #e8e8ec;
      z-index: 101;
      display: flex; flex-direction: column;
      box-shadow: -8px 0 32px rgba(0,0,0,.1);
      animation: slideIn .2s ease;
    }

    @keyframes slideIn {
      from { transform: translateX(100%); }
      to   { transform: translateX(0); }
    }

    .drawer-header { display: flex; align-items: center; justify-content: space-between; padding: 18px 20px; border-bottom: 1px solid #f0f0f3; flex-shrink: 0; }
    .drawer-header-left { display: flex; align-items: center; gap: 12px; }
    .drawer-icon { width: 36px; height: 36px; border-radius: 10px; background: color-mix(in srgb, var(--color-brand) 10%, transparent); color: var(--color-brand); display: flex; align-items: center; justify-content: center; }
    .drawer-title { font-size: 15px; font-weight: 700; color: #111; }
    .drawer-subtitle { font-size: 11px; color: #aaa; margin-top: 1px; }
    .btn-close { width: 30px; height: 30px; border-radius: 7px; border: 1px solid #e8e8ec; background: #fff; color: #888; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: all .12s; &:hover { background: #f4f4f5; } }
    .drawer-body { flex: 1; overflow-y: auto; padding: 16px 20px; display: flex; flex-direction: column; gap: 12px; }
    .drawer-footer { padding: 14px 20px; border-top: 1px solid #f0f0f3; display: flex; gap: 10px; justify-content: flex-end; flex-shrink: 0; background: #fafafa; }

    /* Form sections */
    .form-section { display: flex; flex-direction: column; gap: 10px; padding: 14px; background: #fafafa; border: 1px solid #f0f0f3; border-radius: 10px; }
    .form-section-header { display: flex; align-items: center; gap: 6px; font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #999; svg { color: var(--color-brand); } }
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
    @keyframes spinAnim { to { transform: rotate(360deg); } }
    .spin-icon { animation: spinAnim .8s linear infinite; transform-origin: center; }

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

  plans        = signal<PlanDto[]>([]);
  loading      = signal(true);
  error        = signal('');
  toggling     = signal('');
  deleting     = signal('');
  deleteTarget  = signal<PlanDto | null>(null);
  toast         = signal<string>('');
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
        next: () => { this.saving.set(false); this.drawerOpen.set(false); this.load(); this.showToast(`Plano "${plan.name}" atualizado com sucesso.`); },
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
      next:  () => { this.deleting.set(''); this.load(); this.showToast(`Plano "${plan.name}" excluído com sucesso.`); },
      error: () => { this.deleting.set(''); this.error.set('Erro ao excluir plano.'); },
    });
  }

  private showToast(msg: string) {
    this.toast.set(msg);
    setTimeout(() => this.toast.set(''), 3500);
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
