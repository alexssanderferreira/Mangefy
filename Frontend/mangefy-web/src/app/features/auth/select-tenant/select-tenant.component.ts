import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { TenantOption } from '../../../core/models/auth.models';

@Component({
  selector: 'app-select-tenant',
  standalone: true,
  template: `
    <div class="page">
      <div class="card">
        <div class="brand">
          <div class="brand-icon">🍽</div>
          <span class="brand-name">Mangefy</span>
        </div>

        <h1>Selecionar estabelecimento</h1>
        <p class="subtitle">Escolha em qual estabelecimento deseja entrar</p>

        @if (error()) {
          <div class="alert-error">{{ error() }}</div>
        }

        <div class="tenant-list">
          @for (t of tenants; track t.tenantId) {
            <button class="tenant-item" (click)="select(t)" [disabled]="loading()">
              <div class="tenant-av">{{ t.tenantName[0] }}</div>
              <div class="tenant-info">
                <span class="tenant-name">{{ t.tenantName }}</span>
                <span class="tenant-slug">{{ t.tenantSlug }}</span>
              </div>
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="9 18 15 12 9 6"/></svg>
            </button>
          }
        </div>

        <button class="back-link" (click)="router.navigate(['/auth/login'])">
          ← Voltar ao login
        </button>
      </div>
    </div>
  `,
  styles: [`
    :host { --accent: #f5c400; }

    .page {
      min-height: 100vh; background: #f5f5f7;
      display: flex; align-items: center; justify-content: center; padding: 24px;
    }

    .card {
      background: #fff; border-radius: 16px; padding: 40px;
      width: 100%; max-width: 440px;
      box-shadow: 0 4px 32px rgba(0,0,0,.08);
    }

    .brand { display: flex; align-items: center; gap: 10px; margin-bottom: 28px; }
    .brand-icon {
      width: 36px; height: 36px; background: #0a0a0a; border-radius: 9px;
      display: flex; align-items: center; justify-content: center; font-size: 18px;
    }
    .brand-name { font-size: 18px; font-weight: 700; color: #0a0a0a; }

    h1 { font-size: 20px; font-weight: 700; color: #0a0a0a; margin-bottom: 6px; }
    .subtitle { font-size: 13px; color: #aaa; margin-bottom: 24px; }

    .alert-error {
      background: #fef2f2; color: #b91c1c; border: 1px solid #fecaca;
      border-radius: 8px; padding: 10px 14px; font-size: 13px; margin-bottom: 16px;
    }

    .tenant-list { display: flex; flex-direction: column; gap: 8px; margin-bottom: 24px; }

    .tenant-item {
      display: flex; align-items: center; gap: 12px;
      padding: 14px 16px; border: 1.5px solid #e8e8ec; border-radius: 12px;
      background: #fff; cursor: pointer; text-align: left; width: 100%;
      transition: all .15s;
      &:hover:not(:disabled) { border-color: #0a0a0a; background: #fafafa; }
      &:disabled { opacity: .5; cursor: not-allowed; }
    }

    .tenant-av {
      width: 40px; height: 40px; border-radius: 10px; flex-shrink: 0;
      background: #0a0a0a; color: var(--accent);
      display: flex; align-items: center; justify-content: center;
      font-size: 16px; font-weight: 800;
    }

    .tenant-info { flex: 1; display: flex; flex-direction: column; gap: 2px; }
    .tenant-name { font-size: 14px; font-weight: 700; color: #111; }
    .tenant-slug { font-size: 11px; color: #bbb; }

    .back-link {
      background: none; border: none; color: #888; font-size: 13px;
      cursor: pointer; padding: 0;
      &:hover { color: #333; }
    }
  `]
})
export class SelectTenantComponent implements OnInit {
  private auth = inject(AuthService);
  router = inject(Router);

  tenants: TenantOption[] = [];
  email    = '';
  password = '';
  loading  = signal(false);
  error    = signal('');

  ngOnInit() {
    const pending = this.auth.peekPendingLogin();
    if (!pending?.tenants?.length) {
      this.router.navigate(['/auth/login']);
      return;
    }
    this.tenants  = pending.tenants;
    this.email    = pending.email;
    this.password = pending.password;
  }

  select(tenant: TenantOption) {
    this.loading.set(true);
    this.error.set('');
    // Consome (e limpa) as credenciais pendentes ao iniciar o login final
    this.auth.takePendingLogin();
    this.auth.login(
      { email: this.email, password: this.password, tenantSlug: tenant.tenantSlug },
      this.tenants
    ).subscribe({
      next: () => { this.password = ''; this.router.navigate(['/app/dashboard']); },
      error: (err) => {
        this.password = '';
        this.error.set(err?.error?.message ?? 'Erro ao entrar.');
        this.loading.set(false);
      },
    });
  }
}
