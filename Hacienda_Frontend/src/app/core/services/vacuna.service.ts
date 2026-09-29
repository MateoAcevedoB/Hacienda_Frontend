import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ResultadoOperacion } from '../models/potrero.models';
import { AplicarVacunaRequest, CrearVacunaRequest, VacunaResponse } from '../models/vacuna.models';

@Injectable({ providedIn: 'root' })
export class VacunaService {
  private readonly http = inject(HttpClient);

  obtenerVacunas(): Observable<VacunaResponse[]> {
    return this.http.get<VacunaResponse[]>(`${environment.apiUrl}/api/vacunas`);
  }

  crearVacuna(solicitud: CrearVacunaRequest): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>(`${environment.apiUrl}/api/vacunas`, solicitud);
  }

  aplicarVacuna(solicitud: AplicarVacunaRequest): Observable<ResultadoOperacion> {
    return this.http.post<ResultadoOperacion>(
      `${environment.apiUrl}/api/vacunacion/aplicar`,
      solicitud,
    );
  }
}
