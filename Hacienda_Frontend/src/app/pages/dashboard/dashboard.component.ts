import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';

import { PotreroResponse } from '../../core/models/potrero.models';
import { VacunaResponse } from '../../core/models/vacuna.models';
import { VentaResponse } from '../../core/models/venta.models';
import { AuthService } from '../../core/services/auth.service';
import { PermisosService } from '../../core/services/permisos.service';
import { PotreroService } from '../../core/services/potrero.service';
import { VacunaService } from '../../core/services/vacuna.service';
import { VentaService } from '../../core/services/venta.service';
import { estaVencida } from '../vacunas/vacunas.component';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  protected readonly usuario = inject(AuthService).usuario;
  protected readonly esAdmin = inject(PermisosService).esAdmin;

  protected readonly potreros = signal<PotreroResponse[] | null>(null);
  protected readonly ventas = signal<VentaResponse[] | null>(null);
  protected readonly vacunas = signal<VacunaResponse[] | null>(null);
  protected readonly cargando = signal(true);

  protected readonly totalReses = computed(
    () => this.potreros()?.reduce((suma, p) => suma + p.cantidadReses, 0) ?? null,
  );
  protected readonly montoVentas = computed(
    () => this.ventas()?.reduce((suma, v) => suma + v.monto, 0) ?? null,
  );
  protected readonly vacunasVencidas = computed(
    () => this.vacunas()?.filter((v) => estaVencida(v.fechaVencimiento)).length ?? null,
  );
  protected readonly ultimasVentas = computed(() => (this.ventas() ?? []).slice(0, 5));

  constructor() {
    const sinDatos = () => of(null);
    forkJoin({
      potreros: inject(PotreroService).obtenerPotreros().pipe(catchError(sinDatos)),
      ventas: inject(VentaService).obtenerVentas().pipe(catchError(sinDatos)),
      vacunas: inject(VacunaService).obtenerVacunas().pipe(catchError(sinDatos)),
    }).subscribe((datos) => {
      this.potreros.set(datos.potreros);
      this.ventas.set(datos.ventas);
      this.vacunas.set(datos.vacunas);
      this.cargando.set(false);
    });
  }
}
