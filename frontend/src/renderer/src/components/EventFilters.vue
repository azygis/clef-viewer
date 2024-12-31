<script setup lang="ts">
import { useLimitedTextLength } from '@/composables/limitedTextLength';
import { useEventViewerStore } from '@/stores/event-viewer';

const store = useEventViewerStore();
const { truncate } = useLimitedTextLength(50);
</script>
<template>
    <v-row>
        <v-col>
            <v-chip
                v-for="filter of store.filters"
                :key="filter.property"
                :title="filter.value"
                rounded
                closable
                class="mr-2 mb-2"
                density="compact"
                @click:close="store.removeFilter(filter)"
                >{{ filter.property }}: {{ truncate(filter.value) }}</v-chip
            >
        </v-col>
    </v-row>
</template>
