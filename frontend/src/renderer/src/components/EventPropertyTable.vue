<script setup lang="ts">
import { useEventViewerStore } from '@/stores/event-viewer';
import { LogEvent } from '@/stores/log-session';
import EventLevelChip from './EventLevelChip.vue';
import EventPropertyInnerTable from './EventPropertyInnerTable.vue';

defineProps<{
    item: LogEvent;
}>();

const { setFilter } = useEventViewerStore();
</script>
<template>
    <h2>General properties</h2>
    <v-table hover density="compact">
        <tbody>
            <tr>
                <td>Level</td>
                <td><EventLevelChip :level="item.level" @click="setFilter('@Level', $event)" /></td>
            </tr>
            <tr>
                <td>Timestamp</td>
                <td>{{ item.timestamp }}</td>
            </tr>
            <tr>
                <td>MessageTemplate</td>
                <td class="pointer" @click="setFilter('@MessageTemplate', item.messageTemplate)">
                    {{ item.messageTemplate }}
                </td>
            </tr>
        </tbody>
    </v-table>
    <h2>Other properties</h2>
    <EventPropertyInnerTable :input="item.properties" />
</template>
<style lang="scss" scoped>
.pointer {
    cursor: pointer;
}
</style>
