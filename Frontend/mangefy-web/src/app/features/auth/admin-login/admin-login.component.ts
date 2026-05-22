import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-admin-login',
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
            <span class="brand-badge">PLATFORM</span>
          </div>
          <h2 class="left-headline">Painel de<br>Administração.</h2>
          <p class="left-sub">Gestão centralizada de todos os estabelecimentos, planos e assinaturas da plataforma.</p>

          <div class="warning-box">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
            <span>Acesso restrito. Uso exclusivo da equipa Mangefy.</span>
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
            <div class="form-icon">
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
            </div>
            <h1>Autenticação</h1>
            <p>Credenciais de acesso administrativo</p>
          </div>

          @if (error()) {
            <div class="alert-error">
              <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              {{ error() }}
            </div>
          }

          <form [formGroup]="form" (ngSubmit)="submit()">
            <div class="field">
              <label>E-mail</label>
              <div class="input-wrap">
                <svg class="input-icon" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                <input type="email" formControlName="email" placeholder="admin@mangefy.com.br" autocomplete="off" />
              </div>
            </div>

            <div class="field">
              <label>Senha</label>
              <div class="input-wrap">
                <svg class="input-icon" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                <input [type]="showPw() ? 'text' : 'password'" formControlName="password" placeholder="••••••••" autocomplete="off" />
                <button type="button" class="toggle-pw" (click)="showPw.set(!showPw())">
                  @if (showPw()) {
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
                  } @else {
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                  }
                </button>
              </div>
            </div>

            <button class="btn-submit" type="submit" [disabled]="loading() || form.invalid" [class.loading]="loading()">
              @if (loading()) { <span class="spinner"></span> Autenticando... }
              @else { Entrar }
            </button>
          </form>
        </div>
      </div>

    </div>
  `,
  styles: [`
    :host {
      --accent: #e03131;
      --accent-dark: #c92a2a;
    }

    .login-page { display: flex; min-height: 100vh; }

    /* ── Left ── */
    .login-left {
      flex: 1;
      background: #0f0505;
      display: flex; align-items: center; justify-content: center;
      padding: 48px; position: relative; overflow: hidden;

      @media (max-width: 768px) { display: none; }
    }

    .left-content { position: relative; z-index: 1; max-width: 400px; }

    .brand { display: flex; align-items: center; gap: 10px; margin-bottom: 48px; }
    .brand-icon {
      width: 42px; height: 42px;
      background: var(--accent);
      border-radius: 10px;
      display: flex; align-items: center; justify-content: center;
      font-size: 20px;
    }
    .brand-name  { font-size: 22px; font-weight: 700; color: #fff; }
    .brand-badge {
      font-size: 10px; font-weight: 700;
      background: rgba(224,49,49,.25);
      color: var(--accent);
      border: 1px solid rgba(224,49,49,.4);
      padding: 2px 8px; border-radius: 4px; letter-spacing: .8px;
    }

    .left-headline {
      font-size: 36px; font-weight: 700;
      color: #fff; line-height: 1.2; margin-bottom: 16px;
    }

    .left-sub {
      font-size: 15px; color: rgba(255,255,255,.4);
      line-height: 1.6; margin-bottom: 36px;
    }

    .warning-box {
      display: flex; align-items: flex-start; gap: 10px;
      background: rgba(224,49,49,.1);
      border: 1px solid rgba(224,49,49,.25);
      border-radius: 8px; padding: 12px 14px;
      font-size: 13px; color: rgba(255,255,255,.55);
      svg { color: var(--accent); flex-shrink: 0; margin-top: 1px; }
    }

    .deco-circle {
      position: absolute; border-radius: 50%;
      background: var(--accent); pointer-events: none;
    }
    .deco-1 { width: 420px; height: 420px; bottom: -130px; right: -130px; opacity: .08; }
    .deco-2 { width: 220px; height: 220px; top: -60px;    left: -60px;   opacity: .05; }
    .deco-3 { width: 140px; height: 140px; top: 42%;      right: 24px;   opacity: .04; }

    /* ── Right ── */
    .login-right {
      width: 45%;
      min-width: 420px;
      display: flex; align-items: center; justify-content: center;
      background: #111; padding: 48px;

      @media (max-width: 768px) { width: 100%; min-width: unset; }
    }

    .form-wrapper { width: 100%; max-width: 400px; }

    .form-header {
      margin-bottom: 32px;

      .form-icon {
        width: 44px; height: 44px;
        background: rgba(224,49,49,.15);
        border: 1px solid rgba(224,49,49,.3);
        border-radius: 10px;
        display: flex; align-items: center; justify-content: center;
        color: var(--accent);
        margin-bottom: 16px;
      }

      h1 { font-size: 22px; font-weight: 700; color: #fff; margin-bottom: 5px; }
      p  { font-size: 13px; color: rgba(255,255,255,.4); }
    }

    .alert-error {
      display: flex; align-items: center; gap: 8px;
      background: rgba(224,49,49,.1); color: #f87171;
      border: 1px solid rgba(224,49,49,.3);
      border-radius: 8px; padding: 10px 14px;
      font-size: 13px; margin-bottom: 20px;
    }

    .field {
      display: flex; flex-direction: column; gap: 6px; margin-bottom: 16px;
      label { font-size: 12px; font-weight: 500; color: rgba(255,255,255,.5); text-transform: uppercase; letter-spacing: .4px; }
    }

    .input-wrap {
      position: relative;
      .input-icon {
        position: absolute; left: 13px; top: 50%;
        transform: translateY(-50%); color: rgba(255,255,255,.2); pointer-events: none;
      }
      input {
        width: 100%;
        padding: 11px 42px 11px 40px;
        border: 1px solid rgba(255,255,255,.1); border-radius: 8px;
        font-size: 14px; font-family: inherit;
        color: #fff; background: rgba(255,255,255,.05); outline: none;
        transition: border-color .15s, background .15s;
        &:focus { border-color: var(--accent); background: rgba(255,255,255,.08); box-shadow: 0 0 0 3px rgba(224,49,49,.12); }
        &::placeholder { color: rgba(255,255,255,.2); }
      }
    }

    .toggle-pw {
      position: absolute; right: 12px; top: 50%; transform: translateY(-50%);
      background: none; border: none; color: rgba(255,255,255,.25);
      padding: 4px; display: flex; align-items: center; cursor: pointer;
      transition: color .15s;
      &:hover { color: rgba(255,255,255,.7); }
    }

    .btn-submit {
      width: 100%; padding: 12px;
      background: var(--accent); color: #fff;
      border: none; border-radius: 8px;
      font-size: 15px; font-weight: 700;
      font-family: inherit; cursor: pointer;
      display: flex; align-items: center; justify-content: center; gap: 8px;
      margin-top: 10px;
      transition: background .15s, opacity .15s;

      &:hover:not(:disabled) { background: var(--accent-dark); }
      &:disabled { opacity: .4; cursor: not-allowed; }
    }

    .spinner {
      width: 16px; height: 16px;
      border: 2px solid rgba(255,255,255,.25);
      border-top-color: #fff;
      border-radius: 50%;
      animation: spin .6s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class AdminLoginComponent {
  private fb     = inject(FormBuilder);
  private auth   = inject(AuthService);
  private router = inject(Router);

  loading = signal(false);
  error   = signal('');
  showPw  = signal(false);

  form = this.fb.group({
    email:    ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  submit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.error.set('');
    const { email, password } = this.form.value;
    this.auth.adminLogin({ email: email!, password: password! }).subscribe({
      next: () => this.router.navigate(['/admin/dashboard']),
      error: () => { this.error.set('Credenciais inválidas.'); this.loading.set(false); },
    });
  }
}
