import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe, CurrencyPipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { TenantService, TenantDto, EmployeeDto } from '../services/tenant.service';
import { PlansService, PlanDto } from '../../plans/plans.service';
import { SubscriptionService, SubscriptionDto, InvoiceDto } from '../../subscriptions/subscription.service';
import { ToastService } from '../../../../core/toast/toast.service';
import { environment } from '../../../../../environments/environment';

type Tab = 'info' | 'employees' | 'financeiro';

interface BusinessTypeDto { id: string; name: string; }

const STATUS_LABEL: Record<string, string> = {
  Active: 'Ativo', TrialPeriod: 'Trial', Suspended: 'Suspenso', Cancelled: 'Cancelado',
};
const EMPLOYEE_STATUS_LABEL: Record<string, string> = {
  Active: 'Ativo', PendingActivation: 'Pendente', Inactive: 'Inativo',
};

@Component({
  selector: 'app-tenant-detail',
  standalone: true,
  imports: [DatePipe, CurrencyPipe, DecimalPipe, FormsModule],
  template: `
    <div class="page">

      <!-- Breadcrumb -->
      <div class="breadcrumb">
        <button class="back-btn" (click)="goBack()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>
          Estabelecimentos
        </button>
        @if (tenant()) { <span class="sep">/</span> <span class="bc-current">{{ tenant()!.name }}</span> }
      </div>

      @if (loading()) {
        <div class="loading-state"><div class="spin"></div></div>
      } @else if (!tenant()) {
        <div class="empty-state">Estabelecimento não encontrado.</div>
      } @else {

        <!-- Header -->
        <div class="tenant-header">
          <div class="tenant-meta">
            <div class="tenant-avatar">{{ tenant()!.name.charAt(0).toUpperCase() }}</div>
            <div>
              <div class="tenant-name-row">
                <h1 class="tenant-name">{{ tenant()!.name }}</h1>
                <span class="badge badge-{{ tenant()!.status }}">{{ statusLabel(tenant()!.status) }}</span>
              </div>
              <div class="tenant-slug">{{ tenant()!.slug }}</div>
            </div>
          </div>
          <div class="header-right">
            @if (tenant()!.status === 'TrialPeriod') {
              <button class="btn btn-success" (click)="doAction('activate')" [disabled]="acting()">{{ acting() === 'activate' ? '...' : 'Ativar' }}</button>
            }
            @if (tenant()!.status === 'Active' || tenant()!.status === 'TrialPeriod') {
              <button class="btn btn-warn" (click)="doAction('suspend')" [disabled]="acting()">{{ acting() === 'suspend' ? '...' : 'Suspender' }}</button>
            }
            @if (tenant()!.status === 'Suspended') {
              <button class="btn btn-success" (click)="doAction('reactivate')" [disabled]="acting()">{{ acting() === 'reactivate' ? '...' : 'Reativar' }}</button>
            }
            @if (tenant()!.status !== 'Cancelled') {
              <button class="btn btn-danger-outline" (click)="cancelModal.set(true)" [disabled]="acting()">Cancelar conta</button>
            }
          </div>
        </div>

        <!-- Tabs -->
        <div class="tabs">
          <button class="tab" [class.active]="activeTab() === 'info'" (click)="activeTab.set('info')">Informações</button>
          <button class="tab" [class.active]="activeTab() === 'financeiro'" (click)="loadFinanceiroTab()">
            Financeiro
            @if (subscription()?.status === 'Inadimplente') {
              <span class="tab-badge tab-badge-danger">!</span>
            }
            @if (subscription()?.status === 'AguardandoPagamento') {
              <span class="tab-badge tab-badge-warn">·</span>
            }
          </button>
          <button class="tab" [class.active]="activeTab() === 'employees'" (click)="loadEmployeesTab()">Funcionários</button>
        </div>

        <!-- Tab Informações -->
        @if (activeTab() === 'info') {
          <div class="info-layout">

            <!-- Card: Dados Gerais -->
            <div class="card">
              <div class="card-head">
                <h3 class="card-title">Dados Gerais</h3>
                @if (!editing()) {
                  <button class="btn-edit" (click)="startEdit()">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                    Editar
                  </button>
                }
              </div>

              @if (editing()) {
                <!-- Edit form -->
                <div class="edit-form">
                  <div class="form-section-title">Dados Gerais</div>
                  <div class="field-row">
                    <div class="field">
                      <label class="field-label">Nome</label>
                      <input class="field-input" [(ngModel)]="editForm.name" placeholder="Nome do estabelecimento">
                    </div>
                    <div class="field">
                      <label class="field-label">E-mail (opcional)</label>
                      <input class="field-input" [(ngModel)]="editForm.email" type="email" placeholder="email@exemplo.com">
                    </div>
                  </div>
                  <div class="field-row">
                    <div class="field">
                      <label class="field-label">Telefone</label>
                      <input class="field-input" [(ngModel)]="editForm.phone" type="tel" placeholder="(11) 99999-9999">
                    </div>
                    <div class="field">
                      <label class="field-label">Fuso Horário</label>
                      <select class="field-input" [(ngModel)]="editForm.timezone">
                        <option value="America/Sao_Paulo">America/Sao_Paulo</option>
                        <option value="America/Manaus">America/Manaus</option>
                        <option value="America/Belem">America/Belem</option>
                        <option value="America/Fortaleza">America/Fortaleza</option>
                        <option value="America/Recife">America/Recife</option>
                        <option value="America/Cuiaba">America/Cuiaba</option>
                        <option value="America/Porto_Velho">America/Porto_Velho</option>
                        <option value="America/Boa_Vista">America/Boa_Vista</option>
                        <option value="America/Rio_Branco">America/Rio_Branco</option>
                        <option value="America/Noronha">America/Noronha</option>
                      </select>
                    </div>
                  </div>

                  <div class="form-section-title" style="margin-top: 8px;">Endereço</div>
                  <div class="field-row">
                    <div class="field field-cep">
                      <label class="field-label">CEP</label>
                      <div class="cep-row">
                        <input class="field-input" [(ngModel)]="editForm.cep" placeholder="00000-000" maxlength="9" (blur)="lookupCep()">
                        @if (cepLoading()) { <div class="cep-spin"></div> }
                      </div>
                    </div>
                    <div class="field">
                      <label class="field-label">Logradouro</label>
                      <input class="field-input" [(ngModel)]="editForm.logradouro" placeholder="Rua, Av...">
                    </div>
                  </div>
                  <div class="field-row">
                    <div class="field field-num">
                      <label class="field-label">Número</label>
                      <input class="field-input" [(ngModel)]="editForm.numero" placeholder="Nº">
                    </div>
                    <div class="field">
                      <label class="field-label">Complemento</label>
                      <input class="field-input" [(ngModel)]="editForm.complemento" placeholder="Apto, sala...">
                    </div>
                    <div class="field">
                      <label class="field-label">Bairro</label>
                      <input class="field-input" [(ngModel)]="editForm.bairro" placeholder="Bairro">
                    </div>
                  </div>
                  <div class="field-row">
                    <div class="field">
                      <label class="field-label">Cidade</label>
                      <input class="field-input" [(ngModel)]="editForm.cidade" placeholder="Cidade">
                    </div>
                    <div class="field field-uf">
                      <label class="field-label">UF</label>
                      <input class="field-input" [(ngModel)]="editForm.uf" placeholder="SP" maxlength="2" style="text-transform:uppercase">
                    </div>
                  </div>

                  <div class="edit-actions">
                    <button class="btn btn-primary" (click)="saveEdit()" [disabled]="saving()">{{ saving() ? 'Salvando...' : 'Salvar' }}</button>
                    <button class="btn btn-ghost" (click)="editing.set(false)">Cancelar</button>
                  </div>
                </div>
              } @else {
                <dl class="dl">
                  <dt>Nome</dt>         <dd>{{ tenant()!.name }}</dd>
                  <dt>Slug</dt>         <dd class="mono">{{ tenant()!.slug }}</dd>
                  <dt>E-mail</dt>       <dd>{{ tenant()!.email || '—' }}</dd>
                  <dt>Telefone</dt>     <dd>{{ tenant()!.phone || '—' }}</dd>
                  <dt>Tipo de Negócio</dt>
                  <dd class="dd-inline">
                    @if (changingBusinessType()) {
                      <select class="field-select-sm" [(ngModel)]="selectedBusinessTypeId">
                        @for (bt of businessTypes(); track bt.id) {
                          <option [value]="bt.id">{{ bt.name }}</option>
                        }
                      </select>
                      <button class="btn-xs btn-xs--primary" (click)="saveBusinessType()" [disabled]="acting() === 'bt'">{{ acting() === 'bt' ? '...' : 'Salvar' }}</button>
                      <button class="btn-xs" (click)="changingBusinessType.set(false)">Cancelar</button>
                    } @else {
                      <span>{{ businessTypeName() }}</span>
                      <button class="btn-xs btn-xs--ghost" (click)="openChangeBusinessType()">Alterar</button>
                    }
                  </dd>
                  <dt>Fuso Horário</dt> <dd>{{ tenant()!.timezone }}</dd>
                  <dt>Criado em</dt>    <dd>{{ tenant()!.createdAt | date:'dd/MM/yyyy' }}</dd>
                </dl>
                @if (tenant()!.address; as addr) {
                  <div class="address-block">
                    <div class="address-title">
                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
                      Endereço
                    </div>
                    <div class="address-text">
                      {{ addr.logradouro }}, {{ addr.numero }}
                      @if (addr.complemento) { — {{ addr.complemento }} }
                    </div>
                    <div class="address-text muted">{{ addr.bairro }} · {{ addr.cidade }}/{{ addr.uf }} · CEP {{ addr.cep }}</div>
                  </div>
                }
              }
            </div>

            <!-- Card: Plano -->
            <div class="card plan-card">
              <div class="plan-label-row">
                <span class="plan-label">PLANO</span>
                <span class="plan-name">{{ currentPlan()?.name ?? '—' }}</span>
              </div>

              <div class="plan-price">
                @if (currentPlan(); as plan) {
                  {{ plan.monthlyPrice | currency:'BRL':'symbol':'1.2-2':'pt-BR' }}
                  <span class="plan-per">/mês</span>
                } @else {
                  <span class="plan-empty">Sem plano</span>
                }
              </div>

              @if (tenant()!.status === 'TrialPeriod' && tenant()!.trialEndsAt) {
                <div class="plan-trial">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
                  Trial até {{ tenant()!.trialEndsAt | date:'dd/MM/yyyy' }}
                  <span class="days-left">({{ daysLeft(tenant()!.trialEndsAt!) }} dias)</span>
                </div>
              }

              <div class="plan-footer">
                @if (changingPlan()) {
                  <div class="plan-change-row">
                    <select class="field-select" [(ngModel)]="selectedPlanId">
                      @for (p of activePlans(); track p.id) {
                        <option [value]="p.id">{{ p.name }} — R$ {{ p.monthlyPrice | number:'1.2-2':'pt-BR' }}</option>
                      }
                    </select>
                    <button class="btn-inline btn-inline--primary" (click)="savePlan()" [disabled]="acting() === 'plan'">
                      {{ acting() === 'plan' ? '...' : 'Salvar' }}
                    </button>
                    <button class="btn-inline" (click)="changingPlan.set(false)">Cancelar</button>
                  </div>
                } @else {
                  <button class="plan-alter-btn" (click)="openChangePlan()">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                    Alterar plano
                  </button>
                }
              </div>
            </div>

            <!-- Card: Limites do plano -->
            @if (currentPlan(); as plan) {
              <div class="card limits-card">
                <h3 class="card-title">Limites do Plano</h3>
                <div class="limits-grid">
                  <div class="limit-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="18" height="18" rx="2"/><path d="M3 9h18M3 15h18M9 3v18"/></svg>
                    <div>
                      <div class="limit-val">{{ plan.maxTables }}</div>
                      <div class="limit-lbl">Mesas</div>
                    </div>
                  </div>
                  <div class="limit-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2L2 7l10 5 10-5-10-5z"/><path d="M2 17l10 5 10-5"/><path d="M2 12l10 5 10-5"/></svg>
                    <div>
                      <div class="limit-val">{{ plan.maxMenuItems }}</div>
                      <div class="limit-lbl">Itens no cardápio</div>
                    </div>
                  </div>
                  <div class="limit-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                    <div>
                      <div class="limit-val">{{ plan.maxUsers }}</div>
                      <div class="limit-lbl">Usuários</div>
                    </div>
                  </div>
                  <div class="limit-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="3"/><path d="M19.07 4.93l-1.41 1.41M5.34 18.66l-1.41 1.41M19.07 19.07l-1.41-1.41M5.34 5.34L3.93 3.93M21 12h-2M5 12H3M12 21v-2M12 5V3"/></svg>
                    <div>
                      <div class="limit-val">{{ plan.maxCustomRoles }}</div>
                      <div class="limit-lbl">Funções personalizadas</div>
                    </div>
                  </div>
                </div>
              </div>
            }

          </div>
        }

        <!-- Tab Financeiro -->
        @if (activeTab() === 'financeiro') {
          <div class="fin-section">
            @if (loadingSubscription()) {
              <div class="loading-state"><div class="spin"></div></div>
            } @else if (!subscription()) {
              <div class="fin-empty">
                <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
                <p>Nenhuma assinatura encontrada para este estabelecimento.</p>
              </div>
            } @else {
              <!-- Status + resumo -->
              <div class="fin-summary-grid">
                <div class="fin-summary-card">
                  <div class="fin-summary-label">Status</div>
                  <div class="fin-summary-value">
                    <span [class]="'sub-badge sub-badge-' + subscription()!.status">{{ subStatusLabel(subscription()!.status) }}</span>
                  </div>
                </div>
                <div class="fin-summary-card">
                  <div class="fin-summary-label">Próximo vencimento</div>
                  <div class="fin-summary-value">{{ formatDate(subscription()!.nextDueDate) }}</div>
                </div>
                <div class="fin-summary-card">
                  <div class="fin-summary-label">Fatura em aberto</div>
                  <div class="fin-summary-value">
                    @if (openInvoice(); as inv) {
                      <span [class]="inv.status === 'Overdue' ? 'fin-val-danger' : 'fin-val-warn'">R$ {{ inv.amount.toFixed(2) }}</span>
                      <span class="fin-val-muted">venc. {{ formatDate(inv.dueDate) }}</span>
                    } @else {
                      <span class="fin-val-muted">—</span>
                    }
                  </div>
                </div>
                <div class="fin-summary-card">
                  <div class="fin-summary-label">Último pagamento</div>
                  <div class="fin-summary-value">
                    @if (lastPaid(); as inv) {
                      <span class="fin-val-ok">R$ {{ inv.amount.toFixed(2) }}</span>
                      @if (inv.paidAt) { <span class="fin-val-muted">em {{ formatDate(inv.paidAt) }}</span> }
                    } @else {
                      <span class="fin-val-muted">—</span>
                    }
                  </div>
                </div>
              </div>

              <!-- Ações -->
              <div class="fin-actions">
                @if (!openInvoice()) {
                  <button class="btn btn-primary btn-sm-act" (click)="openInvoiceDrawer()">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
                    Gerar Fatura
                  </button>
                }
                @if (openInvoice()) {
                  <button class="btn btn-success btn-sm-act" (click)="openPaymentDrawer()">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="20 6 9 17 4 12"/></svg>
                    Confirmar Pagamento
                  </button>
                }
              </div>

              <!-- Histórico de faturas -->
              <div class="fin-history">
                <h4 class="fin-history-title">Histórico de Faturas</h4>
                @if (subscription()!.invoices.length === 0) {
                  <div class="fin-no-invoices">Nenhuma fatura gerada ainda.</div>
                } @else {
                  <div class="fin-table-wrap">
                    <table class="fin-table">
                      <thead><tr>
                        <th>Vencimento</th>
                        <th>Valor</th>
                        <th>Status</th>
                        <th>Pago em</th>
                        <th>Referência</th>
                      </tr></thead>
                      <tbody>
                        @for (inv of subscription()!.invoices; track inv.id) {
                          <tr [class.row-overdue]="inv.status === 'Overdue'" [class.row-paid]="inv.status === 'Paid'">
                            <td>{{ formatDate(inv.dueDate) }}</td>
                            <td class="td-amount">R$ {{ inv.amount.toFixed(2) }}</td>
                            <td><span [class]="'inv-badge inv-badge-' + inv.status">{{ invStatusLabel(inv.status) }}</span></td>
                            <td class="td-muted">{{ inv.paidAt ? formatDate(inv.paidAt) : '—' }}</td>
                            <td class="td-muted td-ref">{{ inv.paymentReference || '—' }}</td>
                          </tr>
                        }
                      </tbody>
                    </table>
                  </div>
                }
              </div>
            }
          </div>
        }

        <!-- Tab Funcionários -->
        @if (activeTab() === 'employees') {
          <div class="employees-section">
            @if (loadingEmployees()) {
              <div class="loading-state"><div class="spin"></div></div>
            } @else {
              <div class="table-wrap">
                <table class="table">
                  <thead><tr>
                    <th>Nome</th>
                    <th>E-mail</th>
                    <th>Status</th>
                    <th>Tipo</th>
                    <th>Último acesso</th>
                  </tr></thead>
                  <tbody>
                    @if (employees().length === 0) {
                      <tr><td colspan="5" class="empty-cell">Nenhum funcionário cadastrado.</td></tr>
                    } @else {
                      @for (e of employees(); track e.id) {
                        <tr>
                          <td class="td-name">{{ e.name }}</td>
                          <td class="td-email">{{ e.email }}</td>
                          <td><span class="badge-emp badge-emp-{{ e.status }}">{{ employeeStatusLabel(e.status) }}</span></td>
                          <td>Funcionário</td>
                          <td class="td-muted">{{ e.lastLoginAt ? (e.lastLoginAt | date:'dd/MM/yyyy HH:mm') : 'Nunca' }}</td>
                        </tr>
                      }
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        }
      }
    </div>

    <!-- Drawer: Gerar Fatura -->
    @if (finDrawer() === 'invoice' && subscription()) {
      <div class="drawer-overlay" (click)="finDrawer.set(null)">
        <div class="drawer" (click)="$event.stopPropagation()">
          <div class="drawer-header">
            <h2 class="drawer-title">Gerar Fatura</h2>
            <button class="drawer-close" (click)="finDrawer.set(null)">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="drawer-body">
            <div class="drawer-info-card">
              <div class="dic-name">{{ tenant()!.name }}</div>
              <div class="dic-sub">{{ subscription()!.planName }} · próx. venc. {{ formatDate(subscription()!.nextDueDate) }}</div>
            </div>
            <div class="form-group">
              <label class="form-label">Valor (R$)</label>
              <div class="input-group">
                <span class="input-addon">R$</span>
                <input class="form-control input-group-field" type="text" inputmode="decimal"
                  [value]="finAmountStr" (input)="finAmountStr = $any($event.target).value" placeholder="0,00" />
              </div>
            </div>
            <div class="form-group">
              <label class="form-label">Data de vencimento</label>
              <input class="form-control" type="date" [(ngModel)]="finDueDate" />
            </div>
          </div>
          <div class="drawer-footer">
            <button class="btn btn-ghost" (click)="finDrawer.set(null)">Cancelar</button>
            <button class="btn btn-primary" (click)="finGenerateInvoice()" [disabled]="finActing()">
              {{ finActing() ? 'Aguarde...' : 'Gerar Fatura' }}
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Drawer: Confirmar Pagamento -->
    @if (finDrawer() === 'payment' && subscription()) {
      <div class="drawer-overlay" (click)="finDrawer.set(null)">
        <div class="drawer" (click)="$event.stopPropagation()">
          <div class="drawer-header">
            <h2 class="drawer-title">Confirmar Pagamento</h2>
            <button class="drawer-close" (click)="finDrawer.set(null)">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            </button>
          </div>
          <div class="drawer-body">
            <div class="drawer-info-card">
              <div class="dic-name">{{ tenant()!.name }}</div>
              <div class="dic-sub">Fatura: R$ {{ openInvoice()?.amount?.toFixed(2) }} · venc. {{ formatDate(openInvoice()?.dueDate ?? '') }}</div>
            </div>
            @if (subscription()!.overdueCount > 1) {
              <div class="alert-warn-sm">
                {{ subscription()!.overdueCount }} faturas em atraso. O pagamento será aplicado à mais antiga.
              </div>
            }
            <div class="form-group">
              <label class="form-label">Data do pagamento</label>
              <input class="form-control" type="date" [(ngModel)]="finPaidAt" />
            </div>
            <div class="form-group">
              <label class="form-label">Próximo vencimento</label>
              <input class="form-control" type="date" [(ngModel)]="finNextDueDate" />
            </div>
            <div class="form-group">
              <label class="form-label">Referência de pagamento</label>
              <input class="form-control" type="text" [(ngModel)]="finPaymentRef" placeholder="Número do boleto, código PIX..." />
            </div>
            <div class="form-group">
              <label class="form-label">Observações</label>
              <textarea class="form-control" rows="3" [(ngModel)]="finNotes"></textarea>
            </div>
          </div>
          <div class="drawer-footer">
            <button class="btn btn-ghost" (click)="finDrawer.set(null)">Cancelar</button>
            <button class="btn btn-primary" (click)="finConfirmPayment()" [disabled]="finActing()">
              {{ finActing() ? 'Aguarde...' : 'Confirmar Pagamento' }}
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Modal cancelamento -->
    @if (cancelModal()) {
      <div class="modal-overlay" (click)="cancelModal.set(false)">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-icon warn">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
          </div>
          <h3 class="modal-title">Cancelar conta</h3>
          <p class="modal-body">Tem certeza que quer cancelar a conta de <strong>{{ tenant()?.name }}</strong>?<br>Esta ação é irreversível.</p>
          <div class="modal-actions">
            <button class="btn btn-ghost" (click)="cancelModal.set(false)">Voltar</button>
            <button class="btn btn-danger" (click)="doAction('cancel')">Confirmar cancelamento</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; max-width: 1100px; }

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
    .tenant-header {
      display: flex; align-items: flex-start; justify-content: space-between;
      gap: 16px; margin-bottom: 20px; flex-wrap: wrap;
    }
    .tenant-meta { display: flex; align-items: center; gap: 14px; }
    .tenant-avatar {
      width: 52px; height: 52px; border-radius: 14px;
      background: var(--color-brand); color: #fff;
      display: flex; align-items: center; justify-content: center;
      font-size: 22px; font-weight: 800; flex-shrink: 0;
    }
    .tenant-name-row { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
    .tenant-name { font-size: 22px; font-weight: 700; color: #111; }
    .tenant-slug { font-size: 13px; color: #aaa; font-family: monospace; margin-top: 4px; }
    .header-right { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }

    /* Tabs */
    .tabs { display: flex; border-bottom: 2px solid #f0f0f3; margin-bottom: 20px; }
    .tab {
      padding: 10px 18px; border: none; background: none;
      font-size: 13px; font-weight: 600; color: #aaa; cursor: pointer;
      border-bottom: 2px solid transparent; margin-bottom: -2px; transition: all .15s;
      &:hover { color: #555; }
      &.active { color: var(--color-brand); border-bottom-color: var(--color-brand); }
    }

    /* Info layout: 3 col grid */
    .info-layout {
      display: grid;
      grid-template-columns: 1fr 1fr;
      grid-template-rows: auto auto;
      gap: 16px;
    }
    .limits-card { grid-column: 1 / -1; }

    /* Cards */
    .card {
      background: #fff; border: 1px solid #e8e8ec;
      border-radius: 14px; padding: 20px;
    }
    .card-head {
      display: flex; align-items: center; justify-content: space-between;
      margin-bottom: 16px;
    }
    .card-title { font-size: 13px; font-weight: 700; color: #555; }
    .btn-edit {
      display: inline-flex; align-items: center; gap: 5px;
      padding: 5px 12px; border-radius: 7px; font-size: 12px; font-weight: 600;
      border: 1px solid #e8e8ec; background: #f9f9fb; color: #555; cursor: pointer;
      transition: all .15s;
      &:hover { background: #f0f0f3; color: #111; }
    }

    /* DL */
    .dl {
      display: grid; grid-template-columns: auto 1fr; gap: 10px 20px; align-items: baseline;
      dt { font-size: 12px; color: #aaa; font-weight: 600; white-space: nowrap; }
      dd { font-size: 13px; color: #333; }
    }
    .mono { font-family: monospace; font-size: 12px; }

    .dd-inline { display: flex; align-items: center; gap: 6px; flex-wrap: wrap; }

    .field-select-sm {
      padding: 4px 8px; border: 1px solid #e8e8ec; border-radius: 6px;
      font-size: 12px; color: #111; background: #fff; outline: none; cursor: pointer;
      &:focus { border-color: var(--color-brand); }
    }

    .btn-xs {
      padding: 3px 10px; border-radius: 5px; font-size: 11px; font-weight: 600;
      cursor: pointer; border: 1px solid #e8e8ec; background: #f4f4f5; color: #555;
      white-space: nowrap; transition: all .15s; line-height: 1.6;
      &:hover { background: #ebebef; }
      &:disabled { opacity: .5; cursor: not-allowed; }
      &--primary { background: var(--color-brand); color: #fff; border-color: transparent; &:hover { opacity: .9; } }
      &--ghost { background: transparent; border-color: transparent; color: #aaa; padding: 3px 6px; &:hover { color: var(--color-brand); background: rgba(224,49,49,.06); } }
    }

    /* Edit form */
    .edit-form { display: flex; flex-direction: column; gap: 12px; }
    .form-section-title { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: .06em; color: #aaa; padding-bottom: 4px; border-bottom: 1px solid #f0f0f3; }
    .field-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .field { display: flex; flex-direction: column; gap: 5px; }
    .field-cep { max-width: 160px; }
    .field-num  { max-width: 100px; }
    .field-uf   { max-width: 80px; }
    .field-label { font-size: 12px; font-weight: 600; color: #555; }
    .field-input {
      padding: 9px 12px; border: 1px solid #e8e8ec; border-radius: 8px;
      font-size: 13px; color: #111; outline: none; transition: border-color .15s;
      background: #fff; width: 100%; box-sizing: border-box;
      &:focus { border-color: var(--color-brand); }
    }
    .cep-row { position: relative; display: flex; align-items: center; }
    .cep-spin {
      position: absolute; right: 10px;
      width: 14px; height: 14px; border-radius: 50%;
      border: 2px solid #e8e8ec; border-top-color: var(--color-brand);
      animation: spin .7s linear infinite;
    }
    .edit-actions { display: flex; gap: 8px; padding-top: 4px; }

    /* Address display */
    .address-block {
      margin-top: 14px; padding: 12px 14px;
      background: #fafafa; border: 1px solid #f0f0f3; border-radius: 10px;
    }
    .address-title {
      display: flex; align-items: center; gap: 5px;
      font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: .06em;
      color: #aaa; margin-bottom: 6px;
    }
    .address-text { font-size: 13px; color: #333; line-height: 1.5; }
    .address-text.muted { color: #888; font-size: 12px; }

    /* Plan card */
    .plan-card { display: flex; flex-direction: column; gap: 14px; }
    .plan-label-row { display: flex; align-items: center; justify-content: space-between; }
    .plan-label { font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #aaa; }
    .plan-name  { font-size: 16px; font-weight: 800; color: #111; }
    .plan-price {
      font-size: 30px; font-weight: 800; color: #111;
      display: flex; align-items: baseline; gap: 4px;
    }
    .plan-per   { font-size: 13px; font-weight: 400; color: #aaa; }
    .plan-empty { font-size: 14px; font-weight: 400; color: #ccc; }
    .plan-trial {
      display: flex; align-items: center; gap: 6px;
      font-size: 12px; color: #1d4ed8; font-weight: 500;
      background: #dbeafe; border-radius: 8px; padding: 8px 10px;
    }
    .days-left { font-size: 11px; color: #1d4ed8; margin-left: 4px; }
    .plan-footer { border-top: 1px solid #f0f0f3; padding-top: 14px; margin-top: auto; }
    .plan-alter-btn {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 7px 14px; border-radius: 8px; font-size: 12px; font-weight: 600;
      border: 1px solid #e8e8ec; background: #f9f9fb; color: #555; cursor: pointer;
      transition: all .15s;
      &:hover { background: #f0f0f3; color: #111; }
    }
    .plan-change-row { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
    .field-select {
      flex: 1; padding: 7px 10px; border: 1px solid #e8e8ec; border-radius: 7px;
      font-size: 12px; color: #111; background: #fff; outline: none; cursor: pointer;
      &:focus { border-color: var(--color-brand); }
    }
    .btn-inline {
      padding: 6px 12px; border-radius: 6px; font-size: 12px; font-weight: 600;
      cursor: pointer; border: 1px solid #e8e8ec; background: #f4f4f5; color: #555;
      white-space: nowrap; transition: all .15s;
      &:hover { background: #ebebef; }
      &:disabled { opacity: .5; cursor: not-allowed; }
      &--primary { background: var(--color-brand); color: #fff; border-color: transparent; &:hover { opacity: .9; } }
    }

    /* Limits card */
    .limits-grid {
      display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-top: 4px;
    }
    .limit-item {
      display: flex; align-items: flex-start; gap: 10px;
      padding: 14px; background: #fafafa; border-radius: 10px; border: 1px solid #f0f0f3;
      svg { color: var(--color-brand); flex-shrink: 0; margin-top: 2px; }
    }
    .limit-val { font-size: 20px; font-weight: 800; color: #111; }
    .limit-lbl { font-size: 11px; color: #aaa; margin-top: 2px; }

    /* Employees */
    .table-wrap { background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; overflow: hidden; }
    .table {
      width: 100%; border-collapse: collapse;
      th {
        padding: 11px 16px; text-align: left;
        font-size: 11px; font-weight: 700; text-transform: uppercase;
        letter-spacing: .05em; color: #aaa;
        border-bottom: 1px solid #f0f0f3; background: #fafafa;
      }
      td { padding: 13px 16px; border-bottom: 1px solid #f4f4f6; font-size: 13px; }
      tr:last-child td { border-bottom: none; }
    }
    .td-name  { font-weight: 600; color: #111; }
    .td-email { color: #555; }
    .td-muted { color: #999; font-size: 12px; }
    .empty-cell { text-align: center; padding: 40px; color: #ccc; }

    /* Badges */
    .badge {
      display: inline-block; padding: 4px 12px; border-radius: 99px;
      font-size: 11px; font-weight: 700; white-space: nowrap;
      &-Active      { background: #dcfce7; color: #15803d; }
      &-TrialPeriod { background: #dbeafe; color: #1d4ed8; }
      &-Suspended   { background: #fef3c7; color: #b45309; }
      &-Cancelled   { background: #f4f4f5; color: #71717a; }
    }
    .badge-emp {
      display: inline-block; padding: 3px 10px; border-radius: 99px; font-size: 11px; font-weight: 700;
      &-Active            { background: #dcfce7; color: #15803d; }
      &-PendingActivation { background: #fef9c3; color: #854d0e; }
      &-Inactive          { background: #f4f4f5; color: #71717a; }
    }
    .owner-badge {
      display: inline-block; padding: 2px 8px; border-radius: 99px;
      font-size: 11px; font-weight: 700; background: #fce7f3; color: #9d174d;
    }

    /* Buttons */
    .btn {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 7px 16px; border-radius: 8px;
      font-size: 12px; font-weight: 600; cursor: pointer;
      border: 1px solid transparent; transition: all .15s;
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
    .btn-primary       { background: var(--color-brand); color: #fff; &:hover { opacity: .9; } }
    .btn-warn          { background: #fffbeb; color: #b45309; border-color: #fde68a; &:hover { background: #fef3c7; } }
    .btn-success       { background: #f0fdf4; color: #15803d; border-color: #bbf7d0; &:hover { background: #dcfce7; } }
    .btn-danger-outline{ background: #fef2f2; color: #b91c1c; border-color: #fecaca; &:hover { background: #fee2e2; } }
    .btn-danger        { background: #dc2626; color: #fff; &:hover { background: #b91c1c; } }
    .btn-ghost         { background: #f4f4f5; color: #555; border-color: #e8e8ec; &:hover { background: #ebebef; } }

    /* Loading */
    .loading-state { display: flex; justify-content: center; padding: 60px; }
    .spin {
      width: 32px; height: 32px; border-radius: 50%;
      border: 3px solid #f0f0f3; border-top-color: var(--color-brand);
      animation: spin .7s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }

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
    .modal-icon { width: 48px; height: 48px; border-radius: 50%; display: flex; align-items: center; justify-content: center; &.warn { background: #fef3c7; color: #b45309; } }
    .modal-title { font-size: 17px; font-weight: 700; color: #111; }
    .modal-body  { font-size: 14px; color: #666; text-align: center; line-height: 1.6; strong { color: #111; } }
    .modal-actions { display: flex; gap: 10px; width: 100%; margin-top: 8px; button { flex: 1; justify-content: center; } }

    .empty-state { text-align: center; padding: 60px; color: #ccc; }

    /* Tab badges */
    .tab-badge {
      display: inline-flex; align-items: center; justify-content: center;
      width: 16px; height: 16px; border-radius: 99px; font-size: 10px; font-weight: 800;
      margin-left: 5px; line-height: 1;
    }
    .tab-badge-danger { background: #fee2e2; color: #b91c1c; }
    .tab-badge-warn   { background: #fef3c7; color: #b45309; font-size: 16px; height: 14px; }

    /* Financeiro section */
    .fin-section { display: flex; flex-direction: column; gap: 20px; }
    .fin-empty {
      display: flex; flex-direction: column; align-items: center; gap: 12px;
      padding: 48px; color: #aaa; text-align: center;
      svg { color: #ccc; }
      p { font-size: 14px; margin: 0; }
    }

    .fin-summary-grid {
      display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px;
    }
    .fin-summary-card {
      background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; padding: 16px;
      display: flex; flex-direction: column; gap: 6px;
    }
    .fin-summary-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: .06em; color: #aaa; }
    .fin-summary-value { display: flex; flex-direction: column; gap: 2px; font-size: 14px; font-weight: 600; color: #111; }
    .fin-val-ok      { color: #15803d; }
    .fin-val-warn    { color: #b45309; }
    .fin-val-danger  { color: #b91c1c; }
    .fin-val-muted   { font-size: 11px; color: #aaa; font-weight: 400; }

    .fin-actions { display: flex; gap: 10px; }
    .btn-sm-act { font-size: 13px; padding: 8px 16px; }

    .fin-history { background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; overflow: hidden; }
    .fin-history-title {
      font-size: 13px; font-weight: 700; color: #555;
      padding: 16px 20px; border-bottom: 1px solid #f0f0f3; margin: 0;
    }
    .fin-no-invoices { padding: 32px; text-align: center; color: #ccc; font-size: 13px; }
    .fin-table-wrap { overflow-x: auto; }
    .fin-table {
      width: 100%; border-collapse: collapse; font-size: 13px;
      th {
        padding: 10px 16px; text-align: left; font-size: 11px; font-weight: 700;
        text-transform: uppercase; letter-spacing: .05em; color: #aaa;
        background: #fafafa; border-bottom: 1px solid #f0f0f3;
      }
      td { padding: 12px 16px; border-bottom: 1px solid #f4f4f6; color: #333; vertical-align: middle; }
      tr:last-child td { border-bottom: none; }
    }
    .td-amount { font-weight: 600; }
    .td-muted { color: #999; font-size: 12px; }
    .td-ref { max-width: 160px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .row-overdue td { background: rgba(220,38,38,.03); }
    .row-paid td { background: rgba(21,128,61,.02); }

    /* Status badges */
    .sub-badge {
      display: inline-block; padding: 4px 12px; border-radius: 99px;
      font-size: 12px; font-weight: 700; white-space: nowrap;
    }
    .sub-badge-EmDia              { background: #dcfce7; color: #15803d; }
    .sub-badge-AguardandoPagamento{ background: #fef3c7; color: #b45309; }
    .sub-badge-Inadimplente       { background: #fee2e2; color: #b91c1c; }
    .sub-badge-SemFaturas         { background: #f4f4f5; color: #71717a; }

    .inv-badge {
      display: inline-block; padding: 2px 9px; border-radius: 99px;
      font-size: 11px; font-weight: 700;
    }
    .inv-badge-Paid    { background: #dcfce7; color: #15803d; }
    .inv-badge-Pending { background: #fef3c7; color: #b45309; }
    .inv-badge-Overdue { background: #fee2e2; color: #b91c1c; }

    /* Drawer */
    .drawer-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,.4); z-index: 200;
      display: flex; justify-content: flex-end; animation: fadeInDr .15s ease;
    }
    @keyframes fadeInDr { from{opacity:0} to{opacity:1} }
    .drawer {
      width: 400px; max-width: 95vw; background: #fff;
      border-left: 1px solid #e8e8ec;
      display: flex; flex-direction: column; height: 100vh; animation: slideInDr .2s ease;
    }
    @keyframes slideInDr { from{transform:translateX(40px);opacity:0} to{transform:translateX(0);opacity:1} }
    .drawer-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 20px 24px; border-bottom: 1px solid #e8e8ec;
    }
    .drawer-title { font-size: 16px; font-weight: 700; color: #111; margin: 0; }
    .drawer-close {
      width: 28px; height: 28px; border-radius: 6px; border: none;
      background: transparent; color: #aaa; cursor: pointer;
      display: flex; align-items: center; justify-content: center;
      &:hover { background: #f0f0f3; }
    }
    .drawer-body { flex: 1; overflow-y: auto; padding: 24px; display: flex; flex-direction: column; gap: 16px; }
    .drawer-footer {
      padding: 16px 24px; border-top: 1px solid #e8e8ec;
      display: flex; justify-content: flex-end; gap: 8px;
    }
    .drawer-info-card {
      background: #f9f9fb; border: 1px solid #e8e8ec; border-radius: 10px; padding: 14px 16px;
    }
    .dic-name { font-size: 14px; font-weight: 600; color: #111; }
    .dic-sub  { font-size: 12px; color: #aaa; margin-top: 3px; }

    .alert-warn-sm {
      background: #fef3c7; border: 1px solid #fde68a; border-radius: 8px;
      padding: 10px 14px; font-size: 13px; color: #b45309;
    }

    .input-group {
      display: flex; align-items: center;
      border: 1px solid #e8e8ec; border-radius: 8px;
      overflow: hidden; transition: border-color .15s;
      &:focus-within { border-color: var(--color-brand); }
    }
    .input-addon {
      padding: 0 12px; font-size: 13px; font-weight: 600; color: #aaa;
      border-right: 1px solid #e8e8ec; background: #f9f9fb;
      align-self: stretch; display: flex; align-items: center;
    }
    .input-group-field {
      flex: 1; border: none !important; outline: none;
      background: transparent !important; padding: 10px 12px;
      font-size: 14px; color: #111; border-radius: 0 !important;
    }

    .form-group { display: flex; flex-direction: column; gap: 6px; }
    .form-label { font-size: 12px; font-weight: 600; color: #555; text-transform: uppercase; letter-spacing: .5px; }
    .form-control {
      padding: 9px 12px; border: 1px solid #e8e8ec; border-radius: 8px;
      background: #f9f9fb; color: #111; font-size: 13.5px; outline: none;
      &:focus { border-color: var(--color-brand); }
    }
    textarea.form-control { resize: vertical; font-family: inherit; }

    @media (max-width: 768px) {
      .page { padding: 14px; max-width: 100%; }
      .info-layout { grid-template-columns: 1fr; }
      .limits-card { grid-column: auto; }
      .limits-grid { grid-template-columns: repeat(2, 1fr); }
      .tenant-header { flex-direction: column; }
      .header-right { width: 100%; }
      .modal { width: calc(100% - 32px); }
      .table-wrap { overflow-x: auto; }
      .table { min-width: 500px; }
      .field-row { grid-template-columns: 1fr; }
      .field-cep, .field-num, .field-uf { max-width: 100%; }
    }
  `],
})
export class TenantDetailComponent implements OnInit {
  private route    = inject(ActivatedRoute);
  private router   = inject(Router);
  private svc      = inject(TenantService);
  private plansSvc = inject(PlansService);
  private subSvc   = inject(SubscriptionService);
  private http     = inject(HttpClient);
  private toast    = inject(ToastService);

