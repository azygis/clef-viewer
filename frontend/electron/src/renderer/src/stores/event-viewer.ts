import { defineStore } from 'pinia';
import { ref } from 'vue';

export interface EventFilter {
    property: string;
    value: string;
}

export const useEventViewerStore = defineStore('event-viewer', () => {
    const dateFormatString = ref('YYYY-MM-DD HH:mm:ss.SSS');
    const messageTextLimit = ref<number | null>(200);
    const filters = ref<EventFilter[]>([]);

    function setFilter(property: string, value: string) {
        const existingFilterIndex = filters.value.findIndex((x) => x.property === property);
        if (existingFilterIndex >= 0) {
            filters.value[existingFilterIndex].value = value;
        } else {
            filters.value.push({ property, value });
        }
    }

    function removeFilter(filter: EventFilter) {
        filters.value.splice(filters.value.indexOf(filter), 1);
    }

    return { dateFormatString, messageTextLimit, filters, setFilter, removeFilter };
});
