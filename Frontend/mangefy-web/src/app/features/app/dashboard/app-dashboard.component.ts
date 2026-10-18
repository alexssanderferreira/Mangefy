import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-app-dashboard',
  standalone: true,
  template: `
    <div class="dashboard-page">
      <h1>Bem-vindo, {{ auth.user()?.name }}</h1>
      <p>Painel do estabelecimento — em breve.</p>
    </div>
  `,
  styles: [`
    .dashboard-page {
      padding: 48px 32px;
      h1 { font-size: 24px; font-weight: 700; margin-bottom: 8px; }
      p  { color: var(--text-secondary); }
    }
  `]
})
export class AppDashboardComponent {
  auth = inject(AuthService);
}
