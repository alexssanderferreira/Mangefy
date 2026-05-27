import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ToastService } from '../../../core/toast/toast.service';
import { SupplierService, PlatformSupplierDto, SupplierCategoryDto } from './supplier.service';

type Tab = 'suppliers' | 'categories';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Fornecedores</h1>
          <p class="page-subtitle">Catálogo global de fornecedores da plataforma</p>
        </div>
        <button class="btn-primary" (click)="openCreate()">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          @if (tab() === 'suppliers') { Novo Fornecedor } @else { Nova Categoria }
        </button>
      </div>

      <!-- Tabs -->
      <div class="tabs">
        <button class="tab-btn" [class.active]="tab() === 'suppliers'" (click)="tab.set('suppliers')">
          Fornecedores
          <span class="tab-count">{{ suppliers().length }}</span>
        </button>
        <button class="tab-btn" [class.active]="tab() === 'categories'" (click)="tab.set('categories')">
          Categorias
          <span class="tab-count">{{ categories().length }}</span>
        </button>
      </div>

      <!-- Loading -->
      @if (loading()) {
        <div class="skel-rows">@for (i of [1,2,3,4,5]; track i) { <div class="skel-row"></div> }</div>
      }

      <!-- Suppliers tab -->
      @if (!loading() && tab() === 'suppliers') {
        @if (suppliers().length === 0) {
          <div class="empty-state">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 1 0 0 4 2 2 0 0 0 0-4zm-8 2a2 2 0 1 0 0 4 2 2 0 0 0 0-4z"/></svg>
            <p>Nenhum fornecedor cadastrado</p>
            <button class="btn-primary" (click)="openCreate()">Cadastrar fornecedor</button>
          </div>
        } @else {
          <!-- Filter bar -->
          <div class="filter-bar">
            <div class="search-wrap">
              <svg class="search-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
              <input class="search-input" type="text" placeholder="Buscar fornecedor..." [value]="search()" (input)="search.set($any($event.target).value)" />
            </div>
            <select class="filter-select" [value]="filterCat()" (change)="filterCat.set($any($event.target).value)">
              <option value="">Todas as categorias</option>
              @for (c of categories(); track c.id) {
                <option [value]="c.id">{{ c.name }}</option>
              }
            </select>
            <select class="filter-select" [value]="filterStatus()" (change)="filterStatus.set($any($event.target).value)">
              <option value="">Todos os status</option>
              <option value="active">Ativos</option>
              <option value="inactive">Inativos</option>
            </select>
          </div>

          <div class="table-wrap">
            <table class="data-table">
              <thead><tr>
                <th>Fornecedor</th>
                <th>Categoria</th>
                <th>CNPJ</th>
                <th>Contato</th>
                <th>Status</th>
                <th></th>
              </tr></thead>
              <tbody>
                @for (s of filteredSuppliers(); track s.id) {
                  <tr class="clickable-row" (click)="goToDetail(s.id)">
                    <td>
                      <div class="name-cell">
                        <div class="avatar">{{ s.name[0] }}</div>
                        <div>
                          <div class="fw6">{{ s.name }}</div>
                          @if (s.website) { <div class="sub">{{ s.website }}</div> }
                        </div>
                      </div>
                    </td>
                    <td class="muted">{{ categoryName(s.supplierCategoryId) }}</td>
                    <td class="muted mono">{{ s.cnpj || '—' }}</td>
                    <td>
                      <div class="contact-cell">
                        @if (s.email) { <span class="sub">{{ s.email }}</span> }
                        @if (s.phone) { <span class="sub">{{ s.phone }}</span> }
                        @if (!s.email && !s.phone) { <span class="muted">—</span> }
                      </div>
                    </td>
                    <td>
                      <span class="badge" [class.badge-active]="s.isActive" [class.badge-inactive]="!s.isActive">
                        {{ s.isActive ? 'Ativo' : 'Inativo' }}
                      </span>
                    </td>
                    <td class="action-col" (click)="$event.stopPropagation()">
                      <div class="row-actions">
                        <button class="btn-icon" title="Editar" (click)="openEdit(s)">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                        </button>
                        <button class="btn-icon" [title]="s.isActive ? 'Desativar' : 'Ativar'" (click)="toggleSupplier(s)">
                          @if (s.isActive) {
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/></svg>
                          } @else {
                            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
                          }
                        </button>
                        <button class="btn-icon btn-icon--danger" title="Excluir" (click)="deleteModal.set(s)">
                          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4h6v2"/></svg>
                        </button>
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      }

      <!-- Categories tab -->
      @if (!loading() && tab() === 'categories') {
        @if (categories().length === 0) {
          <div class="empty-state">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>
            <p>Nenhuma categoria cadastrada</p>
            <button class="btn-primary" (click)="openCreate()">Criar categoria</button>
          </div>
        } @else {
          <div class="cards-grid">
            @for (c of categories(); track c.id) {
              <div class="cat-card" [class.cat-inactive]="!c.isActive">
                <div class="cat-header">
                  <div class="cat-avatar">{{ c.name[0] }}</div>
                  <span class="badge" [class.badge-active]="c.isActive" [class.badge-inactive]="!c.isActive">
                    {{ c.isActive ? 'Ativa' : 'Inativa' }}
                  </span>
                </div>
                <div class="cat-name">{{ c.name }}</div>
                @if (c.description) { <div class="cat-desc">{{ c.description }}</div> }
                <div class="cat-meta">
                  {{ supplierCountByCategory(c.id) }} fornecedor{{ supplierCountByCategory(c.id) !== 1 ? 'es' : '' }}
                </div>
                <div class="cat-actions">
                  <button class="btn-icon" (click)="openEditCategory(c)">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                  </button>
                  <button class="btn-icon" [title]="c.isActive ? 'Desativar' : 'Ativar'" (click)="toggleCategory(c)">
                    @if (c.isActive) {
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/></svg>
                    } @else {
                      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
                    }
                  </button>
                  <button class="btn-icon btn-icon--danger" (click)="deleteCatModal.set(c)" [disabled]="supplierCountByCategory(c.id) > 0" [title]="supplierCountByCategory(c.id) > 0 ? 'Categoria em uso' : 'Excluir'">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4h6v2"/></svg>
                  </button>
                </div>
              </div>
            }
          </div>
        }
      }

    </div>

    <!-- ── Drawer Fornecedor ──────────────────────────────────────────────── -->
    @if (drawer()) {
      <div class="overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 1 0 0 4 2 2 0 0 0 0-4zm-8 2a2 2 0 1 0 0 4 2 2 0 0 0 0-4z"/></svg>
            </div>
            <div>
              <h2 class="drawer-title">{{ editingSupplier() ? 'Editar Fornecedor' : 'Novo Fornecedor' }}</h2>
              <p class="drawer-subtitle">{{ editingSupplier() ? editingSupplier()!.name : 'Cadastre um fornecedor global' }}</p>
            </div>
          </div>
          <button class="btn-close" (click)="closeDrawer()">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>
        <form class="drawer-body" [formGroup]="supplierForm" (ngSubmit)="saveSupplier()">

          <div class="form-section">
            <div class="form-section-header">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
              Identificação
            </div>
            <div class="field">
              <label class="field-label">Nome <span class="req">*</span></label>
              <input class="field-ctrl" formControlName="name" placeholder="Nome do fornecedor" />
            </div>
            <div class="field">
              <label class="field-label">Categoria <span class="req">*</span></label>
              <select class="field-ctrl field-select" formControlName="supplierCategoryId">
                <option value="">Selecionar categoria...</option>
                @for (c of activeCategories(); track c.id) {
                  <option [value]="c.id">{{ c.name }}</option>
                }
              </select>
            </div>
          </div>

          <div class="form-section">
            <div class="form-section-header">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07A19.5 19.5 0 0 1 4.69 12a19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 3.6 1.18h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L7.91 8.73a16 16 0 0 0 6.29 6.29l.91-.91a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
              Contato
            </div>
            <div class="field-row">
              <div class="field">
                <label class="field-label">CNPJ</label>
                <input class="field-ctrl" formControlName="cnpj" placeholder="00.000.000/0001-00" />
              </div>
              <div class="field">
                <label class="field-label">Telefone</label>
                <input class="field-ctrl" formControlName="phone" placeholder="+55 11 99999-9999" />
              </div>
            </div>
            <div class="field">
              <label class="field-label">E-mail</label>
              <input class="field-ctrl" formControlName="email" type="email" placeholder="contato@fornecedor.com" />
            </div>
            <div class="field">
              <label class="field-label">Website</label>
              <input class="field-ctrl" formControlName="website" placeholder="https://..." />
            </div>
          </div>

          <div class="form-section">
            <div class="form-section-header">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
              Endereço
            </div>
            <div class="field-row-cep">
              <div class="field">
                <label class="field-label">CEP</label>
                <div class="input-wrap">
                  <input class="field-ctrl" formControlName="cep" placeholder="00000-000" maxlength="9"
                    (blur)="lookupCep()" (keydown.enter)="$event.preventDefault(); lookupCep()" />
                  @if (cepLoading()) {
                    <svg class="input-spin" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 12a9 9 0 1 1-6.219-8.56"/></svg>
                  }
                </div>
              </div>
              <div class="field field-uf">
                <label class="field-label">UF</label>
                <input class="field-ctrl" formControlName="uf" placeholder="SP" maxlength="2" style="text-transform:uppercase" />
              </div>
            </div>
            <div class="field">
              <label class="field-label">Logradouro</label>
              <input class="field-ctrl" formControlName="logradouro" placeholder="Rua, Av..." />
            </div>
            <div class="field-row">
              <div class="field">
                <label class="field-label">Número</label>
                <input class="field-ctrl" formControlName="numero" placeholder="123" />
              </div>
              <div class="field">
                <label class="field-label">Complemento</label>
                <input class="field-ctrl" formControlName="complemento" placeholder="Apto, Sala..." />
              </div>
            </div>
            <div class="field-row">
              <div class="field">
                <label class="field-label">Bairro</label>
                <input class="field-ctrl" formControlName="bairro" placeholder="Bairro" />
              </div>
              <div class="field">
                <label class="field-label">Cidade</label>
                <input class="field-ctrl" formControlName="cidade" placeholder="Cidade" />
              </div>
            </div>
          </div>

          <div class="form-section">
            <div class="form-section-header">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
              Horário de Funcionamento
            </div>
            <div class="field">
              <label class="field-label">Horário</label>
              <input class="field-ctrl" formControlName="businessHours" placeholder="Seg–Sex 08h–18h, Sáb 08h–12h" />
            </div>
          </div>

          <div class="form-section">
            <div class="form-section-header">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>
              Observações
            </div>
            <div class="field">
              <label class="field-label">Descrição</label>
              <textarea class="field-ctrl field-textarea" formControlName="description" rows="3" placeholder="Informações adicionais..."></textarea>
            </div>
          </div>

          <div class="drawer-footer">
            <button type="button" class="btn-ghost" (click)="closeDrawer()">Cancelar</button>
            <button type="submit" class="btn-primary" [disabled]="supplierForm.invalid || saving()">
              @if (saving()) {
                <svg class="spin-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 12a9 9 0 1 1-6.219-8.56"/></svg>
                Salvando...
              } @else { Salvar }
            </button>
          </div>
        </form>
      </aside>
    }

    <!-- ── Drawer Categoria ───────────────────────────────────────────────── -->
    @if (catDrawer()) {
      <div class="overlay" (click)="closeCatDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>
            </div>
            <div>
              <h2 class="drawer-title">{{ editingCategory() ? 'Editar Categoria' : 'Nova Categoria' }}</h2>
              <p class="drawer-subtitle">{{ editingCategory() ? editingCategory()!.name : 'Agrupe fornecedores por categoria' }}</p>
            </div>
          </div>
          <button class="btn-close" (click)="closeCatDrawer()">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>
        <form class="drawer-body" [formGroup]="catForm" (ngSubmit)="saveCategory()">
          <div class="form-section">
            <div class="form-section-header">
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>
              Dados
            </div>
            <div class="field">
              <label class="field-label">Nome <span class="req">*</span></label>
              <input class="field-ctrl" formControlName="name" placeholder="Ex: Laticínios, Bebidas..." />
            </div>
            <div class="field">
              <label class="field-label">Descrição</label>
              <textarea class="field-ctrl field-textarea" formControlName="description" rows="3" placeholder="Descrição opcional..."></textarea>
            </div>
          </div>
          <div class="drawer-footer">
            <button type="button" class="btn-ghost" (click)="closeCatDrawer()">Cancelar</button>
            <button type="submit" class="btn-primary" [disabled]="catForm.invalid || saving()">
              @if (saving()) {
                <svg class="spin-icon" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 12a9 9 0 1 1-6.219-8.56"/></svg>
                Salvando...
              } @else { Salvar }
            </button>
          </div>
        </form>
      </aside>
    }

    <!-- ── Modal excluir fornecedor ──────────────────────────────────────── -->
    @if (deleteModal()) {
      <div class="modal-backdrop" (click)="deleteModal.set(null)">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>Excluir fornecedor?</h3>
          <p>O fornecedor <strong>{{ deleteModal()!.name }}</strong> será removido permanentemente.</p>
          <div class="modal-actions">
            <button class="btn-ghost" (click)="deleteModal.set(null)">Cancelar</button>
            <button class="btn-danger" (click)="doDeleteSupplier()" [disabled]="saving()">
              @if (saving()) { Excluindo... } @else { Excluir }
            </button>
          </div>
        </div>
      </div>
    }

    <!-- ── Modal excluir categoria ───────────────────────────────────────── -->
    @if (deleteCatModal()) {
      <div class="modal-backdrop" (click)="deleteCatModal.set(null)">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>Excluir categoria?</h3>
          <p>A categoria <strong>{{ deleteCatModal()!.name }}</strong> será removida permanentemente.</p>
          <div class="modal-actions">
            <button class="btn-ghost" (click)="deleteCatModal.set(null)">Cancelar</button>
            <button class="btn-danger" (click)="doDeleteCategory()" [disabled]="saving()">
              @if (saving()) { Excluindo... } @else { Excluir }
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .page { padding: 24px 28px; }

    .page-header { display: flex; align-items: flex-start; justify-content: space-between; margin-bottom: 20px; gap: 16px; }
    .page-title  { font-size: 22px; font-weight: 700; color: #111; }
    .page-subtitle { font-size: 12px; color: #aaa; margin-top: 2px; }

    /* Tabs */
    .tabs { display: flex; gap: 4px; border-bottom: 2px solid #f0f0f3; margin-bottom: 20px; }
    .tab-btn {
      display: flex; align-items: center; gap: 6px;
      padding: 10px 16px; background: none; border: none;
      font-size: 13px; font-weight: 600; color: #888; cursor: pointer;
      border-bottom: 2px solid transparent; margin-bottom: -2px; transition: all .15s;
      &:hover { color: #333; }
      &.active { color: var(--color-brand); border-bottom-color: var(--color-brand); }
    }
    .tab-count {
      font-size: 11px; font-weight: 700; padding: 1px 6px;
      border-radius: 99px; background: #f0f0f3; color: #777;
    }
    .tab-btn.active .tab-count { background: color-mix(in srgb, var(--color-brand) 12%, transparent); color: var(--color-brand); }

    /* Filter bar */
    .filter-bar { display: flex; gap: 10px; margin-bottom: 14px; flex-wrap: wrap; }
    .search-wrap { position: relative; flex: 1; min-width: 200px; }
    .search-icon { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: #bbb; pointer-events: none; }
    .search-input {
      width: 100%; padding: 8px 10px 8px 32px;
      border: 1.5px solid #e8e8ec; border-radius: 8px;
      font-size: 13px; font-family: inherit; outline: none;
      &:focus { border-color: var(--color-brand); }
    }
    .filter-select {
      padding: 8px 12px; border: 1.5px solid #e8e8ec; border-radius: 8px;
      font-size: 13px; font-family: inherit; outline: none; background: #fff;
      &:focus { border-color: var(--color-brand); }
    }

    /* Table */
    .table-wrap { background: #fff; border: 1px solid #e8e8ec; border-radius: 12px; overflow: hidden; }
    .data-table { width: 100%; border-collapse: collapse; }
    .data-table th {
      padding: 10px 14px; text-align: left;
      font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .05em;
      color: #bbb; border-bottom: 1px solid #f0f0f3; background: #fafafa;
    }
    .data-table td { padding: 12px 14px; font-size: 13px; border-bottom: 1px solid #f7f7f9; }
    .data-table tbody tr:last-child td { border-bottom: none; }
    .data-table tbody tr:hover td { background: #fafafa; }
    .clickable-row { cursor: pointer; }

    .name-cell { display: flex; align-items: center; gap: 10px; }
    .avatar {
      width: 32px; height: 32px; border-radius: 8px; flex-shrink: 0;
      background: var(--color-brand); color: #fff; font-size: 13px; font-weight: 700;
      display: flex; align-items: center; justify-content: center;
    }
    .fw6     { font-weight: 600; color: #111; }
    .sub     { font-size: 11px; color: #aaa; }
    .muted   { color: #aaa; }
    .mono    { font-family: monospace; font-size: 12px; }
    .contact-cell { display: flex; flex-direction: column; gap: 2px; }
    .action-col { width: 120px; }
    .row-actions { display: flex; gap: 4px; justify-content: flex-end; }

    /* Badges */
    .badge { display: inline-flex; padding: 2px 8px; border-radius: 99px; font-size: 11px; font-weight: 700; }
    .badge-active   { background: #dcfce7; color: #15803d; }
    .badge-inactive { background: #f4f4f5; color: #71717a; }

    /* Categories grid */
    .cards-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 14px; }
    .cat-card {
      background: #fff; border: 1px solid #e8e8ec; border-radius: 14px; padding: 18px;
      transition: box-shadow .15s;
      &:hover { box-shadow: 0 4px 20px rgba(0,0,0,.07); }
      &.cat-inactive { opacity: .6; }
    }
    .cat-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
    .cat-avatar {
      width: 36px; height: 36px; border-radius: 10px;
      background: var(--color-brand); color: #fff; font-size: 16px; font-weight: 700;
      display: flex; align-items: center; justify-content: center;
    }
    .cat-name  { font-size: 14px; font-weight: 700; color: #111; margin-bottom: 4px; }
    .cat-desc  { font-size: 12px; color: #888; margin-bottom: 8px; line-height: 1.4; }
    .cat-meta  { font-size: 11px; color: #bbb; margin-bottom: 12px; }
    .cat-actions { display: flex; gap: 6px; justify-content: flex-end; }

    /* Buttons */
    .btn-primary {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 9px 16px; background: #0a0a0a; color: #fff;
      border: none; border-radius: 8px; font-size: 13px; font-weight: 600;
      font-family: inherit; cursor: pointer; transition: background .15s;
      &:hover { background: #222; }
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
    .btn-ghost {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 9px 16px; background: #fff; color: #555;
      border: 1.5px solid #e8e8ec; border-radius: 8px; font-size: 13px; font-weight: 600;
      font-family: inherit; cursor: pointer; transition: all .15s;
      &:hover { border-color: #bbb; }
    }
    .btn-danger {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 9px 16px; background: #dc2626; color: #fff;
      border: none; border-radius: 8px; font-size: 13px; font-weight: 600;
      font-family: inherit; cursor: pointer;
      &:disabled { opacity: .5; cursor: not-allowed; }
    }
    .btn-icon {
      display: inline-flex; align-items: center; justify-content: center;
      width: 28px; height: 28px; border-radius: 6px;
      border: none; background: none; color: #aaa; cursor: pointer;
      transition: background .1s, color .1s;
      &:hover { background: #f0f0f3; color: #555; }
      &.btn-icon--danger:hover { background: #fef2f2; color: #dc2626; }
      &:disabled { opacity: .4; cursor: not-allowed; }
    }
    .btn-close {
      display: flex; align-items: center; justify-content: center;
      width: 30px; height: 30px; border-radius: 7px;
      border: none; background: none; color: #aaa; cursor: pointer;
      &:hover { background: #f0f0f3; color: #555; }
    }

    /* Drawer */
    .overlay { position: fixed; inset: 0; background: rgba(0,0,0,.3); z-index: 40; backdrop-filter: blur(2px); }
    .drawer {
      position: fixed; top: 0; right: 0; bottom: 0; width: 440px;
      background: #fff; z-index: 50; display: flex; flex-direction: column;
      box-shadow: -8px 0 32px rgba(0,0,0,.12);
      animation: slideIn .2s ease;
      @media (max-width: 480px) { width: 100%; }
    }
    @keyframes slideIn { from { transform: translateX(100%); } to { transform: translateX(0); } }
    .drawer-header { display: flex; align-items: center; justify-content: space-between; padding: 18px 20px; border-bottom: 1px solid #f0f0f3; }
    .drawer-header-left { display: flex; align-items: center; gap: 12px; }
    .drawer-icon { width: 36px; height: 36px; border-radius: 10px; background: color-mix(in srgb, var(--color-brand) 10%, transparent); color: var(--color-brand); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .drawer-title { font-size: 15px; font-weight: 700; color: #111; }
    .drawer-subtitle { font-size: 11px; color: #aaa; margin-top: 1px; }
    .btn-close { display: flex; align-items: center; justify-content: center; width: 30px; height: 30px; border: 1px solid #e8e8ec; background: #fff; border-radius: 7px; cursor: pointer; color: #888; transition: all .15s; &:hover { background: #f4f4f5; color: #333; } }
    .drawer-body { flex: 1; overflow-y: auto; padding: 16px 20px; display: flex; flex-direction: column; gap: 12px; }
    .drawer-footer { display: flex; gap: 8px; justify-content: flex-end; padding: 14px 20px; border-top: 1px solid #f0f0f3; background: #fafafa; flex-shrink: 0; }

    /* Form sections */
    .form-section { display: flex; flex-direction: column; gap: 10px; padding: 14px; background: #fafafa; border: 1px solid #f0f0f3; border-radius: 10px; }
    .form-section-header { display: flex; align-items: center; gap: 6px; font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #999; svg { color: var(--color-brand); } }

    .field { display: flex; flex-direction: column; gap: 5px; }
    .field-row { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
    .field-label { font-size: 11px; font-weight: 600; color: #666; }
    .req { color: var(--color-brand); }
    .field-ctrl {
      width: 100%; padding: 8px 10px; border: 1px solid #e8e8ec; border-radius: 7px;
      font-size: 13px; font-family: inherit; color: #111; outline: none;
      transition: border-color .15s, box-shadow .15s; background: #fff; box-sizing: border-box;
      &:focus { border-color: var(--color-brand); box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-brand) 12%, transparent); }
      &::placeholder { color: #ccc; }
    }
    .field-select { appearance: none; background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='11' height='11' viewBox='0 0 24 24' fill='none' stroke='%23aaa' stroke-width='2.5'%3E%3Cpolyline points='6 9 12 15 18 9'/%3E%3C/svg%3E"); background-repeat: no-repeat; background-position: right 8px center; padding-right: 26px; cursor: pointer; }
    .field-textarea { resize: vertical; min-height: 80px; }
    .field-row-cep { display: grid; grid-template-columns: 1fr 64px; gap: 10px; }
    .input-wrap { position: relative; }
    .input-wrap .field-ctrl { padding-right: 32px; }
    .input-spin { position: absolute; right: 9px; top: 50%; transform: translateY(-50%); color: #aaa; animation: spinAnim .8s linear infinite; pointer-events: none; }
    @keyframes spinAnim { to { transform: rotate(360deg); } }
    .spin-icon { animation: spinAnim .8s linear infinite; transform-origin: center; }

    /* Modal */
    .modal-backdrop {
      position: fixed; inset: 0; background: rgba(0,0,0,.4); z-index: 60;
      display: flex; align-items: center; justify-content: center; padding: 16px;
    }
    .modal {
      background: #fff; border-radius: 14px; padding: 28px;
      max-width: 400px; width: 100%; box-shadow: 0 20px 60px rgba(0,0,0,.2);
      h3 { font-size: 16px; font-weight: 700; margin-bottom: 8px; }
      p  { font-size: 13px; color: #666; line-height: 1.5; }
    }
    .modal-actions { display: flex; gap: 8px; justify-content: flex-end; margin-top: 20px; }

    /* Skeleton */
    .skel-rows { display: flex; flex-direction: column; gap: 8px; }
    .skel-row  { height: 52px; background: #f5f5f7; border-radius: 10px; animation: pulse 1.4s infinite; }
    @keyframes pulse { 0%,100% { opacity:1; } 50% { opacity:.5; } }

    /* Empty */
    .empty-state {
      display: flex; flex-direction: column; align-items: center; gap: 12px;
      padding: 60px 0; color: #ccc;
      p { font-size: 14px; color: #aaa; }
    }

    @media (max-width: 768px) {
      .page { padding: 12px; }
      .filter-bar { flex-direction: column; }
      .data-table th:nth-child(3), .data-table td:nth-child(3),
      .data-table th:nth-child(4), .data-table td:nth-child(4) { display: none; }
    }
  `]
})
export class SuppliersComponent implements OnInit {
  private svc    = inject(SupplierService);
  private fb     = inject(FormBuilder);
  private toast  = inject(ToastService);
  private http   = inject(HttpClient);
  private route  = inject(ActivatedRoute);
  private router = inject(Router);

  tab          = signal<Tab>('suppliers');
  loading      = signal(true);
  saving       = signal(false);
  suppliers    = signal<PlatformSupplierDto[]>([]);
  categories   = signal<SupplierCategoryDto[]>([]);

  search       = signal('');
  filterCat    = signal('');
  filterStatus = signal('');

  cepLoading      = signal(false);
  drawer          = signal(false);
  catDrawer       = signal(false);
  editingSupplier = signal<PlatformSupplierDto | null>(null);
  editingCategory = signal<SupplierCategoryDto | null>(null);
  deleteModal     = signal<PlatformSupplierDto | null>(null);
  deleteCatModal  = signal<SupplierCategoryDto | null>(null);

  activeCategories = computed(() => this.categories().filter(c => c.isActive));

  filteredSuppliers = computed(() => {
    let list = this.suppliers();
    const s = this.search().toLowerCase();
    const cat = this.filterCat();
    const status = this.filterStatus();
    if (s)      list = list.filter(x => x.name.toLowerCase().includes(s) || (x.email ?? '').toLowerCase().includes(s));
    if (cat)    list = list.filter(x => x.supplierCategoryId === cat);
    if (status === 'active')   list = list.filter(x => x.isActive);
    if (status === 'inactive') list = list.filter(x => !x.isActive);
    return list;
  });

  supplierForm = this.fb.group({
    name:               ['', Validators.required],
    supplierCategoryId: ['', Validators.required],
    cnpj:               [''],
    phone:              [''],
    email:              [''],
    website:            [''],
    description:        [''],
    cep:                [''],
    logradouro:         [''],
    numero:             [''],
    complemento:        [''],
    bairro:             [''],
    cidade:             [''],
    uf:                 [''],
    businessHours:      [''],
  });

  catForm = this.fb.group({
    name:        ['', Validators.required],
    description: [''],
  });

  ngOnInit() {
    this.load();
    const t = this.route.snapshot.queryParamMap.get('tab');
    if (t === 'categories') this.tab.set('categories');
  }

  private load() {
    this.loading.set(true);
    forkJoin({ suppliers: this.svc.getSuppliers(), categories: this.svc.getCategories() }).subscribe({
      next: ({ suppliers, categories }) => {
        this.suppliers.set(suppliers);
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: () => { this.toast.error('Erro ao carregar dados.'); this.loading.set(false); }
    });
  }

  categoryName(id: string) {
    return this.categories().find(c => c.id === id)?.name ?? '—';
  }

  supplierCountByCategory(catId: string) {
    return this.suppliers().filter(s => s.supplierCategoryId === catId).length;
  }

  goToDetail(id: string) { this.router.navigate(['/admin/suppliers', id]); }

  // ─── Supplier CRUD ──────────────────────────────────────────────────────────

  openCreate() {
    if (this.tab() === 'suppliers') {
      this.editingSupplier.set(null);
      this.supplierForm.reset();
      this.drawer.set(true);
    } else {
      this.editingCategory.set(null);
      this.catForm.reset();
      this.catDrawer.set(true);
    }
  }

  openEdit(s: PlatformSupplierDto) {
    this.editingSupplier.set(s);
    this.supplierForm.patchValue({
      name: s.name, supplierCategoryId: s.supplierCategoryId,
      cnpj: s.cnpj ?? '', phone: s.phone ?? '', email: s.email ?? '',
      website: s.website ?? '', description: s.description ?? '',
      cep: s.addressCep ?? '', logradouro: s.addressLogradouro ?? '',
      numero: s.addressNumero ?? '', complemento: s.addressComplemento ?? '',
      bairro: s.addressBairro ?? '', cidade: s.addressCidade ?? '',
      uf: s.addressUf ?? '', businessHours: s.businessHours ?? '',
    });
    this.drawer.set(true);
  }

  lookupCep() {
    const cep = (this.supplierForm.value.cep ?? '').replace(/\D/g, '');
    if (cep.length !== 8) return;
    this.cepLoading.set(true);
    this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
      next: (data) => {
        if (!data.erro) {
          this.supplierForm.patchValue({
            logradouro: data.logradouro ?? '',
            bairro: data.bairro ?? '',
            cidade: data.localidade ?? '',
            uf: data.uf ?? '',
          });
        }
        this.cepLoading.set(false);
      },
      error: () => this.cepLoading.set(false),
    });
  }

  closeDrawer() { this.drawer.set(false); }

  saveSupplier() {
    if (this.supplierForm.invalid) return;
    const v = this.supplierForm.value;
    const body = {
      name: v.name!, supplierCategoryId: v.supplierCategoryId!,
      cnpj: v.cnpj || null, phone: v.phone || null, email: v.email || null,
      website: v.website || null, description: v.description || null,
      cep: v.cep || null, logradouro: v.logradouro || null,
      numero: v.numero || null, complemento: v.complemento || null,
      bairro: v.bairro || null, cidade: v.cidade || null,
      uf: v.uf || null, businessHours: v.businessHours || null,
    };
    this.saving.set(true);
    const req = this.editingSupplier()
      ? this.svc.updateSupplier(this.editingSupplier()!.id, body)
      : this.svc.createSupplier(body);
    req.subscribe({
      next: () => { this.toast.success(this.editingSupplier() ? 'Fornecedor atualizado.' : 'Fornecedor criado.'); this.closeDrawer(); this.load(); this.saving.set(false); },
      error: (err) => { this.toast.error(err?.error?.message ?? 'Erro ao salvar.'); this.saving.set(false); }
    });
  }

  toggleSupplier(s: PlatformSupplierDto) {
    const req = s.isActive ? this.svc.deactivateSupplier(s.id) : this.svc.activateSupplier(s.id);
    req.subscribe({
      next: () => { this.toast.success(s.isActive ? 'Fornecedor desativado.' : 'Fornecedor ativado.'); this.load(); },
      error: (err) => this.toast.error(err?.error?.message ?? 'Erro.')
    });
  }

  doDeleteSupplier() {
    const s = this.deleteModal();
    if (!s) return;
    this.saving.set(true);
    this.svc.deleteSupplier(s.id).subscribe({
      next: () => { this.toast.success('Fornecedor excluído.'); this.deleteModal.set(null); this.load(); this.saving.set(false); },
      error: (err) => { this.toast.error(err?.error?.message ?? 'Erro ao excluir.'); this.saving.set(false); }
    });
  }

  // ─── Category CRUD ──────────────────────────────────────────────────────────

  openEditCategory(c: SupplierCategoryDto) {
    this.editingCategory.set(c);
    this.catForm.patchValue({ name: c.name, description: c.description ?? '' });
    this.catDrawer.set(true);
  }

  closeCatDrawer() { this.catDrawer.set(false); }

  saveCategory() {
    if (this.catForm.invalid) return;
    const v = this.catForm.value;
    const body = { name: v.name!, description: v.description || null };
    this.saving.set(true);
    const req = this.editingCategory()
      ? this.svc.updateCategory(this.editingCategory()!.id, body)
      : this.svc.createCategory(body);
    req.subscribe({
      next: () => { this.toast.success(this.editingCategory() ? 'Categoria atualizada.' : 'Categoria criada.'); this.closeCatDrawer(); this.load(); this.saving.set(false); },
      error: (err) => { this.toast.error(err?.error?.message ?? 'Erro ao salvar.'); this.saving.set(false); }
    });
  }

  toggleCategory(c: SupplierCategoryDto) {
    const req = c.isActive ? this.svc.deactivateCategory(c.id) : this.svc.activateCategory(c.id);
    req.subscribe({
      next: () => { this.toast.success(c.isActive ? 'Categoria desativada.' : 'Categoria ativada.'); this.load(); },
      error: (err) => this.toast.error(err?.error?.message ?? 'Erro.')
    });
  }

  doDeleteCategory() {
    const c = this.deleteCatModal();
    if (!c) return;
    this.saving.set(true);
    this.svc.deleteCategory(c.id).subscribe({
      next: () => { this.toast.success('Categoria excluída.'); this.deleteCatModal.set(null); this.load(); this.saving.set(false); },
      error: (err) => { this.toast.error(err?.error?.message ?? 'Erro ao excluir.'); this.saving.set(false); }
    });
  }
}
