import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },

  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES),
  },

  {
    // URL secreta do painel Admin SaaS — não referenciada em nenhuma tela
    path: 'plataforma-mgf-console',
    loadComponent: () => import('./features/auth/admin-login/admin-login.component').then(m => m.AdminLoginComponent),
  },

  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES),
  },

  {
    path: 'app',
    canActivate: [authGuard],
    loadChildren: () => import('./features/app/app.routes').then(m => m.APP_ROUTES),
  },

  // Qualquer URL desconhecida vai para o login do tenant — sem pistas
  { path: '**', redirectTo: '/auth/login' },
];
