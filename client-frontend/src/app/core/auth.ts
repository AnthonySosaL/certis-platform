import { Service, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API_BASE_URL } from './api-config';

const STORAGE_KEY = 'auth-token';

interface AuthResponse {
  token: string;
  email: string;
  expiresAtUtc: string;
  isAdmin: boolean;
  isTutor: boolean;
}

@Service()
export class Auth {
  private readonly http = inject(HttpClient);

  private readonly _token = signal<string | null>(this.readStoredToken());
  private readonly _email = signal<string | null>(this.readStoredEmail());
  private readonly _isAdmin = signal<boolean>(this.readStoredIsAdmin());
  private readonly _isTutor = signal<boolean>(this.readStoredIsTutor());

  readonly token = this._token.asReadonly();
  readonly email = this._email.asReadonly();
  readonly isAdmin = this._isAdmin.asReadonly();
  readonly isTutor = this._isTutor.asReadonly();
  readonly canManage = computed(() => this._isAdmin() || this._isTutor());
  readonly isAuthenticated = computed(() => this._token() !== null);

  async register(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<AuthResponse>(`${API_BASE_URL}/api/auth/register`, { email, password }),
    );
    this.setSession(response);
  }

  async login(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<AuthResponse>(`${API_BASE_URL}/api/auth/login`, { email, password }),
    );
    this.setSession(response);
  }

  logout(): void {
    this._token.set(null);
    this._email.set(null);
    this._isAdmin.set(false);
    this._isTutor.set(false);
    localStorage.removeItem(STORAGE_KEY);
  }

  private setSession(response: AuthResponse): void {
    this._token.set(response.token);
    this._email.set(response.email);
    this._isAdmin.set(response.isAdmin);
    this._isTutor.set(response.isTutor);
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({
        token: response.token,
        email: response.email,
        isAdmin: response.isAdmin,
        isTutor: response.isTutor,
      }),
    );
  }

  private readStoredToken(): string | null {
    return this.readStoredSession()?.token ?? null;
  }

  private readStoredEmail(): string | null {
    return this.readStoredSession()?.email ?? null;
  }

  private readStoredIsAdmin(): boolean {
    return this.readStoredSession()?.isAdmin ?? false;
  }

  private readStoredIsTutor(): boolean {
    return this.readStoredSession()?.isTutor ?? false;
  }

  private readStoredSession(): { token: string; email: string; isAdmin: boolean; isTutor: boolean } | null {
    if (typeof localStorage === 'undefined') return null;
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }
}
