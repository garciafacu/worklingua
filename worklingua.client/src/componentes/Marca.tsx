interface MarcaProps {
    claseAcento?: string;
}

export function Marca({ claseAcento }: MarcaProps) {
    return (
        <>
            Work<span className={claseAcento}>Lingua</span>
        </>
    );
}
