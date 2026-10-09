import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { SelectorEstrellas } from './SelectorEstrellas';
import { LARGO_MAXIMO_OPINION, type OpinionResponse } from '../tipos/opiniones';

interface FormularioOpinionProps {
    nombrePlan: string;
    /**
     * La valoración que esta sesión ya dejó sobre el plan, o null. El estado del
     * formulario arranca de acá: quien lo usa le cambia la `key` cuando cambia la
     * valoración, en lugar de sincronizarla con un efecto.
     */
    valoracionPropia: OpinionResponse | null;
    enviando: boolean;
    alGuardar: (puntaje: number, texto: string) => Promise<boolean>;
}

/**
 * Formulario para valorar el plan elegido: estrellas obligatorias y comentario
 * opcional.
 *
 * Hay una sola valoración por persona y por plan, así que si ya existe el
 * formulario se abre con ella y guardar la actualiza. Solo se valida lo mínimo
 * para la experiencia de uso; las reglas y sus mensajes viven en la BLL.
 */
export function FormularioOpinion({
    nombrePlan,
    valoracionPropia,
    enviando,
    alGuardar,
}: FormularioOpinionProps) {
    const { t } = useTranslation();
    const [puntaje, setPuntaje] = useState(valoracionPropia?.puntaje ?? 0);
    const [texto, setTexto] = useState(valoracionPropia?.texto ?? '');

    const esEdicion = valoracionPropia !== null;

    async function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();

        const guardado = await alGuardar(puntaje, texto);

        // En un alta se limpia; en una edición el padre remonta el formulario
        // con la valoración guardada.
        if (guardado && !esEdicion) {
            setPuntaje(0);
            setTexto('');
        }
    }

    const restantes = LARGO_MAXIMO_OPINION - texto.length;

    return (
        <form onSubmit={manejarEnvio} className="rounded-2xl border border-borde bg-superficie p-6">
            <h2 className="m-0 text-lg font-semibold text-texto">
                {esEdicion
                    ? t('privado.opiniones.formulario.tituloEditar', { plan: nombrePlan })
                    : t('privado.opiniones.formulario.tituloNueva', { plan: nombrePlan })}
            </h2>

            {esEdicion && (
                <p className="mt-2 mb-0 rounded-lg bg-info-fondo px-3 py-2 text-sm text-info">
                    {t('privado.opiniones.formulario.yaValoraste')}
                </p>
            )}

            <div className="mt-5">
                <SelectorEstrellas valor={puntaje} deshabilitado={enviando} alCambiar={setPuntaje} />
            </div>

            <div className="campo mt-5">
                <label htmlFor="textoOpinion">
                    {t('privado.opiniones.formulario.comentarioOpcional')}
                </label>
                <textarea
                    id="textoOpinion"
                    name="texto"
                    rows={4}
                    maxLength={LARGO_MAXIMO_OPINION}
                    value={texto}
                    onChange={(evento) => setTexto(evento.target.value)}
                    placeholder={t('privado.opiniones.formulario.placeholder')}
                    className="w-full rounded-[var(--radio)] border border-borde bg-superficie p-3 text-[15px] text-texto outline-none focus:border-borde-foco"
                />
            </div>

            <div className="mt-4 flex flex-wrap items-center justify-between gap-3">
                <p className="m-0 text-xs text-texto-suave" aria-live="polite">
                    {t('privado.opiniones.formulario.restantes', { restantes })}
                </p>

                <button
                    type="submit"
                    disabled={enviando || puntaje === 0}
                    className="rounded-lg bg-primario px-5 py-2.5 text-sm font-semibold text-white hover:bg-primario-hover disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {enviando
                        ? t('privado.opiniones.formulario.publicando')
                        : esEdicion
                          ? t('privado.opiniones.formulario.actualizarValoracion')
                          : t('privado.opiniones.formulario.publicarValoracion')}
                </button>
            </div>
        </form>
    );
}
