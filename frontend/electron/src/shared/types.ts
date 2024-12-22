export interface ClefViewerApi {
    selectFiles: () => Promise<string[]>;
    selectDirectory: () => Promise<string>;
}
