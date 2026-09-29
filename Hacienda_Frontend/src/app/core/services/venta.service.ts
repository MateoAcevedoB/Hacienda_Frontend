import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { FiltroVentas, VentaResponse } from '../models/venta.models';

const URL_VENTAS = `${environment.apiUrl}/api/ventas`;

@Injectable({ providedIn: 'root' })
export class VentaService {
  private readonly http = inject(HttpClient);

  obtenerVentas(filtro: FiltroVentas = {}): Observable<VentaResponse[]> {
    let params = new HttpParams();
    if (filtro.potreroId) {
      params = params.set('potreroId', filtro.potreroId);
    }
    if (filtro.desde) {
      params = params.set('desde', filtro.desde);
    }
    if (filtro.hasta) {
      params = params.set('hasta', filtro.hasta);
    }
    return this.http.get<VentaResponse[]>(URL_VENTAS, { params });
  }
}
