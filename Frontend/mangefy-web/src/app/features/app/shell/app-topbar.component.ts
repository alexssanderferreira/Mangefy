import { Component, inject } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { LucideAngularModule, Menu, ChevronRight } from 'lucide-angular';
import { AppLayoutService } from './layout.service';
import { filter, map, startWith } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-tenant-topbar',
  standalone: true,
  imports: [LucideAngularModule],
  template: `
    <header class="topbar">
      <div class="topbar-left">
        <button class="hamburger" (click)="layout.toggle()">
          <lucide-icon [img]="Menu" [size]="18" [strokeWidth]="2.5"></lucide-icon>
        </button>
        <nav class="breadcrumb">
          <span class="bc-root">Mangefy</span>
          @if (pageLabel()) {
            <span class="bc-sep">
              <lucide-icon [img]="ChevronRight" [size]="14"></lucide-icon>
            </span>
            <span class="bc-current">{{ pageLabel() }}</span>
          }
        </nav>
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

    .breadcrumb {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 13px;
    }
    .bc-root {
      color: #888;
      font-weight: 500;
    }
    .bc-sep { color: #ccc; display: flex; align-items: center; }
    .bc-current { color: #111; font-weight: 600; }
  `]
})
export class AppTopbarComponent {
  layout = inject(AppLayoutService);
  private router = inject(Router);

  readonly Menu = Menu;
  readonly ChevronRight = ChevronRight;

  private routeLabels: Record<string, string> = {
    'dashboard': 'Dashboard',
    'tables':    'Mesas',
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
}
