import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, Sesion, UsuarioActual } from '../models/auth.models';

export const LOGIN_URL = `${environment.apiUrl}/api/auth/login`;

const CLAVE_SESION = 'hacienda-tobe.sesion';

function esSesion(valor: unknown): valor is Sesion {
  if (typeof valor !== 'object' || valor === null) {
    return false;
  }
  const candidata = valor as Record<string, unknown>;
  return (
    typeof candidata['token'] === 'string' &&
    typeof candidata['expiresAt'] === 'string' &&
    typeof candidata['nombre'] === 'string' &&
    typeof candidata['rol'] === 'string'
  );
}

function sesionVigente(sesion: Sesion): boolean {
  return Date.parse(sesion.expiresAt) > Date.now();
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly sesion = signal<Sesion | null>(this.leerSesionGuardada());

  readonly usuario = computed<UsuarioActual | null>(() => {
    const actual = this.sesion();
    return actual ? { nombre: actual.nombre, rol: actual.rol } : null;
  });

  login(credenciales: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(LOGIN_URL, credenciales)
      .pipe(tap((respuesta) => this.guardarSesion(respuesta)));
  }

  logout(): void {
    this.limpiarSesion();
    void this.router.navigate(['/login']);
  }

  sesionInvalida(): void {
    this.limpiarSesion();
    void this.router.navigate(['/login'], { queryParams: { motivo: 'sesion-invalida' } });
  }

  estaAutenticado(): boolean {
    const actual = this.sesion();
    if (actual === null) {
      return false;
    }
    if (!sesionVigente(actual)) {
      this.limpiarSesion();
      return false;
    }
    return true;
  }

  obtenerToken(): string | null {
    return this.estaAutenticado() ? (this.sesion()?.token ?? null) : null;
  }

  private guardarSesion(respuesta: LoginResponse): void {
    const nueva: Sesion = {
      token: respuesta.token,
      expiresAt: respuesta.expiresAt,
      nombre: respuesta.nombre,
      rol: respuesta.rol,
    };
    this.sesion.set(nueva);
    try {
      localStorage.setItem(CLAVE_SESION, JSON.stringify(nueva));
    } catch {
    }
  }

  private limpiarSesion(): void {
    this.sesion.set(null);
    try {
      localStorage.removeItem(CLAVE_SESION);
    } catch {
    }
  }

  private leerSesionGuardada(): Sesion | null {
    try {
      const guardada = localStorage.getItem(CLAVE_SESION);
      if (guardada === null) {
        return null;
      }
      const datos: unknown = JSON.parse(guardada);
      if (esSesion(datos) && sesionVigente(datos)) {
        return datos;
      }
      localStorage.removeItem(CLAVE_SESION);
    } catch {
    }
    return null;
  }
}
