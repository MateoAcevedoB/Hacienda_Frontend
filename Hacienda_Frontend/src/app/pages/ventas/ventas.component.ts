import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { mensajeDeError } from '../../core/errors/api-error';
import { PotreroResponse } from '../../core/models/potrero.models';
import { FiltroVentas, VentaResponse } from '../../core/models/venta.models';
import { PotreroService } from '../../core/services/potrero.service';
import { VentaService } from '../../core/services/venta.service';

/** Convierte 'YYYY-MM-DD' en el instante de inicio o fin de ese día (sin zona horaria). */
export function limiteDeFecha(fecha: string, fin: boolean): string {
  return `${fecha}T${fin ? '23:59:59' : '00:00:00'}`;
}

@Component({
  selector: 'app-ventas',
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, DatePipe],
  templateUrl: './ventas.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VentasComponent {
  private readonly ventasApi = inject(VentaService);
  private readonly potrerosApi = inject(PotreroService);

  protected readonly filtro = inject(NonNullableFormBuilder).group({
    potreroId: [''],
    desde: [''],
    hasta: [''],
  });

  protected readonly ventas = signal<VentaResponse[]>([]);
  protected readonly potreros = signal<PotreroResponse[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly errorFiltro = signal<string | null>(null);

  protected readonly totalMonto = computed(() =>
    this.ventas().reduce((suma, venta) => suma + venta.monto, 0),
  );
  protected readonly promedio = computed(() =>
    this.ventas().length === 0 ? 0 : this.totalMonto() / this.ventas().length,
  );

  constructor() {
    this.potrerosApi.obtenerPotreros().subscribe({
      next: (lista) => this.potreros.set(lista),
      error: () => this.potreros.set([]),
    });
    this.cargar({});
  }

  protected aplicar(): void {
    const { potreroId, desde, hasta } = this.filtro.getRawValue();
    const hayFechas = desde !== '' || hasta !== '';
    this.errorFiltro.set(null);

    if (potreroId !== '' && hayFechas) {
      this.errorFiltro.set('No se puede combinar el filtro por potrero con el filtro por fechas.');
      return;
    }
    if (hayFechas && (desde === '' || hasta === '')) {
      this.errorFiltro.set('Para filtrar por fechas debes indicar "Desde" y "Hasta".');
      return;
    }
    if (hayFechas && desde > hasta) {
      this.errorFiltro.set('La fecha "Desde" no puede ser posterior a "Hasta".');
      return;
    }

    const filtro: FiltroVentas = {};
    if (potreroId !== '') {
      filtro.potreroId = potreroId;
    } else if (hayFechas) {
      filtro.desde = limiteDeFecha(desde, false);
      filtro.hasta = limiteDeFecha(hasta, true);
    }
    this.cargar(filtro);
  }

  protected limpiar(): void {
    this.filtro.reset();
    this.errorFiltro.set(null);
    this.cargar({});
  }

  protected cargar(filtro: FiltroVentas): void {
    this.cargando.set(true);
    this.error.set(null);
    this.ventasApi
      .obtenerVentas(filtro)
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe({
        next: (ventas) => this.ventas.set(ventas),
        error: (e: unknown) => {
          this.ventas.set([]);
          this.error.set(mensajeDeError(e));
        },
      });
  }
}
