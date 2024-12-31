<script setup lang="ts">
import { useExecutingState } from '@/composables/executingState';
import { LogSessionDTO, useLogSessionStore } from '@/stores/log-session';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const sessions = ref<LogSessionDTO[]>([]);
const { isExecuting: isLoading, execute: executeLoad } = useExecutingState(true);
const { getSessions } = useLogSessionStore();
onMounted(async () => (sessions.value = await executeLoad(getSessions)));
function openSession(session: LogSessionDTO) {
    router.push({ name: 'events', params: { sessionId: session.id } });
}
</script>
<template>
    <v-row>
        <v-col>
            <v-data-table
                :headers="[
                    {
                        title: 'Paths',
                        value: 'paths',
                    },
                    {
                        title: 'Events',
                        value: 'eventCount',
                    },
                ]"
                :items-per-page="-1"
                :items="sessions"
                :loading="isLoading"
                hover
                hide-default-footer
                @click:row="(_, row) => openSession(row.item)"
            >
                <template #top>
                    <h3 class="mt-4 ml-4">History</h3>
                </template>
                <template #[`item.paths`]="{ item }">
                    <span v-if="item.paths.length === 1">{{ item.paths[0] }}</span>
                    <ul v-else class="mx-4 my-4">
                        <li v-for="path of item.paths" :key="path">{{ path }}</li>
                    </ul>
                </template>
            </v-data-table>
        </v-col>
    </v-row>
</template>
