<script setup lang="ts">
import { useEventViewerStore } from '@/stores/event-viewer';
import { storeToRefs } from 'pinia';

const { dateFormatString, messageTextLimit } = storeToRefs(useEventViewerStore());
</script>
<template>
    <v-dialog width="50%">
        <template #activator="{ props }">
            <v-btn variant="plain" icon="mdi-cog" v-bind="props" />
        </template>
        <template #default="{ isActive }">
            <v-card title="Table settings">
                <v-card-text>
                    <v-row>
                        <v-col>
                            <v-text-field
                                v-model="messageTextLimit"
                                label="Message text limit (in table)"
                                density="compact"
                            />
                        </v-col>
                        <v-col>
                            <v-text-field
                                v-model="dateFormatString"
                                label="Timestamp format"
                                density="compact"
                        /></v-col>
                    </v-row>
                </v-card-text>
                <v-card-actions>
                    <v-btn color="secondary" @click="isActive.value = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </template>
    </v-dialog>
</template>
