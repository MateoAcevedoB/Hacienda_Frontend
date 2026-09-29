/** Respuesta de GET /api/usuarios (Api_Hacienda/Models/UsuarioResponse.cs). */
export interface UsuarioResponse {
  nombre: string;
}

/** Cuerpo de POST /api/usuarios (Api_Hacienda/Models/CrearUsuarioRequest.cs). */
export interface CrearUsuarioRequest {
  nombre: string;
  contrasena: string;
}
