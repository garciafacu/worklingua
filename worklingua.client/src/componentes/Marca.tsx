interface MarcaProps {
    /** Clases de la segunda mitad de la palabra, la que va en otro color. */
    claseAcento?: string;
}

/**
 * Texto del logotipo "WorkLingua", con su acento en la segunda mitad. Existe
 * para tener una sola definición del nombre: lo usan las pantallas de cuenta,
 * el encabezado público y la barra superior privada.
 *
 * No trae elemento contenedor propio a propósito: cada lugar lo envuelve en lo
 * que necesita (un `<p>` con margen en las pantallas de cuenta, un `<Link>` en
 * los encabezados). Sin `claseAcento` el color del acento lo decide el CSS del
 * contenedor, que es como funciona la regla `.marca span` heredada.
 */
export function Marca({ claseAcento }: MarcaProps) {
    return (
        <>
            Work<span className={claseAcento}>Lingua</span>
        </>
    );
}
