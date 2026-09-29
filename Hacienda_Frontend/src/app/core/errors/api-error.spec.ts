import { HttpErrorResponse } from '@angular/common/http';

import { ApiError, MENSAJE_SIN_CONEXION, mapearError, mensajeDeError } from './api-error';

const error = (status: number, cuerpo: unknown = null) =>
  new HttpErrorResponse({ status, error: cuerpo });

describe('mapearError', () => {
  it('usa el mensaje del backend en 400 y 404', () => {
    expect(mapearError(error(400, { exito: false, mensaje: 'Peso insuficiente' })).message).toBe('Peso insuficiente');
    expect(mapearError(error(404, { mensaje: 'No se encontró el potrero' })).message).toBe('No se encontró el potrero');
  });

  it('une los errores de validación de [ApiController]', () => {
    const e = mapearError(error(400, { errors: { Nombre: ['Falta nombre'], Lote: ['Falta lote'] } }));
    expect(e.message).toBe('Falta nombre Falta lote');
  });

  it('distingue sin conexión, 403 y 500', () => {
    expect(mapearError(error(0)).message).toBe(MENSAJE_SIN_CONEXION);
    expect(mapearError(error(403)).status).toBe(403);
    expect(mapearError(error(500)).message).toContain('servidor');
  });

  it('mensajeDeError solo confía en ApiError', () => {
    expect(mensajeDeError(new ApiError(400, 'x'))).toBe('x');
    expect(mensajeDeError(new Error('interno'))).toContain('inesperado');
  });
});
