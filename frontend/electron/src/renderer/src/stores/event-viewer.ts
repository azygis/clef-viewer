import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useEventViewerStore = defineStore('event-viewer', () => {
    const dateFormatString = ref('YYYY-MM-DD HH:mm:ss.SSS');
    const messageTextLimit = ref<number | null>(200);
    const searchExpression = ref<string | null | undefined>();

    return { dateFormatString, messageTextLimit, searchExpression };
});
