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
    search?: string;
}

export interface LogEvent {
    level: string;
    message: string;
    messageTemplate: string;
    exception: string | null;
    timestamp: string;
    properties: EventProperties;
}

export interface LogEntriesResponse {
    totalEvents: number;
    events: LogEvent[];
}

export const useLogSessionStore = defineStore('log-session', () => {
    const sessionId = ref<string>(undefined!);
    const events = ref<LogEvent[]>([]);
    const totalEvents = ref<number>(0);
    const hasActiveSession = computed(() => !!sessionId.value);

    function setId(id: string) {
        sessionId.value = id;
    }

    async function loadEvents(request: SearchLogEventsRequest) {
        const response = (
            await axios.post<LogEntriesResponse>(`log-sessions/${sessionId.value}`, request)
        ).data;
        events.value = response.events;
        totalEvents.value = response.totalEvents;
    }

    return { sessionId, hasActiveSession, events, totalEvents, setId, loadEvents };
});
