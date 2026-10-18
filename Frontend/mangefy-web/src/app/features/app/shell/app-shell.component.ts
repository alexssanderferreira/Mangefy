import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AppSidebarComponent } from './app-sidebar.component';
import { AppTopbarComponent } from './app-topbar.component';
import { AppLayoutService } from './layout.service';
import { ToastComponent } from '../../../core/toast/toast.component';

@Component({
  selector: 'app-tenant-shell',
  standalone: true,
  imports: [RouterOutlet, AppSidebarComponent, AppTopbarComponent, ToastComponent],
  template: `
    <div class="shell">
      <!-- Overlay (mobile) -->
      @if (layout.sidebarOpen()) {
        <div class="sidebar-overlay" (click)="layout.close()"></div>
      }

      <app-tenant-sidebar [class.sidebar-open]="layout.sidebarOpen()" />

      <div class="shell-body">
        <app-tenant-topbar />
        <main class="shell-main">
          <router-outlet />
        </main>
      </div>
    </div>
    <app-toast />
  `,
  styles: [`
    :host {
      --color-brand:       #f5c400;
      --color-brand-hover: #d4a900;
      --color-brand-rgb:   245, 196, 0;
    }
    .shell {
      display: flex;
      height: 100vh;
      overflow: hidden;
      background: #14120a;
      position: relative;
    }
    .shell-body {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      background: #f5f5f7;
      min-width: 0;
    }
    .shell-main {
      flex: 1;
      overflow-y: auto;
    }
    .sidebar-overlay {
      display: none;
    }

    @media (max-width: 768px) {
      app-tenant-sidebar {
        position: fixed;
        top: 0; left: 0; bottom: 0;
        z-index: 200;
        transform: translateX(-100%);
        transition: transform .25s ease;
      }
      app-tenant-sidebar.sidebar-open {
        transform: translateX(0);
      }
      .sidebar-overlay {
        display: block;
        position: fixed;
        inset: 0;
        background: rgba(0,0,0,.5);
        z-index: 199;
      }
    }
  `]
})
export class AppShellComponent {
  layout = inject(AppLayoutService);
}
