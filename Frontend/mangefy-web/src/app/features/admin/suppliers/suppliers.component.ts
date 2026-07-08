import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { LucideAngularModule, Plus, ShoppingCart, Search, SquarePen, Ban, CircleCheckBig, Trash2, Folder, X, User, Phone, MapPin, Clock, List, LoaderCircle } from 'lucide-angular';
import { ToastService } from '../../../core/toast/toast.service';
import { SupplierService, PlatformSupplierDto, SupplierCategoryDto } from './supplier.service';

type Tab = 'suppliers' | 'categories';

@Component({
  selector: 'app-suppliers',
  standalone: true,
  imports: [ReactiveFormsModule, LucideAngularModule],
  template: `
    <div class="page">

      <!-- Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">Fornecedores</h1>
          <p class="page-subtitle">Catálogo global de fornecedores da plataforma</p>
        </div>
        <button class="btn btn-primary" (click)="openCreate()">
          <lucide-icon [img]="Plus" [size]="14" [strokeWidth]="2.5"></lucide-icon>
          @if (tab() === 'suppliers') { Novo Fornecedor } @else { Nova Categoria }
        </button>
      </div>

      <!-- Tabs -->
      <div class="tabs">
        <button class="tab" [class.active]="tab() === 'suppliers'" (click)="tab.set('suppliers')">
          Fornecedores
          <span class="tab-count">{{ suppliers().length }}</span>
        </button>
        <button class="tab" [class.active]="tab() === 'categories'" (click)="tab.set('categories')">
          Categorias
          <span class="tab-count">{{ categories().length }}</span>
        </button>
      </div>

      <!-- Loading -->
      @if (loading()) {
        <div class="skel-rows">@for (i of [1,2,3,4,5]; track i) { <div class="skeleton-row" style="height:52px"></div> }</div>
      }

      <!-- Suppliers tab -->
      @if (!loading() && tab() === 'suppliers') {
        @if (suppliers().length === 0) {
          <div class="empty-state">
            <lucide-icon [img]="ShoppingCart" [size]="40" [strokeWidth]="1.5"></lucide-icon>
            <p>Nenhum fornecedor cadastrado</p>
            <button class="btn btn-primary" (click)="openCreate()">Cadastrar fornecedor</button>
          </div>
        } @else {
          <!-- Filter bar -->
          <div class="filter-bar">
            <div class="search-wrap">
              <lucide-icon [img]="Search" [size]="14" [strokeWidth]="2" class="search-icon"></lucide-icon>
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
                      <span class="badge" [class.badge-success]="s.isActive" [class.badge-neutral]="!s.isActive">
                        {{ s.isActive ? 'Ativo' : 'Inativo' }}
                      </span>
                    </td>
                    <td class="action-col" (click)="$event.stopPropagation()">
                      <div class="row-actions">
                        <button class="btn-icon" title="Editar" (click)="openEdit(s)">
                          <lucide-icon [img]="SquarePen" [size]="14" [strokeWidth]="2"></lucide-icon>
                        </button>
                        <button class="btn-icon" [title]="s.isActive ? 'Desativar' : 'Ativar'" (click)="toggleSupplier(s)">
                          @if (s.isActive) {
                            <lucide-icon [img]="Ban" [size]="14" [strokeWidth]="2"></lucide-icon>
                          } @else {
                            <lucide-icon [img]="CircleCheckBig" [size]="14" [strokeWidth]="2"></lucide-icon>
                          }
                        </button>
                        <button class="btn-icon btn-icon--danger" title="Excluir" (click)="deleteModal.set(s)">
                          <lucide-icon [img]="Trash2" [size]="14" [strokeWidth]="2"></lucide-icon>
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
            <lucide-icon [img]="Folder" [size]="40" [strokeWidth]="1.5"></lucide-icon>
            <p>Nenhuma categoria cadastrada</p>
            <button class="btn btn-primary" (click)="openCreate()">Criar categoria</button>
          </div>
        } @else {
          <div class="cards-grid">
            @for (c of categories(); track c.id) {
              <div class="cat-card" [class.cat-inactive]="!c.isActive">
                <div class="cat-header">
                  <div class="cat-avatar">{{ c.name[0] }}</div>
                  <span class="badge" [class.badge-success]="c.isActive" [class.badge-neutral]="!c.isActive">
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
                    <lucide-icon [img]="SquarePen" [size]="13" [strokeWidth]="2"></lucide-icon>
                  </button>
                  <button class="btn-icon" [title]="c.isActive ? 'Desativar' : 'Ativar'" (click)="toggleCategory(c)">
                    @if (c.isActive) {
                      <lucide-icon [img]="Ban" [size]="13" [strokeWidth]="2"></lucide-icon>
                    } @else {
                      <lucide-icon [img]="CircleCheckBig" [size]="13" [strokeWidth]="2"></lucide-icon>
                    }
                  </button>
                  <button class="btn-icon btn-icon--danger" (click)="deleteCatModal.set(c)" [disabled]="supplierCountByCategory(c.id) > 0" [title]="supplierCountByCategory(c.id) > 0 ? 'Categoria em uso' : 'Excluir'">
                    <lucide-icon [img]="Trash2" [size]="13" [strokeWidth]="2"></lucide-icon>
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
      <div class="drawer-overlay" (click)="closeDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <lucide-icon [img]="ShoppingCart" [size]="16" [strokeWidth]="2"></lucide-icon>
            </div>
            <div>
              <h2 class="drawer-title">{{ editingSupplier() ? 'Editar Fornecedor' : 'Novo Fornecedor' }}</h2>
              <p class="drawer-subtitle">{{ editingSupplier() ? editingSupplier()!.name : 'Cadastre um fornecedor global' }}</p>
            </div>
          </div>
          <button class="btn-close" (click)="closeDrawer()">
            <lucide-icon [img]="X" [size]="16" [strokeWidth]="2.5"></lucide-icon>
          </button>
        </div>
        <form class="drawer-body" [formGroup]="supplierForm" (ngSubmit)="saveSupplier()">

          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="User" [size]="12" [strokeWidth]="2.5"></lucide-icon>
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
              <lucide-icon [img]="Phone" [size]="12" [strokeWidth]="2.5"></lucide-icon>
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
              <lucide-icon [img]="MapPin" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Endereço
            </div>
            <div class="field-row-cep">
              <div class="field">
                <label class="field-label">CEP</label>
                <div class="input-wrap">
                  <input class="field-ctrl" formControlName="cep" placeholder="00000-000" maxlength="9"
                    (blur)="lookupCep()" (keydown.enter)="$event.preventDefault(); lookupCep()" />
                  @if (cepLoading()) {
                    <lucide-icon [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5" class="input-spin"></lucide-icon>
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
              <lucide-icon [img]="Clock" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Horário de Funcionamento
            </div>
            <div class="field">
              <label class="field-label">Horário</label>
              <input class="field-ctrl" formControlName="businessHours" placeholder="Seg–Sex 08h–18h, Sáb 08h–12h" />
            </div>
          </div>

          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="List" [size]="12" [strokeWidth]="2.5"></lucide-icon>
              Observações
            </div>
            <div class="field">
              <label class="field-label">Descrição</label>
              <textarea class="field-ctrl field-textarea" formControlName="description" rows="3" placeholder="Informações adicionais..."></textarea>
            </div>
          </div>

          <div class="drawer-footer">
            <button type="button" class="btn btn-ghost" (click)="closeDrawer()">Cancelar</button>
            <button type="submit" class="btn btn-primary" [disabled]="supplierForm.invalid || saving()">
              @if (saving()) {
                <lucide-icon [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5" class="icon-spin"></lucide-icon>
                Salvando...
              } @else { Salvar }
            </button>
          </div>
        </form>
      </aside>
    }

    <!-- ── Drawer Categoria ───────────────────────────────────────────────── -->
    @if (catDrawer()) {
      <div class="drawer-overlay" (click)="closeCatDrawer()"></div>
      <aside class="drawer">
        <div class="drawer-header">
          <div class="drawer-header-left">
            <div class="drawer-icon">
              <lucide-icon [img]="Folder" [size]="16" [strokeWidth]="2"></lucide-icon>
            </div>
            <div>
              <h2 class="drawer-title">{{ editingCategory() ? 'Editar Categoria' : 'Nova Categoria' }}</h2>
              <p class="drawer-subtitle">{{ editingCategory() ? editingCategory()!.name : 'Agrupe fornecedores por categoria' }}</p>
            </div>
          </div>
          <button class="btn-close" (click)="closeCatDrawer()">
            <lucide-icon [img]="X" [size]="16" [strokeWidth]="2.5"></lucide-icon>
          </button>
        </div>
        <form class="drawer-body" [formGroup]="catForm" (ngSubmit)="saveCategory()">
          <div class="form-section">
            <div class="form-section-header">
              <lucide-icon [img]="Folder" [size]="12" [strokeWidth]="2.5"></lucide-icon>
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
            <button type="button" class="btn btn-ghost" (click)="closeCatDrawer()">Cancelar</button>
            <button type="submit" class="btn btn-primary" [disabled]="catForm.invalid || saving()">
              @if (saving()) {
                <lucide-icon [img]="LoaderCircle" [size]="14" [strokeWidth]="2.5" class="icon-spin"></lucide-icon>
                Salvando...
              } @else { Salvar }
            </button>
          </div>
        </form>
      </aside>
    }

    <!-- ── Modal excluir fornecedor ──────────────────────────────────────── -->
    @if (deleteModal()) {
      <div class="modal-overlay" (click)="deleteModal.set(null)">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-icon">
            <lucide-icon [img]="Trash2" [size]="24" [strokeWidth]="2"></lucide-icon>
          </div>
          <h3 class="modal-title">Excluir fornecedor?</h3>
          <p class="modal-body">O fornecedor <strong>{{ deleteModal()!.name }}</strong> será removido permanentemente.</p>
          <div class="modal-actions">
            <button class="btn btn-ghost" (click)="deleteModal.set(null)">Cancelar</button>
            <button class="btn btn-danger" (click)="doDeleteSupplier()" [disabled]="saving()">
              @if (saving()) { Excluindo... } @else { Excluir }
            </button>
          </div>
        </div>
      </div>
    }

    <!-- ── Modal excluir categoria ───────────────────────────────────────── -->
    @if (deleteCatModal()) {
      <div class="modal-overlay" (click)="deleteCatModal.set(null)">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-icon">
            <lucide-icon [img]="Trash2" [size]="24" [strokeWidth]="2"></lucide-icon>
          </div>
          <h3 class="modal-title">Excluir categoria?</h3>
          <p class="modal-body">A categoria <strong>{{ deleteCatModal()!.name }}</strong> será removida permanentemente.</p>
          <div class="modal-actions">
            <button class="btn btn-ghost" (click)="deleteCatModal.set(null)">Cancelar</button>
            <button class="btn btn-danger" (click)="doDeleteCategory()" [disabled]="saving()">
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
    .page-subtitle { font-size: 12px; color: #aaa; margin-top: 2px; }

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

    /* Form sections */
    .form-section { display: flex; flex-direction: column; gap: 10px; padding: 14px; background: #fafafa; border: 1px solid #f0f0f3; border-radius: 10px; }
    .form-section-header { display: flex; align-items: center; gap: 6px; font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: .07em; color: #999; lucide-icon { color: var(--color-brand); } }

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
    .input-spin { position: absolute; right: 9px; top: 50%; transform: translateY(-50%); color: #aaa; animation: spin .8s linear infinite; pointer-events: none; }

    /* Skeleton */
    .skel-rows { display: flex; flex-direction: column; gap: 8px; }

    @media (max-width: 768px) {
      .page { padding: 12px; }
      .filter-bar { flex-direction: column; }
      .data-table th:nth-child(3), .data-table td:nth-child(3),
      .data-table th:nth-child(4), .data-table td:nth-child(4) { display: none; }
      .drawer { width: 100%; }
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

  readonly Plus = Plus;
  readonly ShoppingCart = ShoppingCart;
  readonly Search = Search;
  readonly SquarePen = SquarePen;
  readonly Ban = Ban;
  readonly CircleCheckBig = CircleCheckBig;
  readonly Trash2 = Trash2;
  readonly Folder = Folder;
  readonly X = X;
  readonly User = User;
  readonly Phone = Phone;
  readonly MapPin = MapPin;
  readonly Clock = Clock;
  readonly List = List;
  readonly LoaderCircle = LoaderCircle;

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
