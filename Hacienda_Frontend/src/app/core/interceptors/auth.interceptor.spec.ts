import { HttpClient } from '@angular/common/http';
import { HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';

import { environment } from '../../../environments/environment';
import { guardarSesion, proveedoresHttpDePrueba } from '../../testing/helpers';
import { LOGIN_URL } from '../services/auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let controlador: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    guardarSesion('admin', 'admin', 'token-123');
    TestBed.configureTestingModule({ providers: proveedoresHttpDePrueba() });
    http = TestBed.inject(HttpClient);
    controlador = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controlador.verify());

  it('agrega el Bearer a las peticiones de la API', () => {
    http.get(`${environment.apiUrl}/api/potreros`).subscribe();
    const req = controlador.expectOne(`${environment.apiUrl}/api/potreros`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer token-123');
    req.flush([]);
  });

  it('no agrega el token al login ni a otros dominios', () => {
    http.post(LOGIN_URL, {}).subscribe();
    expect(controlador.expectOne(LOGIN_URL).request.headers.has('Authorization')).toBe(false);
    http.get('https://otro.dominio/x').subscribe();
    expect(controlador.expectOne('https://otro.dominio/x').request.headers.has('Authorization')).toBe(false);
  });

  it('cierra la sesión y va a /login ante un 401', () => {
    const navegar = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    http.get(`${environment.apiUrl}/api/ventas`).subscribe({ error: () => undefined });
    controlador
      .expectOne(`${environment.apiUrl}/api/ventas`)
      .flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(localStorage.getItem('hacienda-tobe.sesion')).toBeNull();
    expect(navegar).toHaveBeenCalledWith(['/login'], { queryParams: { motivo: 'sesion-invalida' } });
  });

  it('convierte los errores en ApiError con el mensaje del backend', () => {
    let mensaje = '';
    http.get(`${environment.apiUrl}/api/potreros/x`).subscribe({ error: (e: Error) => (mensaje = e.message) });
    controlador
      .expectOne(`${environment.apiUrl}/api/potreros/x`)
      .flush({ mensaje: 'No existe' }, { status: 404, statusText: 'Not Found' });
    expect(mensaje).toBe('No existe');
  });
});
