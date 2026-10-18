import { Routes } from '@angular/router';
import { AppShellComponent } from './shell/app-shell.component';

export const APP_ROUTES: Routes = [
  {
    path: '',
    component: AppShellComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/app-dashboard.component').then(m => m.AppDashboardComponent),
      },
      {
        path: 'tables',
        loadComponent: () => import('./tables/tables.component').then(m => m.TablesComponent),
      },
    ],
  },
];