  tenant    = signal<TenantDto | null>(null);
  loading   = signal(true);
  acting    = signal('');
  activeTab = signal<Tab>('info');
  cancelModal      = signal(false);

  employees        = signal<EmployeeDto[]>([]);
  loadingEmployees = signal(false);

  allPlans         = signal<PlanDto[]>([]);
  businessTypes    = signal<BusinessTypeDto[]>([]);
  activePlans      = computed(() => this.allPlans().filter(p => p.status === 'Active'));
  changingPlan         = signal(false);
  selectedPlanId       = '';
  changingBusinessType = signal(false);
  selectedBusinessTypeId = '';

  // Financeiro
  subscription        = signal<SubscriptionDto | null>(null);
  loadingSubscription = signal(false);
  finDrawer  = signal<'invoice' | 'payment' | null>(null);
  finActing  = signal(false);
  finAmountStr = '';
  finDueDate   = '';
  finPaidAt    = '';
  finNextDueDate = '';
  finPaymentRef  = '';
  finNotes       = '';

  openInvoice() {
    return this.subscription()?.invoices
      .filter(i => i.status === 'Pending' || i.status === 'Overdue')
      .sort((a, b) => a.dueDate.localeCompare(b.dueDate))[0];
  }

  lastPaid() {
    return this.subscription()?.invoices
      .filter(i => i.status === 'Paid')
      .sort((a, b) => b.dueDate.localeCompare(a.dueDate))[0];
  }

