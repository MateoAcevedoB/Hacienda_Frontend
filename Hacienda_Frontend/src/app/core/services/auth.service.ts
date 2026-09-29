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

/**
 * Única clase que conoce el JWT: lo pide al backend, lo guarda, lo entrega
 * al interceptor y lo elimina al cerrar sesión.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly sesion = signal<Sesion | null>(this.leerSesionGuardada());

  /** Nombre y rol del usuario autenticado (null si no hay sesión). */
  readonly usuario = computed<UsuarioActual | null>(() => {
    const actual = this.sesion();
    return actual ? { nombre: actual.nombre, rol: actual.rol } : null;
  });

  login(credenciales: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(LOGIN_URL, credenciales)
      .pipe(tap((respuesta) => this.guardarSesion(respuesta)));
  }

  /** Cierre voluntario: limpia la sesión y vuelve al login. */
  logout(): void {
    this.limpiarSesion();
    void this.router.navigate(['/login']);
  }

  /** Cierre forzado por un 401 en un endpoint protegido. */
  sesionInvalida(): void {
    this.limpiarSesion();
    void this.router.navigate(['/login'], { queryParams: { motivo: 'sesion-invalida' } });
  }

  /** true si hay token y todavía no ha vencido (expiresAt). */
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

  /** Solo para el interceptor HTTP. Los componentes no deben llamarlo. */
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
      // Sin almacenamiento disponible: la sesión vive solo hasta recargar.
    }
  }

  private limpiarSesion(): void {
    this.sesion.set(null);
    try {
      localStorage.removeItem(CLAVE_SESION);
    } catch {
      // Nada que limpiar si el almacenamiento no está disponible.
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
      // JSON corrupto o almacenamiento bloqueado: se trata como sin sesión.
    }
    return null;
  }
}
