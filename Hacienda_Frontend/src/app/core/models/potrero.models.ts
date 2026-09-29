export interface PotreroResponse {
  identificacion: string;
  tipoPotrero: string;
  cantidadReses: number;
}

export interface CrearPotreroRequest {
  identificacion: string;
  tipoPotrero: string;
}

export interface ResResponse {
  nombre: string;
  peso: number;
  edad: number;
  tipo: string;
}

export interface ResultadoOperacion {
  exito: boolean;
  mensaje: string;
}

export interface TipoPotrero {
  valor: string;
  etiqueta: string;
}

export const TIPOS_POTRERO: readonly TipoPotrero[] = [
  { valor: 'ternero', etiqueta: 'Ternero (0-12 meses)' },
  { valor: 'cebon', etiqueta: 'Cebón (13-48 meses)' },
  { valor: 'novillo', etiqueta: 'Novillo (49+ meses)' },
];

export function etiquetaDeTipo(valor: string): string {
  return TIPOS_POTRERO.find((tipo) => tipo.valor === valor)?.etiqueta ?? valor;
}

export interface CrearResRequest {
  nombre: string;
  edad: number;
  peso: number;
}

export interface VenderResRequest {
  monto: number;
}

export const REGLAS_TIPO: Readonly<Record<string, { edadMax: number | null; pesoMin: number; pesoVenta: number }>> = {
  ternero: { edadMax: 12, pesoMin: 150, pesoVenta: 250 },
  cebon: { edadMax: 48, pesoMin: 290, pesoVenta: 420 },
  novillo: { edadMax: null, pesoMin: 400, pesoVenta: 550 },
};
