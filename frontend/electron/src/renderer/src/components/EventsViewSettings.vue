<script setup lang="ts">
import { useEventViewerStore } from '@/stores/event-viewer';
import { storeToRefs } from 'pinia';
import { ref } from 'vue';

const { dateFormatString, messageTextLimit } = storeToRefs(useEventViewerStore());
const isOpen = ref(false);
</script>
<template>
    <v-row>
        <v-col class="d-flex flex-row-reverse">
            <v-icon @click="isOpen = !isOpen">{{ isOpen ? 'mdi-cog-outline' : 'mdi-cog' }}</v-icon>
        </v-col>
    </v-row>
    <v-row v-if="isOpen">
        <v-col class="d-flex flex-row-reverse">
            <v-text-field
                v-model="messageTextLimit"
                label="Message text limit (in table)"
                density="compact"
            />
            <v-text-field v-model="dateFormatString" label="Timestamp format" density="compact" />
        </v-col>
    </v-row>
</template>
