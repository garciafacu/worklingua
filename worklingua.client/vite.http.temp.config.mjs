// Config temporal solo para mirar las pantallas en el navegador: HTTP, sin
// certificado. Se borra al terminar la verificacion.
import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

const target = 'http://localhost:5299';

export default defineConfig({
    plugins: [plugin(), tailwindcss()],
    resolve: {
        alias: {
            '@': fileURLToPath(new URL('./src', import.meta.url)),
        },
    },
    server: {
        proxy: {
            '^/api': { target, secure: false },
            '^/activos': { target, secure: false },
        },
        port: 59023,
    },
});
