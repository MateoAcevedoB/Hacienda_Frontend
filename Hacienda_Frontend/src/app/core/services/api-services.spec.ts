import { HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { guardarSesion, proveedoresHttpDePrueba } from '../../testing/helpers';
import { PotreroService } from './potrero.service';
import { ResService } from './res.service';
import { UsuarioService } from './usuario.service';
import { VacunaService } from './vacuna.service';
import { VentaService } from './venta.service';

const API = environment.apiUrl;

describe('servicios HTTP (contratos reales, HTTP simulado)', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    guardarSesion('admin');
    TestBed.configureTestingModule({ providers: proveedoresHttpDePrueba() });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('PotreroService: GET/POST /api/potreros y reses, con ids codificados', () => {
    const s = TestBed.inject(PotreroService);
    s.obtenerPotrero('Potrero Norte/1').subscribe();
    http.expectOne(`${API}/api/potreros/Potrero%20Norte%2F1`).flush({});
    s.obtenerReses('P-1').subscribe();
    http.expectOne(`${API}/api/potreros/P-1/reses`).flush([]);
    s.crearPotrero({ identificacion: 'P-2', tipoPotrero: 'cebon' }).subscribe();
    const r = http.expectOne(`${API}/api/potreros`);
    expect(r.request.method).toBe('POST');
    expect(r.request.body).toEqual({ identificacion: 'P-2', tipoPotrero: 'cebon' });
    r.flush({ exito: true, mensaje: '' });
  });

  it('ResService: agregar, obtener y vender', () => {
    const s = TestBed.inject(ResService);
    s.agregarRes('P-1', { nombre: 'Luna', edad: 10, peso: 200 }).subscribe();
    const a = http.expectOne(`${API}/api/potreros/P-1/reses`);
    expect(a.request.method).toBe('POST');
    expect(a.request.body).toEqual({ nombre: 'Luna', edad: 10, peso: 200 });
    a.flush({ exito: true, mensaje: '' });

    s.obtenerRes('P-1', 'Luna Azul').subscribe();
    http.expectOne(`${API}/api/potreros/P-1/reses/Luna%20Azul`).flush({});

    s.venderRes('P-1', 'Luna', { monto: 900 }).subscribe();
    const v = http.expectOne(`${API}/api/potreros/P-1/reses/Luna/vender`);
    expect(v.request.method).toBe('POST');
    expect(v.request.body).toEqual({ monto: 900 });
    v.flush({ exito: true, mensaje: '' });
  });

  it('VentaService: envía solo los filtros indicados', () => {
    const s = TestBed.inject(VentaService);
    s.obtenerVentas().subscribe();
    expect(http.expectOne(`${API}/api/ventas`).request.params.keys()).toEqual([]);

    s.obtenerVentas({ potreroId: 'P-1' }).subscribe();
    const p = http.expectOne((r) => r.url === `${API}/api/ventas`);
    expect(p.request.params.get('potreroId')).toBe('P-1');
    p.flush([]);

    s.obtenerVentas({ desde: '2026-01-01T00:00:00', hasta: '2026-01-31T23:59:59' }).subscribe();
    const f = http.expectOne((r) => r.url === `${API}/api/ventas`);
    expect(f.request.params.get('desde')).toBe('2026-01-01T00:00:00');
    expect(f.request.params.has('potreroId')).toBe(false);
    f.flush([]);
  });

  it('VacunaService: listar, crear y aplicar', () => {
    const s = TestBed.inject(VacunaService);
    s.obtenerVacunas().subscribe();
    http.expectOne(`${API}/api/vacunas`).flush([]);
    s.crearVacuna({ nombre: 'A', lote: 'L1', fechaAplicacion: '2026-01-01', fechaVencimiento: '2027-01-01', periodoAplicacion: 3 }).subscribe();
    const c = http.expectOne(`${API}/api/vacunas`);
    expect(c.request.method).toBe('POST');
    c.flush({ exito: true, mensaje: '' });
    s.aplicarVacuna({ potreroId: 'P-1', nombreRes: 'Luna', loteVacuna: 'L1' }).subscribe();
    const a = http.expectOne(`${API}/api/vacunacion/aplicar`);
    expect(a.request.body).toEqual({ potreroId: 'P-1', nombreRes: 'Luna', loteVacuna: 'L1' });
    a.flush({ exito: true, mensaje: '' });
  });

  it('UsuarioService: GET y POST /api/usuarios', () => {
    const s = TestBed.inject(UsuarioService);
    s.obtenerUsuarios().subscribe();
    http.expectOne(`${API}/api/usuarios`).flush([]);
    s.crearUsuario({ nombre: 'ana', contrasena: 'x' }).subscribe();
    const c = http.expectOne(`${API}/api/usuarios`);
    expect(c.request.body).toEqual({ nombre: 'ana', contrasena: 'x' });
    c.flush({ exito: true, mensaje: '' });
  });
});
