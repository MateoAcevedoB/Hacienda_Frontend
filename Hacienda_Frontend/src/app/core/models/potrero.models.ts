/** Respuesta de GET /api/potreros y GET /api/potreros/{id} (Api_Hacienda/Models/PotreroResponse.cs). */
export interface PotreroResponse {
  identificacion: string;
  tipoPotrero: string;
  cantidadReses: number;
}

/** Cuerpo de POST /api/potreros (Api_Hacienda/Models/PotreroRequest.cs). */
export interface CrearPotreroRequest {
  identificacion: string;
  tipoPotrero: string;
}

/** Respuesta de GET /api/potreros/{potreroId}/reses (Api_Hacienda/Models/ResResponse.cs). */
export interface ResResponse {
  nombre: string;
  peso: number;
  edad: number;
  tipo: string;
}

/** Resultado de las operaciones de escritura (Bib_Hacienda/Dominio/ResultadoOperacion.cs). */
export interface ResultadoOperacion {
  exito: boolean;
  mensaje: string;
}

export interface TipoPotrero {
  /** Valor exacto que espera el backend (claves de RegistroFabricasRes). */
  valor: string;
  etiqueta: string;
}

/** Los tres únicos tipos con los que el backend puede añadir reses a un potrero. */
export const TIPOS_POTRERO: readonly TipoPotrero[] = [
  { valor: 'ternero', etiqueta: 'Ternero (0-12 meses)' },
  { valor: 'cebon', etiqueta: 'Cebón (13-48 meses)' },
  { valor: 'novillo', etiqueta: 'Novillo (49+ meses)' },
];

/** Etiqueta amigable de un tipo; si el backend trae otro valor, se muestra tal cual. */
export function etiquetaDeTipo(valor: string): string {
  return TIPOS_POTRERO.find((tipo) => tipo.valor === valor)?.etiqueta ?? valor;
}

/** Cuerpo de POST /api/potreros/{potreroId}/reses (Api_Hacienda/Models/ResRequest.cs). */
export interface CrearResRequest {
  nombre: string;
  edad: number;
  peso: number;
}

/** Cuerpo de POST /api/potreros/{potreroId}/reses/{nombre}/vender (VenderResRequest.cs). */
export interface VenderResRequest {
  monto: number;
}

/** Reglas informativas por tipo (Bib_Hacienda/Dominio/Reglas/ReglaRes.cs). El backend es quien valida. */
export const REGLAS_TIPO: Readonly<Record<string, { edadMax: number | null; pesoMin: number; pesoVenta: number }>> = {
  ternero: { edadMax: 12, pesoMin: 150, pesoVenta: 250 },
  cebon: { edadMax: 48, pesoMin: 290, pesoVenta: 420 },
  novillo: { edadMax: null, pesoMin: 400, pesoVenta: 550 },
};
