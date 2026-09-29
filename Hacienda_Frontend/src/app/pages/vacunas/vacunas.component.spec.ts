import { HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { escribir, guardarSesion, proveedoresHttpDePrueba, texto } from '../../testing/helpers';
import { VacunasComponent, estaVencida } from './vacunas.component';

const API = environment.apiUrl;
const VACUNAS = [
  { nombre: 'Aftosa', lote: 'L-1', fechaAplicacion: '2020-01-01', fechaVencimiento: '2020-06-01', tipo: 'Bacteriana', periodoAplicacion: 3 },
  { nombre: 'Rabia', lote: 'L-2', fechaAplicacion: '2026-01-01', fechaVencimiento: '2099-01-01', tipo: 'Viva', periodoAplicacion: null },
];

describe('VacunasComponent', () => {
  function crear(rol = 'empleado') {
    localStorage.clear();
    guardarSesion(rol);
    TestBed.configureTestingModule({ providers: proveedoresHttpDePrueba() });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(VacunasComponent);
    fixture.detectChanges();
    http.expectOne(`${API}/api/vacunas`).flush(VACUNAS);
    http.expectOne(`${API}/api/potreros`).flush([{ identificacion: 'P-1', tipoPotrero: 'cebon', cantidadReses: 1 }]);
    fixture.detectChanges();
    return { http, fixture, el: fixture.nativeElement as HTMLElement };
  }

  function llenarComun(el: HTMLElement) {
    escribir(el, '#v-nombre', 'Nueva');
    escribir(el, '#v-lote', 'L-9');
    escribir(el, '#v-aplic', '2026-01-01');
    escribir(el, '#v-venc', '2027-01-01');
  }

  it('marca las vacunas vencidas', () => {
    const { el, http } = crear();
    expect(el.querySelectorAll('.aw-badge-danger').length).toBe(1);
    expect(estaVencida('2000-01-01')).toBe(true);
    expect(estaVencida('2999-01-01')).toBe(false);
    http.verify();
  });

  it('bacteriana envía periodoAplicacion y NO atenuacionGrado', () => {
    const { el, http, fixture } = crear();
    llenarComun(el);
    escribir(el, '#v-periodo', '3');
    el.querySelectorAll<HTMLFormElement>('form')[0].dispatchEvent(new Event('submit'));
    const post = http.expectOne((r) => r.method === 'POST');
    expect(post.request.body.periodoAplicacion).toBe(3);
    expect('atenuacionGrado' in post.request.body).toBe(false);
    post.flush({ exito: true, mensaje: 'ok' });
    http.expectOne(`${API}/api/vacunas`).flush(VACUNAS);
    fixture.detectChanges();
  });

  it('viva envía atenuacionGrado y NO periodoAplicacion', () => {
    const { el, http, fixture } = crear();
    escribir(el, '#v-tipo', 'viva');
    fixture.detectChanges();
    llenarComun(el);
    escribir(el, '#v-grado', '20');
    el.querySelectorAll<HTMLFormElement>('form')[0].dispatchEvent(new Event('submit'));
    const post = http.expectOne((r) => r.method === 'POST');
    expect(post.request.body.atenuacionGrado).toBe(20);
    expect('periodoAplicacion' in post.request.body).toBe(false);
    post.flush({ exito: true, mensaje: 'ok' });
    http.expectOne(`${API}/api/vacunas`).flush(VACUNAS);
  });

  it('bloquea un vencimiento no posterior a la aplicación', () => {
    const { el, http, fixture } = crear();
    llenarComun(el);
    escribir(el, '#v-venc', '2026-01-01');
    el.querySelectorAll<HTMLFormElement>('form')[0].dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(texto(el)).toContain('posterior a la de aplicación');
    http.verify();
  });

  it('al elegir potrero carga sus reses y aplica la vacuna por lote', () => {
    const { el, http, fixture } = crear();
    escribir(el, '#a-potrero', 'P-1');
    http.expectOne(`${API}/api/potreros/P-1/reses`).flush([{ nombre: 'Luna', peso: 300, edad: 20, tipo: 'Cebon' }]);
    fixture.detectChanges();
    escribir(el, '#a-res', 'Luna');
    escribir(el, '#a-lote', 'L-2');
    el.querySelectorAll<HTMLFormElement>('form')[1].dispatchEvent(new Event('submit'));
    const post = http.expectOne(`${API}/api/vacunacion/aplicar`);
    expect(post.request.body).toEqual({ potreroId: 'P-1', nombreRes: 'Luna', loteVacuna: 'L-2' });
    post.flush({ exito: true, mensaje: 'Aplicada' });
    http.expectOne(`${API}/api/vacunas`).flush(VACUNAS);
  });

  it('el visitante no ve los formularios de escritura', () => {
    const { el, http } = crear('visitante');
    expect(el.querySelectorAll('form').length).toBe(0);
    expect(texto(el)).toContain('solo consulta');
    http.verify();
  });
});
