import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-activate-owner',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="activate-page">
      <div class="card">
        <div class="brand">
          <div class="brand-icon">🍽</div>
          <span class="brand-name">Mangefy</span>
        </div>

        <h1>Ativar conta de dono</h1>
        <p class="sub">Defina sua senha para ativar sua conta e começar a gerenciar seus estabelecimentos.</p>

        @if (!token()) {
          <div class="alert-error">Link de ativação inválido. Solicite um novo ao administrador.</div>
        } @else if (success()) {
          <div class="alert-success">
            ✓ Conta ativada com sucesso! Redirecionando para o login…
          </div>
        } @else {
          @if (error()) {
            <div class="alert-error">{{ error() }}</div>
          }
          <form [formGroup]="form" (ngSubmit)="submit()">
            <div class="field">
              <label>Nova senha</label>
              <input type="password" formControlName="newPassword" placeholder="Mínimo 8 caracteres" autocomplete="new-password" />
            </div>
            <div class="field">
              <label>Confirmar senha</label>
              <input type="password" formControlName="confirmPassword" autocomplete="new-password" />
            </div>
            <button type="submit" [disabled]="loading() || form.invalid">
              {{ loading() ? 'Ativando…' : 'Ativar conta' }}
            </button>
          </form>
        }

        <a routerLink="/auth/login" class="back">Voltar ao login</a>
      </div>
    </div>
  `,
  styles: [`
    .activate-page { min-height: 100vh; display: flex; align-items: center; justify-content: center; background: #f6f7fb; padding: 24px; }
    .card { background: #fff; border-radius: 16px; padding: 40px; max-width: 460px; width: 100%; box-shadow: 0 8px 32px rgba(0,0,0,0.08); }
    .brand { display: flex; align-items: center; gap: 8px; margin-bottom: 24px; }
    .brand-icon { font-size: 28px; }
    .brand-name { font-size: 20px; font-weight: 700; }
    h1 { margin: 0 0 8px; font-size: 22px; }
    .sub { color: #666; margin: 0 0 24px; font-size: 14px; }
    .field { margin-bottom: 16px; }
    .field label { display: block; font-size: 13px; font-weight: 500; margin-bottom: 6px; }
    .field input { width: 100%; padding: 10px 12px; border: 1px solid #d4d4d8; border-radius: 8px; font-size: 14px; box-sizing: border-box; }
    .field input:focus { outline: none; border-color: #6366f1; }
    button { width: 100%; padding: 12px; background: #6366f1; color: #fff; border: 0; border-radius: 8px; font-size: 14px; font-weight: 500; cursor: pointer; }
    button:disabled { opacity: 0.6; cursor: not-allowed; }
    .alert-error { background: #fef2f2; color: #b91c1c; padding: 12px; border-radius: 8px; margin-bottom: 16px; font-size: 13px; }
    .alert-success { background: #f0fdf4; color: #15803d; padding: 12px; border-radius: 8px; margin-bottom: 16px; font-size: 13px; }
    .back { display: block; text-align: center; margin-top: 16px; color: #6366f1; text-decoration: none; font-size: 13px; }
  `]
})
export class ActivateOwnerComponent {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);

  token = signal<string | null>(this.route.snapshot.queryParamMap.get('token'));
  loading = signal(false);
  error = signal<string | null>(null);
  success = signal(false);

  form = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  });

  submit() {
    const { newPassword, confirmPassword } = this.form.value;
    if (newPassword !== confirmPassword) {
      this.error.set('As senhas não conferem.');
      return;
    }
    const tkn = this.token();
    if (!tkn) return;

    this.loading.set(true);
    this.error.set(null);

    this.http.post(`${environment.apiUrl}/auth/owner/activate`, {
      token: tkn,
      newPassword
    }).subscribe({
      next: () => {
        this.loading.set(false);
        this.success.set(true);
        setTimeout(() => this.router.navigate(['/auth/login']), 1800);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.detail ?? err?.error?.title ?? 'Não foi possível ativar a conta. O link pode ter expirado.');
      }
    });
  }
}
