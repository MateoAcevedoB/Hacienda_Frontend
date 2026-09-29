import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EMPTY, catchError, finalize, map, of, switchMap, tap } from 'rxjs';

import { ApiError, mensajeDeError } from '../../core/errors/api-error';
import {
  PotreroResponse,
  REGLAS_TIPO,
  ResResponse,
  etiquetaDeTipo,
} from '../../core/models/potrero.models';
import { PermisosService } from '../../core/services/permisos.service';
import { ResService } from '../../core/services/res.service';
import { PotreroService } from '../../core/services/potrero.service';

@Component({
  selector: 'app-potrero-detalle',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './potrero-detalle.component.html',
  styleUrl: './potrero-detalle.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PotreroDetalleComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly servicio = inject(PotreroService);

  private readonly resApi = inject(ResService);
  protected readonly puedeEscribir = inject(PermisosService).puedeEscribir;
  protected readonly etiquetaDeTipo = etiquetaDeTipo;

  protected readonly formRes = inject(NonNullableFormBuilder).group({
    nombre: ['', [Validators.required, Validators.maxLength(200), Validators.pattern(/\S/)]],
    edad: [0, [Validators.required, Validators.min(0), Validators.max(65535)]],
    peso: [0, [Validators.required, Validators.min(1)]],
  });
  protected readonly guardandoRes = signal(false);
  protected readonly errorRes = signal<string | null>(null);
  protected readonly exitoRes = signal<string | null>(null);

  protected readonly resEnVenta = signal<string | null>(null);
  protected readonly monto = new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(1)] });
  protected readonly vendiendo = signal(false);
  protected readonly errorVenta = signal<string | null>(null);
  protected readonly exitoVenta = signal<string | null>(null);

  protected readonly potrero = signal<PotreroResponse | null>(null);
  protected readonly cargandoPotrero = signal(true);
  protected readonly errorPotrero = signal<string | null>(null);
  protected readonly noEncontrado = signal(false);

  protected readonly reses = signal<ResResponse[]>([]);
  protected readonly cargandoReses = signal(false);
  protected readonly errorReses = signal<string | null>(null);

  constructor() {
    this.route.paramMap
      .pipe(
        map((parametros) => parametros.get('id') ?? ''),
        tap(() => this.reiniciar()),
        switchMap((id) =>
          this.servicio.obtenerPotrero(id).pipe(
            tap((potrero) => {
              this.potrero.set(potrero);
              this.cargandoPotrero.set(false);
              this.cargandoReses.set(true);
            }),
            switchMap((potrero) =>
              this.servicio.obtenerReses(potrero.identificacion).pipe(
                catchError((error: unknown) => {
                  this.errorReses.set(mensajeDeError(error));
                  return of<ResResponse[]>([]);
                }),
              ),
            ),
            catchError((error: unknown) => {
              this.cargandoPotrero.set(false);
              this.noEncontrado.set(error instanceof ApiError && error.status === 404);
              this.errorPotrero.set(mensajeDeError(error));
              return EMPTY;
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((reses) => {
        this.reses.set(reses);
        this.cargandoReses.set(false);
      });
  }

  protected get reglas() {
    return REGLAS_TIPO[this.potrero()?.tipoPotrero ?? ''] ?? null;
  }

  protected agregarRes(): void {
    const potrero = this.potrero();
    if (!potrero || this.guardandoRes()) {
      return;
    }
    if (this.formRes.invalid) {
      this.formRes.markAllAsTouched();
      return;
    }
    const { nombre, edad, peso } = this.formRes.getRawValue();
    this.guardandoRes.set(true);
    this.errorRes.set(null);
    this.exitoRes.set(null);
    this.resApi
      .agregarRes(potrero.identificacion, { nombre: nombre.trim(), edad: Number(edad), peso: Number(peso) })
      .pipe(finalize(() => this.guardandoRes.set(false)))
      .subscribe({
        next: (r) => {
          if (!r.exito) {
            this.errorRes.set(r.mensaje);
            return;
          }
          this.exitoRes.set(r.mensaje);
          this.formRes.reset({ nombre: '', edad: 0, peso: 0 });
          this.recargar();
        },
        error: (e: unknown) => this.errorRes.set(mensajeDeError(e)),
      });
  }

  protected abrirVenta(nombre: string): void {
    this.resEnVenta.set(this.resEnVenta() === nombre ? null : nombre);
    this.monto.reset(0);
    this.errorVenta.set(null);
    this.exitoVenta.set(null);
  }

  protected venderRes(): void {
    const potrero = this.potrero();
    const nombre = this.resEnVenta();
    if (!potrero || nombre === null || this.vendiendo()) {
      return;
    }
    if (this.monto.invalid) {
      this.monto.markAsTouched();
      return;
    }
    this.vendiendo.set(true);
    this.errorVenta.set(null);
    this.resApi
      .venderRes(potrero.identificacion, nombre, { monto: Number(this.monto.value) })
      .pipe(finalize(() => this.vendiendo.set(false)))
      .subscribe({
        next: (r) => {
          if (!r.exito) {
            this.errorVenta.set(r.mensaje);
            return;
          }
          this.exitoVenta.set(r.mensaje);
          this.resEnVenta.set(null);
          this.recargar();
        },
        error: (e: unknown) => this.errorVenta.set(mensajeDeError(e)),
      });
  }

  /** Refresca potrero y reses sin volver a mostrar el estado de carga inicial. */
  private recargar(): void {
    const potrero = this.potrero();
    if (!potrero) {
      return;
    }
    this.servicio.obtenerPotrero(potrero.identificacion).subscribe({
      next: (p) => this.potrero.set(p),
      error: () => undefined,
    });
    this.servicio.obtenerReses(potrero.identificacion).subscribe({
      next: (lista) => {
        this.reses.set(lista);
        this.errorReses.set(null);
      },
      error: (e: unknown) => this.errorReses.set(mensajeDeError(e)),
    });
  }

  private reiniciar(): void {
    this.potrero.set(null);
    this.cargandoPotrero.set(true);
    this.errorPotrero.set(null);
    this.noEncontrado.set(false);
    this.reses.set([]);
    this.cargandoReses.set(false);
    this.errorReses.set(null);
  }
}
