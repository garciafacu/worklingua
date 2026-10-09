import { descargarTexto } from './descarga';

/**
 * Parser de CSV mínimo para el padrón de empleados.
 *
 * Solo parte el texto en filas y columnas: no valida ni interpreta nada, de eso
 * se encarga el backend. Contempla lo que de verdad sale de un Excel en
 * castellano: separador `,` o `;`, comillas dobles con `""` adentro, BOM al
 * principio y saltos CRLF.
 */

/** Una fila ya partida en celdas, con el número de línea del archivo. */
export interface FilaCsv {
    numero: number;
    celdas: string[];
}

const BOM = '﻿';

/** Elige el separador contando cuál aparece más en la primera línea. */
function detectarSeparador(primeraLinea: string): string {
    let comas = 0;
    let puntoYComas = 0;
    let dentroDeComillas = false;

    for (const caracter of primeraLinea) {
        if (caracter === '"') {
            dentroDeComillas = !dentroDeComillas;
        } else if (!dentroDeComillas && caracter === ',') {
            comas++;
        } else if (!dentroDeComillas && caracter === ';') {
            puntoYComas++;
        }
    }

    return puntoYComas > comas ? ';' : ',';
}

/**
 * Parte el contenido en filas. Las líneas en blanco se descartan: un CSV
 * exportado suele terminar con un salto de línea de más.
 */
export function parsearCsv(contenido: string): FilaCsv[] {
    const texto = contenido.startsWith(BOM) ? contenido.slice(1) : contenido;

    if (texto.trim() === '') {
        return [];
    }

    const separador = detectarSeparador(texto.split(/\r?\n/)[0] ?? '');

    const filas: FilaCsv[] = [];
    let celdas: string[] = [];
    let valor = '';
    let dentroDeComillas = false;
    let numero = 1;

    function cerrarCelda() {
        celdas.push(valor.trim());
        valor = '';
    }

    function cerrarFila() {
        cerrarCelda();

        if (celdas.some((celda) => celda !== '')) {
            filas.push({ numero, celdas });
        }

        celdas = [];
        numero++;
    }

    for (let i = 0; i < texto.length; i++) {
        const caracter = texto[i];

        if (dentroDeComillas) {
            if (caracter === '"') {
                // Dos comillas seguidas son una comilla literal.
                if (texto[i + 1] === '"') {
                    valor += '"';
                    i++;
                } else {
                    dentroDeComillas = false;
                }
            } else {
                valor += caracter;
            }

            continue;
        }

        if (caracter === '"') {
            dentroDeComillas = true;
        } else if (caracter === separador) {
            cerrarCelda();
        } else if (caracter === '\n') {
            cerrarFila();
        } else if (caracter !== '\r') {
            valor += caracter;
        }
    }

    // La última fila puede no terminar en salto de línea.
    if (valor !== '' || celdas.length > 0) {
        cerrarFila();
    }

    return filas;
}

/**
 * Ubica las columnas por nombre de encabezado, sin distinguir mayúsculas ni
 * acentos: un archivo puede traer "email", "Email" o "E-mail".
 */
export function indiceDeColumnas(encabezado: string[]): Record<string, number> {
    const indices: Record<string, number> = {};

    encabezado.forEach((celda, indice) => {
        const clave = celda
            .normalize('NFD')
            .replace(/[̀-ͯ]/g, '')
            .replace(/[^a-zA-Z]/g, '')
            .toLowerCase();

        if (clave !== '' && !(clave in indices)) {
            indices[clave] = indice;
        }
    });

    return indices;
}

/** Escapa una celda: entre comillas solo si hace falta. */
function escribirCelda(valor: string | number): string {
    const texto = String(valor ?? '');

    return /[";\r\n]/.test(texto) ? `"${texto.replace(/"/g, '""')}"` : texto;
}

/**
 * Arma el texto de un CSV con `;` como separador, que es lo que espera un Excel
 * configurado en castellano.
 */
export function generarCsv(encabezados: string[], filas: (string | number)[][]): string {
    const lineas = [encabezados, ...filas].map((fila) => fila.map(escribirCelda).join(';'));

    return lineas.join('\r\n');
}

/**
 * Descarga el CSV como archivo, sin pasar por el backend: el navegador ya tiene
 * los datos en pantalla, así que un endpoint de exportación solo repetiría la
 * consulta.
 *
 * Lleva BOM porque sin él Excel abre los acentos mal.
 */
export function descargarCsv(nombreArchivo: string, contenido: string): void {
    descargarTexto(nombreArchivo, BOM + contenido, 'text/csv;charset=utf-8');
}
