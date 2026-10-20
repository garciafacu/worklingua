import type { InputHTMLAttributes, Ref } from 'react';

interface CampoArchivoProps extends InputHTMLAttributes<HTMLInputElement> {
    etiqueta: string;
    identificador: string;
    error?: string;
    archivo?: string | null;
    ref?: Ref<HTMLInputElement>;
}

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
