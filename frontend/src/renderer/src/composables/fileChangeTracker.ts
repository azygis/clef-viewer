import { useLogSessionStore } from '@/stores/log-session';
import { useSignalR } from '@dreamonkey/vue-signalr';
import { computed, reactive } from 'vue';
import { useExecutingState } from './executingState';

const changedFiles = reactive(new Map<string, Set<string>>());

export function useFileChangeTracker() {
    const signalr = useSignalR();
    function startChangeTracking() {
        signalr.on('FilesChanged', onFileChanged);
    }

    function stopChangeTracking() {
        signalr.off('FilesChanged', onFileChanged);
    }

    function onFileChanged(sessionId: string, filePath: string) {
        let fileSet = changedFiles.get(sessionId);
        if (!fileSet) {
            fileSet = new Set<string>();
            changedFiles.set(sessionId, fileSet);
        }
        fileSet.add(filePath);
    }

    function forSession(sessionId: string) {
        const { isExecuting: isReloadingChanges, execute: executeReload } = useExecutingState();
        const hasChangedFiles = computed(
            () => changedFiles.size > 0 && (changedFiles.get(sessionId)?.size ?? 0) > 0,
        );

        async function reloadChanges() {
            const fileSet = changedFiles.get(sessionId);
            if (!fileSet || !fileSet.size) {
                return;
            }

            const filePaths = [...fileSet];
            await executeReload(() => useLogSessionStore().reload(filePaths));
            fileSet.clear();
        }

        function clearChanges() {
            const fileSet = changedFiles.get(sessionId);
            if (!fileSet || !fileSet.size) {
                return;
            }

            fileSet.clear();
        }

        return { isReloadingChanges, hasChangedFiles, reloadChanges, clearChanges };
    }

    return { startChangeTracking, stopChangeTracking, forSession };
}
