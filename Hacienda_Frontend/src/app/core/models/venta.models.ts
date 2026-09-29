/** Respuesta de GET /api/ventas (Api_Hacienda/Models/VentaResponse.cs). */
export interface VentaResponse {
  potreroId: string;
  nombreRes: string;
  fecha: string;
  monto: number;
}

/**
 * Filtros de GET /api/ventas. El backend rechaza combinar potreroId con fechas
 * y exige `desde` y `hasta` juntos.
 */
export interface FiltroVentas {
  potreroId?: string;
  desde?: string;
  hasta?: string;
}