  editing    = signal(false);
  saving     = signal(false);
  cepLoading = signal(false);
  editForm = { name: '', email: '', phone: '', timezone: '', cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidade: '', uf: '' };

  currentPlan() {
    const t = this.tenant();
    return t ? (this.allPlans().find(x => x.id === t.planId) ?? null) : null;
  }

  businessTypeName() {
    const t = this.tenant();
    if (!t) return '—';
    return this.businessTypes().find(b => b.id === t.businessTypeId)?.name ?? '—';
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id')!;
    forkJoin({
      tenant: this.svc.getById(id),
      plans:  this.plansSvc.getAll(),
      bts:    this.http.get<BusinessTypeDto[]>(`${environment.apiUrl}/admin/business-types`),
    }).subscribe({
      next: ({ tenant, plans, bts }) => {
        this.tenant.set(tenant);
        this.allPlans.set(plans);
        this.businessTypes.set(bts);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  startEdit() {
    const t = this.tenant()!;
    const a = t.address;
    this.editForm = {
      name: t.name, email: t.email ?? '', phone: t.phone ?? '', timezone: t.timezone,
      cep: a?.cep ?? '', logradouro: a?.logradouro ?? '', numero: a?.numero ?? '',
      complemento: a?.complemento ?? '', bairro: a?.bairro ?? '',
      cidade: a?.cidade ?? '', uf: a?.uf ?? '',
    };
    this.editing.set(true);
  }

  lookupCep() {
    const cep = this.editForm.cep.replace(/\D/g, '');
    if (cep.length !== 8) return;
    this.cepLoading.set(true);
    this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
      next: (data) => {
        this.cepLoading.set(false);
        if (data.erro) { this.toast.warning('CEP não encontrado.'); return; }
        this.editForm.logradouro = data.logradouro ?? this.editForm.logradouro;
        this.editForm.bairro     = data.bairro     ?? this.editForm.bairro;
        this.editForm.cidade     = data.localidade ?? this.editForm.cidade;
        this.editForm.uf         = data.uf         ?? this.editForm.uf;
        this.editForm.complemento = data.complemento ?? this.editForm.complemento;
      },
      error: () => { this.cepLoading.set(false); },
    });
  }

  saveEdit() {
    if (!this.editForm.name.trim()) { this.toast.error('O nome é obrigatório.'); return; }
    this.saving.set(true);
    this.http.put(`${environment.apiUrl}/tenants/${this.tenant()!.id}`, {
      name: this.editForm.name,
      logoUrl: this.tenant()!.name ? null : null,
      email: this.editForm.email || null,
      timezone: this.editForm.timezone,
      phone: this.editForm.phone || null,
      cep: this.editForm.cep || null,
      logradouro: this.editForm.logradouro || null,
      numero: this.editForm.numero || null,
      complemento: this.editForm.complemento || null,
      bairro: this.editForm.bairro || null,
      cidade: this.editForm.cidade || null,
      uf: this.editForm.uf || null,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.editing.set(false);
        this.toast.success('Dados atualizados com sucesso!');
        this.svc.getById(this.tenant()!.id).subscribe(t => this.tenant.set(t));
      },
      error: (err: any) => { this.saving.set(false); this.toast.error(err?.error?.message ?? 'Erro ao salvar as alterações.'); },
    });
  }

  goBack() { this.router.navigate(['/admin/tenants']); }

  openChangePlan() {
    this.selectedPlanId = this.tenant()!.planId;
    this.changingPlan.set(true);
  }

  openChangeBusinessType() {
    this.selectedBusinessTypeId = this.tenant()!.businessTypeId;
    this.changingBusinessType.set(true);
  }

  saveBusinessType() {
    if (!this.selectedBusinessTypeId || this.selectedBusinessTypeId === this.tenant()!.businessTypeId) {
      this.changingBusinessType.set(false); return;
    }
    this.acting.set('bt');
    this.http.patch(`${environment.apiUrl}/tenants/${this.tenant()!.id}/business-type`, { businessTypeId: this.selectedBusinessTypeId }).subscribe({
      next: () => {
        this.acting.set(''); this.changingBusinessType.set(false);
        this.toast.success('Tipo de negócio alterado com sucesso!');
        this.svc.getById(this.tenant()!.id).subscribe(t => this.tenant.set(t));
      },
      error: (err: any) => { this.acting.set(''); this.toast.error(err?.error?.message ?? 'Erro ao alterar tipo de negócio.'); },
    });
  }

  savePlan() {
    if (!this.selectedPlanId || this.selectedPlanId === this.tenant()!.planId) {
      this.changingPlan.set(false); return;
    }
    this.acting.set('plan');
    this.plansSvc.changeTenantPlan(this.tenant()!.id, this.selectedPlanId).subscribe({
      next: () => {
        this.acting.set(''); this.changingPlan.set(false);
        this.toast.success('Plano alterado com sucesso!');
        this.svc.getById(this.tenant()!.id).subscribe(t => this.tenant.set(t));
      },
      error: () => { this.acting.set(''); this.toast.error('Erro ao alterar o plano.'); },
    });
  }

  doAction(type: 'activate' | 'suspend' | 'reactivate' | 'cancel') {
    this.cancelModal.set(false);
    this.acting.set(type);
    const id = this.tenant()!.id;
    const call = type === 'suspend'    ? this.svc.suspend(id)
               : type === 'cancel'     ? this.svc.cancel(id)
               :                         this.svc.reactivate(id);
    const successMsg: Record<string, string> = {
      suspend: 'Conta suspensa.', cancel: 'Conta cancelada.', reactivate: 'Conta reativada.', activate: 'Conta ativada.',
    };
    call.subscribe({
      next:  () => { this.acting.set(''); this.toast.success(successMsg[type] ?? 'Ação realizada.'); this.svc.getById(id).subscribe(t => this.tenant.set(t)); },
      error: () => { this.acting.set(''); this.toast.error('Erro ao executar a ação. Tente novamente.'); },
    });
  }

  loadEmployeesTab() {
    this.activeTab.set('employees');
    if (this.employees().length > 0) return;
    this.loadingEmployees.set(true);
    this.svc.getEmployees(this.tenant()!.id).subscribe({
      next:  es => { this.employees.set(es); this.loadingEmployees.set(false); },
      error: () => this.loadingEmployees.set(false),
    });
  }

  loadFinanceiroTab() {
    this.activeTab.set('financeiro');
    if (this.subscription() !== null || this.loadingSubscription()) return;
    this.loadingSubscription.set(true);
    this.subSvc.getByTenant(this.tenant()!.id).subscribe({
      next: sub => { this.subscription.set(sub); this.loadingSubscription.set(false); },
      error: () => { this.subscription.set(null); this.loadingSubscription.set(false); },
    });
  }

  openInvoiceDrawer() {
    this.finAmountStr = '';
    this.finDueDate = this.subscription()!.nextDueDate.split('T')[0];
    this.finDrawer.set('invoice');
  }

  openPaymentDrawer() {
    const nd = new Date(this.subscription()!.nextDueDate.split('T')[0] + 'T12:00:00');
    nd.setMonth(nd.getMonth() + 1);
    this.finPaidAt = new Date().toISOString().split('T')[0];
    this.finNextDueDate = nd.toISOString().split('T')[0];
    this.finPaymentRef = '';
    this.finNotes = '';
    this.finDrawer.set('payment');
  }

  finGenerateInvoice() {
    const amount = parseFloat(this.finAmountStr.replace(',', '.'));
    if (!amount || isNaN(amount) || !this.finDueDate) return;
    this.finActing.set(true);
    this.subSvc.generateInvoice(this.subscription()!.id, { amount, dueDate: this.finDueDate }).subscribe({
      next: () => {
        this.toast.success('Fatura gerada com sucesso.');
        this.finDrawer.set(null);
        this.finActing.set(false);
        this.reloadSubscription();
      },
      error: () => { this.toast.error('Erro ao gerar fatura.'); this.finActing.set(false); },
    });
  }

  finConfirmPayment() {
    const inv = this.openInvoice();
    if (!inv || !this.finPaidAt || !this.finNextDueDate) return;
    this.finActing.set(true);
    this.subSvc.confirmPayment(this.subscription()!.id, inv.id, {
      paidAt: this.finPaidAt,
      nextDueDate: this.finNextDueDate,
      paymentReference: this.finPaymentRef || null,
      notes: this.finNotes || null,
    }).subscribe({
      next: () => {
        this.toast.success('Pagamento confirmado.');
        this.finDrawer.set(null);
        this.finActing.set(false);
        this.reloadSubscription();
      },
      error: () => { this.toast.error('Erro ao confirmar pagamento.'); this.finActing.set(false); },
    });
  }

  private reloadSubscription() {
    this.subSvc.getByTenant(this.tenant()!.id).subscribe(sub => this.subscription.set(sub));
  }

  subStatusLabel(s: string) {
    return { EmDia: 'Em dia', AguardandoPagamento: 'Aguardando pagamento', Inadimplente: 'Inadimplente', SemFaturas: 'Sem faturas' }[s] ?? s;
  }

  invStatusLabel(s: string) {
    return { Paid: 'Pago', Pending: 'Pendente', Overdue: 'Em atraso' }[s] ?? s;
  }

  formatDate(d: string | undefined | null) {
    if (!d) return '—';
    const [y, m, day] = d.split('T')[0].split('-');
    return `${day}/${m}/${y}`;
  }

  daysLeft(d: string)            { return this.svc.daysUntil(d); }
  statusLabel(s: string)         { return STATUS_LABEL[s] ?? s; }
  employeeStatusLabel(s: string) { return EMPLOYEE_STATUS_LABEL[s] ?? s; }
}
