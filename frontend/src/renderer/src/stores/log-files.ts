import axios from 'axios';
import { defineStore } from 'pinia';

export const useLogFilesStore = defineStore('log-files', () => {
    async function loadLogFiles(paths: string[]) {
        return (await axios.post<string>('/log-files', paths)).data;
    }

    return { loadLogFiles };
});
