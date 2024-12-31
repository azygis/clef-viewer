<script setup lang="ts">
import EventCounts from '@/components/EventCounts.vue';
import EventFilters from '@/components/EventFilters.vue';
import EventLevelChip from '@/components/EventLevelChip.vue';
import EventPropertyTable from '@/components/EventPropertyTable.vue';
import EventsViewSettings from '@/components/EventsViewSettings.vue';
import { useExecutingState } from '@/composables/executingState';
import { useLimitedTextLength } from '@/composables/limitedTextLength';
import { useEventViewerStore } from '@/stores/event-viewer';
import { LogEvent, SearchLogEventsRequest, useLogSessionStore } from '@/stores/log-session';
import { useSignalR } from '@dreamonkey/vue-signalr';
import { formatDate, normalizeDate } from '@vueuse/core';
import { storeToRefs } from 'pinia';
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';

const { dateFormatString, messageTextLimit, filters } = storeToRefs(useEventViewerStore());
const { truncate } = useLimitedTextLength(messageTextLimit);
const sessionStore = useLogSessionStore();
const { sessionId: currentSessionId, events, totalEvents } = storeToRefs(sessionStore);
const { isExecuting: isLoading, execute: executeLoad } = useExecutingState(true);
const { isExecuting: isReloading, execute: executeReload } = useExecutingState();
const searchExpression = ref<string | null | undefined>();
const searchRequest = reactive<SearchLogEventsRequest>({
    pageNumber: 1,
    pageSize: 50,
    sortOrder: 'desc',
    filters: filters.value,
});
watch(filters, (filters) => (searchRequest.filters = filters), { deep: true });
watch(searchRequest, (request) => loadEvents(request), { immediate: true });
function loadEvents(request: SearchLogEventsRequest) {
    return executeLoad(() => sessionStore.loadEvents(request));
}

const expanded = ref<string[]>();

function clearSearch() {
    searchExpression.value = undefined;
    searchRequest.expression = undefined;
}

function changeOptions(input: {
    page: number;
    itemsPerPage: number;
    sortBy: { order: 'asc' | 'desc' }[];
}) {
    const { page, itemsPerPage, sortBy } = input;
    searchRequest.pageNumber = page;
    searchRequest.pageSize = itemsPerPage;
    searchRequest.sortOrder = sortBy[0].order;
}

function formatTimestamp(item: LogEvent) {
    return formatDate(normalizeDate(item.timestamp), dateFormatString.value);
}

const changedFiles = reactive(new Set<string>());
const hasChangedFiles = computed(() => changedFiles.size > 0);
const signalr = useSignalR();
function onFileChanged(sessionId: string, filePath: string) {
    if (currentSessionId.value === sessionId) {
        changedFiles.add(filePath);
    }
}
onMounted(() => {
    signalr.on('FileChanged', onFileChanged);
});
onBeforeUnmount(() => {
    signalr.off('FileChanged');
    sessionStore.setId(undefined);
});
async function reload() {
    const filePaths = [...changedFiles];
    await executeReload(() => sessionStore.reload(filePaths));
    changedFiles.clear();
    loadEvents(searchRequest);
}
</script>
<template>
    <v-container fluid>
        <v-form @submit.prevent="searchRequest.expression = searchExpression">
            <v-row>
                <v-col cols="10">
                    <v-text-field
                        v-model="searchExpression"
                        clearable
                        hide-details
                        density="compact"
                        label="Search expression"
                        @click:clear="clearSearch"
                    />
                </v-col>
                <v-col cols="2">
                    <v-btn type="submit">Search</v-btn>
                </v-col>
            </v-row>
        </v-form>
        <EventFilters />
        <EventCounts />
        <v-row>
            <v-col>
                <v-data-table-server
                    v-model:expanded="expanded"
                    :headers="[
                        {
                            title: 'Timestamp',
                            value: 'timestamp',
                            sortable: true,
                        },
                        {
                            title: 'Level',
                            value: 'level',
                        },
                        {
                            title: 'Message',
                            value: 'message',
                        },
                    ]"
                    :sort-by="[{ key: 'timestamp', order: searchRequest.sortOrder }]"
                    :page="searchRequest.pageNumber"
                    :items-per-page="searchRequest.pageSize"
                    :items-per-page-options="[10, 25, 50, 100, 250, 500]"
                    :items-length="totalEvents"
                    :items="events"
                    :loading="isLoading"
                    item-value="timestamp"
                    class="event-table"
                    show-expand
                    hover
                    @update:options="changeOptions"
                >
                    <template #top>
                        <v-sheet class="d-flex mt-4 mx-4">
                            <h2 class="me-auto">Events</h2>
                            <EventsViewSettings />
                        </v-sheet>
                    </template>
                    <template #[`item.timestamp`]="{ item }">
                        <span>{{ formatTimestamp(item) }}</span>
                    </template>
                    <template #[`item.level`]="{ item }">
                        <EventLevelChip :level="item.level" />
                    </template>
                    <template #[`item.message`]="{ item }">
                        <span>{{ truncate(item.message) }}</span>
                    </template>
                    <template #expanded-row="{ item, columns }">
                        <tr class="expanded">
                            <td :colspan="columns.length">
                                <v-row class="message">
                                    <v-col>
                                        <h2>Rendered message</h2>
                                        <blockquote>
                                            <pre>{{ item.message }}</pre>
                                        </blockquote>
                                    </v-col>
                                </v-row>
                                <v-row v-if="item.exception" class="exception">
                                    <v-col>
                                        <h2>Exception</h2>
                                        <blockquote>
                                            <pre>{{ item.exception }}</pre>
                                        </blockquote>
                                    </v-col>
                                </v-row>
                                <v-row class="properties">
                                    <v-col>
                                        <EventPropertyTable :item="item" />
                                    </v-col>
                                </v-row>
                            </td>
                        </tr>
                    </template>
                </v-data-table-server>
            </v-col>
        </v-row>
        <v-snackbar v-model="hasChangedFiles">
            Log files have been changed
            <template #actions>
                <v-btn color="primary" :loading="isReloading" @click="reload">Reload</v-btn>
                <v-btn color="secondary" @click="changedFiles.clear()">Close</v-btn>
            </template>
        </v-snackbar>
    </v-container>
</template>
<style lang="scss" scoped>
.event-table {
    .expanded {
        .message,
        .exception {
            max-width: calc(100vw - 112px);

            > div {
                overflow: auto;
            }

            blockquote {
                border-left: 1px solid;
                padding-left: 10px;
            }
        }

        .message {
            blockquote {
                border-left-color: green;
            }
        }

        .exception {
            h2 {
                color: red;
            }

            blockquote {
                border-left-color: red;
            }
        }
    }
}
</style>
