<script setup lang="ts">
import EventPropertyTable from '@/components/EventPropertyTable.vue';
import EventsViewSettings from '@/components/EventsViewSettings.vue';
import { useExecutingState } from '@/composables/useExecutingState';
import { useEventViewerStore } from '@/stores/event-viewer';
import { LogEvent, SearchLogEventsRequest, useLogSessionStore } from '@/stores/log-session';
import { formatDate, normalizeDate } from '@vueuse/core';
import { storeToRefs } from 'pinia';
import { reactive, ref, watch } from 'vue';

const { dateFormatString, messageTextLimit } = storeToRefs(useEventViewerStore());
const sessionStore = useLogSessionStore();
const { events, totalEvents } = storeToRefs(sessionStore);
const { isExecuting: isLoading, execute: executeLoad } = useExecutingState(true);
const searchRequest = reactive<SearchLogEventsRequest>({
    pageNumber: 1,
    pageSize: 50,
});
watch(
    searchRequest,
    (request) => {
        executeLoad(() => sessionStore.loadEvents(request));
    },
    { immediate: true },
);

const expanded = ref();

function changeOptions({ page, itemsPerPage }: { page: number; itemsPerPage: number }) {
    searchRequest.pageNumber = page;
    searchRequest.pageSize = itemsPerPage;
}

function formatTimestamp(item: LogEvent) {
    return formatDate(normalizeDate(item.timestamp), dateFormatString.value);
}

function formatMessage(item: LogEvent) {
    const message = item.message;
    const limit = messageTextLimit.value;
    if (!limit || limit < 1 || message.length <= limit) {
        return item.message;
    }

    const remainder = message.length - limit;
    return `${item.message.slice(0, limit)}… (${remainder} more)`;
}
</script>
<template>
    <v-container fluid>
        <EventsViewSettings />
        <v-row>
            <v-col>
                <v-data-table-server
                    v-model:expanded="expanded"
                    :headers="[
                        {
                            title: 'Timestamp',
                            value: 'timestamp',
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
                    :page="searchRequest.pageNumber"
                    :items-per-page="searchRequest.pageSize"
                    :items-per-page-options="[10, 25, 50, 100, 250]"
                    :items-length="totalEvents"
                    :items="events"
                    :loading="isLoading"
                    item-value="timestamp"
                    class="event-table"
                    show-expand
                    @update:options="changeOptions"
                >
                    <template #[`item.timestamp`]="{ item }">
                        <span>{{ formatTimestamp(item) }}</span>
                    </template>
                    <template #[`item.message`]="{ item }">
                        <span>{{ formatMessage(item) }}</span>
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
                                        <h2>Properties</h2>
                                        <EventPropertyTable :item="item" />
                                    </v-col>
                                </v-row>
                            </td>
                        </tr>
                    </template>
                </v-data-table-server>
            </v-col>
        </v-row>
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
