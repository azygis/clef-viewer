<script setup lang="ts">
import { useEventViewerStore } from '@/stores/event-viewer';

const store = useEventViewerStore();

function formatValue(value: string | number) {
    if (typeof value === 'number') {
        return value;
    }

    const limit = 50;
    if (value.length <= limit) {
        return value;
    }

    const remainder = value.length - limit;
    return `${value.slice(0, limit)}… (${remainder} more)`;
}
</script>
<template>
    <v-row>
        <v-col>
            <v-chip
                v-for="filter of store.filters"
                :key="filter.property"
                rounded
                closable
                class="mr-2 mb-2"
                density="compact"
                @click:close="store.removeFilter(filter)"
                >{{ filter.property }}: {{ formatValue(filter.value) }}</v-chip
            >
        </v-col>
    </v-row>
</template>
