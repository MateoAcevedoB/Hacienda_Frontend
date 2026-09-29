import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // Las respuestas de error recorren la lista al revés: primero authInterceptor
    // (ve el HttpErrorResponse original y cierra sesión en un 401) y después
    // errorInterceptor (lo convierte en ApiError con mensaje amigable).
    provideHttpClient(withInterceptors([errorInterceptor, authInterceptor])),
  ],
};
