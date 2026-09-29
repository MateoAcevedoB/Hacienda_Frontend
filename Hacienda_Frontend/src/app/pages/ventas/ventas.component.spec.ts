import { HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { escribir, guardarSesion, proveedoresHttpDePrueba, texto } from '../../testing/helpers';
import { VentasComponent, limiteDeFecha } from './ventas.component';

const URL = `${environment.apiUrl}/api/ventas`;
const VENTAS = [
  { potreroId: 'P-1', nombreRes: 'Luna', fecha: '2026-03-01T10:00:00', monto: 1000 },
  { potreroId: 'P-2', nombreRes: 'Sol', fecha: '2026-03-02T10:00:00', monto: 500 },
];

describe('VentasComponent', () => {
  function crear() {
    localStorage.clear();
    guardarSesion('empleado');
    TestBed.configureTestingModule({ providers: proveedoresHttpDePrueba() });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(VentasComponent);
    fixture.detectChanges();
    http.expectOne(`${environment.apiUrl}/api/potreros`).flush([{ identificacion: 'P-1', tipoPotrero: 'cebon', cantidadReses: 1 }]);
    http.expectOne(URL).flush(VENTAS);
    fixture.detectChanges();
    return { http, fixture, el: fixture.nativeElement as HTMLElement };
  }

  it('lista las ventas y calcula el monto total', () => {
    const { el, http } = crear();
    expect(el.querySelectorAll('tbody tr').length).toBe(2);
    expect(texto(el)).toContain('$1,500');
    http.verify();
  });

  it('rechaza combinar potrero con fechas sin llamar al backend', () => {
    const { el, fixture, http } = crear();
    escribir(el, '#f-potrero', 'P-1');
    escribir(el, '#f-desde', '2026-03-01');
    escribir(el, '#f-hasta', '2026-03-31');
    el.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(texto(el)).toContain('No se puede combinar');
    http.verify();
  });

  it('exige ambas fechas', () => {
    const { el, fixture, http } = crear();
    escribir(el, '#f-desde', '2026-03-01');
    el.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(texto(el)).toContain('"Desde" y "Hasta"');
    http.verify();
  });

  it('filtra por fechas incluyendo todo el día final', () => {
    const { el, http } = crear();
    escribir(el, '#f-desde', '2026-03-01');
    escribir(el, '#f-hasta', '2026-03-31');
    el.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    const req = http.expectOne((r) => r.url === URL);
    expect(req.request.params.get('desde')).toBe('2026-03-01T00:00:00');
    expect(req.request.params.get('hasta')).toBe('2026-03-31T23:59:59');
    req.flush([]);
  });

  it('limiteDeFecha arma el inicio y el fin del día', () => {
    expect(limiteDeFecha('2026-01-05', false)).toBe('2026-01-05T00:00:00');
    expect(limiteDeFecha('2026-01-05', true)).toBe('2026-01-05T23:59:59');
  });
});
