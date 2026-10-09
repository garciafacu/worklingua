import type { InputHTMLAttributes, Ref } from 'react';

interface CampoArchivoProps extends InputHTMLAttributes<HTMLInputElement> {
    etiqueta: string;
    identificador: string;
    error?: string;
    /** Nombre del archivo elegido, para mostrarlo debajo. */
    archivo?: string | null;
    /** Para poder limpiar el control después de subir: su valor no es controlado. */
    ref?: Ref<HTMLInputElement>;
}

/**
 * Selector de archivo, con el mismo contrato que `CampoTexto`.
 *
 * Es el único control de carga del proyecto: el resto de los formularios son de
 * texto. Muestra el nombre del archivo elegido porque el control nativo lo
 * recorta y no se lee bien.
 */
export function CampoArchivo({
    etiqueta,
    identificador,
    error,
    archivo,
    ...resto
}: CampoArchivoProps) {
    const idError = `${identificador}-error`;

    return (
        <div className="campo">
            <label htmlFor={identificador}>{etiqueta}</label>
            <input
                id={identificador}
                name={identificador}
                type="file"
                aria-invalid={error ? true : undefined}
                aria-describedby={error ? idError : undefined}
                className={error ? 'con-error' : undefined}
                {...resto}
            />
            {archivo && <span className="ayuda">{archivo}</span>}
            {error && (
                <span id={idError} className="campo-error">
                    {error}
                </span>
            )}
        </div>
    );
}
