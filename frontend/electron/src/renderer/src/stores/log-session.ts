import axios from 'axios';
import { defineStore } from 'pinia';
import { computed, ref } from 'vue';

export type EventProperties = Record<
    string,
    { value: unknown } | { properties: { name: string; value: { value: unknown } }[] }
>;

export interface SearchLogEventsRequest {
    pageNumber: number;
    pageSize: number;
    sortOrder: 'asc' | 'desc';
    expression?: string | null;
}

export interface LogEvent {
    level: string;
    message: string;
    messageTemplate: string;
    exception: string | null;
    timestamp: string;
    properties: EventProperties;
}

export interface LogEntryCounts {
    total: number;
    debug: number;
    error: number;
    fatal: number;
    info: number;
    verbose: number;
    warning: number;
    messageTemplates: Record<string, number>;
}

export interface LogEntriesResponse {
    counts: LogEntryCounts;
    events: LogEvent[];
}

export const useLogSessionStore = defineStore('log-session', () => {
    const sessionId = ref<string>(undefined!);
    const events = ref<LogEvent[]>([]);
    const counts = ref<LogEntryCounts>();
    const totalEvents = computed(() => counts.value?.total ?? 0);
    const hasActiveSession = computed(() => !!sessionId.value);

    function setId(id: string) {
        sessionId.value = id;
    }

    async function loadEvents(request: SearchLogEventsRequest) {
        const response = (
            await axios.post<LogEntriesResponse>(`log-sessions/${sessionId.value}`, request)
        ).data;
        events.value = response.events;
        counts.value = response.counts;
    }

    return { sessionId, hasActiveSession, events, totalEvents, counts, setId, loadEvents };
});
