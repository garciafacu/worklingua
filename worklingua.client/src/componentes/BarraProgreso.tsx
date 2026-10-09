interface BarraProgresoProps {
    /** De 0 a 100. */
    porcentaje: number;
    etiqueta: string;
}

/** Barra de avance accesible: el porcentaje también lo leen los lectores de pantalla. */
export function BarraProgreso({ porcentaje, etiqueta }: BarraProgresoProps) {
    const valor = Math.max(0, Math.min(100, porcentaje));

    return (
        <div
            role="progressbar"
            aria-label={etiqueta}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-valuenow={valor}
            className="h-2 w-full overflow-hidden rounded-full bg-fondo"
        >
            <div
                className={`h-full rounded-full transition-[width] ${valor === 100 ? 'bg-exito' : 'bg-primario'}`}
                style={{ width: `${valor}%` }}
            />
        </div>
    );
}
