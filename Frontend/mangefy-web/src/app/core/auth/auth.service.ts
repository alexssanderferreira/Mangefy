import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import {
  LoginRequest, AdminLoginRequest, ResolveTenantsRequest, TenantOption,
  LoginApiResponse, AdminLoginApiResponse,
  CurrentUser
} from '../models/auth.models';
import { environment } from '../../../environments/environment';

const TOKEN_KEY   = 'mgf_token';
const USER_KEY    = 'mgf_user';
const TENANTS_KEY = 'mgf_pending_tenants'; // lista antes de selecionar tenant

interface PendingLogin {
  email: string;
  password: string;
  tenants: TenantOption[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _user = signal<CurrentUser | null>(this.loadFromStorage());

  /**
   * Credenciais retidas em memória entre o resolve-tenants e a seleção de
   * estabelecimento. NÃO persiste em storage/history. Limpa após uso ou logout.
   */
  private _pendingLogin: PendingLogin | null = null;

  readonly user     = this._user.asReadonly();
  readonly isAdmin  = computed(() => this._user()?.isAdmin ?? false);
  readonly loggedIn = computed(() => this._user() !== null);
  readonly availableTenants = computed(() => this._user()?.availableTenants ?? []);
  readonly hasMultipleTenants = computed(() => (this._user()?.availableTenants?.length ?? 0) > 1);

  constructor(private http: HttpClient, private router: Router) {}

  setPendingLogin(p: PendingLogin) { this._pendingLogin = p; }
  takePendingLogin(): PendingLogin | null {
    const p = this._pendingLogin;
    this._pendingLogin = null;
    return p;
  }
  peekPendingLogin(): PendingLogin | null { return this._pendingLogin; }

  // Etapa 1: verifica credenciais e devolve lista de tenants
  resolveTenants(req: ResolveTenantsRequest) {
    return this.http.post<TenantOption[]>(`${environment.apiUrl}/auth/resolve-tenants`, req);
  }

  // Etapa 2: faz login no tenant escolhido
  login(req: LoginRequest, availableTenants: TenantOption[] = []) {
    return this.http.post<LoginApiResponse>(`${environment.apiUrl}/auth/login`, req).pipe(
      tap(res => {
        const chosen = availableTenants.find(t => t.tenantSlug === req.tenantSlug);
        const user: CurrentUser = {
          token:            res.accessToken,
          expiresAt:        res.expiresAt,
          employeeId:       res.employeeId,
          ownerId:          res.ownerId,
          tenantId:         res.tenantId,
          tenantName:       chosen?.tenantName ?? null,
          tenantSlug:       req.tenantSlug,
          name:             res.name,
          permissions:      res.permissions,
          isAdmin:          false,
          isOwner:          res.isOwner,
          availableTenants,
        };
        this.persist(user);
      })
    );
  }

  // Troca de tenant sem redigitar senha
  switchTenant(targetTenantSlug: string) {
    return this.http.post<LoginApiResponse>(`${environment.apiUrl}/auth/switch-tenant`, { targetTenantSlug }).pipe(
      tap(res => {
        const current = this._user()!;
        const chosen = current.availableTenants.find(t => t.tenantSlug === targetTenantSlug);
        const user: CurrentUser = {
          ...current,
          token:       res.accessToken,
          expiresAt:   res.expiresAt,
          employeeId:  res.employeeId,
          ownerId:     res.ownerId,
          tenantId:    res.tenantId,
          tenantName:  chosen?.tenantName ?? null,
          tenantSlug:  targetTenantSlug,
          name:        res.name,
          permissions: res.permissions,
          isOwner:     res.isOwner,
        };
        this.persist(user);
      })
    );
  }

  adminLogin(req: AdminLoginRequest) {
    return this.http.post<AdminLoginApiResponse>(`${environment.apiUrl}/auth/admin/login`, req).pipe(
      tap(res => {
        const user: CurrentUser = {
          token:            res.accessToken,
          expiresAt:        res.expiresAt,
          employeeId:       null,
          ownerId:          null,
          tenantId:         null,
          tenantName:       null,
          tenantSlug:       null,
          name:             res.email,
          permissions:      [],
          isAdmin:          true,
          isOwner:          false,
          availableTenants: [],
        };
        this.persist(user);
      })
    );
  }

  logout(expired = false) {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(TENANTS_KEY);
    this._pendingLogin = null;
    this._user.set(null);
    this.router.navigate(['/auth/login'], expired ? { queryParams: { expired: '1' } } : {});
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  hasPermission(permission: string): boolean {
    if (this._user()?.isAdmin) return true;
    return this._user()?.permissions.includes(permission) ?? false;
  }

  isSessionValid(): boolean {
    const user = this._user();
    if (!user) return false;
    return new Date(user.expiresAt) > new Date();
  }

  private persist(user: CurrentUser) {
    localStorage.setItem(TOKEN_KEY, user.token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this._user.set(user);
  }

  private loadFromStorage(): CurrentUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    const user: CurrentUser = JSON.parse(raw);
    if (new Date(user.expiresAt) <= new Date()) {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
      return null;
    }
    return user;
  }
}
