import { Component, inject } from '@angular/core';
import { NgClass } from '@angular/common';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [NgClass],
  template: `
    <div class="toast-container">
      @for (t of svc.toasts(); track t.id) {
        <div class="toast" [ngClass]="t.type">
          <span class="toast-icon" [innerHTML]="icon(t.type)"></span>
          <span class="toast-msg">{{ t.message }}</span>
          <button class="toast-close" (click)="svc.dismiss(t.id)">×</button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed; bottom: 24px; right: 24px;
      display: flex; flex-direction: column; gap: 10px;
      z-index: 9999; max-width: 360px; width: calc(100vw - 32px);
    }
    .toast {
      display: flex; align-items: flex-start; gap: 10px;
      padding: 12px 14px; border-radius: 10px;
      box-shadow: 0 4px 20px rgba(0,0,0,.12);
      font-size: 13px; font-weight: 500;
      animation: slideUp .2s ease;
      border-left: 3px solid transparent;
    }
    @keyframes slideUp {
      from { transform: translateY(12px); opacity: 0; }
      to   { transform: translateY(0);    opacity: 1; }
    }
    .toast.success { background: #f0fdf4; color: #15803d; border-color: #16a34a; }
    .toast.error   { background: #fef2f2; color: #b91c1c; border-color: #dc2626; }
    .toast.warning { background: #fffbeb; color: #b45309; border-color: #d97706; }
    .toast.info    { background: #eff6ff; color: #1d4ed8; border-color: #2563eb; }
    .toast-icon    { flex-shrink: 0; display: flex; align-items: center; margin-top: 1px; }
    .toast-msg     { flex: 1; line-height: 1.4; }
    .toast-close   {
      flex-shrink: 0; background: none; border: none; cursor: pointer;
      font-size: 18px; line-height: 1; color: currentColor; opacity: .5;
      padding: 0; margin-left: 4px;
      &:hover { opacity: 1; }
    }

    @media (max-width: 640px) {
      .toast-container { bottom: 16px; right: 16px; left: 16px; max-width: none; }
    }
  `]
})
export class ToastComponent {
  svc = inject(ToastService);

  icon(type: string): string {
    const icons: Record<string, string> = {
      success: `<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>`,
      error:   `<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>`,
      warning: `<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>`,
      info:    `<svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>`,
    };
    return icons[type] ?? icons['info'];
  }
}
