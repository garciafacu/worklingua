import type { InputHTMLAttributes } from 'react';

interface CampoTextoProps extends InputHTMLAttributes<HTMLInputElement> {
    etiqueta: string;
    identificador: string;
    error?: string;
}

/** Campo de formulario con etiqueta y mensaje de error asociados. */
export function CampoTexto({ etiqueta, identificador, error, ...resto }: CampoTextoProps) {
    const idError = `${identificador}-error`;

    return (
        <div className="campo">
            <label htmlFor={identificador}>{etiqueta}</label>
            <input
                id={identificador}
                name={identificador}
                aria-invalid={error ? true : undefined}
                aria-describedby={error ? idError : undefined}
                className={error ? 'con-error' : undefined}
                {...resto}
            />
            {error && (
                <span id={idError} className="campo-error">
                    {error}
                </span>
            )}
        </div>
    );
}
