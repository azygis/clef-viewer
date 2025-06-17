import { electronApp, is, optimizer } from '@electron-toolkit/utils';
import { ChildProcess, spawn } from 'child_process';
import { app, BrowserWindow, globalShortcut, ipcMain } from 'electron';
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

app.whenReady().then(async () => {
    electronApp.setAppUserModelId('com.azygis.clef-viewer');

    app.on('browser-window-created', (_, window) => {
        optimizer.watchWindowShortcuts(window);
    });

    startEventListeners();
    startBackend();
    // Wait for backend to be alive by polling the health endpoint
    try {
        await waitForBackendAlive();
    } catch (error) {
        console.error('Failed to wait for backend:', error);
    }

    registerKeyboardShortcuts();
    createWindow();
});

// Quit when all windows are closed
app.on('window-all-closed', () => {
    app.quit();
});

app.on('will-quit', () => {
    globalShortcut.unregisterAll();
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

app.on('quit', () => {
    backendProcess?.kill('SIGTERM');
    globalShortcut.unregisterAll();
});

let backendProcess: ChildProcess | undefined;
function startBackend() {
    if (is.dev) {
        // In dev it should be started outside of electron itself
        return;
    }

    const path = join(__dirname, '..', '..', '..', 'backend', 'ClefViewer.API');
    backendProcess = spawn(path);
}

function registerKeyboardShortcuts() {
    // Register Ctrl+Shift+F12 to toggle DevTools
    globalShortcut.register('CommandOrControl+Shift+F12', () => {
        const focusedWindow = BrowserWindow.getFocusedWindow();
        if (focusedWindow) {
            focusedWindow.webContents.toggleDevTools();
        }
    });
}

async function waitForBackendAlive(): Promise<void> {
    const maxAttempts = 60; // 30 seconds / 500ms = 60 attempts
    const delayMs = 500;
    const backendUrl = 'http://localhost:61455/live';

    // Create a loading dialog
    const loadingWindow = new BrowserWindow({
        width: 350,
        height: 180,
        show: false,
        frame: false,
        alwaysOnTop: false,
        resizable: false,
        skipTaskbar: true,
        webPreferences: {
            nodeIntegration: true,
            contextIsolation: false,
        },
    });

    // Set up IPC handler for quit app
    const quitHandler = () => {
        loadingWindow.close();
        app.quit();
    };
    ipcMain.removeAllListeners('quit-app'); // Remove any existing listeners
    ipcMain.on('quit-app', quitHandler);

    // Function to update the loading message via IPC
    const updateLoadingMessage = (attempt: number, status: string) => {
        loadingWindow.webContents.send('update-loading-message', {
            attempt,
            maxAttempts,
            status,
        });
    };

    // Load the HTML file from the correct path
    const htmlPath = is.dev
        ? join(__dirname, '../../src/main/loading-dialog.html')
        : join(__dirname, 'loading-dialog.html');
    await loadingWindow.loadFile(htmlPath);
    loadingWindow.show();

    try {
        for (let attempt = 1; attempt <= maxAttempts; attempt++) {
            try {
                const response = await fetch(backendUrl, { method: 'HEAD' });
                if (response.status === 200) {
                    await updateLoadingMessage(attempt, 'Backend connected successfully!');
                    // Brief delay to show success message
                    await new Promise((resolve) => setTimeout(resolve, 300));
                    loadingWindow.close();
                    return; // Backend is alive, continue
                }
                await updateLoadingMessage(attempt, `Failed: HTTP ${response.status}`);
            } catch (error) {
                // Connection failed, continue trying
                const errorMessage =
                    error instanceof Error
                        ? `${error.message}: ${error.cause}`
                        : 'Connection failed';
                await updateLoadingMessage(attempt, `Failed: ${errorMessage}`);
            }

            if (attempt < maxAttempts) {
                await new Promise((resolve) => setTimeout(resolve, delayMs));
            }
        }

        // If we get here, all attempts failed
        loadingWindow.close();
        const { dialog } = await import('electron');
        dialog.showErrorBox(
            'Backend Connection Error',
            'Failed to connect to the backend after 10 seconds. The application may not work correctly.',
        );
    } catch (error) {
        loadingWindow.close();
        throw error;
    }
}
