import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BusinessTypeService, BusinessTypeDto, RoleTemplateDto } from './business-type.service';
import { ToastService } from '../../../core/toast/toast.service';

type Tab = 'info' | 'templates';

interface TemplateForm { name: string; description: string; }
function emptyTemplateForm(): TemplateForm { return { name: '', description: '' }; }

@Component({
  selector: 'app-business-type-detail',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="page">

      <!-- Breadcrumb -->
      <div class="breadcrumb">
        <button class="back-btn" (click)="goBack()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="15 18 9 12 15 6"/></svg>
          Tipos de Negócio
        </button>
        @if (item()) { <span class="sep">/</span> <span class="bc-current">{{ item()!.name }}</span> }
      </div>

      @if (loading()) {
        <div class="loading-state"><div class="spin"></div></div>
      } @else if (!item()) {
        <div class="empty-state">Tipo de negócio não encontrado.</div>
      } @else {

        <!-- Header -->
        <div class="detail-header">
          <div class="detail-meta">
            <div class="detail-avatar">{{ item()!.name.charAt(0).toUpperCase() }}</div>
            <div>
              <h1 class="detail-name">{{ item()!.name }}</h1>
              @if (item()!.description) {
                <div class="detail-desc">{{ item()!.description }}</div>
              }
            </div>
          </div>
          <div class="header-actions">
            <span class="badge" [class.badge-active]="item()!.isActive" [class.badge-inactive]="!item()!.isActive">
              {{ item()!.isActive ? 'Ativo' : 'Inativo' }}
            </span>
            @if (item()!.isActive) {
              <button class="btn btn-warn" (click)="doToggle(false)" [disabled]="acting()">{{ acting() === 'toggle' ? '...' : 'Desativar' }}</button>
            } @else {
              <button class="btn btn-success" (click)="doToggle(true)" [disabled]="acting()">{{ acting() === 'toggle' ? '...' : 'Ativar' }}</button>
            }
            <button class="btn btn-danger-outline" (click)="deleteModal.set(true)" [disabled]="acting()">Excluir</button>
          </div>
        </div>

        <!-- Tabs -->
        <div class="tabs">
          <button class="tab" [class.active]="activeTab() === 'info'" (click)="activeTab.set('info')">Informações</button>
          <button class="tab" [class.active]="activeTab() === 'templates'" (click)="activeTab.set('templates')">
            Templates de Cargo
            <span class="tab-count">{{ item()!.roleTemplates.length }}</span>
          </button>
        </div>

        <!-- Tab: Informações -->
        @if (activeTab() === 'info') {
          <div class="card">
            <div class="card-head">
              <h3 class="card-title">Dados</h3>
              @if (!editing()) {
                <button class="btn-edit" (click)="startEdit()">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                  Editar
                </button>
              }
            </div>

            @if (editing()) {
              <div class="edit-form">
                <div class="field">
                  <label class="field-label">Nome <span class="required">*</span></label>
                  <input class="field-input" [(ngModel)]="editForm.name" placeholder="Nome do tipo">
                </div>
                <div class="field">
                  <label class="field-label">Descrição</label>
                  <textarea class="field-input field-textarea" [(ngModel)]="editForm.description" placeholder="Descrição opcional..." rows="3"></textarea>
                </div>
                <div class="edit-actions">
                  <button class="btn btn-primary" (click)="saveEdit()" [disabled]="saving()">{{ saving() ? 'Salvando...' : 'Salvar' }}</button>
                  <button class="btn btn-ghost" (click)="editing.set(false)">Cancelar</button>
                </div>
              </div>
            } @else {
              <dl class="dl">
                <dt>Nome</dt>        <dd>{{ item()!.name }}</dd>
                <dt>Descrição</dt>   <dd>{{ item()!.description || '—' }}</dd>
                <dt>Status</dt>      <dd>{{ item()!.isActive ? 'Ativo' : 'Inativo' }}</dd>
                <dt>Tenants</dt>     <dd>{{ item()!.tenantCount }} tenant{{ item()!.tenantCount !== 1 ? 's' : '' }} usando este tipo</dd>
                <dt>Templates</dt>   <dd>{{ item()!.roleTemplates.length }} cargo{{ item()!.roleTemplates.length !== 1 ? 's' : '' }}</dd>
              </dl>
            }
          </div>
        }

        <!-- Tab: Templates de Cargo -->
        @if (activeTab() === 'templates') {
          <div class="templates-section">

            <div class="templates-header">
              <p class="templates-hint">Os templates ativos são copiados para novos tenants deste tipo ao fazerem onboarding.</p>
              <button class="btn btn-primary btn-sm" (click)="openTemplateDrawer(null)">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                Novo Template
              </button>
            </div>

            @if (item()!.roleTemplates.length === 0) {
              <div class="empty-templates">
                <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
                <p>Nenhum template de cargo ainda.</p>
              </div>
            } @else {
              <div class="templates-list">
                @for (t of item()!.roleTemplates; track t.id) {
                  <div class="template-card" [class.template-inactive]="!t.isActive">
                    <div class="template-card-head">
                      <div class="template-info">
                        <span class="template-name">{{ t.name }}</span>
                        <span class="badge badge-sm" [class.badge-active]="t.isActive" [class.badge-inactive]="!t.isActive">
                          {{ t.isActive ? 'Ativo' : 'Inativo' }}
                        </span>
                      </div>
                      <div class="template-actions">
                        @if (t.usageCount > 0) {
                          <span class="usage-badge" title="Cargos em uso por tenants">
                            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                            {{ t.usageCount }}
                          </span>
                        }
                        <button class="btn-icon" title="Editar" (click)="openTemplateDrawer(t)">
                          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                        </button>
                        @if (t.isActive) {
                          <button class="btn-icon btn-icon--warn" title="Desativar" (click)="toggleTemplate(t, false)" [disabled]="togglingId() === t.id">
                            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="8" y1="12" x2="16" y2="12"/></svg>
                          </button>
                        } @else {
                          <button class="btn-icon btn-icon--success" title="Ativar" (click)="toggleTemplate(t, true)" [disabled]="togglingId() === t.id">
                            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="16"/><line x1="8" y1="12" x2="16" y2="12"/></svg>
                          </button>
                        }
                        <button class="btn-icon btn-icon--danger" title="Excluir" (click)="deleteTemplateModal.set(t)" [disabled]="t.usageCount > 0">
                          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
                        </button>
                      </div>
                    </div>
                    @if (t.description) {
                      <p class="template-desc">{{ t.description }}</p>
                    }
                    @if (t.permissions.length > 0) {
                      <div class="permissions-wrap">
                        @for (p of t.permissions; track p) {
                          <span class="perm-chip">{{ p }}</span>
                        }
                      </div>
                    }
                  </div>
                }
              </div>
            }
          </div>
        }
      }
    </div>

    <!-- Modal: Excluir -->
    @if (deleteModal()) {
      <div class="modal-overlay" (click)="deleteModal.set(false)">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
          </div>
          <h3 class="modal-title">Excluir tipo de negócio</h3>
          <p class="modal-body">Tem certeza que quer excluir <strong>{{ item()?.name }}</strong>?<br>Tipos com templates de cargo não podem ser excluídos.</p>
          <div class="modal-actions">
            <button class="btn btn-ghost" (click)="deleteModal.set(false)">Cancelar</button>
            <button class="btn btn-danger" (click)="doDelete()" [disabled]="acting() === 'delete'">
              {{ acting() === 'delete' ? 'Excluindo...' : 'Excluir' }}
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Modal: Excluir Template -->
    @if (deleteTemplateModal()) {
      <div class="modal-overlay" (click)="deleteTemplateModal.set(null)">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
          </div>
          <h3 class="modal-title">Excluir template</h3>
          <p class="modal-body">Tem certeza que quer excluir <strong>{{ deleteTemplateModal()!.name }}</strong>?<br>Esta ação não pode ser desfeita.</p>
          <div class="modal-actions">
            <button class="btn btn-ghost" (click)="deleteTemplateModal.set(null)">Cancelar</button>
            <button class="btn btn-danger" (click)="doDeleteTemplate(deleteTemplateModal()!)" [disabled]="acting() === 'del-template'">
              {{ acting() === 'del-template' ? 'Excluindo...' : 'Excluir' }}
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Drawer: Template -->
    @if (templateDrawerOpen()) {
      <div class="drawer-overlay" (click)="closeTemplateDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <h2 class="drawer-title">{{ editingTemplate() ? 'Editar Template' : 'Novo Template de Cargo' }}</h2>
          <button class="drawer-close" (click)="closeTemplateDrawer()">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>

        <div class="drawer-body">
          <div class="field">
            <label class="field-label">Nome <span class="required">*</span></label>
            <input class="field-input" [(ngModel)]="templateForm.name" placeholder="ex: Gerente, Garçom, Cozinheiro">
          </div>
          <div class="field">
            <label class="field-label">Descrição</label>
            <textarea class="field-input field-textarea" [(ngModel)]="templateForm.description" placeholder="Descrição do cargo..." rows="3"></textarea>
          </div>
          <div class="field">
            <label class="field-label">Permissões</label>
            <p class="field-hint">Clique para adicionar ou digite e pressione Enter</p>
            <div class="chips-wrap" (click)="focusPermInput()">
              @for (p of permChips; track p) {
                <span class="chip">
                  {{ p }}
                  <button class="chip-remove" type="button" (click)="$event.stopPropagation(); removeChip(p)">×</button>
                </span>
              }
              <input
                id="perm-input"
                class="chips-input"
                [(ngModel)]="permInputValue"
                (keydown)="onPermKey($event)"
                (blur)="addChipFromInput()"
                placeholder="{{ permChips.length === 0 ? 'Digite ou clique abaixo...' : '' }}">
            </div>
            <div class="perm-suggestions">
              @for (group of permGroups; track group.label) {
                <div class="perm-group">
                  <span class="perm-group-label">{{ group.label }}</span>
                  @for (p of group.perms; track p) {
                    <button
                      type="button"
                      class="perm-pill"
                      [class.perm-pill--active]="permChips.includes(p)"
                      (click)="togglePerm(p)">
                      {{ p }}
                    </button>
                  }
                </div>
              }
            </div>
          </div>
        </div>

        <div class="drawer-footer">
          <button class="btn btn-primary" (click)="saveTemplate()" [disabled]="savingTemplate()">
            {{ savingTemplate() ? 'Salvando...' : (editingTemplate() ? 'Salvar' : 'Criar Template') }}
          </button>
          <button class="btn btn-ghost" (click)="closeTemplateDrawer()">Cancelar</button>
        </div>
      </aside>
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
    .modal-icon {
      width: 48px; height: 48px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      background: #fef2f2; color: #b91c1c;
    }
    .modal-title { font-size: 17px; font-weight: 700; color: #111; }
    .modal-body  { font-size: 14px; color: #666; text-align: center; line-height: 1.6; strong { color: #111; } }
    .modal-actions { display: flex; gap: 10px; width: 100%; margin-top: 8px; button { flex: 1; justify-content: center; } }

    /* Tabs */
    .tabs { display: flex; border-bottom: 2px solid #f0f0f3; margin-bottom: 20px; }
    .tab {
      display: flex; align-items: center; gap: 6px;
      padding: 10px 18px; border: none; background: none;
      font-size: 13px; font-weight: 600; color: #aaa; cursor: pointer;
      border-bottom: 2px solid transparent; margin-bottom: -2px; transition: all .15s;
      &:hover { color: #555; }
      &.active { color: var(--color-brand); border-bottom-color: var(--color-brand); }
    }
    .tab-count {
      display: inline-flex; align-items: center; justify-content: center;
      min-width: 20px; height: 20px; padding: 0 6px;
      background: #f0f0f3; border-radius: 99px;
      font-size: 11px; font-weight: 700; color: #555;
    }
    .tab.active .tab-count { background: rgba(224,49,49,.12); color: var(--color-brand); }

    /* Card */
    .card { background: #fff; border: 1px solid #e8e8ec; border-radius: 14px; padding: 20px; }
    .card-head {
      display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px;
    }
    .card-title { font-size: 13px; font-weight: 700; color: #555; }
    .btn-edit {
      display: inline-flex; align-items: center; gap: 5px;
      padding: 5px 12px; border-radius: 7px; font-size: 12px; font-weight: 600;
      border: 1px solid #e8e8ec; background: #f9f9fb; color: #555; cursor: pointer;
      transition: all .15s; &:hover { background: #f0f0f3; color: #111; }
    }

    /* DL */
    .dl {
      display: grid; grid-template-columns: auto 1fr; gap: 10px 20px; align-items: baseline;
      dt { font-size: 12px; color: #aaa; font-weight: 600; white-space: nowrap; }
      dd { font-size: 13px; color: #333; }
    }

    /* Edit form */
    .edit-form { display: flex; flex-direction: column; gap: 14px; }
    .edit-actions { display: flex; gap: 8px; padding-top: 4px; }
    .field { display: flex; flex-direction: column; gap: 6px; }
    .field-label { font-size: 12px; font-weight: 600; color: #555; }
    .field-hint { font-size: 11px; color: #aaa; margin: 0; }
    .required { color: var(--color-brand); }
    .field-input {
      padding: 9px 12px; border: 1px solid #e8e8ec; border-radius: 8px;
      font-size: 13px; color: #111; outline: none; transition: border-color .15s;
      background: #fff; width: 100%; box-sizing: border-box;
      &:focus { border-color: var(--color-brand); }
    }
    .field-textarea { resize: vertical; min-height: 80px; font-family: inherit; }

    .chips-wrap {
      min-height: 44px; padding: 6px 8px;
      border: 1px solid #e8e8ec; border-radius: 8px; background: #fff;
      display: flex; flex-wrap: wrap; gap: 5px; align-items: center; cursor: text;
      transition: border-color .15s;
      &:focus-within { border-color: var(--color-brand); }
    }
    .chip {
      display: inline-flex; align-items: center; gap: 4px;
      padding: 2px 8px 2px 10px; border-radius: 5px;
      background: #f0f0f3; color: #333;
      font-size: 12px; font-weight: 600; font-family: monospace;
    }
    .chip-remove {
      display: flex; align-items: center; justify-content: center;
      width: 16px; height: 16px; border: none; background: none;
      color: #aaa; cursor: pointer; font-size: 14px; line-height: 1; padding: 0;
      border-radius: 3px;
      &:hover { background: #e0e0e4; color: #333; }
    }
    .chips-input {
      border: none; outline: none; background: transparent;
      font-size: 12px; color: #111; min-width: 120px; flex: 1;
      font-family: monospace;
    }

    .perm-suggestions {
      display: flex; flex-direction: column; gap: 8px;
      margin-top: 10px; padding: 12px;
      background: #fafafa; border: 1px solid #f0f0f3; border-radius: 8px;
    }
    .perm-group { display: flex; flex-wrap: wrap; align-items: center; gap: 5px; }
    .perm-group-label {
      font-size: 10px; font-weight: 700; text-transform: uppercase;
      letter-spacing: .05em; color: #aaa; white-space: nowrap; min-width: 72px;
    }
    .perm-pill {
      padding: 2px 9px; border-radius: 5px; border: 1px solid #e0e0e4;
      background: #fff; font-size: 11px; font-family: monospace; color: #555;
      cursor: pointer; transition: all .12s;
      &:hover { border-color: var(--color-brand); color: var(--color-brand); }
      &--active { background: var(--color-brand); color: #fff; border-color: var(--color-brand); }
    }

    /* Templates section */
    .templates-section { display: flex; flex-direction: column; gap: 16px; }
    .templates-header {
      display: flex; align-items: center; justify-content: space-between; gap: 12px; flex-wrap: wrap;
    }
    .templates-hint { font-size: 12px; color: #999; margin: 0; }

    .templates-list { display: flex; flex-direction: column; gap: 10px; }
    .template-card {
      background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; padding: 16px;
      transition: border-color .15s;
      &:hover { border-color: #d0d0d8; }
    }
    .template-inactive { background: #fafafa; opacity: .7; }
    .template-card-head { display: flex; align-items: center; justify-content: space-between; gap: 8px; margin-bottom: 6px; }
    .template-info { display: flex; align-items: center; gap: 8px; }
    .template-name { font-size: 14px; font-weight: 600; color: #111; }
    .template-desc { font-size: 12px; color: #888; margin: 0 0 10px; }
    .template-actions { display: flex; gap: 4px; }

    .permissions-wrap { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 8px; }
    .perm-chip {
      display: inline-block; padding: 2px 8px; border-radius: 5px;
      font-size: 11px; font-weight: 600; font-family: monospace;
      background: #f0f0f3; color: #555;
    }

    .empty-templates {
      display: flex; flex-direction: column; align-items: center; gap: 10px;
      padding: 60px 20px; color: #ccc;
      p { font-size: 13px; color: #999; }
    }

    /* Badges */
    .badge {
      display: inline-block; padding: 3px 10px; border-radius: 99px;
      font-size: 11px; font-weight: 700;
      &-active   { background: #dcfce7; color: #15803d; }
      &-inactive { background: #f4f4f5; color: #71717a; }
    }
    .badge-sm { padding: 2px 8px; font-size: 10px; }

    /* Icon buttons */
    .btn-icon {
      display: inline-flex; align-items: center; justify-content: center;
      width: 28px; height: 28px; border-radius: 6px; border: 1px solid #e8e8ec;
      background: #f9f9fb; color: #555; cursor: pointer; transition: all .15s;
      &:hover { background: #f0f0f3; color: #111; }
      &:disabled { opacity: .4; cursor: not-allowed; }
      &--warn    { &:hover { background: #fef3c7; color: #b45309; border-color: #fde68a; } }
      &--success { &:hover { background: #dcfce7; color: #15803d; border-color: #bbf7d0; } }
      &--danger  { &:hover { background: #fee2e2; color: #b91c1c; border-color: #fecaca; } }
    }

    .usage-badge {
      display: inline-flex; align-items: center; gap: 3px;
      padding: 2px 7px; border-radius: 5px;
      font-size: 11px; font-weight: 600; color: #6366f1;
      background: #eef2ff; border: 1px solid #c7d2fe;
    }

    /* Buttons */
    .btn {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 8px 16px; border-radius: 8px;
      font-size: 13px; font-weight: 600; cursor: pointer;
      border: 1px solid transparent; transition: all .15s;
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
    .btn-primary       { background: var(--color-brand); color: #fff; &:hover { opacity: .9; } }
    .btn-ghost         { background: #f4f4f5; color: #555; border-color: #e8e8ec; &:hover { background: #ebebef; } }
    .btn-warn          { background: #fffbeb; color: #b45309; border-color: #fde68a; &:hover { background: #fef3c7; } }
    .btn-success       { background: #f0fdf4; color: #15803d; border-color: #bbf7d0; &:hover { background: #dcfce7; } }
    .btn-danger-outline{ background: #fef2f2; color: #b91c1c; border-color: #fecaca; &:hover { background: #fee2e2; } }
    .btn-danger        { background: #dc2626; color: #fff; &:hover { background: #b91c1c; } }
    .btn-sm            { padding: 6px 14px; font-size: 12px; }

    /* Loading */
    .loading-state { display: flex; justify-content: center; padding: 60px; }
    .spin {
      width: 32px; height: 32px; border-radius: 50%;
      border: 3px solid #f0f0f3; border-top-color: var(--color-brand);
      animation: spin .7s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .empty-state { text-align: center; padding: 60px; color: #ccc; }

    /* Drawer */
    .drawer-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,.35); z-index: 100;
      backdrop-filter: blur(2px); animation: fadeIn .15s ease;
    }
    .drawer {
      position: fixed; top: 0; right: 0; bottom: 0; width: 440px;
      background: #fff; z-index: 101; display: flex; flex-direction: column;
      box-shadow: -8px 0 40px rgba(0,0,0,.12);
      animation: slideIn .2s ease;
    }
    @keyframes fadeIn  { from { opacity: 0; } to { opacity: 1; } }
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }

    .drawer-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 20px 24px; border-bottom: 1px solid #f0f0f3;
    }
    .drawer-title { font-size: 16px; font-weight: 700; color: #111; }
    .drawer-close {
      display: flex; align-items: center; justify-content: center;
      width: 32px; height: 32px; border: none; background: #f4f4f5;
      border-radius: 8px; cursor: pointer; color: #666;
      transition: all .15s; &:hover { background: #ebebef; color: #111; }
    }
    .drawer-body   { flex: 1; overflow-y: auto; padding: 24px; display: flex; flex-direction: column; gap: 16px; }
    .drawer-footer { padding: 16px 24px; border-top: 1px solid #f0f0f3; display: flex; gap: 8px; }

    @media (max-width: 768px) {
      .page { padding: 14px; max-width: 100%; }
      .drawer { width: 100%; }
    }
  `],
})
export class BusinessTypeDetailComponent implements OnInit {
  private route  = inject(ActivatedRoute);
  private router = inject(Router);
  private svc    = inject(BusinessTypeService);
  private toast  = inject(ToastService);

  item      = signal<BusinessTypeDto | null>(null);
  loading   = signal(true);
  activeTab = signal<Tab>('info');

  acting    = signal('');
  deleteModal = signal(false);
  deleteTemplateModal = signal<RoleTemplateDto | null>(null);

  editing = signal(false);
  saving  = signal(false);
  editForm = { name: '', description: '' };

  templateDrawerOpen = signal(false);
  editingTemplate    = signal<RoleTemplateDto | null>(null);
  savingTemplate     = signal(false);
  togglingId         = signal('');
  templateForm: TemplateForm = emptyTemplateForm();
  permChips: string[] = [];
  permInputValue = '';

  readonly permGroups = [
    { label: 'Pedidos',     perms: ['orders.read','orders.create','orders.update_status','orders.cancel','orders.cancel_after_sent','orders.cancel_in_preparation','orders.cancel_delivered'] },
    { label: 'Comandas',    perms: ['tabs.read','tabs.create','tabs.close','tabs.cancel','tabs.apply_discount','tabs.apply_discount_override','tabs.apply_courtesy'] },
    { label: 'Cardápio',    perms: ['menu.read','menu.manage'] },
    { label: 'Mesas',       perms: ['tables.read','tables.manage'] },
    { label: 'Estoque',     perms: ['stock.read','stock.manage'] },
    { label: 'Funcionários',perms: ['employees.read','employees.manage'] },
    { label: 'Funções',     perms: ['roles.read','roles.manage'] },
    { label: 'Caixa',       perms: ['cash.manage'] },
    { label: 'Reservas',    perms: ['reservations.read','reservations.manage'] },
    { label: 'Relatórios',  perms: ['reports.read'] },
    { label: 'Config.',     perms: ['settings.manage'] },
  ];

  private get id() { return this.route.snapshot.paramMap.get('id')!; }

  ngOnInit() { this.load(); }

  private load() {
    this.loading.set(true);
    this.svc.getAll().subscribe({
      next: items => {
        this.item.set(items.find(i => i.id === this.id) ?? null);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.toast.error('Erro ao carregar tipo de negócio.'); },
    });
  }

  goBack() { this.router.navigate(['/admin/business-types']); }

  doToggle(activate: boolean) {
    this.acting.set('toggle');
    const call = activate ? this.svc.activate(this.id) : this.svc.deactivate(this.id);
    call.subscribe({
      next: () => {
        this.acting.set('');
        this.toast.success(activate ? 'Tipo ativado com sucesso.' : 'Tipo desativado com sucesso.');
        this.load();
      },
      error: (err: any) => { this.acting.set(''); this.toast.error(err?.error?.message ?? 'Erro ao alterar status.'); },
    });
  }

  doDelete() {
    this.acting.set('delete');
    this.svc.delete(this.id).subscribe({
      next: () => {
        this.acting.set('');
        this.deleteModal.set(false);
        this.toast.success('Tipo de negócio excluído.');
        this.router.navigate(['/admin/business-types']);
      },
      error: (err: any) => { this.acting.set(''); this.toast.error(err?.error?.message ?? 'Erro ao excluir.'); },
    });
  }

  startEdit() {
    const i = this.item()!;
    this.editForm = { name: i.name, description: i.description ?? '' };
    this.editing.set(true);
  }

  saveEdit() {
    if (!this.editForm.name.trim()) { this.toast.error('O nome é obrigatório.'); return; }
    this.saving.set(true);
    this.svc.update(this.id, { name: this.editForm.name, description: this.editForm.description || null }).subscribe({
      next: () => {
        this.saving.set(false);
        this.editing.set(false);
        this.toast.success('Dados atualizados com sucesso!');
        this.load();
      },
      error: (err: any) => { this.saving.set(false); this.toast.error(err?.error?.message ?? 'Erro ao salvar.'); },
    });
  }

  openTemplateDrawer(t: RoleTemplateDto | null) {
    this.editingTemplate.set(t);
    this.templateForm = t ? { name: t.name, description: t.description ?? '' } : emptyTemplateForm();
    this.permChips = t ? [...t.permissions] : [];
    this.permInputValue = '';
    this.templateDrawerOpen.set(true);
  }

  closeTemplateDrawer() {
    this.templateDrawerOpen.set(false);
    this.editingTemplate.set(null);
    this.templateForm = emptyTemplateForm();
    this.permChips = [];
    this.permInputValue = '';
  }

  focusPermInput() { document.getElementById('perm-input')?.focus(); }

  togglePerm(p: string) {
    if (this.permChips.includes(p)) this.permChips = this.permChips.filter(c => c !== p);
    else this.permChips.push(p);
  }

  onPermKey(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === ',') {
      event.preventDefault();
      this.addChipFromInput();
    } else if (event.key === 'Backspace' && !this.permInputValue && this.permChips.length > 0) {
      this.permChips.pop();
    }
  }

  addChipFromInput() {
    const val = this.permInputValue.replace(/,/g, '').trim();
    if (val && !this.permChips.includes(val)) this.permChips.push(val);
    this.permInputValue = '';
  }

  removeChip(p: string) {
    this.permChips = this.permChips.filter(c => c !== p);
  }

  saveTemplate() {
    if (!this.templateForm.name.trim()) { this.toast.error('O nome é obrigatório.'); return; }
    this.addChipFromInput();
    const req = {
      name: this.templateForm.name,
      description: this.templateForm.description || null,
      permissions: this.permChips,
    };

    this.savingTemplate.set(true);
    const editing = this.editingTemplate();
    const call: import('rxjs').Observable<unknown> = editing
      ? this.svc.updateTemplate(this.id, editing.id, req)
      : this.svc.addTemplate(this.id, req);

    call.subscribe({
      next: () => {
        this.savingTemplate.set(false);
        this.closeTemplateDrawer();
        this.toast.success(editing ? 'Template atualizado!' : 'Template criado!');
        this.load();
      },
      error: (err: any) => { this.savingTemplate.set(false); this.toast.error(err?.error?.message ?? 'Erro ao salvar template.'); },
    });
  }

  doDeleteTemplate(t: RoleTemplateDto) {
    this.acting.set('del-template');
    this.svc.deleteTemplate(this.id, t.id).subscribe({
      next: () => {
        this.acting.set('');
        this.deleteTemplateModal.set(null);
        this.toast.success('Template excluído.');
        this.load();
      },
      error: (err: any) => {
        this.acting.set('');
        this.toast.error(err?.error?.message ?? 'Erro ao excluir template.');
      },
    });
  }

  toggleTemplate(t: RoleTemplateDto, activate: boolean) {
    this.togglingId.set(t.id);
    const call = activate
      ? this.svc.activateTemplate(this.id, t.id)
      : this.svc.deactivateTemplate(this.id, t.id);

    call.subscribe({
      next: () => {
        this.togglingId.set('');
        this.toast.success(activate ? 'Template ativado.' : 'Template desativado.');
        this.load();
      },
      error: (err: any) => { this.togglingId.set(''); this.toast.error(err?.error?.message ?? 'Erro ao alterar template.'); },
    });
  }
}
