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

    // Handle close button click to quit the app
    loadingWindow.webContents.on('dom-ready', () => {
        loadingWindow.webContents.executeJavaScript(`
            document.addEventListener('click', (event) => {
                if (event.target.classList.contains('close-btn')) {
                    require('electron').ipcRenderer.send('quit-app');
                }
            });
        `);
    });

    // Set up IPC handler for quit app
    const quitHandler = () => {
        loadingWindow.close();
        app.quit();
    };
    ipcMain.removeAllListeners('quit-app'); // Remove any existing listeners
    ipcMain.on('quit-app', quitHandler);

    // Function to update the loading message
    const updateLoadingMessage = (attempt: number, status: string) => {
        const loadingHtml = `
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {
                        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                        margin: 0;
                        padding: 20px;
                        background: #f0f0f0;
                        display: flex;
                        flex-direction: column;
                        align-items: center;
                        justify-content: center;
                        height: 100vh;
                        box-sizing: border-box;
                        position: relative;
                    }
                    .close-btn {
                        position: absolute;
                        top: 10px;
                        right: 15px;
                        width: 20px;
                        height: 20px;
                        background: none;
                        border: none;
                        font-size: 16px;
                        color: #999;
                        cursor: pointer;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                        border-radius: 50%;
                        transition: all 0.2s ease;
                    }
                    .close-btn:hover {
                        background: #e0e0e0;
                        color: #666;
                    }
                    .spinner {
                        border: 4px solid #f3f3f3;
                        border-top: 4px solid #3498db;
                        border-radius: 50%;
                        width: 30px;
                        height: 30px;
                        animation: spin .5s linear infinite;
                        margin-bottom: 15px;
                    }
                    @keyframes spin {
                        0% { transform: rotate(0deg); }
                        100% { transform: rotate(360deg); }
                    }
                    .message {
                        text-align: center;
                        color: #333;
                        font-size: 14px;
                        line-height: 1.4;
                    }
                    .attempt-info {
                        color: #666;
                        font-size: 12px;
                        margin-top: 8px;
                    }
                    .status {
                        margin-top: 5px;
                        font-size: 12px;
                        color: ${status.includes('Failed') ? '#e74c3c' : '#7f8c8d'};
                    }
                </style>
            </head>
            <body>
                <button class="close-btn" title="Close application">×</button>
                <div class="spinner"></div>
                <div class="message">Starting backend service...</div>
                <div class="attempt-info">Attempt ${attempt} of ${maxAttempts}</div>
                <div class="status">${status}</div>
            </body>
            </html>
        `;
        return loadingWindow.loadURL(
            `data:text/html;charset=utf-8,${encodeURIComponent(loadingHtml)}`,
        );
    };

    // Show initial loading state
    await updateLoadingMessage(1, 'Connecting...');
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
