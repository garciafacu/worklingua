/**
 * Descarga un contenido generado en el navegador como archivo.
 *
 * No pasa por el backend: lo que se baja ya está en pantalla, así que un
 * endpoint de descarga solo repetiría la consulta. Lo usan la exportación a
 * CSV de los reportes y el XML de las versiones de un curso.
 */
export function descargarTexto(nombreArchivo: string, contenido: string, tipo: string): void {
    const blob = new Blob([contenido], { type: tipo });
    const url = URL.createObjectURL(blob);
    const enlace = document.createElement('a');

    enlace.href = url;
    enlace.download = nombreArchivo;
    enlace.click();

    URL.revokeObjectURL(url);
}
