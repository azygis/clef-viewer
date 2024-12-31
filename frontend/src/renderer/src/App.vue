<script setup lang="ts">
import { storeToRefs } from 'pinia';
import { useLogSessionStore } from './stores/log-session';
import { useSnackbarStore } from './stores/snackbar';

const { visible, currentMessage, currentColor } = storeToRefs(useSnackbarStore());
const { hasActiveSession, sessionId } = storeToRefs(useLogSessionStore());
</script>

<template>
    <v-app id="inspire">
        <v-navigation-drawer absolute permanent rail expand-on-hover open-delay="500">
            <v-list density="compact" nav>
                <v-list-item
                    prepend-icon="mdi-open-in-app"
                    title="Open new"
                    :to="{ name: 'home' }"
                ></v-list-item>
                <v-list-item
                    v-if="hasActiveSession"
                    prepend-icon="mdi-table-large"
                    title="Current session"
                    :to="{ name: 'events', params: { sessionId } }"
                ></v-list-item>
            </v-list>
        </v-navigation-drawer>

        <v-main>
            <router-view />
        </v-main>
        <v-snackbar v-model="visible" :color="currentColor">{{ currentMessage }}</v-snackbar>
    </v-app>
</template>
