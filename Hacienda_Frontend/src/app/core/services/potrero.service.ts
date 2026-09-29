import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CrearPotreroRequest,
  PotreroResponse,
  ResResponse,
  ResultadoOperacion,
} from '../models/potrero.models';

const URL_POTREROS = `${environment.apiUrl}/api/potreros`;

@Injectable({ providedIn: 'root' })
export class PotreroService {
  private readonly http = inject(HttpClient);

  obtenerPotreros(): Observable<PotreroResponse[]> {
    return this.http.get<PotreroResponse[]>(URL_POTREROS);
  }

  obtenerPotrero(identificacion: string): Observable<PotreroResponse> {
    return this.http.get<PotreroResponse>(`${URL_POTREROS}/${encodeURIComponent(identificacion)}`);
  }

  crearPotrero(solicitud: CrearPotreroRequest): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>(URL_POTREROS, solicitud);
  }

  obtenerReses(potreroId: string): Observable<ResResponse[]> {
    return this.http.get<ResResponse[]>(`${URL_POTREROS}/${encodeURIComponent(potreroId)}/reses`);
  }
}
