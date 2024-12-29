<script setup lang="ts">
import { useEventViewerStore } from '@/stores/event-viewer';
import { storeToRefs } from 'pinia';
import { ref } from 'vue';

const { dateFormatString, messageTextLimit, searchExpression } = storeToRefs(useEventViewerStore());
const expression = ref(searchExpression.value);
const isOpen = ref(false);
</script>
<template>
    <v-row>
        <v-col cols="11">
            <v-form @submit.prevent="searchExpression = expression">
                <v-row>
                    <v-col cols="10">
                        <v-text-field
                            v-model="expression"
                            clearable
                            density="compact"
                            label="Search expression"
                            @click:clear="searchExpression = null"
                        />
                    </v-col>
                    <v-col cols="2">
                        <v-btn type="submit">Search</v-btn>
                    </v-col>
                </v-row>
            </v-form>
        </v-col>
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
