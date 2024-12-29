<script setup lang="ts">
import { LogEvent } from '@/stores/log-session';
import EventLevelChip from './EventLevelChip.vue';

defineProps<{
    item: LogEvent;
}>();
</script>
<template>
    <v-table density="compact">
        <tbody>
            <tr>
                <td>Level</td>
                <td><EventLevelChip :level="item.level" /></td>
            </tr>
            <tr>
                <td>Timestamp</td>
                <td>{{ item.timestamp }}</td>
            </tr>
            <tr>
                <td>MessageTemplate</td>
                <td>{{ item.messageTemplate }}</td>
            </tr>
            <tr v-for="(property, key) of item.properties" :key="key">
                <td>{{ key }}</td>
                <td v-if="'value' in property">
                    {{ property.value }}
                </td>
                <td v-else-if="'properties' in property">
                    <v-table density="compact">
                        <tbody>
                            <tr v-for="inner of property.properties" :key="inner.name">
                                <td>{{ inner.name }}</td>
                                <td>{{ inner.value.value }}</td>
                            </tr>
                        </tbody>
                    </v-table>
                </td>
            </tr>
        </tbody>
    </v-table>
</template>
