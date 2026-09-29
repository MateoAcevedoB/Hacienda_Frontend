import { TestBed } from '@angular/core/testing';

import { guardarSesion, proveedoresHttpDePrueba } from '../../testing/helpers';
import { PermisosService } from './permisos.service';

function permisosPara(rol: string): PermisosService {
  localStorage.clear();
  guardarSesion(rol);
  TestBed.configureTestingModule({ providers: proveedoresHttpDePrueba() });
  return TestBed.inject(PermisosService);
}

describe('PermisosService', () => {
  it('admin escribe y administra usuarios', () => {
    const p = permisosPara('admin');
    expect(p.esAdmin()).toBe(true);
    expect(p.puedeEscribir()).toBe(true);
  });

  it('empleado escribe pero no administra usuarios', () => {
    const p = permisosPara('empleado');
    expect(p.esAdmin()).toBe(false);
    expect(p.puedeEscribir()).toBe(true);
  });

  it('visitante y roles desconocidos solo consultan', () => {
    expect(permisosPara('visitante').puedeEscribir()).toBe(false);
    TestBed.resetTestingModule();
    expect(permisosPara('otro').puedeEscribir()).toBe(false);
  });

  it('ignora mayúsculas del rol', () => {
    expect(permisosPara('Admin').esAdmin()).toBe(true);
  });
});
