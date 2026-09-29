import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

import { mapearError } from '../errors/api-error';

/** Convierte cualquier HttpErrorResponse en un ApiError con mensaje para el usuario. */
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: unknown) =>
      throwError(() => (error instanceof HttpErrorResponse ? mapearError(error) : error)),
    ),
  );
