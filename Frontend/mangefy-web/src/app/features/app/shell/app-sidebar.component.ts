import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, type LucideIconData, LayoutGrid, Table, LogOut, ChevronsUpDown, Check, Store } from 'lucide-angular';
import { AuthService } from '../../../core/auth/auth.service';
import { TenantOption } from '../../../core/models/auth.models';
import { ToastService } from '../../../core/toast/toast.service';
import { AppLayoutService } from './layout.service';

interface NavItem {
  label: string;
  route: string;
  icon: LucideIconData;
  permission?: string;
}

@Component({
  selector: 'app-tenant-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, LucideAngularModule],
  template: `
    <aside class="sidebar">

      <!-- Logo -->
      <div class="sidebar-logo">
        <div class="logo-mark">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <path d="M3 11l19-9-9 19-2-8-8-2z"/>
          </svg>
        </div>
        <span class="logo-text">Mangefy</span>
      </div>

      <!-- Estabelecimento atual -->
      <div class="tenant-zone">
        @if (auth.hasMultipleTenants()) {
          <button class="tenant-card" [class.open]="tenantMenuOpen()" (click)="tenantMenuOpen.set(!tenantMenuOpen())">
            <span class="tenant-card-icon"><lucide-icon [img]="Store" [size]="14" [strokeWidth]="2"></lucide-icon></span>
            <span class="tenant-card-name">{{ auth.user()?.tenantName }}</span>
            <lucide-icon class="tenant-card-chevron" [img]="ChevronsUpDown" [size]="13" [strokeWidth]="2"></lucide-icon>
          </button>

          @if (tenantMenuOpen()) {
            <div class="tenant-menu-overlay" (click)="tenantMenuOpen.set(false)"></div>
            <div class="tenant-menu">
              <span class="tenant-menu-label">Trocar estabelecimento</span>
              @for (t of auth.availableTenants(); track t.tenantId) {
                <button class="tenant-menu-item" [class.current]="t.tenantSlug === auth.user()?.tenantSlug" [disabled]="switching()" (click)="switchTo(t)">
                  <span class="tenant-menu-avatar">{{ t.tenantName.charAt(0).toUpperCase() }}</span>
                  <span class="tenant-menu-name">{{ t.tenantName }}</span>
                  @if (t.tenantSlug === auth.user()?.tenantSlug) {
                    <lucide-icon [img]="Check" [size]="14" [strokeWidth]="2.5"></lucide-icon>
                  }
                </button>
              }
            </div>
          }
        } @else {
          <div class="tenant-card tenant-card--static">
            <span class="tenant-card-icon"><lucide-icon [img]="Store" [size]="14" [strokeWidth]="2"></lucide-icon></span>
            <span class="tenant-card-name">{{ auth.user()?.tenantName }}</span>
          </div>
        }
      </div>

      <!-- Nav -->
      <nav class="sidebar-nav">
        <div class="nav-group">
          @for (item of visibleItems(); track item.route) {
            <a class="nav-item" [routerLink]="['/app', item.route]" routerLinkActive="active" (click)="layout.close()">
              <span class="nav-item-left">
                <span class="nav-icon"><lucide-icon [img]="item.icon" [size]="15"></lucide-icon></span>
                <span class="nav-label">{{ item.label }}</span>
              </span>
            </a>
          }
        </div>
      </nav>

      <!-- Footer -->
      <div class="sidebar-footer">
        <div class="user-block">
          <div class="user-avatar">{{ initial() }}</div>
          <div class="user-meta">
            <span class="user-name">{{ auth.user()?.name }}</span>
            <span class="user-role">{{ auth.user()?.isOwner ? 'Proprietário' : 'Colaborador' }}</span>
          </div>
        </div>
        <button class="btn-logout" (click)="logout()" title="Sair">
          <lucide-icon [img]="LogOut" [size]="15"></lucide-icon>
        </button>
      </div>

    </aside>
  `,
  styles: [`
    .sidebar {
      width: var(--sidebar-width);
      min-width: var(--sidebar-width);
      background: #111318;
      border-right: 1px solid rgba(255,255,255,.05);
      display: flex;
      flex-direction: column;
      height: 100vh;
      overflow-y: auto;
      overflow-x: hidden;
    }

    /* ── Logo ── */
    .sidebar-logo {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 22px 18px 18px;
      border-bottom: 1px solid rgba(255,255,255,.05);
      flex-shrink: 0;
    }
    .logo-mark {
      width: 32px; height: 32px;
      background: var(--color-brand);
      border-radius: 8px;
      display: flex; align-items: center; justify-content: center;
      color: #14120a;
      flex-shrink: 0;
    }
    .logo-text {
      font-size: 15px;
      font-weight: 700;
      color: #fff;
      letter-spacing: -.2px;
    }

    /* ── Estabelecimento atual ── */
    .tenant-zone {
      position: relative;
      padding: 12px 14px;
      border-bottom: 1px solid rgba(255,255,255,.05);
      flex-shrink: 0;
    }

    .tenant-card {
      display: flex;
      align-items: center;
      gap: 8px;
      width: 100%;
      padding: 8px 9px;
      border-radius: 9px;
      border: 1px solid rgba(255,255,255,.08);
      background: rgba(255,255,255,.04);
      cursor: pointer;
      transition: background .15s, border-color .15s;
      text-align: left;

      &:hover, &.open {
        background: rgba(255,255,255,.08);
        border-color: rgba(255,255,255,.14);
      }

      &.tenant-card--static {
        cursor: default;
        &:hover { background: rgba(255,255,255,.04); border-color: rgba(255,255,255,.08); }
      }
    }

    .tenant-card-icon {
      display: flex; align-items: center; justify-content: center;
      width: 24px; height: 24px; border-radius: 6px; flex-shrink: 0;
      background: rgba(var(--color-brand-rgb),.18);
      color: var(--color-brand);
    }

    .tenant-card-name {
      flex: 1;
      min-width: 0;
      font-size: 12.5px;
      font-weight: 600;
      color: #fff;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .tenant-card-chevron {
      flex-shrink: 0;
      color: rgba(255,255,255,.35);
    }

    .tenant-menu-overlay {
      position: fixed;
      inset: 0;
      z-index: 210;
    }

    .tenant-menu {
      position: absolute;
      top: calc(100% + 6px);
      left: 14px;
      right: 14px;
      background: #1e2028;
      border: 1px solid rgba(255,255,255,.1);
      border-radius: 10px;
      box-shadow: 0 16px 40px rgba(0,0,0,.45);
      z-index: 211;
      padding: 6px;
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .tenant-menu-label {
      font-size: 10px;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: .06em;
      color: rgba(255,255,255,.35);
      padding: 7px 8px 5px;
    }

    .tenant-menu-item {
      display: flex;
      align-items: center;
      gap: 9px;
      padding: 7px 8px;
      border-radius: 7px;
      border: none;
      background: none;
      color: rgba(255,255,255,.8);
      font-size: 12.5px;
      font-weight: 500;
      text-align: left;
      cursor: pointer;
      transition: background .12s;

      &:hover:not(:disabled) { background: rgba(255,255,255,.07); color: #fff; }
      &:disabled { opacity: .5; cursor: not-allowed; }
      &.current { color: #fff; }

      lucide-icon { color: var(--color-brand); flex-shrink: 0; margin-left: auto; }
    }

    .tenant-menu-avatar {
      display: flex; align-items: center; justify-content: center;
      width: 22px; height: 22px; border-radius: 6px; flex-shrink: 0;
      background: rgba(255,255,255,.08);
      color: rgba(255,255,255,.7);
      font-size: 10.5px; font-weight: 700;
    }
    .tenant-menu-item.current .tenant-menu-avatar {
      background: var(--color-brand);
      color: #14120a;
    }

    .tenant-menu-name {
      flex: 1;
      min-width: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    /* ── Nav ── */
    .sidebar-nav {
      flex: 1;
      padding: 16px 10px;
      display: flex;
      flex-direction: column;
      gap: 24px;
    }

    .nav-item {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 8px 10px;
      border-radius: 7px;
      color: rgba(255,255,255,.45);
      font-size: 13.5px;
      font-weight: 500;
      transition: background .15s, color .15s;
      cursor: pointer;
      text-decoration: none;

      &:hover {
        background: rgba(255,255,255,.06);
        color: rgba(255,255,255,.9);

        .nav-icon { color: rgba(255,255,255,.7); }
      }

      &.active {
        background: rgba(var(--color-brand-rgb),.15);
        color: #fff;
        font-weight: 600;

        .nav-icon { color: var(--color-brand); }
      }
    }

    .nav-item-left {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .nav-icon {
      display: flex;
      align-items: center;
      color: rgba(255,255,255,.25);
      transition: color .15s;
      flex-shrink: 0;
    }

    /* ── Footer ── */
    .sidebar-footer {
      padding: 14px 14px;
      border-top: 1px solid rgba(255,255,255,.05);
      display: flex;
      align-items: center;
      gap: 8px;
      flex-shrink: 0;
    }

    .user-block {
      display: flex;
      align-items: center;
      gap: 10px;
      flex: 1;
      min-width: 0;
    }

    .user-avatar {
      width: 34px; height: 34px;
      border-radius: 8px;
      background: var(--color-brand);
      color: #14120a;
      display: flex; align-items: center; justify-content: center;
      font-size: 13px; font-weight: 700;
      flex-shrink: 0;
      letter-spacing: -.5px;
    }

    .user-meta {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    .user-name {
      font-size: 13px;
      font-weight: 600;
      color: rgba(255,255,255,.9);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .user-role {
      font-size: 11px;
      color: rgba(255,255,255,.3);
      margin-top: 1px;
    }

    .btn-logout {
      background: none;
      border: none;
      color: rgba(255,255,255,.25);
      padding: 6px;
      border-radius: 6px;
      display: flex;
      align-items: center;
      cursor: pointer;
      transition: background .15s, color .15s;
      flex-shrink: 0;

      &:hover {
        background: rgba(255,255,255,.07);
        color: rgba(255,255,255,.8);
      }
    }
  `]
})
export class AppSidebarComponent {
  auth   = inject(AuthService);
  layout = inject(AppLayoutService);
  private router = inject(Router);
  private toast  = inject(ToastService);

