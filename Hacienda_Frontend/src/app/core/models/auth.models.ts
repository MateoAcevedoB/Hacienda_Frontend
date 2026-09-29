export interface LoginRequest {
  nombre: string;
  contrasena: string;
}

export interface LoginResponse {
  token: string;
  tokenType: string;
  expiresAt: string;
  nombre: string;
  rol: string;
}

export interface Sesion {
  token: string;
  expiresAt: string;
  nombre: string;
  rol: string;
}

export interface UsuarioActual {
  nombre: string;
  rol: string;
}
