import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AdminSidebarComponent } from './admin-sidebar.component';
import { AdminTopbarComponent } from './admin-topbar.component';
import { LayoutService } from './layout.service';
import { ToastComponent } from '../../../core/toast/toast.component';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [RouterOutlet, AdminSidebarComponent, AdminTopbarComponent, ToastComponent],
  template: `
    <div class="shell">
      <!-- Overlay (mobile) -->
      @if (layout.sidebarOpen()) {
        <div class="sidebar-overlay" (click)="layout.close()"></div>
      }

      <app-admin-sidebar [class.sidebar-open]="layout.sidebarOpen()" />

      <div class="shell-body">
        <app-admin-topbar />
        <main class="shell-main">
          <router-outlet />
        </main>
      </div>
    </div>
    <app-toast />
  `,
  styles: [`
    .shell {
      display: flex;
      height: 100vh;
      overflow: hidden;
      background: #0d0f12;
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
      app-admin-sidebar {
        position: fixed;
        top: 0; left: 0; bottom: 0;
        z-index: 200;
        transform: translateX(-100%);
        transition: transform .25s ease;
      }
      app-admin-sidebar.sidebar-open {
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
export class AdminShellComponent {
  layout = inject(LayoutService);
}
