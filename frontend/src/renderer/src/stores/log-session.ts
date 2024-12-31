import axios from 'axios';
import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { EventFilter } from './event-viewer';

export interface LogSessionDTO {
    id: string;
    eventCount: number;
    paths: string[];
}

export interface EventElement {
    typeTag?: string;
    properties?: EventProperty[];
    value?: string | number | { value: unknown };
}

export interface EventProperty {
    name?: string;
    value?: {
        value?: unknown;
        elements?: EventElement[];
    };
    elements?: EventElement[];
}
export type EventProperties = Record<
    string,
    | { properties: EventProperty[] }
    | { elements: EventElement[] }
    | { value: string | number | { elements: EventProperty[] } }
>;

export interface SearchLogEventsRequest {
    pageNumber: number;
    pageSize: number;
    sortOrder: 'asc' | 'desc';
    expression?: string | null;
    filters?: EventFilter[];
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
    const sessionId = ref<string>();
    const events = ref<LogEvent[]>([]);
    const counts = ref<LogEntryCounts>();
    const totalEvents = computed(() => counts.value?.total ?? 0);
    const hasActiveSession = computed(() => !!sessionId.value);

    async function getSessions() {
        return (await axios.get<LogSessionDTO[]>('log-sessions')).data;
    }

    async function setId(id: string | undefined) {
        await Promise.all([setTrackChanges(sessionId.value, false), setTrackChanges(id, true)]);
        sessionId.value = id;
        if (!id) {
            events.value = [];
            counts.value = undefined;
        }
    }

    async function loadEvents(request: SearchLogEventsRequest) {
        const response = (
            await axios.post<LogEntriesResponse>(`log-sessions/${sessionId.value}`, request)
        ).data;
        events.value = response.events;
        counts.value = response.counts;
    }

    function setTrackChanges(id: string | undefined, track: boolean) {
        if (!id) {
            return Promise.resolve();
        }

        return axios.patch(`log-sessions/${id}/track`, null, { params: { track } });
    }

    function reload(filePath: string[]) {
        return axios.post(`log-sessions/${sessionId.value}/reload`, null, { params: { filePath } });
    }

    return {
        sessionId,
        hasActiveSession,
        events,
        totalEvents,
        counts,
        getSessions,
        setId,
        loadEvents,
        reload,
    };
});
