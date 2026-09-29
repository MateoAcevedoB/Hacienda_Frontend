import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../services/auth.service';
import { PermisosService } from '../services/permisos.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.estaAutenticado() ? true : inject(Router).createUrlTree(['/login']);
};

export const sinSesionGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  return auth.estaAutenticado() ? inject(Router).createUrlTree(['/dashboard']) : true;
};

export const adminGuard: CanActivateFn = () =>
  inject(PermisosService).esAdmin() ? true : inject(Router).createUrlTree(['/dashboard']);
