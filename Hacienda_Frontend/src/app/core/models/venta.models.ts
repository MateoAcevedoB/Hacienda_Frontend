export interface VentaResponse {
  potreroId: string;
  nombreRes: string;
  fecha: string;
  monto: number;
}

export interface FiltroVentas {
  potreroId?: string;
  desde?: string;
  hasta?: string;
}
