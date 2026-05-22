import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="login-page">

      <!-- Left panel — branding -->
      <div class="login-left">
        <div class="left-content">
          <div class="brand">
            <div class="brand-icon">🍽</div>
            <span class="brand-name">Mangefy</span>
          </div>
          <h2 class="left-headline">Gestão completa<br>do seu restaurante.</h2>
          <p class="left-sub">Comandas, pedidos, estoque, caixa e muito mais — tudo num só lugar.</p>

          <div class="feature-list">
            @for (f of features; track f) {
              <div class="feature-item">
                <span class="feature-dot"></span>
                <span>{{ f }}</span>
              </div>
            }
          </div>
        </div>
        <div class="deco-circle deco-1"></div>
        <div class="deco-circle deco-2"></div>
        <div class="deco-circle deco-3"></div>
      </div>

      <!-- Right panel — form -->
      <div class="login-right">
        <div class="form-wrapper">
          <div class="form-header">
            <h1>Bem-vindo de volta</h1>
            <p>Entre com as suas credenciais para continuar</p>
          </div>

          @if (error()) {
            <div class="alert-error">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              {{ error() }}
            </div>
          }

          <form [formGroup]="form" (ngSubmit)="submit()">
            <div class="field">
              <label>E-mail</label>
              <div class="input-wrap">
                <svg class="input-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                <input type="email" formControlName="email" placeholder="seu@restaurante.com" autocomplete="email" />
              </div>
            </div>

            <div class="field">
              <label>Senha</label>
              <div class="input-wrap">
                <svg class="input-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                <input [type]="showPw() ? 'text' : 'password'" formControlName="password" placeholder="••••••••" autocomplete="current-password" />
                <button type="button" class="toggle-pw" (click)="showPw.set(!showPw())">
                  @if (showPw()) {
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
                  } @else {
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                  }
                </button>
              </div>
            </div>

            @if (askSlug()) {
              <div class="field">
                <label>Estabelecimento (slug)</label>
                <div class="input-wrap">
                  <svg class="input-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/></svg>
                  <input type="text" formControlName="tenantSlug" placeholder="ex: cantina-ze" autocomplete="off" />
                </div>
                <small class="hint">Informe o slug do seu estabelecimento (fornecido pelo dono).</small>
              </div>
            }

            <button class="btn-submit" type="submit" [disabled]="loading() || form.invalid" [class.loading]="loading()">
              @if (loading()) { <span class="spinner"></span> Entrando... }
              @else { Entrar no sistema }
            </button>
          </form>
        </div>
      </div>

    </div>
  `,
  styles: [`
    :host {
      --accent: #f5c400;
      --accent-dark: #d4a900;
    }

    .login-page { display: flex; min-height: 100vh; }

    /* ── Left ── */
    .login-left {
      flex: 1;
      background: #0a0a0a;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 48px;
      position: relative;
      overflow: hidden;

      @media (max-width: 768px) { display: none; }
    }

    .left-content { position: relative; z-index: 1; max-width: 420px; }

    .brand { display: flex; align-items: center; gap: 10px; margin-bottom: 48px; }
    .brand-icon {
      width: 42px; height: 42px;
      background: var(--accent);
      border-radius: 10px;
      display: flex; align-items: center; justify-content: center;
      font-size: 20px;
    }
    .brand-name { font-size: 22px; font-weight: 700; color: #fff; }

    .left-headline {
      font-size: 36px; font-weight: 700;
      color: #fff; line-height: 1.2;
      margin-bottom: 16px;
    }

    .left-sub {
      font-size: 15px; color: rgba(255,255,255,.45);
      line-height: 1.6; margin-bottom: 36px;
    }

    .feature-list { display: flex; flex-direction: column; gap: 12px; }
    .feature-item {
      display: flex; align-items: center; gap: 12px;
      font-size: 14px; color: rgba(255,255,255,.65);
    }
    .feature-dot {
      width: 7px; height: 7px; border-radius: 50%;
      background: var(--accent); flex-shrink: 0;
    }

    .deco-circle {
      position: absolute; border-radius: 50%;
      background: var(--accent); pointer-events: none;
    }
    .deco-1 { width: 420px; height: 420px; bottom: -130px; right: -130px; opacity: .07; }
    .deco-2 { width: 220px; height: 220px; top: -60px;    left: -60px;   opacity: .05; }
    .deco-3 { width: 140px; height: 140px; top: 42%;      right: 24px;   opacity: .04; }

    /* ── Right ── */
    .login-right {
      width: 45%;
      min-width: 420px;
      display: flex; align-items: center; justify-content: center;
      background: #fff; padding: 48px;

      @media (max-width: 768px) { width: 100%; min-width: unset; }
    }

    .form-wrapper { width: 100%; max-width: 400px; }

    .form-header {
      margin-bottom: 32px;
      h1 { font-size: 24px; font-weight: 700; color: #0a0a0a; margin-bottom: 6px; }
      p  { font-size: 14px; color: var(--text-secondary); }
    }

    .alert-error {
      display: flex; align-items: center; gap: 8px;
      background: #fef2f2; color: #b91c1c;
      border: 1px solid #fecaca;
      border-radius: 8px; padding: 10px 14px;
      font-size: 13px; margin-bottom: 20px;
    }

    .field {
      display: flex; flex-direction: column; gap: 6px; margin-bottom: 18px;
      label { font-size: 13px; font-weight: 500; color: #0a0a0a; }
    }

    .input-wrap {
      position: relative;
      .input-icon {
        position: absolute; left: 13px; top: 50%;
        transform: translateY(-50%); color: #b0b0b8; pointer-events: none;
      }
      input {
        width: 100%;
        padding: 11px 42px 11px 40px;
        border: 1.5px solid #e5e7eb; border-radius: 8px;
        font-size: 14px; font-family: inherit;
        color: #0a0a0a; background: #fafafa; outline: none;
        transition: border-color .15s, background .15s;
        &:focus { border-color: var(--accent); background: #fff; box-shadow: 0 0 0 3px rgba(245,196,0,.12); }
        &::placeholder { color: #b0b0b8; }
      }
    }

    .toggle-pw {
      position: absolute; right: 12px; top: 50%; transform: translateY(-50%);
      background: none; border: none; color: #b0b0b8;
      padding: 4px; display: flex; align-items: center; cursor: pointer;
      transition: color .15s;
      &:hover { color: #0a0a0a; }
    }

    .btn-submit {
      width: 100%; padding: 12px;
      background: #0a0a0a; color: var(--accent);
      border: none; border-radius: 8px;
      font-size: 15px; font-weight: 700;
      font-family: inherit; cursor: pointer;
      display: flex; align-items: center; justify-content: center; gap: 8px;
      margin-top: 8px;
      transition: background .15s, opacity .15s;
      letter-spacing: .2px;

      &:hover:not(:disabled) { background: #222; }
      &:disabled { opacity: .5; cursor: not-allowed; }
    }

    .spinner {
      width: 16px; height: 16px;
      border: 2px solid rgba(245,196,0,.3);
      border-top-color: var(--accent);
      border-radius: 50%;
      animation: spin .6s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }

    .hint { font-size: 11px; color: #888; margin-top: 4px; }
  `]
})
export class LoginComponent {
  private fb     = inject(FormBuilder);
  private auth   = inject(AuthService);
  private router = inject(Router);
  private route  = inject(ActivatedRoute);

  loading = signal(false);
  error   = signal(this.route.snapshot.queryParamMap.get('expired') ? 'Sua sessão expirou. Entre novamente.' : '');
  showPw  = signal(false);
  askSlug = signal(false);

  features = [
    'Gestão de comandas e pedidos em tempo real',
    'KDS — Kitchen Display System integrado',
    'Controlo de estoque automático',
    'Relatórios de vendas e caixa',
    'Reservas e gestão de mesas',
  ];

  form = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
    tenantSlug: [''],
  });

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    const { email, password, tenantSlug } = this.form.value;

    // Caminho 2 (Employee): slug informado → login direto
    if (this.askSlug() && tenantSlug?.trim()) {
      this.auth.login({ email: email!, password: password!, tenantSlug: tenantSlug.trim() }, []).subscribe({
        next: () => this.router.navigate(['/app/dashboard']),
        error: (err) => {
          this.error.set(err?.error?.message ?? 'Credenciais inválidas.');
          this.loading.set(false);
        },
      });
      return;
    }

    // Caminho 1 (Owner): resolve tenants pelo email/senha
    this.auth.resolveTenants({ email: email!, password: password! }).subscribe({
      next: (tenants) => {
        if (tenants.length === 1) {
          this.auth.login({ email: email!, password: password!, tenantSlug: tenants[0].tenantSlug }, tenants).subscribe({
            next: () => this.router.navigate(['/app/dashboard']),
            error: () => { this.error.set('Credenciais inválidas.'); this.loading.set(false); },
          });
        } else {
          this.auth.setPendingLogin({ email: email!, password: password!, tenants });
          this.router.navigate(['/auth/select-tenant']);
        }
      },
      error: (err) => {
        // 403 com mensagem específica = não é dono; mostra campo de slug para Employee
        const isOwnerPathRejected = err?.status === 403 || err?.status === 404;
        if (isOwnerPathRejected && !this.askSlug()) {
          this.askSlug.set(true);
          this.error.set('Não é dono? Informe o slug do seu estabelecimento e tente novamente.');
        } else {
          this.error.set(err?.error?.message ?? 'Credenciais inválidas.');
        }
        this.loading.set(false);
      },
    });
  }
}
