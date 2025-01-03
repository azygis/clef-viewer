import { electronApp, is, optimizer } from '@electron-toolkit/utils';
import { ChildProcess, spawn } from 'child_process';
import { app, BrowserWindow } from 'electron';
import { join } from 'path';
import icon from '../../resources/icon.png?asset';
import { startEventListeners } from './events';

let win: BrowserWindow | undefined;

function createWindow(): void {
    const mainWindow = new BrowserWindow({
        width: 900,
        height: 670,
        show: false,
        autoHideMenuBar: true,
        ...(process.platform === 'linux' ? { icon } : {}),
        webPreferences: {
            preload: join(__dirname, '../preload/index.js'),
        },
    });

    mainWindow.on('ready-to-show', () => mainWindow.show());

    // HMR for renderer base on electron-vite cli.
    // Load the remote URL for development or the local html file for production.
    if (is.dev && process.env['ELECTRON_RENDERER_URL']) {
        mainWindow.loadURL(process.env['ELECTRON_RENDERER_URL']);
    } else {
        mainWindow.loadFile(join(__dirname, '../renderer/index.html'));
    }
    win = mainWindow;
}

app.whenReady().then(() => {
    electronApp.setAppUserModelId('com.azygis.clef-viewer');

    app.on('browser-window-created', (_, window) => {
        optimizer.watchWindowShortcuts(window);
    });

    startEventListeners();
    startBackend();

    createWindow();

    app.on('activate', function () {
        // On macOS it's common to re-create a window in the app when the
        // dock icon is clicked and there are no other windows open.
        if (BrowserWindow.getAllWindows().length === 0) createWindow();
    });
});

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

if (!app.requestSingleInstanceLock()) {
    // Lock is acquired by another instance already, quit the app
    app.quit();
} else {
    app.on('second-instance', () => {
        if (win && win.isMinimized()) {
            win.restore();
            win.focus();
        }
    });
}

app.on('quit', () => backendProcess?.kill('SIGTERM'));

let backendProcess: ChildProcess | undefined;
function startBackend() {
    if (is.dev) {
        // In dev it should be started outside of electron itself
        return;
    }

    const path = join(__dirname, '..', '..', '..', 'backend', 'ClefViewer.API');
    backendProcess = spawn(path);
}
