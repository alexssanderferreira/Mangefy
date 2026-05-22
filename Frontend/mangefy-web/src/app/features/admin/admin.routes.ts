import { Routes } from '@angular/router';
import { AdminShellComponent } from './shell/admin-shell.component';

export const ADMIN_ROUTES: Routes = [
  {
    path: '',
    component: AdminShellComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent),
      },
      {
        path: 'tenants',
        loadComponent: () => import('./tenants/tenant-list/tenant-list.component').then(m => m.TenantListComponent),
      },
      {
        path: 'tenants/:id',
        loadComponent: () => import('./tenants/tenant-detail/tenant-detail.component').then(m => m.TenantDetailComponent),
      },
      {
        path: 'plans',
        loadComponent: () => import('./plans/plans.component').then(m => m.PlansComponent),
      },
      {
        path: 'business-types',
        loadComponent: () => import('./business-types/business-type-list.component').then(m => m.BusinessTypeListComponent),
      },
      {
        path: 'business-types/:id',
        loadComponent: () => import('./business-types/business-type-detail.component').then(m => m.BusinessTypeDetailComponent),
      },
      {
        path: 'owners',
        loadComponent: () => import('./owners/owner-list/owner-list.component').then(m => m.OwnerListComponent),
      },
      {
        path: 'owners/:id',
        loadComponent: () => import('./owners/owner-detail/owner-detail.component').then(m => m.OwnerDetailComponent),
      },
      {
        path: 'suppliers',
        loadComponent: () => import('./suppliers/suppliers.component').then(m => m.SuppliersComponent),
      },
    ],
  },
];
