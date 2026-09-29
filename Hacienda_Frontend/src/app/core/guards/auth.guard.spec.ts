import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';

import { guardarSesion, proveedoresHttpDePrueba } from '../../testing/helpers';
import { adminGuard, authGuard, sinSesionGuard } from './auth.guard';

function ejecutar(guard: typeof authGuard): boolean | UrlTree {
  return TestBed.runInInjectionContext(() => guard({} as never, {} as never)) as boolean | UrlTree;
}

function preparar(rol?: string): void {
  localStorage.clear();
  if (rol) {
    guardarSesion(rol);
  }
  TestBed.configureTestingModule({ providers: proveedoresHttpDePrueba() });
}

describe('guards', () => {
  it('authGuard redirige a /login sin sesión', () => {
    preparar();
    expect(TestBed.inject(Router).serializeUrl(ejecutar(authGuard) as UrlTree)).toBe('/login');
  });

  it('authGuard deja pasar con sesión vigente', () => {
    preparar('empleado');
    expect(ejecutar(authGuard)).toBe(true);
  });

  it('sinSesionGuard manda al dashboard si ya hay sesión', () => {
    preparar('empleado');
    expect(TestBed.inject(Router).serializeUrl(ejecutar(sinSesionGuard) as UrlTree)).toBe('/dashboard');
  });

  it('adminGuard solo permite al admin', () => {
    preparar('admin');
    expect(ejecutar(adminGuard)).toBe(true);
    TestBed.resetTestingModule();
    preparar('empleado');
    expect(TestBed.inject(Router).serializeUrl(ejecutar(adminGuard) as UrlTree)).toBe('/dashboard');
  });
});
