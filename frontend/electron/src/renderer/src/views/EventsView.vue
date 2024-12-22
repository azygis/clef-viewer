<script setup lang="ts">
import { useExecutingState } from '@/composables/useExecutingState';
import { SearchLogEventsRequest, useLogSessionStore } from '@/stores/log-session';
import { storeToRefs } from 'pinia';
import { reactive, watch } from 'vue';

const store = useLogSessionStore();
const { events, totalEvents } = storeToRefs(store);
const { isExecuting: isLoading, execute: executeLoad } = useExecutingState(true);
const searchRequest = reactive<SearchLogEventsRequest>({
    pageNumber: 1,
    pageSize: 50,
});
watch(
    searchRequest,
    (request) => {
        executeLoad(() => store.loadEvents(request));
    },
    { immediate: true },
);

function changeOptions({ page, itemsPerPage }: { page: number; itemsPerPage: number }) {
    searchRequest.pageNumber = page;
    searchRequest.pageSize = itemsPerPage;
}
</script>
<template>
    <v-container fluid>
        <v-data-table-server
            :headers="[
                {
                    title: 'Level',
                    value: 'level',
                },
                {
                    title: 'Timestamp',
                    value: 'timestamp',
                },
                {
                    title: 'Message',
                    value: 'message',
                },
            ]"
            :page="searchRequest.pageNumber"
            :items-per-page="searchRequest.pageSize"
            :items-per-page-options="[10, 25, 50, 100, 250]"
            :items-length="totalEvents"
            :items="events"
            :loading="isLoading"
            density="compact"
            @update:options="changeOptions"
        ></v-data-table-server>
    </v-container>
</template>
