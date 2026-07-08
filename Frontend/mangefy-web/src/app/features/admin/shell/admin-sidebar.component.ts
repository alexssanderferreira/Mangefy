import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, type LucideIconData, LayoutGrid, User, Home, Star, DollarSign, TriangleAlert, Box, Truck, LogOut } from 'lucide-angular';
import { AuthService } from '../../../core/auth/auth.service';
import { LayoutService } from './layout.service';
import { SubscriptionService } from '../subscriptions/subscription.service';

interface NavItem {
  label: string;
  route: string;
  icon: LucideIconData;
  badge?: number;
  queryParams?: Record<string, string>;
}

interface NavGroup {
  label: string;
  items: NavItem[];
}

@Component({
  selector: 'app-admin-sidebar',
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
        <span class="logo-badge">ADMIN</span>
      </div>

      <!-- Nav -->
      <nav class="sidebar-nav">
        @for (group of navGroups; track group.label) {
          <div class="nav-group">
            <span class="nav-group-label">{{ group.label }}</span>
            @for (item of group.items; track item.route) {
              <a class="nav-item" [routerLink]="['/admin', item.route]" [queryParams]="item.queryParams ?? {}" routerLinkActive="active" (click)="layout.close()">
                <span class="nav-item-left">
                  <span class="nav-icon"><lucide-icon [img]="item.icon" [size]="15"></lucide-icon></span>
                  <span class="nav-label">{{ item.label }}</span>
                </span>
                @if (item.route === 'overdue' && overdueCount() > 0) {
                  <span class="nav-badge">{{ overdueCount() }}</span>
                } @else if (item.badge && item.route !== 'overdue') {
                  <span class="nav-badge">{{ item.badge }}</span>
                }
              </a>
            }
          </div>
        }
      </nav>

      <!-- Footer -->
      <div class="sidebar-footer">
        <div class="user-block">
          <div class="user-avatar">{{ initial }}</div>
          <div class="user-meta">
            <span class="user-name">{{ userName }}</span>
            <span class="user-role">Administrador</span>
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
      color: #fff;
      flex-shrink: 0;
    }
    .logo-text {
      font-size: 16px;
      font-weight: 700;
      color: #fff;
      letter-spacing: -.2px;
      flex: 1;
    }
    .logo-badge {
      font-size: 9px;
      font-weight: 700;
      background: rgba(var(--color-brand-rgb),.2);
      color: var(--color-brand);
      border: 1px solid rgba(var(--color-brand-rgb),.3);
      padding: 2px 7px;
      border-radius: 4px;
      letter-spacing: .6px;
    }

    /* ── Nav ── */
    .sidebar-nav {
      flex: 1;
      padding: 16px 10px;
      display: flex;
      flex-direction: column;
      gap: 24px;
    }

    .nav-group-label {
      display: block;
      font-size: 10px;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 1px;
      color: rgba(255,255,255,.22);
      padding: 0 10px;
      margin-bottom: 4px;
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

    .nav-badge {
      font-size: 10px;
      font-weight: 700;
      background: var(--color-danger);
      color: #fff;
      padding: 1px 7px;
      border-radius: 99px;
      min-width: 20px;
      text-align: center;
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
      color: #fff;
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
export class AdminSidebarComponent implements OnInit {
  private auth      = inject(AuthService);
  private subscriptionSvc = inject(SubscriptionService);
  layout = inject(LayoutService);

  readonly LogOut = LogOut;

  overdueCount = signal(0);

  get userName() { return 'Admin'; }
  get initial()  { return 'A'; }

  ngOnInit() {
    this.subscriptionSvc.getOverdue().subscribe({
      next: data => this.overdueCount.set(data.length),
      error: () => {}
    });
  }

  navGroups: NavGroup[] = [
    {
      label: 'Principal',
      items: [
        { label: 'Dashboard', route: 'dashboard', icon: LayoutGrid },
        { label: 'Responsáveis', route: 'owners', icon: User },
        { label: 'Estabelecimentos', route: 'tenants', icon: Home },
        { label: 'Planos', route: 'plans', icon: Star },
      ]
    },
    {
      label: 'Financeiro',
      items: [
        { label: 'Assinaturas', route: 'subscriptions', icon: DollarSign },
        { label: 'Inadimplências', route: 'overdue', icon: TriangleAlert },
      ]
    },
    {
      label: 'Configuração',
      items: [
        { label: 'Tipos de Negócio', route: 'business-types', icon: Box },
        { label: 'Fornecedores', route: 'suppliers', icon: Truck },
        { label: 'Matriz de Features', route: 'feature-matrix', icon: LayoutGrid },
      ]
    },
  ];

  logout() { this.auth.logout(); }
}
