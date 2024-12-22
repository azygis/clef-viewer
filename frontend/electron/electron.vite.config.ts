import vue from '@vitejs/plugin-vue';
import { defineConfig, externalizeDepsPlugin } from 'electron-vite';
import { resolve } from 'path';
import vuetify, { transformAssetUrls } from 'vite-plugin-vuetify';

export default defineConfig({
    main: {
        plugins: [externalizeDepsPlugin()],
    },
    preload: {
        plugins: [externalizeDepsPlugin()],
    },
    renderer: {
        resolve: {
            alias: {
                '@': resolve('src/renderer/src'),
                '@shared': resolve('src/shared'),
            },
        },
        plugins: [vue({ template: { transformAssetUrls } }), vuetify()],
    },
});
