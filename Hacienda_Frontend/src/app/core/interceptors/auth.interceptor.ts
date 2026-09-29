import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService, LOGIN_URL } from '../services/auth.service';

/**
 * Adjunta "Authorization: Bearer <token>" a las peticiones hacia Api_Hacienda
 * (excepto el login) y cierra la sesión si un endpoint protegido responde 401.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  const esPeticionDeLaApi = req.url.startsWith(environment.apiUrl);
  const esLogin = req.url === LOGIN_URL;

  if (!esPeticionDeLaApi || esLogin) {
    return next(req);
  }

  const token = auth.obtenerToken();
  const peticion = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(peticion).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        auth.sesionInvalida();
      }
      return throwError(() => error);
    }),
  );
};
