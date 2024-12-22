<script setup lang="ts">
import { useExecutingState } from '@/composables/useExecutingState';
import { useLogFilesStore } from '@/stores/log-files';
import { useSnackbarStore } from '@/stores/snackbar';
import { computed, reactive } from 'vue';
import { useRouter } from 'vue-router';

const { loadLogFiles } = useLogFilesStore();
const { isExecuting, execute } = useExecutingState();
const paths = reactive({
    files: new Set<string>(),
    directories: new Set<string>(),
});
const pathArray = computed(() => [...paths.files, ...paths.directories]);
const { showMessage } = useSnackbarStore();
const router = useRouter();

async function selectFiles() {
    let filesSkipped = false;
    for (const path of await window.api.selectFiles()) {
        let skipFile = false;
        for (const directoryPath of [...paths.directories]) {
            if (path.startsWith(directoryPath)) {
                filesSkipped = true;
                skipFile = true;
            }
        }
        if (!skipFile) {
            paths.files.add(path);
        }
    }
    if (filesSkipped) {
        showMessage(
            'Some files were skipped since selected directories already contain them',
            'warning',
        );
    }
}

async function selectDirectory() {
    const path = await window.api.selectDirectory();
    let filesRemoved = false;
    if (path) {
        paths.directories.add(path);
        for (const filePath of [...paths.files]) {
            if (filePath.startsWith(path)) {
                paths.files.delete(filePath);
                filesRemoved = true;
            }
        }
        if (filesRemoved) {
            showMessage('Some files were removed since directory already contains them', 'warning');
        }
    }
}

async function load() {
    const sessionId = await execute(() => loadLogFiles([...paths.files, ...paths.directories]));
    router.push({ name: 'events', params: { sessionId } });
}

function getFileName(path: string) {
    return path.substring(path.lastIndexOf('/') + 1);
}

function removePath(path: string) {
    paths.directories.delete(path);
    paths.files.delete(path);
}
</script>
<template>
    <v-row>
        <v-col cols="12">
            <v-btn-group>
                <v-btn @click="selectFiles">Select files</v-btn>
                <v-btn @click="selectDirectory">Select directory</v-btn>
                <v-btn :disabled="!pathArray.length" :loading="isExecuting" @click="load"
                    >Load selected {{ pathArray.length }} files/directories</v-btn
                >
            </v-btn-group>
        </v-col>
        <v-col v-if="pathArray.length" cols="12">
            <v-chip-group>
                <v-chip
                    v-for="(path, index) of pathArray"
                    :key="index"
                    :title="path"
                    closable
                    rounded
                    @click:close="removePath(path)"
                >
                    <template #prepend>
                        <v-icon v-if="paths.files.has(path)">mdi-file</v-icon>
                        <v-icon v-if="paths.directories.has(path)">mdi-folder</v-icon>
                    </template>
                    {{ getFileName(path) }}</v-chip
                >
            </v-chip-group>
        </v-col>
    </v-row>
    <v-row>
        <v-col>
            Selected files:
            <pre>
        {{ paths }}
    </pre
            >
        </v-col>
    </v-row>
</template>
