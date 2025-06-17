import { electronApp, is, optimizer } from '@electron-toolkit/utils';
import { ChildProcess, spawn } from 'child_process';
import { app, BrowserWindow, globalShortcut } from 'electron';
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
            nodeIntegration: true,
            contextIsolation: false,
        },
    });

    // Show loading screen first
    const loadingHtmlPath = is.dev
        ? join(__dirname, '../../src/main/loading-dialog.html')
        : join(__dirname, 'loading-dialog.html');

    mainWindow.loadFile(loadingHtmlPath);

    mainWindow.once('ready-to-show', async () => {
        mainWindow.show();

        try {
            // Wait for backend to be ready
            await waitForBackendAlive(mainWindow);

            // Backend is ready, load the main application
            if (is.dev && process.env['ELECTRON_RENDERER_URL']) {
                mainWindow.loadURL(process.env['ELECTRON_RENDERER_URL']);
            } else {
                mainWindow.loadFile(join(__dirname, '../renderer/index.html'));
            }
        } catch (error) {
            console.error('Failed to start backend:', error);
        }
    });

    win = mainWindow;
}

app.whenReady().then(() => {
    electronApp.setAppUserModelId('com.azygis.clef-viewer');

    app.on('browser-window-created', (_, window) => {
        optimizer.watchWindowShortcuts(window);
    });

    startEventListeners();
    startBackend();
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

async function waitForBackendAlive(loadingWindow?: BrowserWindow): Promise<void> {
    const maxAttempts = 60; // 30 seconds / 500ms = 60 attempts
    const delayMs = 500;
    const backendUrl = 'http://localhost:61455/live';

    // Function to update the loading message
    const updateLoadingMessage = (attempt: number, status: string) => {
        if (loadingWindow && !loadingWindow.isDestroyed()) {
            loadingWindow.webContents.send('update-loading-message', {
                attempt,
                maxAttempts,
                status,
            });
        }
    };

    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
        try {
            const response = await fetch(backendUrl, { method: 'HEAD' });
            if (response.status === 200) {
                updateLoadingMessage(attempt, 'Backend connected successfully!');
                // Brief delay to show success message
                await new Promise((resolve) => setTimeout(resolve, 300));
                return; // Backend is alive, exit immediately
            }
            updateLoadingMessage(attempt, `Failed: HTTP ${response.status}`);
        } catch (error) {
            // Connection failed, continue trying
            const errorMessage = error instanceof Error ? `${error.message}` : 'Connection failed';
            updateLoadingMessage(attempt, `Connecting... (${errorMessage})`);
        }

        if (attempt < maxAttempts) {
            await new Promise((resolve) => setTimeout(resolve, delayMs));
        }
    }

    // If we get here, all attempts failed
    const { dialog } = await import('electron');
    dialog.showErrorBox(
        'Backend Connection Error',
        'Failed to connect to the backend after 30 seconds. The application may not work correctly.',
    );
}
