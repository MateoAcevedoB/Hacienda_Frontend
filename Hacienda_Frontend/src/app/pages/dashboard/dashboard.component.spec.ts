import { HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { guardarSesion, proveedoresHttpDePrueba, texto } from '../../testing/helpers';
import { DashboardComponent } from './dashboard.component';

const API = environment.apiUrl;

describe('DashboardComponent', () => {
  function crear(rol: string) {
    localStorage.clear();
    guardarSesion(rol);
    TestBed.configureTestingModule({ providers: proveedoresHttpDePrueba() });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(DashboardComponent);
    fixture.detectChanges();
    return { http, fixture, el: fixture.nativeElement as HTMLElement };
  }

  it('resume potreros, reses, ventas y vacunas', () => {
    const { http, fixture, el } = crear('admin');
    http.expectOne(`${API}/api/potreros`).flush([
      { identificacion: 'P-1', tipoPotrero: 'cebon', cantidadReses: 3 },
      { identificacion: 'P-2', tipoPotrero: 'ternero', cantidadReses: 2 },
    ]);
    http.expectOne(`${API}/api/ventas`).flush([{ potreroId: 'P-1', nombreRes: 'Luna', fecha: '2026-03-01T10:00:00', monto: 800 }]);
    http.expectOne(`${API}/api/vacunas`).flush([
      { nombre: 'A', lote: 'L1', fechaAplicacion: '2020-01-01', fechaVencimiento: '2020-02-01', tipo: 'Viva', periodoAplicacion: null },
    ]);
    fixture.detectChanges();
    const valores = Array.from(el.querySelectorAll('.aw-stat-value')).map((n) => texto(n as HTMLElement).trim());
    expect(valores).toEqual(['2', '5', '1', '$800', '1', '1']);
    expect(texto(el)).toContain('Usuarios');
  });

  it('un fallo parcial no rompe el panel y oculta Usuarios a no admins', () => {
    const { http, fixture, el } = crear('empleado');
    http.expectOne(`${API}/api/potreros`).flush([]);
    http.expectOne(`${API}/api/ventas`).flush({ mensaje: 'x' }, { status: 500, statusText: 'Server Error' });
    http.expectOne(`${API}/api/vacunas`).flush([]);
    fixture.detectChanges();
    expect(texto(el)).toContain('no se pudieron cargar');
    expect(el.querySelector('a[href="/usuarios"]')).toBeNull();
  });
});
