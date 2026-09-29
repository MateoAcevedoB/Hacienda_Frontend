import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { authInterceptor } from '../core/interceptors/auth.interceptor';
import { errorInterceptor } from '../core/interceptors/error.interceptor';

export const CLAVE_SESION = 'hacienda-tobe.sesion';

/** Guarda una sesión vigente en localStorage antes de crear AuthService. */
export function guardarSesion(rol: string, nombre = rol, token = 'jwt-de-prueba'): void {
  localStorage.setItem(
    CLAVE_SESION,
    JSON.stringify({ token, nombre, rol, expiresAt: new Date(Date.now() + 3_600_000).toISOString() }),
  );
}

/** Proveedores comunes: HTTP simulado (no llama al backend real) con los interceptores reales. */
export function proveedoresHttpDePrueba() {
  return [
    provideHttpClient(withInterceptors([errorInterceptor, authInterceptor])),
    provideHttpClientTesting(),
    provideRouter([]),
  ];
}

/** Escribe en un input/select y dispara el evento para que el formulario reactivo lo lea. */
export function escribir(raiz: HTMLElement, selector: string, valor: string): void {
  const el = raiz.querySelector<HTMLInputElement | HTMLSelectElement>(selector);
  if (!el) {
    throw new Error(`No existe ${selector}`);
  }
  el.value = valor;
  el.dispatchEvent(new Event(el instanceof HTMLSelectElement ? 'change' : 'input'));
}

export function texto(raiz: HTMLElement): string {
  return (raiz.textContent ?? '').replace(/\s+/g, ' ');
}
