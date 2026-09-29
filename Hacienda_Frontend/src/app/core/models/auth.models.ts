/** Cuerpo de POST /api/auth/login (Api_Hacienda/Models/LoginRequest.cs). */
export interface LoginRequest {
  nombre: string;
  contrasena: string;
}

/** Respuesta de POST /api/auth/login (Api_Hacienda/Models/LoginResponse.cs). */
export interface LoginResponse {
  token: string;
  tokenType: string;
  expiresAt: string;
  nombre: string;
  rol: string;
}

/** Datos que se guardan en localStorage para mantener la sesión. */
export interface Sesion {
  token: string;
  expiresAt: string;
  nombre: string;
  rol: string;
}

/** Lo único de la sesión que pueden ver los componentes (nunca el token). */
export interface UsuarioActual {
  nombre: string;
  rol: string;
}
