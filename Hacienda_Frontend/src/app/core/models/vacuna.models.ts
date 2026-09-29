/** Respuesta de GET /api/vacunas (Api_Hacienda/Models/VacunaResponse.cs). Tipo: "Bacteriana" | "Viva". */
export interface VacunaResponse {
  nombre: string;
  lote: string;
  fechaVencimiento: string;
  fechaAplicacion: string;
  tipo: string;
  periodoAplicacion: number | null;
}

/**
 * Cuerpo de POST /api/vacunas (VacunaRequest.cs). Debe enviarse exactamente uno de
 * `periodoAplicacion` (bacteriana) o `atenuacionGrado` (viva); si no, el backend responde 400.
 */
export interface CrearVacunaRequest {
  nombre: string;
  lote: string;
  fechaVencimiento: string;
  fechaAplicacion: string;
  periodoAplicacion?: number;
  atenuacionGrado?: number;
}

/** Cuerpo de POST /api/vacunacion/aplicar (AplicarVacunaRequest.cs). */
export interface AplicarVacunaRequest {
  potreroId: string;
  nombreRes: string;
  loteVacuna: string;
}

/** Únicos grados que acepta el backend (enum_l_atenuaciones). */
export const GRADOS_ATENUACION: readonly number[] = [10, 20, 30];

/** Rango de semanas de la vacuna bacteriana (ReglaVacuna: mínimo 2, máximo 4). */
export const PERIODO_BACTERIANA = { min: 2, max: 4 } as const;
