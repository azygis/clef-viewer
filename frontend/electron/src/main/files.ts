import { BrowserWindow, dialog, ipcMain, WebContents } from 'electron';
import { FileEvents } from '../shared/files';

export function startFileEventListeners() {
    ipcMain.handle(FileEvents.SelectFiles, ({ sender }) => selectFiles(sender));
    ipcMain.handle(FileEvents.SelectDirectory, ({ sender }) => selectDirectory(sender));

    async function selectFiles(sender: WebContents) {
        const currentWindow = BrowserWindow.fromWebContents(sender)!;
        const { filePaths } = await dialog.showOpenDialog(currentWindow, {
            filters: [
                { name: 'Log File', extensions: ['txt', 'json', 'clef'] },
                { name: 'All Files', extensions: ['*'] },
            ],
            properties: ['openFile', 'multiSelections'],
            title: 'Open Log',
        });
        return filePaths;
    }

    async function selectDirectory(sender: WebContents) {
        const currentWindow = BrowserWindow.fromWebContents(sender)!;
        const { filePaths } = await dialog.showOpenDialog(currentWindow, {
            properties: ['openDirectory'],
            title: 'Open Log Directory',
        });
        return filePaths[0];
    }
}
