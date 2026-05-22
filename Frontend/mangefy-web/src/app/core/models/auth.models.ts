export interface LoginRequest {
  email: string;
  password: string;
  tenantSlug: string;
}

export interface ResolveTenantsRequest {
  email: string;
  password: string;
}

export interface TenantOption {
  tenantId: string;
  tenantSlug: string;
  tenantName: string;
  logoUrl: string | null;
}

export interface AdminLoginRequest {
  email: string;
  password: string;
}

// Resposta da API — tenant
export interface LoginApiResponse {
  accessToken: string;
  expiresAt: string;
  employeeId: string | null;
  ownerId: string | null;
  tenantId: string;
  name: string;
  isOwner: boolean;
  permissions: string[];
}

// Resposta da API — admin
export interface AdminLoginApiResponse {
  accessToken: string;
  expiresAt: string;
  email: string;
}

// Estado interno do AuthService
export interface CurrentUser {
  token: string;
  expiresAt: string; // ISO 8601
  employeeId: string | null;
  ownerId: string | null;
  tenantId: string | null;
  tenantName: string | null;
  tenantSlug: string | null;
  name: string;
  permissions: string[];
  isAdmin: boolean;
  isOwner: boolean;
  availableTenants: TenantOption[]; // tenants disponíveis para switch
}
