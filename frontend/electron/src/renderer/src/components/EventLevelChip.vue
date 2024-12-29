<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
    level: string;
}>();
const normalized = computed(() => {
    let level = props.level.toLowerCase();
    if (level === 'information') {
        level = 'info';
    }
    return level;
});
const color = computed(() => {
    const level = normalized.value;
    switch (level) {
        case 'debug':
            return 'success';
        case 'fatal':
            return 'red';
        case 'verbose':
            return '';
        default:
            return level;
    }
});
const text = computed(() => {
    let level = normalized.value;
    if (level === 'info') {
        level = 'information';
    }
    return String(level).charAt(0).toUpperCase() + String(level).slice(1);
});
</script>
<template>
    <v-chip rounded :color="color">{{ text }}</v-chip>
</template>
