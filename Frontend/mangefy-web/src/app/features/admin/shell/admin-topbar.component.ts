import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { LucideAngularModule, Menu, ChevronRight } from 'lucide-angular';
import { AuthService } from '../../../core/auth/auth.service';
import { LayoutService } from './layout.service';
import { filter, map, startWith } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { environment } from '../../../../environments/environment';

const ENV_LABEL = environment.production ? null : 'DEV';

@Component({
  selector: 'app-admin-topbar',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  template: `
    <header class="topbar">
      <div class="topbar-left">
        <button class="hamburger" (click)="layout.toggle()">
          <lucide-icon [img]="Menu" [size]="18" [strokeWidth]="2.5"></lucide-icon>
        </button>
        <nav class="breadcrumb">
          <a routerLink="/admin/dashboard" class="bc-root">Mangefy Admin</a>
          @if (pageLabel()) {
            <span class="bc-sep">
              <lucide-icon [img]="ChevronRight" [size]="14"></lucide-icon>
            </span>
            <span class="bc-current">{{ pageLabel() }}</span>
          }
        </nav>
      </div>

      <div class="topbar-right">
        @if (envLabel) {
          <span class="env-badge">{{ envLabel }}</span>
        }
        <!-- API Status -->
        <div class="api-status" [class.online]="apiOnline()" [class.offline]="!apiOnline()" [class.checking]="apiChecking()">
          <span class="api-dot"></span>
          <span class="api-label">
            @if (apiChecking()) { Verificando... }
            @else if (apiOnline()) { API Online }
            @else { API Offline }
          </span>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .topbar {
      height: 52px;
      background: #fff;
      border-bottom: 1px solid #e8e8ec;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 24px;
      flex-shrink: 0;
      gap: 16px;
    }

    /* Hamburger — hidden on desktop */
    .hamburger {
      display: none;
      align-items: center; justify-content: center;
      width: 34px; height: 34px; border-radius: 8px;
      border: none; background: none; color: #555; cursor: pointer;
      flex-shrink: 0;
      &:hover { background: #f5f5f7; }
    }

    @media (max-width: 768px) {
      .topbar { padding: 0 14px; }
      .hamburger { display: flex; }
      .bc-root { display: none; }
      .bc-sep  { display: none; }
    }

    /* Left */
    .breadcrumb {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 13px;
    }
    .bc-root {
      color: #888;
      font-weight: 500;
      transition: color .15s;
      text-decoration: none;
      &:hover { color: #333; }
    }
    .bc-sep { color: #ccc; display: flex; align-items: center; }
    .bc-current { color: #111; font-weight: 600; }

    /* Right */
    .topbar-right {
      display: flex;
      align-items: center;
      gap: 14px;
    }

    /* API Status */
    .api-status {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 4px 10px;
      border-radius: 99px;
      font-size: 11.5px;
      font-weight: 600;
      transition: background .3s, color .3s;

      &.checking {
        background: #f4f4f5;
        color: #888;
        .api-dot {
          background: #bbb;
          animation: pulse 1.2s infinite;
        }
      }

      &.online {
        background: #dcfce7;
        color: #15803d;
        .api-dot { background: #16a34a; }
      }

      &.offline {
        background: #fee2e2;
        color: #b91c1c;
        .api-dot { background: #dc2626; animation: pulse 1s infinite; }
      }
    }

    .api-dot {
      width: 7px;
      height: 7px;
      border-radius: 50%;
      flex-shrink: 0;
    }

    .api-label { white-space: nowrap; }

    .env-badge {
      font-size: 9px; font-weight: 800; letter-spacing: .8px;
      padding: 3px 8px; border-radius: 4px;
      background: rgba(37,99,235,.12); color: #1d4ed8;
      border: 1px solid rgba(37,99,235,.2);
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50%       { opacity: .35; }
    }
  `]
})
export class AdminTopbarComponent implements OnInit, OnDestroy {
  private auth   = inject(AuthService);
  private router = inject(Router);
  private http   = inject(HttpClient);
  layout   = inject(LayoutService);
  envLabel = ENV_LABEL;
  readonly Menu = Menu;
  readonly ChevronRight = ChevronRight;

  apiOnline  = signal(false);
  apiChecking = signal(true);

  private intervalId: ReturnType<typeof setInterval> | null = null;

  private routeLabels: Record<string, string> = {
    'dashboard':            'Dashboard',
    'owners':               'Responsáveis',
    'tenants':              'Estabelecimentos',
    'plans':                'Planos',
    'subscriptions':        'Assinaturas',
    'overdue':              'Inadimplências',
    'business-types':       'Tipos de Negócio',
    'feature-sets':         'Matriz de Features',
    'suppliers':            'Fornecedores',
    'supplier-categories':  'Categorias',
  };

  pageLabel = toSignal(
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      startWith(null),
      map(() => {
        const seg = this.router.url.split('/').pop()?.split('?')[0] ?? '';
        return this.routeLabels[seg] ?? '';
      })
    ),
    { initialValue: '' }
  );

  ngOnInit() {
    this.checkApi();
    this.intervalId = setInterval(() => this.checkApi(), 30_000);
  }

  ngOnDestroy() {
    if (this.intervalId) clearInterval(this.intervalId);
  }

  private checkApi() {
    this.http.get(`${environment.apiUrl}/health`, { observe: 'response' }).subscribe({
      next:  () => { this.apiOnline.set(true);  this.apiChecking.set(false); },
      error: () => { this.apiOnline.set(false); this.apiChecking.set(false); },
    });
  }
}
