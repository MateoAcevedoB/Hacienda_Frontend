import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { mensajeDeError } from '../../core/errors/api-error';
import {
  PotreroResponse,
  TIPOS_POTRERO,
  etiquetaDeTipo,
} from '../../core/models/potrero.models';
import { PermisosService } from '../../core/services/permisos.service';
import { PotreroService } from '../../core/services/potrero.service';

@Component({
  selector: 'app-potreros',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './potreros.component.html',
  styleUrl: './potreros.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PotrerosComponent {
  private readonly servicio = inject(PotreroService);
  protected readonly puedeEscribir = inject(PermisosService).puedeEscribir;

  protected readonly tipos = TIPOS_POTRERO;
  protected readonly etiquetaDeTipo = etiquetaDeTipo;

  protected readonly formulario = inject(NonNullableFormBuilder).group({
    identificacion: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)]],
    tipoPotrero: ['', [Validators.required]],
  });

  protected readonly potreros = signal<PotreroResponse[]>([]);
  protected readonly cargando = signal(true);
  protected readonly errorListado = signal<string | null>(null);

  protected readonly guardando = signal(false);
  protected readonly errorCreacion = signal<string | null>(null);
  protected readonly exito = signal<string | null>(null);

  protected readonly totalPotreros = computed(() => this.potreros().length);
  protected readonly totalReses = computed(() =>
    this.potreros().reduce((suma, potrero) => suma + potrero.cantidadReses, 0),
  );
  protected readonly potrerosVacios = computed(
    () => this.potreros().filter((potrero) => potrero.cantidadReses === 0).length,
  );
  protected readonly potrerosConReses = computed(
    () => this.potreros().filter((potrero) => potrero.cantidadReses > 0).length,
  );

  constructor() {
    this.cargarPotreros();
  }

  protected get identificacion() {
    return this.formulario.controls.identificacion;
  }

  protected get tipoPotrero() {
    return this.formulario.controls.tipoPotrero;
  }

  protected cargarPotreros(): void {
    this.cargando.set(true);
    this.errorListado.set(null);

    this.servicio
      .obtenerPotreros()
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe({
        next: (potreros) => this.potreros.set(potreros),
        error: (error: unknown) => this.errorListado.set(mensajeDeError(error)),
      });
  }

  protected enviar(): void {
    if (this.guardando()) {
      return;
    }
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    this.guardando.set(true);
    this.errorCreacion.set(null);
    this.exito.set(null);

    const { identificacion, tipoPotrero } = this.formulario.getRawValue();

    this.servicio
      .crearPotrero({ identificacion: identificacion.trim(), tipoPotrero })
      .pipe(finalize(() => this.guardando.set(false)))
      .subscribe({
        next: (resultado) => {
          if (!resultado.exito) {
            this.errorCreacion.set(resultado.mensaje);
            return;
          }
          this.exito.set(resultado.mensaje);
          this.formulario.reset();
          this.cargarPotreros();
        },
        error: (error: unknown) => this.errorCreacion.set(mensajeDeError(error)),
      });
  }
}
