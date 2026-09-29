import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { mensajeDeError } from '../../core/errors/api-error';
import { UsuarioResponse } from '../../core/models/usuario.models';
import { UsuarioService } from '../../core/services/usuario.service';

@Component({
  selector: 'app-usuarios',
  imports: [ReactiveFormsModule],
  templateUrl: './usuarios.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuariosComponent {
  private readonly servicio = inject(UsuarioService);

  protected readonly formulario = inject(NonNullableFormBuilder).group({
    nombre: ['', [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)]],
    contrasena: ['', [Validators.required, Validators.pattern(/\S/)]],
  });

  protected readonly usuarios = signal<UsuarioResponse[]>([]);
  protected readonly cargando = signal(true);
  protected readonly errorListado = signal<string | null>(null);
  protected readonly guardando = signal(false);
  protected readonly errorCreacion = signal<string | null>(null);
  protected readonly exito = signal<string | null>(null);

  constructor() {
    this.cargar();
  }

  protected get nombre() {
    return this.formulario.controls.nombre;
  }
  protected get contrasena() {
    return this.formulario.controls.contrasena;
  }

  protected cargar(): void {
    this.cargando.set(true);
    this.errorListado.set(null);
    this.servicio
      .obtenerUsuarios()
      .pipe(finalize(() => this.cargando.set(false)))
      .subscribe({
        next: (lista) => this.usuarios.set(lista),
        error: (e: unknown) => this.errorListado.set(mensajeDeError(e)),
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
    const { nombre, contrasena } = this.formulario.getRawValue();
    this.servicio
      .crearUsuario({ nombre: nombre.trim(), contrasena })
      .pipe(finalize(() => this.guardando.set(false)))
      .subscribe({
        next: (resultado) => {
          if (!resultado.exito) {
            this.errorCreacion.set(resultado.mensaje);
            return;
          }
          this.exito.set(resultado.mensaje);
          this.formulario.reset();
          this.cargar();
        },
        error: (e: unknown) => this.errorCreacion.set(mensajeDeError(e)),
      });
  }
}
