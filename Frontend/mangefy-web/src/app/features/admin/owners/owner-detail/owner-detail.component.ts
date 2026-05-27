import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OwnerService, OwnerDetailDto, UpdateOwnerRequest } from '../owner.service';
import { ToastService } from '../../../../core/toast/toast.service';

const STATUS_LABEL: Record<string, string> = {
  Active: 'Ativo', PendingActivation: 'Aguardando ativação', Inactive: 'Inativo',
};
const TENANT_STATUS_LABEL: Record<string, string> = {
  Active: 'Ativo', TrialPeriod: 'Trial', Suspended: 'Suspenso', Cancelled: 'Cancelado',
};

interface EditForm {
  name: string;
  email: string;
  phone: string;
  documentType: '' | 'CPF' | 'CNPJ';
  documentNumber: string;
  notes: string;
  cep: string;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  uf: string;
}

function emptyForm(): EditForm {
  return { name: '', email: '', phone: '', documentType: '', documentNumber: '', notes: '', cep: '', logradouro: '', numero: '', complemento: '', bairro: '', cidade: '', uf: '' };
}

@Component({
  selector: 'app-owner-detail',
  standalone: true,
  imports: [DatePipe, CurrencyPipe, FormsModule],
  template: `
    <div class="page">

      <div class="breadcrumb">
        <button class="back-btn" (click)="goBack()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>
          Clientes
        </button>
        @if (owner()) { <span class="sep">/</span> <span class="bc-current">{{ owner()!.name }}</span> }
      </div>

      @if (loading()) {
        <div class="loading-state"><div class="spin"></div></div>
      } @else if (!owner()) {
        <div class="empty-state">Cliente não encontrado.</div>
      } @else {

        <!-- Header -->
        <div class="detail-header">
          <div class="detail-meta">
            <div class="detail-avatar">{{ owner()!.name.charAt(0).toUpperCase() }}</div>
            <div>
              <div class="detail-name-row">
                <h1 class="detail-name">{{ owner()!.name }}</h1>
                <span class="badge badge-{{ owner()!.status }}">{{ statusLabel(owner()!.status) }}</span>
              </div>
              <div class="detail-sub">
                <span>{{ owner()!.email }}</span>
                @if (owner()!.phone) { <span class="dot">·</span> <span>{{ formatPhone(owner()!.phone!) }}</span> }
              </div>
            </div>
          </div>
          <div class="header-actions">
            <button class="btn btn-primary" (click)="openCreateTenant()">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
              Novo Estabelecimento
            </button>
            <button class="btn btn-ghost" (click)="openEdit()">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              Editar
            </button>
            @if (owner()!.status === 'PendingActivation') {
              <button class="btn btn-ghost" (click)="doResend()" [disabled]="acting()">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                {{ acting() === 'resend' ? 'Enviando…' : 'Reenviar ativação' }}
              </button>
            }
            @if (owner()!.status === 'Inactive') {
              <button class="btn btn-success" (click)="doAction('activate')" [disabled]="acting()">
                {{ acting() === 'activate' ? '…' : 'Ativar cliente' }}
              </button>
            }
            @if (owner()!.status === 'Active') {
              <button class="btn btn-warn" (click)="doAction('deactivate')" [disabled]="acting()">
                {{ acting() === 'deactivate' ? '…' : 'Desativar cliente' }}
              </button>
            }
          </div>
        </div>

        <!-- Banner de link de ativação após resend -->
        @if (resendInfo()) {
          <div class="resend-banner" [class.warn]="!resendInfo()!.emailSent">
            <div class="resend-banner-header">
              @if (resendInfo()!.emailSent) {
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
                <strong>E-mail de ativação enviado.</strong>
                <span class="muted">Também disponível em:</span>
              } @else {
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
                <strong>E-mail não pôde ser enviado.</strong>
                <span class="muted">Envie este link manualmente ao cliente:</span>
              }
            </div>
            <div class="resend-link-row">
              <code class="resend-link">{{ resendInfo()!.url }}</code>
              <button class="btn-copy" (click)="copyLink()">{{ copyHint() || 'Copiar' }}</button>
              <a class="btn-copy" [href]="resendInfo()!.url" target="_blank">Abrir</a>
            </div>
            @if (!resendInfo()!.emailSent) {
              <div class="resend-hint">
                Em ambiente de testes do Resend, o e-mail só entrega para a conta registrada (<code>onboarding&#64;resend.dev</code>). Configure um domínio próprio para envio em produção.
              </div>
            }
          </div>
        }

        <!-- Métricas -->
        <div class="metrics">
          <div class="metric-card">
            <div class="metric-label">Estabelecimentos</div>
            <div class="metric-value">{{ owner()!.metrics.totalEstablishments }}</div>
            <div class="metric-sub">
              <span class="chip chip-green">{{ owner()!.metrics.activeEstablishments }} ativos</span>
              <span class="chip chip-purple">{{ owner()!.metrics.trialEstablishments }} trial</span>
              @if (owner()!.metrics.suspendedEstablishments > 0) {
                <span class="chip chip-amber">{{ owner()!.metrics.suspendedEstablishments }} suspensos</span>
              }
            </div>
          </div>

          <div class="metric-card">
            <div class="metric-label">MRR estimado</div>
            <div class="metric-value">{{ owner()!.metrics.estimatedMrr | currency:'BRL':'symbol':'1.2-2' }}</div>
            <div class="metric-sub muted">soma mensal dos planos ativos</div>
          </div>

          <div class="metric-card">
            <div class="metric-label">Plano(s)</div>
            <div class="metric-value-small">
              @if (owner()!.metrics.plans.length === 0) { <span class="muted">—</span> }
              @for (p of owner()!.metrics.plans; track p) { <span class="chip chip-dark">{{ p }}</span> }
            </div>
          </div>

          <div class="metric-card">
            <div class="metric-label">Cliente há</div>
            <div class="metric-value">{{ owner()!.metrics.daysAsClient }}</div>
            <div class="metric-sub muted">dia{{ owner()!.metrics.daysAsClient === 1 ? '' : 's' }}</div>
          </div>
        </div>

        <!-- Dados da conta -->
        <div class="section">
          <h2 class="section-title">Dados da conta</h2>
          <div class="dl-grid">
            <div><dt>E-mail</dt><dd>{{ owner()!.email }}</dd></div>
            <div><dt>Telefone</dt><dd>{{ owner()!.phone ? formatPhone(owner()!.phone!) : '—' }}</dd></div>
            <div><dt>Documento</dt><dd>{{ owner()!.documentNumber ? owner()!.documentType + ': ' + formatDoc(owner()!.documentNumber!, owner()!.documentType!) : '—' }}</dd></div>
            <div><dt>Status</dt><dd><span class="badge badge-{{ owner()!.status }}">{{ statusLabel(owner()!.status) }}</span></dd></div>
            <div><dt>Último acesso</dt><dd>{{ owner()!.lastLoginAt ? (owner()!.lastLoginAt | date:'dd/MM/yyyy HH:mm') : 'Nunca' }}</dd></div>
            <div><dt>Cadastrado em</dt><dd>{{ owner()!.createdAt | date:'dd/MM/yyyy' }}</dd></div>
          </div>
        </div>

        <!-- Endereço -->
        @if (owner()!.address) {
          <div class="section">
            <h2 class="section-title">Endereço</h2>
            <div class="address-box">
              <div>{{ owner()!.address!.logradouro }}, {{ owner()!.address!.numero }}@if (owner()!.address!.complemento) { <span> — {{ owner()!.address!.complemento }}</span> }</div>
              <div>{{ owner()!.address!.bairro }} — {{ owner()!.address!.cidade }}/{{ owner()!.address!.uf }}</div>
              <div class="muted">CEP {{ formatCep(owner()!.address!.cep) }}</div>
            </div>
          </div>
        }

        <!-- Notas -->
        @if (owner()!.notes) {
          <div class="section">
            <h2 class="section-title">Notas internas</h2>
            <div class="notes-box">{{ owner()!.notes }}</div>
          </div>
        }

        <!-- Estabelecimentos -->
        <div class="section">
          <h2 class="section-title">
            Estabelecimentos
            <span class="section-count">{{ owner()!.tenants.length }}</span>
          </h2>

          @if (owner()!.tenants.length === 0) {
            <div class="empty-tenants">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 7V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v2"/></svg>
              <p>Nenhum estabelecimento vinculado.</p>
              <button class="btn btn-primary btn-sm" (click)="openCreateTenant()">Criar primeiro estabelecimento</button>
            </div>
          } @else {
            <div class="table-wrap">
              <table class="table">
                <thead>
                  <tr>
                    <th>Nome</th>
                    <th>Slug</th>
                    <th>Plano</th>
                    <th>Status</th>
                    <th>Mensalidade</th>
                  </tr>
                </thead>
                <tbody>
                  @for (t of owner()!.tenants; track t.tenantId) {
                    <tr class="row-clickable" (click)="goTenant(t.tenantId)">
                      <td class="td-name">{{ t.tenantName }}</td>
                      <td class="td-slug">{{ t.tenantSlug }}</td>
                      <td>{{ t.planName ?? '—' }}</td>
                      <td><span class="badge badge-tenant-{{ t.status }}">{{ tenantStatusLabel(t.status) }}</span></td>
                      <td>{{ t.planPrice !== null ? (t.planPrice | currency:'BRL':'symbol':'1.2-2') : '—' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </div>
      }

    </div>

    <!-- Drawer Editar -->
    @if (drawerOpen()) {
      <div class="overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <h3 class="drawer-title">Editar cliente</h3>
          <button class="btn-close" (click)="closeDrawer()">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>
        <div class="drawer-body">
          @if (drawerError()) {
            <div class="alert-error">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              {{ drawerError() }}
            </div>
          }

          <!-- ── Identificação ── -->
          <div class="drawer-section">
            <div class="drawer-section-head">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
              Identificação
            </div>
            <div class="field">
              <label class="field-label">Nome completo *</label>
              <input class="field-input" [(ngModel)]="form.name" placeholder="João da Silva" />
            </div>
            <div class="field">
              <label class="field-label">E-mail *</label>
              <input class="field-input" type="email" [(ngModel)]="form.email" placeholder="cliente@exemplo.com" autocomplete="off" />
              <small class="field-hint">Usado para login. Alterar muda o acesso do cliente.</small>
            </div>
          </div>

          <!-- ── Contato e documento ── -->
          <div class="drawer-section">
            <div class="drawer-section-head">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.13.96.36 1.9.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.91.34 1.85.57 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
              Contato e documento
            </div>
            <div class="field-row">
              <div class="field">
                <label class="field-label">Telefone</label>
                <input class="field-input" [ngModel]="form.phone" (ngModelChange)="onPhoneChange($event)" placeholder="(11) 99999-9999" maxlength="20" />
              </div>
            </div>
            <div class="field-row">
              <div class="field" style="max-width: 130px;">
                <label class="field-label">Tipo doc.</label>
                <select class="field-input" [ngModel]="form.documentType" (ngModelChange)="onDocTypeChange($event)">
                  <option value="">—</option>
                  <option value="CPF">CPF</option>
                  <option value="CNPJ">CNPJ</option>
                </select>
              </div>
              <div class="field">
                <label class="field-label">Número do documento</label>
                <input class="field-input"
                  [ngModel]="form.documentNumber"
                  (ngModelChange)="onDocNumberChange($event)"
                  [disabled]="!form.documentType"
                  [maxlength]="form.documentType === 'CPF' ? 14 : 18"
                  [placeholder]="form.documentType === 'CPF' ? '000.000.000-00' : form.documentType === 'CNPJ' ? '00.000.000/0000-00' : ''" />
              </div>
            </div>
          </div>

          <!-- ── Endereço ── -->
          <div class="drawer-section">
            <div class="drawer-section-head">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
              Endereço
              <span class="optional">opcional</span>
            </div>
            <div class="field-row">
              <div class="field" style="max-width: 140px;">
                <label class="field-label">CEP</label>
                <input class="field-input"
                  [ngModel]="form.cep"
                  (ngModelChange)="onCepChange($event)"
                  placeholder="00000-000" maxlength="9" />
                @if (cepLoading()) { <small class="field-hint">Buscando endereço…</small> }
                @if (cepError()) { <small class="field-hint err">{{ cepError() }}</small> }
              </div>
              <div class="field">
                <label class="field-label">Logradouro</label>
                <input class="field-input" [(ngModel)]="form.logradouro" placeholder="Rua, Avenida…" />
              </div>
              <div class="field" style="max-width: 90px;">
                <label class="field-label">Nº</label>
                <input class="field-input" [(ngModel)]="form.numero" placeholder="000" />
              </div>
            </div>
            <div class="field-row">
              <div class="field">
                <label class="field-label">Complemento</label>
                <input class="field-input" [(ngModel)]="form.complemento" placeholder="Apto, sala, bloco…" />
              </div>
              <div class="field">
                <label class="field-label">Bairro</label>
                <input class="field-input" [(ngModel)]="form.bairro" />
              </div>
            </div>
            <div class="field-row">
              <div class="field">
                <label class="field-label">Cidade</label>
                <input class="field-input" [(ngModel)]="form.cidade" />
              </div>
              <div class="field" style="max-width: 90px;">
                <label class="field-label">UF</label>
                <input class="field-input" [ngModel]="form.uf" (ngModelChange)="form.uf = $event.toUpperCase().slice(0,2)" maxlength="2" placeholder="SP" />
              </div>
            </div>
          </div>

          <!-- ── Notas ── -->
          <div class="drawer-section">
            <div class="drawer-section-head">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
              Notas internas
              <span class="optional">não visíveis ao cliente</span>
            </div>
            <div class="field">
              <textarea class="field-input" rows="4" [(ngModel)]="form.notes" maxlength="2000" placeholder="Observações comerciais, contexto da conta, histórico…"></textarea>
              <small class="field-hint right">{{ form.notes.length }} / 2000</small>
            </div>
          </div>
        </div>
        <div class="drawer-footer">
          <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
          <button class="btn btn-primary" (click)="saveEdit()" [disabled]="saving()">
            {{ saving() ? 'Salvando…' : 'Salvar alterações' }}
          </button>
        </div>
      </aside>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; max-width: 1100px; }
    .breadcrumb { display: flex; align-items: center; gap: 8px; margin-bottom: 18px; }
    .back-btn { display: inline-flex; align-items: center; gap: 6px; background: none; border: none; color: #666; font-size: 13px; font-weight: 500; cursor: pointer; padding: 4px 6px; border-radius: 6px; &:hover { background: #f4f4f6; } }
    .sep { color: #ccc; }
    .bc-current { font-size: 13px; color: #111; font-weight: 500; }
    .loading-state { display: flex; justify-content: center; padding: 80px; }
    .spin { width: 28px; height: 28px; border: 3px solid #e8e8ec; border-top-color: var(--color-brand); border-radius: 50%; animation: spin .7s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    .empty-state { text-align: center; padding: 80px; color: #aaa; }

    /* Header */
    .detail-header {
      display: flex; align-items: center; justify-content: space-between;
      gap: 16px; margin-bottom: 24px; flex-wrap: wrap;
    }
    .detail-meta { display: flex; align-items: center; gap: 14px; }
    .detail-avatar {
      width: 52px; height: 52px; border-radius: 14px;
      background: var(--color-brand); color: #fff;
      display: flex; align-items: center; justify-content: center;
      font-size: 22px; font-weight: 800; flex-shrink: 0;
    }
    .detail-name-row { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
    .detail-name { font-size: 22px; font-weight: 700; color: #111; line-height: 1.2; }
    .detail-sub { display: flex; flex-wrap: wrap; gap: 4px 8px; font-size: 12px; color: #888; margin-top: 4px; }
    .dot { color: #ccc; }
    .header-actions { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }

    /* Métricas */
    .metrics { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 12px; margin-bottom: 28px; }
    .metric-card {
      background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; padding: 18px 20px;
      display: flex; flex-direction: column; gap: 8px;
    }
    .metric-label { font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #aaa; }
    .metric-value { font-size: 26px; font-weight: 800; color: #111; line-height: 1; }
    .metric-value-small { display: flex; flex-wrap: wrap; gap: 6px; min-height: 26px; align-items: center; }
    .metric-sub { display: flex; gap: 6px; flex-wrap: wrap; }
    .muted { color: #aaa; font-size: 12px; }

    /* Chips */
    .chip { display: inline-flex; align-items: center; padding: 2px 9px; border-radius: 99px; font-size: 11px; font-weight: 600; }
    .chip-green  { background: #dcfce7; color: #15803d; }
    .chip-purple { background: #ede9fe; color: #7c3aed; }
    .chip-amber  { background: #fef3c7; color: #b45309; }
    .chip-dark   { background: #f0f0f3; color: #333; }

    /* Section */
    .section { margin-bottom: 20px; }
    .section-title { font-size: 13px; font-weight: 700; color: #555; margin-bottom: 10px; display: flex; align-items: center; gap: 8px; }
    .section-count { background: #f0f0f3; color: #555; font-size: 11px; padding: 2px 8px; border-radius: 99px; font-weight: 700; }

    /* DL card */
    .dl-grid {
      background: #fff; border: 1px solid #e8e8ec; border-radius: 14px;
      padding: 20px; display: grid; grid-template-columns: auto 1fr; gap: 10px 24px; align-items: baseline;
    }
    .dl-grid > div { display: contents; }
    .dl-grid dt { font-size: 12px; font-weight: 600; color: #aaa; white-space: nowrap; }
    .dl-grid dd { font-size: 13px; color: #333; margin: 0; }

    /* Endereço */
    .address-box {
      background: #fff; border: 1px solid #e8e8ec; border-radius: 14px;
      padding: 20px; font-size: 13px; color: #333; line-height: 1.7;
    }

    /* Notas */
    .notes-box { background: #fffbeb; border: 1px solid #fde68a; border-radius: 14px; padding: 20px; font-size: 13px; color: #4a3c0a; white-space: pre-wrap; line-height: 1.6; }

    /* Tabela */
    .table-wrap { background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; overflow: hidden; }
    .table { width: 100%; border-collapse: collapse;
      th { padding: 9px 14px; text-align: left; font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .06em; color: #bbb; border-bottom: 1px solid #f0f0f3; background: #fafafa; }
      td { padding: 10px 14px; border-bottom: 1px solid #f4f4f6; font-size: 13px; }
      tr:last-child td { border-bottom: none; }
    }
    .row-clickable { cursor: pointer; transition: background .1s; &:hover td { background: #f9f9fb; } }
    .td-name { font-weight: 600; color: #111; }
    .td-slug { font-family: monospace; font-size: 12px; color: #888; }
    .empty-tenants { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 48px; color: #ccc; background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; p { font-size: 13px; margin: 0; } }

    /* Badges */
    .badge { display: inline-block; padding: 3px 10px; border-radius: 99px; font-size: 11px; font-weight: 700; white-space: nowrap;
      &-Active            { background: #dcfce7; color: #15803d; }
      &-PendingActivation { background: #fef3c7; color: #b45309; }
      &-Inactive          { background: #f4f4f5; color: #71717a; }
      &-tenant-Active     { background: #dcfce7; color: #15803d; }
      &-tenant-TrialPeriod{ background: #ede9fe; color: #7c3aed; }
      &-tenant-Suspended  { background: #fef3c7; color: #b45309; }
      &-tenant-Cancelled  { background: #f4f4f5; color: #71717a; }
    }

    /* Buttons */
    .btn { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border-radius: 8px; font-size: 13px; font-weight: 600; cursor: pointer; border: none; transition: all .15s; &:disabled { opacity: .5; cursor: not-allowed; } }
    .btn-sm { padding: 6px 12px; font-size: 12px; }
    .btn-primary { background: var(--color-brand); color: #fff; &:hover:not(:disabled) { opacity: .9; } }
    .btn-ghost   { background: #fff; color: #555; border: 1px solid #e8e8ec; &:hover:not(:disabled) { background: #f7f7f9; border-color: #d0d0d8; color: #333; } }
    .btn-success { background: #dcfce7; color: #15803d; border: 1px solid #bbf7d0; &:hover:not(:disabled) { background: #bbf7d0; } }
    .btn-warn    { background: #fef3c7; color: #b45309; border: 1px solid #fde68a; &:hover:not(:disabled) { background: #fde68a; } }

    /* Resend banner */
    .resend-banner {
      margin-bottom: 22px;
      padding: 14px 16px;
      background: #ecfdf5;
      border: 1px solid #a7f3d0;
      border-radius: 10px;
      font-size: 12px;
      color: #065f46;
      display: flex; flex-direction: column; gap: 10px;
      &.warn { background: #fffbeb; border-color: #fde68a; color: #92400e; }
    }
    .resend-banner-header { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
    .resend-banner-header svg { flex-shrink: 0; }
    .resend-banner-header strong { font-size: 13px; }
    .resend-banner-header .muted { color: currentColor; opacity: .75; font-weight: 500; }
    .resend-link-row { display: flex; align-items: center; gap: 8px; }
    .resend-link {
      flex: 1; padding: 8px 12px;
      background: rgba(255,255,255,.6); border: 1px solid rgba(0,0,0,.05); border-radius: 6px;
      font-family: ui-monospace, monospace; font-size: 11px;
      color: inherit; word-break: break-all; line-height: 1.4;
    }
    .btn-copy {
      flex-shrink: 0; background: rgba(255,255,255,.85);
      border: 1px solid rgba(0,0,0,.08); padding: 7px 12px; border-radius: 6px;
      font-size: 12px; font-weight: 600; color: inherit; cursor: pointer;
      text-decoration: none;
      &:hover { background: #fff; }
    }
    .resend-hint { font-size: 11px; opacity: .85; line-height: 1.5;
      code { background: rgba(0,0,0,.05); padding: 1px 4px; border-radius: 3px; font-size: 10px; }
    }

    /* Drawer */
    .overlay { position: fixed; inset: 0; background: rgba(0,0,0,.3); z-index: 100; backdrop-filter: blur(2px); }
    .drawer { position: fixed; top: 0; right: 0; bottom: 0; width: 480px; background: #fff; border-left: 1px solid #e8e8ec; z-index: 101; display: flex; flex-direction: column; box-shadow: -8px 0 32px rgba(0,0,0,.1); animation: slideIn .2s ease; }
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }
    .drawer-header { display: flex; justify-content: space-between; align-items: center; padding: 18px 22px; border-bottom: 1px solid #f0f0f3; }
    .drawer-title { font-size: 16px; font-weight: 700; color: #111; }
    .btn-close { width: 32px; height: 32px; border-radius: 8px; border: none; background: #f4f4f5; color: #666; display: flex; align-items: center; justify-content: center; cursor: pointer; &:hover { background: #ebebef; } }
    .drawer-body { flex: 1; overflow-y: auto; padding: 22px; display: flex; flex-direction: column; gap: 14px; }
    .drawer-footer { padding: 14px 22px; border-top: 1px solid #f0f0f3; display: flex; justify-content: flex-end; gap: 10px; }
    .field { display: flex; flex-direction: column; gap: 6px; flex: 1; min-width: 0; }
    .field-row { display: flex; gap: 12px; align-items: flex-start; }
    .field-label { font-size: 11px; font-weight: 600; color: #444; }
    .field-hint { font-size: 11px; color: #999; margin-top: 2px; line-height: 1.4; &.err { color: #b91c1c; } &.right { text-align: right; } }
    .field-input { padding: 10px 12px; border: 1px solid #e2e2e6; border-radius: 8px; font-size: 13px; outline: none; transition: border-color .15s, box-shadow .15s; font-family: inherit; background: #fff;
      &:focus { border-color: var(--color-brand); box-shadow: 0 0 0 3px rgba(245,196,0,.12); }
      &:disabled { background: #f7f7f9; color: #aaa; cursor: not-allowed; }
      &::placeholder { color: #c4c4c8; }
    }
    textarea.field-input { resize: vertical; min-height: 80px; }

    /* Section blocks */
    .drawer-section { background: #fafafa; border: 1px solid #f0f0f3; border-radius: 12px; padding: 14px 16px; display: flex; flex-direction: column; gap: 12px; }
    .drawer-section-head { display: flex; align-items: center; gap: 8px; font-size: 12px; font-weight: 700; color: #111; text-transform: uppercase; letter-spacing: .04em;
      svg { color: var(--color-brand); flex-shrink: 0; }
      .optional { margin-left: auto; font-size: 10px; font-weight: 600; color: #aaa; text-transform: none; letter-spacing: 0; }
    }

    .alert-error { display: flex; align-items: center; gap: 8px; background: #fef2f2; color: #b91c1c; border: 1px solid #fecaca; border-radius: 8px; padding: 10px 12px; font-size: 12px; font-weight: 500; }
  `]
})
export class OwnerDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private svc = inject(OwnerService);
  private toast = inject(ToastService);

  owner   = signal<OwnerDetailDto | null>(null);
  loading = signal(true);
  acting  = signal<string | false>(false);
  resendInfo = signal<{ url: string; emailSent: boolean } | null>(null);
  copyHint = signal('');

  drawerOpen = signal(false);
  drawerError = signal('');
  saving = signal(false);
  form: EditForm = emptyForm();

  cepLoading = signal(false);
  cepError = signal('');

  ngOnInit() {
    this.reload();
  }

  private reload() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loading.set(true);
    this.svc.getById(id).subscribe({
      next: dto => { this.owner.set(dto); this.loading.set(false); },
      error: () => { this.owner.set(null); this.loading.set(false); },
    });
  }

  goBack() { this.router.navigate(['/admin/owners']); }
  goTenant(id: string) { this.router.navigate(['/admin/tenants', id]); }
  openCreateTenant() {
    this.router.navigate(['/admin/tenants'], { queryParams: { newWithOwner: this.owner()!.id } });
  }
  statusLabel(s: string) { return STATUS_LABEL[s] ?? s; }
  tenantStatusLabel(s: string) { return TENANT_STATUS_LABEL[s] ?? s; }

  formatPhone(v: string): string {
    const d = v.replace(/\D/g, '');
    if (d.length === 13) return `+${d.slice(0,2)} (${d.slice(2,4)}) ${d.slice(4,9)}-${d.slice(9)}`;
    if (d.length === 12) return `+${d.slice(0,2)} (${d.slice(2,4)}) ${d.slice(4,8)}-${d.slice(8)}`;
    if (d.length === 11) return `(${d.slice(0,2)}) ${d.slice(2,7)}-${d.slice(7)}`;
    if (d.length === 10) return `(${d.slice(0,2)}) ${d.slice(2,6)}-${d.slice(6)}`;
    return v;
  }

  formatDoc(v: string, type: 'CPF' | 'CNPJ'): string {
    const d = v.replace(/\D/g, '');
    if (type === 'CPF' && d.length === 11) return `${d.slice(0,3)}.${d.slice(3,6)}.${d.slice(6,9)}-${d.slice(9)}`;
    if (type === 'CNPJ' && d.length === 14) return `${d.slice(0,2)}.${d.slice(2,5)}.${d.slice(5,8)}/${d.slice(8,12)}-${d.slice(12)}`;
    return v;
  }

  formatCep(v: string): string {
    const d = v.replace(/\D/g, '');
    return d.length === 8 ? `${d.slice(0,5)}-${d.slice(5)}` : v;
  }

  doAction(action: 'activate' | 'deactivate') {
    const id = this.owner()!.id;
    this.acting.set(action);
    const call = action === 'activate' ? this.svc.activate(id) : this.svc.deactivate(id);
    call.subscribe({
      next: () => {
        this.acting.set(false);
        this.toast.show(action === 'activate' ? 'Cliente ativado.' : 'Cliente desativado.', 'success');
        this.reload();
      },
      error: (err) => {
        this.acting.set(false);
        this.toast.show(err?.error?.message ?? 'Erro ao alterar status.', 'error');
      },
    });
  }

  doResend() {
    const id = this.owner()!.id;
    this.acting.set('resend');
    this.svc.resendActivation(id).subscribe({
      next: (res) => {
        this.acting.set(false);
        const url = `${window.location.origin}/auth/activate-owner?token=${res.activationToken}`;
        this.resendInfo.set({ url, emailSent: res.emailSent });
        if (res.emailSent) {
          this.toast.show('E-mail de ativação enviado.', 'success');
        } else {
          this.toast.show('Token gerado, mas o e-mail não foi enviado. Use o link abaixo.', 'warning');
        }
      },
      error: (err) => {
        this.acting.set(false);
        this.toast.show(err?.error?.message ?? 'Erro ao reenviar ativação.', 'error');
      },
    });
  }

  copyLink() {
    const info = this.resendInfo();
    if (!info) return;
    navigator.clipboard.writeText(info.url).then(() => {
      this.copyHint.set('Copiado!');
      setTimeout(() => this.copyHint.set(''), 1500);
    });
  }

  openEdit() {
    const o = this.owner()!;
    this.form = {
      name: o.name,
      email: o.email,
      phone: o.phone ?? '',
      documentType: (o.documentType ?? '') as any,
      documentNumber: o.documentNumber ?? '',
      notes: o.notes ?? '',
      cep: o.address?.cep ?? '',
      logradouro: o.address?.logradouro ?? '',
      numero: o.address?.numero ?? '',
      complemento: o.address?.complemento ?? '',
      bairro: o.address?.bairro ?? '',
      cidade: o.address?.cidade ?? '',
      uf: o.address?.uf ?? '',
    };
    this.drawerError.set('');
    this.drawerOpen.set(true);
  }

  closeDrawer() {
    if (this.saving()) return;
    this.drawerOpen.set(false);
    this.cepError.set('');
    this.cepLoading.set(false);
  }

  onPhoneChange(v: string) {
    const d = v.replace(/\D/g, '').slice(0, 13);
    if (d.length === 0) { this.form.phone = ''; return; }
    if (d.length <= 2)  { this.form.phone = `(${d}`; return; }
    if (d.length <= 6)  { this.form.phone = `(${d.slice(0,2)}) ${d.slice(2)}`; return; }
    if (d.length <= 10) { this.form.phone = `(${d.slice(0,2)}) ${d.slice(2,6)}-${d.slice(6)}`; return; }
    if (d.length === 11) { this.form.phone = `(${d.slice(0,2)}) ${d.slice(2,7)}-${d.slice(7)}`; return; }
    // 12 ou 13 dígitos: assume +DDI (DD) NNNNN-NNNN
    this.form.phone = `+${d.slice(0, d.length - 11)} (${d.slice(-11,-9)}) ${d.slice(-9,-4)}-${d.slice(-4)}`;
  }

  onDocTypeChange(v: '' | 'CPF' | 'CNPJ') {
    this.form.documentType = v;
    if (!v) { this.form.documentNumber = ''; return; }
    // Re-mascara o número conforme o tipo
    this.onDocNumberChange(this.form.documentNumber);
  }

  onDocNumberChange(v: string) {
    const d = v.replace(/\D/g, '');
    if (this.form.documentType === 'CPF') {
      const s = d.slice(0, 11);
      if (s.length <= 3)  this.form.documentNumber = s;
      else if (s.length <= 6)  this.form.documentNumber = `${s.slice(0,3)}.${s.slice(3)}`;
      else if (s.length <= 9)  this.form.documentNumber = `${s.slice(0,3)}.${s.slice(3,6)}.${s.slice(6)}`;
      else                     this.form.documentNumber = `${s.slice(0,3)}.${s.slice(3,6)}.${s.slice(6,9)}-${s.slice(9)}`;
    } else if (this.form.documentType === 'CNPJ') {
      const s = d.slice(0, 14);
      if (s.length <= 2)        this.form.documentNumber = s;
      else if (s.length <= 5)   this.form.documentNumber = `${s.slice(0,2)}.${s.slice(2)}`;
      else if (s.length <= 8)   this.form.documentNumber = `${s.slice(0,2)}.${s.slice(2,5)}.${s.slice(5)}`;
      else if (s.length <= 12)  this.form.documentNumber = `${s.slice(0,2)}.${s.slice(2,5)}.${s.slice(5,8)}/${s.slice(8)}`;
      else                      this.form.documentNumber = `${s.slice(0,2)}.${s.slice(2,5)}.${s.slice(5,8)}/${s.slice(8,12)}-${s.slice(12)}`;
    } else {
      this.form.documentNumber = d;
    }
  }

  private cepDebounce: any;
  onCepChange(v: string) {
    const d = v.replace(/\D/g, '').slice(0, 8);
    this.form.cep = d.length > 5 ? `${d.slice(0,5)}-${d.slice(5)}` : d;
    this.cepError.set('');
    clearTimeout(this.cepDebounce);
    if (d.length === 8) {
      this.cepDebounce = setTimeout(() => this.lookupCep(d), 300);
    }
  }

  private lookupCep(cep: string) {
    this.cepLoading.set(true);
    fetch(`https://viacep.com.br/ws/${cep}/json/`)
      .then(r => r.json())
      .then((data: any) => {
        this.cepLoading.set(false);
        if (data?.erro) {
          this.cepError.set('CEP não encontrado.');
          return;
        }
        this.form.logradouro = data.logradouro || this.form.logradouro;
        this.form.bairro     = data.bairro     || this.form.bairro;
        this.form.cidade     = data.localidade || this.form.cidade;
        this.form.uf         = data.uf         || this.form.uf;
        // Foca no número após preencher
        setTimeout(() => {
          const el = document.querySelector<HTMLInputElement>('input[placeholder="000"]');
          el?.focus();
        }, 50);
      })
      .catch(() => {
        this.cepLoading.set(false);
        this.cepError.set('Erro ao consultar CEP.');
      });
  }

  saveEdit() {
    if (!this.form.name.trim()) { this.drawerError.set('Nome é obrigatório.'); return; }
    if (!this.form.email.trim()) { this.drawerError.set('E-mail é obrigatório.'); return; }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.form.email.trim())) {
      this.drawerError.set('E-mail inválido.'); return;
    }

    const hasAnyAddress = this.form.cep || this.form.logradouro || this.form.numero || this.form.bairro || this.form.cidade || this.form.uf;
    const allRequired = this.form.cep && this.form.logradouro && this.form.numero && this.form.bairro && this.form.cidade && this.form.uf;
    if (hasAnyAddress && !allRequired) {
      this.drawerError.set('Preencha todos os campos do endereço ou deixe-o vazio.');
      return;
    }

    const req: UpdateOwnerRequest = {
      name: this.form.name.trim(),
      email: this.form.email.trim(),
      phone: this.form.phone.trim() || null,
      documentType: (this.form.documentType || null) as any,
      documentNumber: this.form.documentNumber.trim() || null,
      notes: this.form.notes.trim() || null,
      address: hasAnyAddress ? {
        cep: this.form.cep, logradouro: this.form.logradouro, numero: this.form.numero,
        complemento: this.form.complemento || null,
        bairro: this.form.bairro, cidade: this.form.cidade, uf: this.form.uf.toUpperCase(),
      } : null,
    };

    this.saving.set(true);
    this.svc.update(this.owner()!.id, req).subscribe({
      next: () => {
        this.saving.set(false);
        this.drawerOpen.set(false);
        this.toast.show('Cliente atualizado.', 'success');
        this.reload();
      },
      error: (err) => {
        this.saving.set(false);
        this.drawerError.set(err?.error?.message ?? err?.error?.detail ?? 'Erro ao atualizar.');
      },
    });
  }
}
