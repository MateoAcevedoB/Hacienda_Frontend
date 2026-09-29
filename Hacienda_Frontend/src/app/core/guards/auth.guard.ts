import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../services/auth.service';
import { PermisosService } from '../services/permisos.service';

/** Rutas privadas: sin sesión vigente se redirige a /login. */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.estaAutenticado() ? true : inject(Router).createUrlTree(['/login']);
};

/** Ruta /login: si ya hay sesión se redirige a la aplicación. */
export const sinSesionGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.estaAutenticado() ? inject(Router).createUrlTree(['/dashboard']) : true;
};

/** Rutas solo para admin (usuarios): el resto de roles vuelve al dashboard. */
export const adminGuard: CanActivateFn = () =>
  inject(PermisosService).esAdmin() ? true : inject(Router).createUrlTree(['/dashboard']);
