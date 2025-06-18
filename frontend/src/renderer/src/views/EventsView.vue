<script setup lang="ts">
import EventCounts from '@/components/EventCounts.vue';
import EventFilters from '@/components/EventFilters.vue';
import EventLevelChip from '@/components/EventLevelChip.vue';
import EventPropertyTable from '@/components/EventPropertyTable.vue';
import EventsViewSettings from '@/components/EventsViewSettings.vue';
import { useExecutingState } from '@/composables/executingState';
import { useFileChangeTracker } from '@/composables/fileChangeTracker';
import { useLimitedTextLength } from '@/composables/limitedTextLength';
import { useEventViewerStore } from '@/stores/event-viewer';
import { LogEvent, SearchLogEventsRequest, useLogSessionStore } from '@/stores/log-session';
import { formatDate, normalizeDate } from '@vueuse/core';
import { storeToRefs } from 'pinia';
import { computed, reactive, ref, watch } from 'vue';
import { type VDataTableServer } from 'vuetify/components';

const {
    dateFormatString,
    messageTextLimit,
    filters,
    columns: additionalColumns,
} = storeToRefs(useEventViewerStore());
const { toggleColumn } = useEventViewerStore();
const { truncate } = useLimitedTextLength(messageTextLimit);
const sessionStore = useLogSessionStore();
const { sessionId: currentSessionId, events, totalEvents } = storeToRefs(sessionStore);
const { isExecuting: isLoading, execute: executeLoad } = useExecutingState(true);
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
type TableHeaders = VDataTableServer['headers'];
const defaultHeaders: TableHeaders = [
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
];
const headers = computed<TableHeaders>(() => {
    const result = [...defaultHeaders];
    for (const column of additionalColumns.value) {
        result.push({
            title: column,
            key: column,
            sortable: false,
            value: (item) => truncate(item.properties[column]?.value) ?? '-',
        });
    }
    return result;
});

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

const { hasChangedFiles, isReloadingChanges, reloadChanges, clearChanges } =
    useFileChangeTracker().forSession(currentSessionId.value!);
async function reloadChangedFiles() {
    await reloadChanges();
    if (searchRequest.pageNumber > 1) {
        searchRequest.pageNumber = 1;
    } else {
        loadEvents(searchRequest);
    }
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
                    :headers="headers"
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
                    <template
                        v-for="name of additionalColumns"
                        :key="name"
                        #[`header.${name}`]="{ column }"
                    >
                        {{ column.title }}
                        <v-icon size="small" @click="toggleColumn(name)">mdi-close</v-icon>
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
                <div class="snackbar-actions">
                    <v-btn
                        color="primary"
                        density="comfortable"
                        variant="flat"
                        :loading="isReloadingChanges"
                        @click="reloadChangedFiles"
                        >Reload</v-btn
                    >
                    <v-btn
                        color="secondary"
                        density="comfortable"
                        variant="flat"
                        @click="clearChanges"
                        >Close</v-btn
                    >
                </div>
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

.snackbar-actions {
    display: flex;
    gap: 8px;
}
</style>
