import { Injectable, computed, inject } from '@angular/core';

import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class PermisosService {
  private readonly auth = inject(AuthService);

  private readonly rol = computed(() => this.auth.usuario()?.rol.trim().toLowerCase() ?? '');

  readonly esAdmin = computed(() => this.rol() === 'admin');

  readonly puedeEscribir = computed(() => this.rol() === 'admin' || this.rol() === 'empleado');
}
