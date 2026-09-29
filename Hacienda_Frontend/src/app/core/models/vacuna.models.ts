export interface VacunaResponse {
  nombre: string;
  lote: string;
  fechaVencimiento: string;
  fechaAplicacion: string;
  tipo: string;
  periodoAplicacion: number | null;
}

export interface CrearVacunaRequest {
  nombre: string;
  lote: string;
  fechaVencimiento: string;
  fechaAplicacion: string;
  periodoAplicacion?: number;
  atenuacionGrado?: number;
}

export interface AplicarVacunaRequest {
  potreroId: string;
  nombreRes: string;
  loteVacuna: string;
}

export const GRADOS_ATENUACION: readonly number[] = [10, 20, 30];

export const PERIODO_BACTERIANA = { min: 2, max: 4 } as const;
