import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { finalize } from 'rxjs';

import { mensajeDeError } from '../../core/errors/api-error';
import { PotreroResponse, ResResponse } from '../../core/models/potrero.models';
import {
  CrearVacunaRequest,
  GRADOS_ATENUACION,
  PERIODO_BACTERIANA,
  VacunaResponse,
} from '../../core/models/vacuna.models';
import { PermisosService } from '../../core/services/permisos.service';
import { PotreroService } from '../../core/services/potrero.service';
import { VacunaService } from '../../core/services/vacuna.service';

export function vencimientoPosterior(grupo: AbstractControl): ValidationErrors | null {
  const aplicacion = grupo.get('fechaAplicacion')?.value as string;
  const vencimiento = grupo.get('fechaVencimiento')?.value as string;
  return aplicacion && vencimiento && vencimiento <= aplicacion ? { vencimientoAnterior: true } : null;
}

export function estaVencida(fechaVencimiento: string, ahora: Date = new Date()): boolean {
  return Date.parse(fechaVencimiento) < ahora.getTime();
}

@Component({
  selector: 'app-vacunas',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './vacunas.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VacunasComponent {
  private readonly vacunasApi = inject(VacunaService);
  private readonly potrerosApi = inject(PotreroService);
  private readonly fb = inject(NonNullableFormBuilder);
  protected readonly puedeEscribir = inject(PermisosService).puedeEscribir;

  protected readonly grados = GRADOS_ATENUACION;
  protected readonly periodo = PERIODO_BACTERIANA;
  protected readonly estaVencida = estaVencida;

  protected readonly vacunas = signal<VacunaResponse[]>([]);
  protected readonly potreros = signal<PotreroResponse[]>([]);
  protected readonly reses = signal<ResResponse[]>([]);
  protected readonly cargando = signal(true);
  protected readonly errorListado = signal<string | null>(null);
  protected readonly vencidas = computed(() => this.vacunas().filter((v) => estaVencida(v.fechaVencimiento)).length);

  protected readonly formCrear = this.fb.group(
    {
      tipo: ['bacteriana'],
      nombre: ['', [Validators.required, Validators.maxLength(200), Validators.pattern(/\S/)]],
      lote: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)]],
      fechaAplicacion: ['', Validators.required],
      fechaVencimiento: ['', Validators.required],
      periodoAplicacion: [PERIODO_BACTERIANA.min, [Validators.min(PERIODO_BACTERIANA.min), Validators.max(PERIODO_BACTERIANA.max)]],
      atenuacionGrado: [GRADOS_ATENUACION[0]],
    },
    { validators: vencimientoPosterior },
  );
  protected readonly guardandoVacuna = signal(false);
  protected readonly errorCrear = signal<string | null>(null);
  protected readonly exitoCrear = signal<string | null>(null);

  protected readonly formAplicar = this.fb.group({
    potreroId: ['', Validators.required],
    nombreRes: ['', Validators.required],
    loteVacuna: ['', Validators.required],
  });
  protected readonly aplicando = signal(false);
  protected readonly cargandoReses = signal(false);
  protected readonly errorAplicar = signal<string | null>(null);
  protected readonly exitoAplicar = signal<string | null>(null);

  constructor() {
    this.cargarVacunas();
    this.potrerosApi.obtenerPotreros().subscribe({
      next: (lista) => this.potreros.set(lista),
      error: () => this.potreros.set([]),
    });
    this.formAplicar.controls.potreroId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((id) => this.cargarReses(id));
  }

  protected get esBacteriana(): boolean {
    return this.formCrear.controls.tipo.value === 'bacteriana';
  }

  protected cargarVacunas(): void {
    this.cargando.set(true);
    this.errorListado.set(null);
    this.vacunasApi
      .obtenerVacunas()
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe({
        next: (lista) => this.vacunas.set(lista),
        error: (e: unknown) => this.errorListado.set(mensajeDeError(e)),
      });
  }

  private cargarReses(potreroId: string): void {
    this.reses.set([]);
    this.formAplicar.controls.nombreRes.setValue('');
    if (potreroId === '') {
      return;
    }
    this.cargandoReses.set(true);
    this.potrerosApi
      .obtenerReses(potreroId)
      .pipe(finalize(() => this.cargandoReses.set(false)))
      .subscribe({
        next: (lista) => this.reses.set(lista),
        error: (e: unknown) => this.errorAplicar.set(mensajeDeError(e)),
      });
  }

  protected crear(): void {
    if (this.guardandoVacuna()) {
      return;
    }
    if (this.formCrear.invalid) {
      this.formCrear.markAllAsTouched();
      return;
    }
    const v = this.formCrear.getRawValue();
    const solicitud: CrearVacunaRequest = {
      nombre: v.nombre.trim(),
      lote: v.lote.trim(),
      fechaAplicacion: v.fechaAplicacion,
      fechaVencimiento: v.fechaVencimiento,
      ...(v.tipo === 'bacteriana'
        ? { periodoAplicacion: Number(v.periodoAplicacion) }
        : { atenuacionGrado: Number(v.atenuacionGrado) }),
    };
    this.guardandoVacuna.set(true);
    this.errorCrear.set(null);
    this.exitoCrear.set(null);
    this.vacunasApi
      .crearVacuna(solicitud)
      .pipe(finalize(() => this.guardandoVacuna.set(false)))
      .subscribe({
        next: (r) => {
          if (!r.exito) {
            this.errorCrear.set(r.mensaje);
            return;
          }
          this.exitoCrear.set(r.mensaje);
          this.formCrear.patchValue({ nombre: '', lote: '', fechaAplicacion: '', fechaVencimiento: '' });
          this.formCrear.markAsUntouched();
          this.cargarVacunas();
        },
        error: (e: unknown) => this.errorCrear.set(mensajeDeError(e)),
      });
  }

  protected aplicar(): void {
    if (this.aplicando()) {
      return;
    }
    if (this.formAplicar.invalid) {
      this.formAplicar.markAllAsTouched();
      return;
    }
    this.aplicando.set(true);
    this.errorAplicar.set(null);
    this.exitoAplicar.set(null);
    this.vacunasApi
      .aplicarVacuna(this.formAplicar.getRawValue())
      .pipe(finalize(() => this.aplicando.set(false)))
      .subscribe({
        next: (r) => {
          if (!r.exito) {
            this.errorAplicar.set(r.mensaje);
            return;
          }
          this.exitoAplicar.set(r.mensaje);
          this.formAplicar.controls.loteVacuna.setValue('');
          this.formAplicar.controls.loteVacuna.markAsUntouched();
          this.cargarVacunas();
        },
        error: (e: unknown) => this.errorAplicar.set(mensajeDeError(e)),
      });
  }
}
