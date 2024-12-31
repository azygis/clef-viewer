import type { ClefViewerApi } from '@shared/types';

declare global {
    interface Window {
        api: ClefViewerApi;
    }
}
