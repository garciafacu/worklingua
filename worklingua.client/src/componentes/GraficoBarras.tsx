import {
    Bar,
    BarChart,
    CartesianGrid,
    Cell,
    LabelList,
    ResponsiveContainer,
    Tooltip,
    XAxis,
    YAxis,
} from 'recharts';

export interface DatoGrafico {
    etiqueta: string;
    valor: number;
    /** Resalta una barra, por ejemplo la opción que votó el usuario. */
    destacado?: boolean;
}

interface GraficoBarrasProps {
    datos: DatoGrafico[];
    /** Nombre de lo que se mide; se usa en el tooltip y en la tabla accesible. */
    etiquetaSerie: string;
    mensajeVacio: string;
    /** Texto de cada valor. Por defecto, el número tal cual. */
    formatearValor?: (valor: number) => string;
}

const COLOR_BARRA = '#2563eb';
const COLOR_BARRA_SUAVE = '#93c5fd';
const ALTO_FILA = 44;

/**
 * El eje siempre incluye el cero, aunque los datos no lo toquen: el largo de una
 * barra solo se puede leer si se mide desde cero. Sin esto Recharts ajusta el
 * dominio a los datos y, por ejemplo, un único valor negativo queda dibujado como
 * una barra completa, que no dice nada.
 */
const DOMINIO_CON_CERO: [(minimo: number) => number, (maximo: number) => number] = [
    (minimo) => Math.min(0, minimo),
    (maximo) => Math.max(0, maximo),
];

/**
 * Barras horizontales de una sola serie: encuestas, ganancias por período y
 * rendimiento usan la misma pieza.
 *
 * Es una sola serie, así que no lleva leyenda ni un color por barra: el color no
 * codifica nada y el valor va escrito al final de cada barra. Debajo queda la
 * misma información en una tabla, que es lo que leen los lectores de pantalla y
 * lo que se ve si el gráfico no puede renderizarse.
 */
export function GraficoBarras({ datos, etiquetaSerie, mensajeVacio, formatearValor }: GraficoBarrasProps) {
    const formatear = formatearValor ?? ((valor: number) => String(valor));

    if (datos.length === 0) {
        return <p className="text-sm text-texto-suave">{mensajeVacio}</p>;
    }

    return (
        <div>
            <div aria-hidden="true">
                <ResponsiveContainer width="100%" height={Math.max(datos.length * ALTO_FILA, 120)}>
                    <BarChart data={datos} layout="vertical" margin={{ top: 4, right: 80, bottom: 4, left: 4 }}>
                        <CartesianGrid horizontal={false} stroke="var(--color-borde)" />
                        <XAxis
                            type="number"
                            allowDecimals={false}
                            domain={DOMINIO_CON_CERO}
                            tickFormatter={formatear}
                            tick={{ fill: 'var(--color-texto-suave)', fontSize: 12 }}
                            axisLine={{ stroke: 'var(--color-borde)' }}
                            tickLine={false}
                        />
                        <YAxis
                            type="category"
                            dataKey="etiqueta"
                            width={180}
                            tick={{ fill: 'var(--color-texto)', fontSize: 13 }}
                            axisLine={false}
                            tickLine={false}
                        />
                        <Tooltip
                            cursor={{ fill: 'var(--color-fondo)' }}
                            formatter={(valor) => [formatear(Number(valor ?? 0)), etiquetaSerie]}
                            contentStyle={{
                                borderRadius: 'var(--radio)',
                                border: '1px solid var(--color-borde)',
                                background: 'var(--color-superficie)',
                                color: 'var(--color-texto)',
                                fontSize: 13,
                            }}
                        />
                        <Bar dataKey="valor" name={etiquetaSerie} radius={[0, 4, 4, 0]} barSize={16}>
                            {datos.map((dato) => (
                                <Cell
                                    key={dato.etiqueta}
                                    fill={dato.destacado === false ? COLOR_BARRA_SUAVE : COLOR_BARRA}
                                />
                            ))}
                            <LabelList
                                dataKey="valor"
                                position="right"
                                formatter={(valor) => formatear(Number(valor ?? 0))}
                                style={{ fill: 'var(--color-texto)', fontSize: 12 }}
                            />
                        </Bar>
                    </BarChart>
                </ResponsiveContainer>
            </div>

            <table className="sr-only">
                <caption>{etiquetaSerie}</caption>
                <tbody>
                    {datos.map((dato) => (
                        <tr key={dato.etiqueta}>
                            <th scope="row">{dato.etiqueta}</th>
                            <td>{formatear(dato.valor)}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
