<script setup lang="ts">
import { useEventViewerStore } from '@/stores/event-viewer';
import { LogEntryCounts, useLogSessionStore } from '@/stores/log-session';
import { storeToRefs } from 'pinia';
import { computed, ref } from 'vue';
import EventLevelChip from './EventLevelChip.vue';

const { setFilter } = useEventViewerStore();
const { counts: countsRef } = storeToRefs(useLogSessionStore());
const counts = computed<LogEntryCounts>(() => {
    const value = countsRef.value;
    const total = value?.total ?? 0;
    const fatal = value?.fatal ?? 0;
    const error = value?.error ?? 0;
    const warning = value?.warning ?? 0;
    const info = value?.info ?? 0;
    const debug = value?.debug ?? 0;
    const verbose = value?.verbose ?? 0;
    const messageTemplates = value?.messageTemplates ?? {};
    return { fatal, error, warning, info, debug, verbose, total, messageTemplates };
});
const logLevelCounts = computed(() => {
    const exclude = ['total', 'messageTemplates'];
    return Object.entries(counts.value)
        .filter((x) => !exclude.includes(x[0]))
        .map((x) => {
            return { level: x[0], count: x[1] };
        });
});
const messageTemplates = computed(() =>
    Object.entries(counts.value.messageTemplates)
        .sort((a, b) => (a[1] < b[1] ? 1 : -1))
        .map((x) => {
            return { template: x[0], count: x[1] };
        }),
);
const messageTemplateCount = computed(() => Object.entries(messageTemplates.value).length);
const errorLikeCount = computed(() => counts.value.fatal + counts.value.error);
const isExpanded = ref(false);
</script>
<template>
    <v-row dense>
        <v-col v-if="!!countsRef">
            <span
                >{{ counts.total }} events, {{ errorLikeCount }} error-like.
                {{ messageTemplateCount }} message templates.</span
            >
            <v-icon @click="isExpanded = !isExpanded">{{
                isExpanded ? 'mdi-chevron-up' : 'mdi-chevron-down'
            }}</v-icon>
        </v-col>
        <v-col v-else>
            <span>…</span>
        </v-col>
    </v-row>
    <v-row v-if="isExpanded" dense>
        <v-col>
            <h2>Levels</h2>
            <v-table>
                <tbody>
                    <tr v-for="entry of logLevelCounts" :key="entry.level">
                        <td>
                            <EventLevelChip
                                :level="entry.level"
                                @click="setFilter('@Level', $event)"
                            />
                        </td>
                        <td>{{ entry.count }}</td>
                    </tr>
                </tbody>
            </v-table>
        </v-col>
        <v-col>
            <h2>Message templates</h2>
            <v-table class="message-templates">
                <tbody>
                    <tr v-for="(entry, index) of messageTemplates" :key="index">
                        <td @click="setFilter('@MessageTemplate', entry.template)">
                            {{ entry.template }}
                        </td>
                        <td>{{ entry.count }}</td>
                    </tr>
                </tbody>
            </v-table>
        </v-col>
    </v-row>
</template>
<style lang="scss" scoped>
.message-templates {
    max-height: 312px;
    overflow: auto;

    tr > td:first-child {
        cursor: pointer;
    }
}
</style>
