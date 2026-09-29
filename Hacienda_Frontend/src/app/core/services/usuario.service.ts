import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ResultadoOperacion } from '../models/potrero.models';
import { CrearUsuarioRequest, UsuarioResponse } from '../models/usuario.models';

const URL_USUARIOS = `${environment.apiUrl}/api/usuarios`;

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  private readonly http = inject(HttpClient);

  obtenerUsuarios(): Observable<UsuarioResponse[]> {
    return this.http.get<UsuarioResponse[]>(URL_USUARIOS);
  }

  crearUsuario(solicitud: CrearUsuarioRequest): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>(URL_USUARIOS, solicitud);
  }
}
