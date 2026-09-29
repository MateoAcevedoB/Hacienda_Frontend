import { HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { escribir, guardarSesion, proveedoresHttpDePrueba, texto } from '../../testing/helpers';
import { UsuariosComponent } from './usuarios.component';

const URL = `${environment.apiUrl}/api/usuarios`;

describe('UsuariosComponent', () => {
  function crear() {
    localStorage.clear();
    guardarSesion('admin');
    TestBed.configureTestingModule({ providers: proveedoresHttpDePrueba() });
    const http = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(UsuariosComponent);
    fixture.detectChanges();
    http.expectOne(URL).flush([{ nombre: 'admin' }]);
    fixture.detectChanges();
    return { http, fixture, el: fixture.nativeElement as HTMLElement };
  }

  it('no envía el formulario vacío', () => {
    const { el, fixture, http } = crear();
    el.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    expect(texto(el)).toContain('El nombre es obligatorio');
    http.verify();
  });

  it('crea el usuario y recarga la lista', () => {
    const { el, fixture, http } = crear();
    escribir(el, '#u-nombre', ' ana ');
    escribir(el, '#u-clave', 'secreto');
    el.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    const post = http.expectOne((r) => r.method === 'POST');
    expect(post.request.body).toEqual({ nombre: 'ana', contrasena: 'secreto' });
    post.flush({ exito: true, mensaje: "Usuario 'ana' creado." });
    http.expectOne((r) => r.method === 'GET').flush([{ nombre: 'admin' }, { nombre: 'ana' }]);
    fixture.detectChanges();
    expect(texto(el)).toContain("Usuario 'ana' creado.");
    expect(el.querySelectorAll('tbody tr').length).toBe(2);
  });

  it('muestra el mensaje de negocio cuando el backend responde 400', () => {
    const { el, fixture, http } = crear();
    escribir(el, '#u-nombre', 'admin');
    escribir(el, '#u-clave', 'x');
    el.querySelector<HTMLFormElement>('form')!.dispatchEvent(new Event('submit'));
    http
      .expectOne((r) => r.method === 'POST')
      .flush({ exito: false, mensaje: "Ya existe un usuario con el nombre 'admin'." }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();
    expect(texto(el)).toContain('Ya existe un usuario');
  });
});
