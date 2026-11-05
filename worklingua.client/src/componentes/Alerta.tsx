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

export function AlertasResultado({ error, exito }: { error: string | null; exito: string | null }) {
    return (
        <div className="mt-6 flex flex-col gap-3">
            <Alerta tipo="error" mensaje={error} />
            <Alerta tipo="exito" mensaje={exito} />
        </div>
    );
}
