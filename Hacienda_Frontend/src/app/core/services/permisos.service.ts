import { Injectable, computed, inject } from '@angular/core';

import { AuthService } from './auth.service';

/**
 * Permisos de la interfaz según el rol. En el backend el rol es el nombre del usuario
 * (admin | empleado | visitante, ver AuthController y ProveedorPermisosPorRol).
 * Esto solo decide qué se muestra: la autorización real la hace la API.
 */
@Injectable({ providedIn: 'root' })
export class PermisosService {
  private readonly auth = inject(AuthService);

  private readonly rol = computed(() => this.auth.usuario()?.rol.trim().toLowerCase() ?? '');

  /** Solo admin puede usar /api/usuarios ([Authorize(Roles = "admin")]). */
  readonly esAdmin = computed(() => this.rol() === 'admin');

  /** admin y empleado pueden escribir; visitante (y roles desconocidos) solo consultan. */
  readonly puedeEscribir = computed(() => this.rol() === 'admin' || this.rol() === 'empleado');
}
