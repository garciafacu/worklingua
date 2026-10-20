import { descargarTexto } from './descarga';

export interface FilaCsv {
    numero: number;
    celdas: string[];
}

const BOM = '﻿';

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

    if (valor !== '' || celdas.length > 0) {
        cerrarFila();
    }

    return filas;
}

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

function escribirCelda(valor: string | number): string {
    const texto = String(valor ?? '');

    return /[";\r\n]/.test(texto) ? `"${texto.replace(/"/g, '""')}"` : texto;
}

export function generarCsv(encabezados: string[], filas: (string | number)[][]): string {
    const lineas = [encabezados, ...filas].map((fila) => fila.map(escribirCelda).join(';'));

    return lineas.join('\r\n');
}

export function descargarCsv(nombreArchivo: string, contenido: string): void {
    descargarTexto(nombreArchivo, BOM + contenido, 'text/csv;charset=utf-8');
}
