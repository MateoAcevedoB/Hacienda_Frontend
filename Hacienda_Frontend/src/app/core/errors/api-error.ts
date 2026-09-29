import { HttpErrorResponse } from '@angular/common/http';

export const MENSAJE_SIN_CONEXION =
  'No se pudo conectar con el servidor. Verifica que el backend esté ejecutándose.';
export const MENSAJE_401 = 'La sesión no es válida o las credenciales son incorrectas.';
const MENSAJE_400 = 'La solicitud no es válida.';
const MENSAJE_403 = 'No tienes permisos para realizar esta acción.';
const MENSAJE_404 = 'El recurso solicitado no existe.';
const MENSAJE_500 = 'El servidor tuvo un problema al procesar la solicitud. Inténtalo de nuevo más tarde.';
const MENSAJE_GENERICO = 'Ocurrió un error inesperado. Inténtalo de nuevo.';

/** Error ya interpretado: el mensaje es apto para mostrarse al usuario. */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    mensaje: string,
  ) {
    super(mensaje);
    this.name = 'ApiError';
  }
}

/** Devuelve el mensaje para el usuario a partir de cualquier error capturado. */
export function mensajeDeError(error: unknown): string {
  return error instanceof ApiError ? error.message : MENSAJE_GENERICO;
}

type Cuerpo = Record<string, unknown>;

function esCuerpo(valor: unknown): valor is Cuerpo {
  return typeof valor === 'object' && valor !== null && !Array.isArray(valor);
}

/**
 * Formatos reales de error de Api_Hacienda:
 *  - { mensaje }                          (BadRequest/NotFound/Unauthorized de los Controllers)
 *  - { exito, mensaje }                   (ResultadoOperacion en fallos de negocio)
 *  - { errors: { Campo: [msg] }, ... }    (ValidationProblemDetails de [ApiController])
 *  - ProblemDetails                       (500 del manejador global)
 *  - cuerpo vacío                         (401/403 del middleware JWT)
 */
function mensajeDelBackend(cuerpo: unknown): string | null {
  if (!esCuerpo(cuerpo)) {
    return null;
  }

  const mensaje = cuerpo['mensaje'];
  if (typeof mensaje === 'string' && mensaje.trim() !== '') {
    return mensaje;
  }

  const errores = cuerpo['errors'];
  if (esCuerpo(errores)) {
    const mensajes = Object.values(errores)
      .flatMap((valor) => (Array.isArray(valor) ? valor : []))
      .filter((texto): texto is string => typeof texto === 'string');
    if (mensajes.length > 0) {
      return mensajes.join(' ');
    }
  }

  return null;
}

/** Traduce un HttpErrorResponse a un ApiError con mensaje amigable. */
export function mapearError(error: HttpErrorResponse): ApiError {
  const delBackend = mensajeDelBackend(error.error);

  switch (error.status) {
    case 0:
      return new ApiError(0, MENSAJE_SIN_CONEXION);
    case 400:
      return new ApiError(400, delBackend ?? MENSAJE_400);
    case 401:
      return new ApiError(401, delBackend ?? MENSAJE_401);
    case 403:
      return new ApiError(403, MENSAJE_403);
    case 404:
      return new ApiError(404, delBackend ?? MENSAJE_404);
    default:
      return new ApiError(error.status, error.status >= 500 ? MENSAJE_500 : MENSAJE_GENERICO);
  }
}