  readonly LogOut = LogOut;
  readonly ChevronsUpDown = ChevronsUpDown;
  readonly Check = Check;
  readonly Store = Store;

  tenantMenuOpen = signal(false);
  switching = signal(false);

  private navItems: NavItem[] = [
    { label: 'Dashboard', route: 'dashboard', icon: LayoutGrid },
    { label: 'Mesas',     route: 'tables',    icon: Table, permission: 'tables.read' },
  ];

  visibleItems = computed(() =>
    this.navItems.filter(item => !item.permission || this.auth.hasPermission(item.permission))
  );

  initial() {
    const name = this.auth.user()?.name ?? '';
    return name.charAt(0).toUpperCase() || '?';
  }

  switchTo(tenant: TenantOption) {
    if (tenant.tenantSlug === this.auth.user()?.tenantSlug) {
      this.tenantMenuOpen.set(false);
      return;
    }
    this.switching.set(true);
    this.auth.switchTenant(tenant.tenantSlug).subscribe({
      next: () => {
        this.switching.set(false);
        this.tenantMenuOpen.set(false);
        this.toast.success(`Agora você está em ${tenant.tenantName}.`);
        this.router.navigate(['/app/dashboard']);
      },
      error: () => {
        this.switching.set(false);
        this.toast.error('Não foi possível trocar de estabelecimento.');
      },
    });
  }

  logout() { this.auth.logout(); }
}
