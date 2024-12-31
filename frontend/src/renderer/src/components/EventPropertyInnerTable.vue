<script setup lang="ts">
import { useEventViewerStore } from '@/stores/event-viewer';
import { EventElement, EventProperties, EventProperty } from '@/stores/log-session';

defineProps<{
    input: EventProperties | EventProperty[] | EventElement[];
}>();

const { setFilter } = useEventViewerStore();
</script>
<template>
    <v-table hover density="compact">
        <tbody>
            <template v-if="!Array.isArray(input)">
                <tr v-for="(property, key) of input" :key="key">
                    <td v-if="key">{{ key }}</td>
                    <td v-if="'value' in property">
                        <EventPropertyInnerTable
                            v-if="typeof property.value === 'object'"
                            :input="property.value.elements"
                        />
                        <span
                            v-else
                            class="pointer"
                            @click="setFilter(key, property.value.toString())"
                            >{{ property.value }}</span
                        >
                    </td>
                    <td v-if="'properties' in property">
                        <EventPropertyInnerTable :input="property.properties" />
                    </td>
                    <td v-if="'elements' in property">
                        <EventPropertyInnerTable :input="property.elements" />
                    </td>
                </tr>
            </template>
            <template v-else-if="input.length > 0">
                <tr v-for="(prop, idx) of input" :key="idx">
                    <td v-if="'name' in prop">{{ prop.name }}</td>
                    <td v-if="typeof prop.value === 'object'">
                        <EventPropertyInnerTable
                            v-if="'elements' in prop.value"
                            :input="prop.value.elements!"
                        />
                        <span v-else>{{ prop.value.value }}</span>
                    </td>
                    <td v-else-if="prop.value">{{ prop.value }}</td>
                    <td v-if="'properties' in prop">
                        <EventPropertyInnerTable :input="prop.properties!" />
                    </td>
                    <td v-if="'elements' in prop">
                        <EventPropertyInnerTable :input="prop.elements!" />
                    </td>
                </tr>
            </template>
        </tbody>
    </v-table>
</template>
<style lang="scss" scoped>
.pointer {
    cursor: pointer;
}
</style>
