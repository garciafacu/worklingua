import { useState, type FormEvent } from 'react';
import { useTranslation } from 'react-i18next';
import { CampoSelect } from './CampoSelect';
import { CampoTexto } from './CampoTexto';
import { MODULOS_BITACORA, NIVELES_BITACORA, type FiltrosBitacora } from '../tipos/bitacora';

interface BuscadorBitacoraProps {
    buscando: boolean;
    alBuscar: (filtros: FiltrosBitacora) => void;
}

const FORMULARIO_VACIO = {
    texto: '',
    usuario: '',
    modulo: '',
    accion: '',
    nivel: '',
    desde: '',
    hasta: '',
};

type Formulario = typeof FORMULARIO_VACIO;


export function BuscadorBitacora({ buscando, alBuscar }: BuscadorBitacoraProps) {
    const { t } = useTranslation();

    const [formulario, setFormulario] = useState<Formulario>(FORMULARIO_VACIO);

    const hayFiltrosAvanzados =
        formulario.usuario !== '' ||
        formulario.modulo !== '' ||
        formulario.accion !== '' ||
        formulario.nivel !== '' ||
        formulario.desde !== '' ||
        formulario.hasta !== '';

    function actualizar(campo: keyof Formulario, valor: string) {
        setFormulario((actual) => ({ ...actual, [campo]: valor }));
    }

    function aCriterios(datos: Formulario): FiltrosBitacora {
        return {
            texto: datos.texto.trim() || undefined,
            usuario: datos.usuario.trim() || undefined,
            modulo: datos.modulo || undefined,
            accion: datos.accion.trim() || undefined,
            nivel: datos.nivel || undefined,
            desde: datos.desde || undefined,
            hasta: datos.hasta || undefined,
        };
    }

    function manejarEnvio(evento: FormEvent) {
        evento.preventDefault();
        alBuscar(aCriterios(formulario));
    }

    function limpiar() {
        setFormulario(FORMULARIO_VACIO);
        // Objeto nuevo en cada limpieza: el contenedor rehace la consulta cuando
        // cambia la identidad de los criterios, así que reutilizar una constante
        // haría que dos limpiezas seguidas no dispararan la segunda.
        alBuscar({});
    }

    return (
        <form
            onSubmit={manejarEnvio}
            className="rounded-2xl border border-borde bg-superficie p-6"
            role="search"
        >
            <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
                <div className="flex-1">
                    <CampoTexto
                        etiqueta={t('admin.bitacora.buscador.texto')}
                        identificador="textoBitacora"
                        placeholder={t('admin.bitacora.buscador.textoPlaceholder')}
                        value={formulario.texto}
                        onChange={(evento) => actualizar('texto', evento.target.value)}
                    />
                </div>

                <button
                    type="submit"
                    disabled={buscando}
                    className="rounded-lg bg-primario px-5 py-2.5 text-sm font-semibold text-white hover:bg-primario-hover disabled:cursor-not-allowed disabled:opacity-60"
                >
                    {buscando ? t('comun.boton.buscando') : t('comun.boton.buscar')}
                </button>
            </div>

            <details className="mt-4" open={hayFiltrosAvanzados}>
                <summary className="cursor-pointer text-sm font-semibold text-primario">
                    {t('publico.catalogo.buscador.avanzada')}
                </summary>

                <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    <CampoTexto
                        etiqueta={t('admin.bitacora.tabla.usuario')}
                        identificador="usuarioBitacora"
                        placeholder={t('admin.bitacora.buscador.usuarioPlaceholder')}
                        value={formulario.usuario}
                        onChange={(evento) => actualizar('usuario', evento.target.value)}
                    />

                    <CampoSelect
                        etiqueta={t('admin.bitacora.tabla.modulo')}
                        identificador="moduloBitacora"
                        value={formulario.modulo}
                        onChange={(evento) => actualizar('modulo', evento.target.value)}
                    >
                        <option value="">{t('admin.bitacora.buscador.todosModulos')}</option>
                        {MODULOS_BITACORA.map((modulo) => (
                            <option key={modulo} value={modulo}>
                                {modulo}
                            </option>
                        ))}
                    </CampoSelect>

                    <CampoTexto
                        etiqueta={t('admin.bitacora.tabla.accion')}
                        identificador="accionBitacora"
                        placeholder={t('admin.bitacora.buscador.accionPlaceholder')}
                        value={formulario.accion}
                        onChange={(evento) => actualizar('accion', evento.target.value)}
                    />

                    <CampoSelect
                        etiqueta={t('admin.bitacora.tabla.nivel')}
                        identificador="nivelBitacora"
                        value={formulario.nivel}
                        onChange={(evento) => actualizar('nivel', evento.target.value)}
                    >
                        <option value="">{t('publico.catalogo.buscador.todosNiveles')}</option>
                        {NIVELES_BITACORA.map((nivel) => (
                            <option key={nivel.valor} value={nivel.valor}>
                                {t(nivel.etiqueta)}
                            </option>
                        ))}
                    </CampoSelect>

                    <div className="grid grid-cols-2 gap-4">
                        <CampoTexto
                            etiqueta={t('comun.campo.desde')}
                            identificador="desdeBitacora"
                            type="date"
                            value={formulario.desde}
                            onChange={(evento) => actualizar('desde', evento.target.value)}
                        />

                        <CampoTexto
                            etiqueta={t('comun.campo.hasta')}
                            identificador="hastaBitacora"
                            type="date"
                            value={formulario.hasta}
                            onChange={(evento) => actualizar('hasta', evento.target.value)}
                        />
                    </div>
                </div>
            </details>

            <div className="mt-4 flex justify-end">
                <button
                    type="button"
                    onClick={limpiar}
                    className="rounded-lg border border-borde bg-superficie px-4 py-2 text-sm font-semibold text-texto hover:bg-fondo"
                >
                    {t('comun.boton.limpiar')}
                </button>
            </div>
        </form>
    );
}
