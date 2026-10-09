/** Cuerpo del alta y de la modificación de un operador, espejo de `BEGuardarOperador.cs`. */
export interface GuardarOperadorRequest {
    nombre: string;
    apellido: string;
    documento: string | null;
    email: string;
}
