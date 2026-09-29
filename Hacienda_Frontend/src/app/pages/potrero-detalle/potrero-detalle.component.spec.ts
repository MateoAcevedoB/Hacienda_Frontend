import { HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';

import { environment } from '../../../environments/environment';
import { escribir, guardarSesion, proveedoresHttpDePrueba, texto } from '../../testing/helpers';
import { PotreroDetalleComponent } from './potrero-detalle.component';

const API = environment.apiUrl;
const POTRERO = { identificacion: 'P-1', tipoPotrero: 'cebon', cantidadReses: 1 };
const RESES = [{ nombre: 'Luna', peso: 300, edad: 20, tipo: 'Cebon' }];

describe('PotreroDetalleComponent', () => {
  function crear(rol = 'empleado') {
    localStorage.clear();
    guardarSesion(rol);
    TestBed.configureTestingModule({
      providers: [
        ...proveedoresHttpDePrueba(),
        { provide: ActivatedRoute, useValue: { paramMap: of(convertToParamMap({ id: 'P-1' })) } },
      ],
    });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(PotreroDetalleComponent);
    fixture.detectChanges();
    http.expectOne(`${API}/api/potreros/P-1`).flush(POTRERO);
    http.expectOne(`${API}/api/potreros/P-1/reses`).flush(RESES);
    fixture.detectChanges();
    return { http, fixture, el: fixture.nativeElement as HTMLElement };
  }

  function recargar(http: HttpTestingController) {
    http.expectOne(`${API}/api/potreros/P-1`).flush(POTRERO);
    http.expectOne(`${API}/api/potreros/P-1/reses`).flush(RESES);
  }

  it('muestra el potrero y sus reses', () => {
    const { el, http } = crear();
    expect(texto(el)).toContain('Luna');
    expect(texto(el)).toContain('Cebón (13-48 meses)');
    http.verify();
  });

  it('agrega una res con el cuerpo real del backend y recarga', () => {
    const { el, fixture, http } = crear();
    escribir(el, '#r-nombre', ' Estrella ');
    escribir(el, '#r-edad', '15');
    escribir(el, '#r-peso', '320');
    el.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    const post = http.expectOne((r) => r.method === 'POST');
    expect(post.request.url).toBe(`${API}/api/potreros/P-1/reses`);
    expect(post.request.body).toEqual({ nombre: 'Estrella', edad: 15, peso: 320 });
    post.flush({ exito: true, mensaje: 'Res añadida' });
    recargar(http);
    fixture.detectChanges();
    expect(texto(el)).toContain('Res añadida');
  });

  it('muestra el error de negocio del backend al agregar una res', () => {
    const { el, fixture, http } = crear();
    escribir(el, '#r-nombre', 'Flaca');
    escribir(el, '#r-edad', '5');
    escribir(el, '#r-peso', '10');
    el.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    http
      .expectOne((r) => r.method === 'POST')
      .flush({ exito: false, mensaje: 'Peso mínimo no alcanzado' }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(texto(el)).toContain('Peso mínimo no alcanzado');
  });

  it('vende una res enviando el monto', () => {
    const { el, fixture, http } = crear();
    el.querySelector<HTMLButtonElement>('tbody button')!.click();
    fixture.detectChanges();
    escribir(el, '#monto-Luna', '1200');
    el.querySelector<HTMLFormElement>('tr.aw-row-panel form')!.dispatchEvent(new Event('submit'));
    const post = http.expectOne((r) => r.method === 'POST');
    expect(post.request.url).toBe(`${API}/api/potreros/P-1/reses/Luna/vender`);
    expect(post.request.body).toEqual({ monto: 1200 });
    post.flush({ exito: true, mensaje: 'Venta registrada' }, { status: 201, statusText: 'Created' });
    recargar(http);
    fixture.detectChanges();
    expect(texto(el)).toContain('Venta registrada');
  });

  it('no permite vender con monto 0', () => {
    const { el, fixture, http } = crear();
    el.querySelector<HTMLButtonElement>('tbody button')!.click();
    fixture.detectChanges();
    el.querySelector<HTMLFormElement>('tr.aw-row-panel form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(texto(el)).toContain('mayor a 0');
    http.verify();
  });

  it('el visitante no ve acciones de escritura', () => {
    const { el, http } = crear('visitante');
    expect(el.querySelector('form')).toBeNull();
    expect(el.querySelector('tbody button')).toBeNull();
    http.verify();
  });
});
