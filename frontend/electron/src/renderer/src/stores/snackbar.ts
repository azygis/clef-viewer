import { defineStore } from 'pinia';
import { ref } from 'vue';

type SnackbarColor = 'success' | 'primary' | 'warning' | 'error';

export const useSnackbarStore = defineStore('snackbar', () => {
    const visible = ref(false);
    const currentMessage = ref<string>();
    const currentColor = ref<SnackbarColor>();
    let currentTimeout: number | undefined;

    function showMessage(message: string, color: SnackbarColor = 'primary', timeout = 5000) {
        currentMessage.value = message;
        currentColor.value = color;
        visible.value = true;
        replaceTimeout(timeout);
    }

    function replaceTimeout(timeout: number) {
        window.clearTimeout(currentTimeout);
        currentTimeout = window.setTimeout(() => (visible.value = false), timeout);
    }

    return { visible, currentMessage, currentColor, showMessage };
});
