import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { AuthService } from '../../../core/auth/auth.service';
import { LayoutService } from './layout.service';

interface NavItem {
  label: string;
  route: string;
  icon: SafeHtml;
  badge?: number;
}

interface NavGroup {
  label: string;
  items: NavItem[];
}

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
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
              <a class="nav-item" [routerLink]="['/admin', item.route]" routerLinkActive="active" (click)="layout.close()">
                <span class="nav-item-left">
                  <span class="nav-icon" [innerHTML]="item.icon"></span>
                  <span class="nav-label">{{ item.label }}</span>
                </span>
                @if (item.badge) {
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
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
            <polyline points="16 17 21 12 16 7"/>
            <line x1="21" y1="12" x2="9" y2="12"/>
          </svg>
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
      background: rgba(224,49,49,.2);
      color: var(--color-brand);
      border: 1px solid rgba(224,49,49,.3);
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
        background: rgba(224,49,49,.15);
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
export class AdminSidebarComponent {
  private auth      = inject(AuthService);
  private sanitizer = inject(DomSanitizer);
  layout = inject(LayoutService);

  get userName() { return 'Admin'; }
  get initial()  { return 'A'; }

  private svg(html: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(html);
  }

  navGroups: NavGroup[] = [
    {
      label: 'Principal',
      items: [
        {
          label: 'Dashboard', route: 'dashboard',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>`)
        },
        {
          label: 'Clientes', route: 'owners',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>`)
        },
        {
          label: 'Estabelecimentos', route: 'tenants',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>`)
        },
        {
          label: 'Planos', route: 'plans',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>`)
        },
      ]
    },
    {
      label: 'Financeiro',
      items: [
        {
          label: 'Assinaturas', route: 'subscriptions',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>`)
        },
        {
          label: 'Inadimplências', route: 'overdue',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>`),
          badge: 3
        },
      ]
    },
    {
      label: 'Configuração',
      items: [
        {
          label: 'Tipos de Negócio', route: 'business-types',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>`)
        },
        {
          label: 'Matriz Features', route: 'feature-sets',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>`)
        },
        {
          label: 'Fornecedores', route: 'suppliers',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="1" y="3" width="15" height="13"/><polygon points="16 8 20 8 23 11 23 16 16 16 16 8"/><circle cx="5.5" cy="18.5" r="2.5"/><circle cx="18.5" cy="18.5" r="2.5"/></svg>`)
        },
        {
          label: 'Categorias', route: 'supplier-categories',
          icon: this.svg(`<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>`)
        },
      ]
    },
  ];

  logout() { this.auth.logout(); }
}
