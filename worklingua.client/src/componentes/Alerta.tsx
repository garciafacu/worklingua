type TipoAlerta = 'error' | 'exito' | 'info';

interface AlertaProps {
    tipo: TipoAlerta;
    mensaje: string | null;
}

export function Alerta({ tipo, mensaje }: AlertaProps) {
    if (!mensaje) {
        return null;
    }

    return (
        <p className={`alerta alerta-${tipo}`} role={tipo === 'error' ? 'alert' : 'status'}>
            {mensaje}
        </p>
    );
}
