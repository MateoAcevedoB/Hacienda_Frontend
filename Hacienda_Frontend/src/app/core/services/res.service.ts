import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CrearResRequest,
  ResResponse,
  ResultadoOperacion,
  VenderResRequest,
} from '../models/potrero.models';

const URL_POTREROS = `${environment.apiUrl}/api/potreros`;

/** Reses de un potrero y su venta (endpoints anidados de PotrerosController y VentasController). */
@Injectable({ providedIn: 'root' })
export class ResService {
  private readonly http = inject(HttpClient);

  private urlReses(potreroId: string): string {
    return `${URL_POTREROS}/${encodeURIComponent(potreroId)}/reses`;
  }

  agregarRes(potreroId: string, solicitud: CrearResRequest): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>(this.urlReses(potreroId), solicitud);
  }

  obtenerRes(potreroId: string, nombre: string): Observable<ResResponse> {
    return this.http.get<ResResponse>(`${this.urlReses(potreroId)}/${encodeURIComponent(nombre)}`);
  }

  venderRes(
    potreroId: string,
    nombre: string,
    solicitud: VenderResRequest,
  ): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>(
      `${this.urlReses(potreroId)}/${encodeURIComponent(nombre)}/vender`,
      solicitud,
    );
  }
}
