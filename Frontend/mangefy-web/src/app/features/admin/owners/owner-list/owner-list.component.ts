import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { OwnerService, OwnerListItemDto } from '../owner.service';

const STATUS_LABEL: Record<string, string> = {
  Active: 'Ativo', PendingActivation: 'Aguardando ativação', Inactive: 'Inativo',
};

@Component({
  selector: 'app-owner-list',
  standalone: true,
  imports: [FormsModule, DatePipe],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Responsáveis</h1>
          <p class="page-subtitle">{{ total() }} responsável{{ total() !== 1 ? 'is' : '' }} cadastrado{{ total() !== 1 ? 's' : '' }}</p>
        </div>
        <button class="btn btn-primary" (click)="openCreate()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Novo Responsável
        </button>
      </div>

      <!-- Quick filter tabs -->
      <div class="qtabs">
        <button class="qtab" [class.active]="filterStatus() === ''"                  (click)="filterStatus.set('')">Todos</button>
        <button class="qtab" [class.active]="filterStatus() === 'Active'"             (click)="filterStatus.set('Active')">Ativos</button>
        <button class="qtab" [class.active]="filterStatus() === 'PendingActivation'"  (click)="filterStatus.set('PendingActivation')">Aguardando ativação</button>
        <button class="qtab" [class.active]="filterStatus() === 'Inactive'"           (click)="filterStatus.set('Inactive')">Inativos</button>
        @if (search() || filterStatus()) {
          <button class="qtab-clear" (click)="search.set(''); filterStatus.set('')">
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
            Limpar filtros
          </button>
        }
      </div>

      <!-- Filtro -->
      <div class="filters">
        <div class="search-wrap">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
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
                  <td><span class="badge badge-{{ o.status }}">{{ statusLabel(o.status) }}</span></td>
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
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>
          </button>
          @for (p of pageNumbers(); track p) {
            @if (p === -1) { <span class="pg-dots">…</span> }
            @else { <button class="pg-btn" [class.pg-active]="p === page()" (click)="goPage(p)">{{ p }}</button> }
          }
          <button class="pg-btn" (click)="goPage(page() + 1)" [disabled]="page() === totalPages()">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="9 18 15 12 9 6"/></svg>
          </button>
          <span class="pg-info">{{ (page()-1)*pageSize()+1 }}–{{ min(page()*pageSize(), total()) }} de {{ total() }}</span>
        </div>
      }
    </div>

    <!-- Drawer criar -->
    @if (drawerOpen()) {
      <div class="overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            </div>
            <div>
              <h3 class="drawer-title">Novo Responsável</h3>
              <p class="drawer-subtitle">Preencha os dados do responsável</p>
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

          <!-- Seção: Identificação -->
          <div class="form-section">
            <div class="form-section-header">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
              Identificação
            </div>
            <div class="form-grid-1">
              <div class="field">
                <label class="field-label">Nome completo <span class="req">*</span></label>
                <div class="input-wrap">
                  <svg class="input-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                  <input class="field-input has-icon" [(ngModel)]="form.name" placeholder="Ex: João Silva" maxlength="200" />
                </div>
              </div>
            </div>
            <div class="form-grid-2">
              <div class="field">
                <label class="field-label">E-mail <span class="req">*</span></label>
                <div class="input-wrap">
                  <svg class="input-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                  <input class="field-input has-icon" type="email" [(ngModel)]="form.email" placeholder="joao@email.com.br" />
                </div>
              </div>
              <div class="field">
                <label class="field-label">Telefone</label>
                <div class="input-wrap">
                  <svg class="input-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07A19.5 19.5 0 0 1 4.69 12a19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 3.6 1.18h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L7.91 8.73a16 16 0 0 0 6.29 6.29l.91-.91a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                  <input class="field-input has-icon" [(ngModel)]="form.phone" placeholder="+55 11 99999-9999" maxlength="20" />
                </div>
              </div>
            </div>
          </div>

          <!-- Seção: Documento -->
          <div class="form-section">
            <div class="form-section-header">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="2" y="5" width="20" height="14" rx="2"/><line x1="2" y1="10" x2="22" y2="10"/></svg>
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
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
              Endereço
            </div>
            <div class="form-grid-cep">
              <div class="field">
                <label class="field-label">CEP</label>
                <div class="input-wrap">
                  @if (cepLoading()) {
                    <svg class="input-icon spin" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 12a9 9 0 1 1-6.219-8.56"/></svg>
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
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
            Um link de ativação de 48h será enviado ao cliente para definir sua senha.
          </div>
        </div>

        <div class="drawer-footer">
          <button class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
          <button class="btn btn-primary" (click)="submit()" [disabled]="saving()">
            @if (saving()) {
              <svg class="spin" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 12a9 9 0 1 1-6.219-8.56"/></svg>
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
    .page-title  { font-size: 20px; font-weight: 700; color: #111; }
    .page-subtitle { font-size: 12px; color: #aaa; margin-top: 2px; }

    .filters { display: flex; gap: 10px; margin-bottom: 12px; }
    .search-wrap {
      flex: 1; display: flex; align-items: center; gap: 8px;
      background: #fff; border: 1px solid #e8e8ec; border-radius: 8px; padding: 7px 12px;
      svg { color: #bbb; flex-shrink: 0; }
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
    .empty-cell { text-align: center; padding: 48px; color: #ccc; font-size: 13px; }

    /* Skeleton */
    .skel { height: 12px; border-radius: 6px; background: linear-gradient(90deg, #f0f0f3 25%, #e8e8ec 50%, #f0f0f3 75%); background-size: 200% 100%; animation: shimmer 1.4s infinite; }
    @keyframes shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }

    /* Badges */
    .badge {
      display: inline-block; padding: 3px 10px; border-radius: 99px;
      font-size: 11px; font-weight: 700; white-space: nowrap;
      &-Active           { background: #dcfce7; color: #15803d; }
      &-PendingActivation{ background: #fef3c7; color: #b45309; }
      &-Inactive         { background: #f4f4f5; color: #71717a; }
    }

    /* Quick filter tabs */
    .qtabs { display: flex; align-items: center; gap: 4px; flex-wrap: wrap; margin-bottom: 10px; }
    .qtab { padding: 5px 14px; border-radius: 99px; border: 1px solid #e8e8ec; background: #fff; color: #666; font-size: 12px; font-weight: 600; cursor: pointer; transition: all .15s; &:hover { background: #f5f5f7; border-color: #d0d0d8; color: #333; } &.active { background: var(--color-brand); color: #fff; border-color: transparent; } }
    .qtab-clear { display: inline-flex; align-items: center; gap: 5px; margin-left: 4px; padding: 5px 12px; border-radius: 99px; border: none; background: #fef2f2; color: #b91c1c; font-size: 11px; font-weight: 600; cursor: pointer; transition: background .15s; &:hover { background: #fee2e2; } }

    /* Pagination */
    .pagination { display: flex; align-items: center; gap: 4px; padding: 14px 16px; border-top: 1px solid #f0f0f3; flex-wrap: wrap; }
    .pg-btn { min-width: 32px; height: 32px; padding: 0 8px; border-radius: 7px; border: 1px solid #e8e8ec; background: #fff; color: #555; font-size: 13px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all .12s; &:hover:not(:disabled) { background: #f4f4f6; } &:disabled { opacity: .35; cursor: not-allowed; } &.pg-active { background: var(--color-brand); color: #fff; border-color: transparent; } }
    .pg-dots { padding: 0 4px; color: #aaa; font-size: 13px; }
    .pg-info { margin-left: 8px; font-size: 12px; color: #aaa; white-space: nowrap; }

    /* Alert */
    .alert-error { display: flex; align-items: center; gap: 8px; background: #fef2f2; color: #b91c1c; border: 1px solid #fecaca; border-radius: 10px; padding: 12px 14px; font-size: 13px; margin-bottom: 20px; }
    .mb-16 { margin-bottom: 16px; }

    /* Buttons */
    .btn { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border-radius: 8px; font-size: 13px; font-weight: 600; cursor: pointer; border: none; transition: all .15s; }
    .btn-primary { background: var(--color-brand); color: #fff; &:hover { opacity: .9; } &:disabled { opacity: .5; cursor: not-allowed; } }
    .btn-ghost   { background: #f4f4f5; color: #555; border: 1px solid #e8e8ec; &:hover { background: #ebebef; } }

    /* Drawer */
    .overlay { position: fixed; inset: 0; background: rgba(0,0,0,.3); z-index: 100; backdrop-filter: blur(2px); }
    .drawer { position: fixed; top: 0; right: 0; bottom: 0; width: 420px; background: #fff; border-left: 1px solid #e8e8ec; z-index: 101; display: flex; flex-direction: column; box-shadow: -8px 0 32px rgba(0,0,0,.1); animation: slideIn .2s ease; }
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }
    .drawer-header { display: flex; align-items: center; justify-content: space-between; padding: 18px 20px; border-bottom: 1px solid #f0f0f3; flex-shrink: 0; background: #fff; }
    .drawer-header-left { display: flex; align-items: center; gap: 12px; }
    .drawer-icon { width: 36px; height: 36px; border-radius: 10px; background: color-mix(in srgb, var(--color-brand) 10%, transparent); color: var(--color-brand); display: flex; align-items: center; justify-content: center; }
    .drawer-title { font-size: 15px; font-weight: 700; color: #111; }
    .drawer-subtitle { font-size: 11px; color: #aaa; margin-top: 1px; }
    .btn-close { width: 30px; height: 30px; border-radius: 7px; border: 1px solid #e8e8ec; background: #fff; color: #888; display: flex; align-items: center; justify-content: center; cursor: pointer; transition: all .12s; &:hover { background: #f4f4f5; color: #333; } }
    .drawer-body { flex: 1; overflow-y: auto; padding: 16px 20px; display: flex; flex-direction: column; gap: 12px; }
    .drawer-footer { padding: 14px 20px; border-top: 1px solid #f0f0f3; display: flex; gap: 10px; justify-content: flex-end; flex-shrink: 0; background: #fafafa; }

    /* Form sections */
    .form-section { display: flex; flex-direction: column; gap: 10px; padding: 16px; background: #fafafa; border: 1px solid #f0f0f3; border-radius: 10px; }
    .form-section-header { display: flex; align-items: center; gap: 6px; font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #999; svg { color: var(--color-brand); } }
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
    @keyframes spin { to { transform: rotate(360deg); } }
    .spin { animation: spin .8s linear infinite; transform-origin: center; }

    /* Info box */
    .info-box { display: flex; align-items: flex-start; gap: 8px; background: #f0f9ff; border: 1px solid #bae6fd; border-radius: 8px; padding: 10px 12px; font-size: 12px; color: #0369a1; svg { flex-shrink: 0; margin-top: 1px; } }
  `],
})
export class OwnerListComponent implements OnInit {
  private svc    = inject(OwnerService);
  private router = inject(Router);
  private route  = inject(ActivatedRoute);
  private http   = inject(HttpClient);

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
