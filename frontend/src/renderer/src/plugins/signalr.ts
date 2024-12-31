import { VueSignalR } from '@dreamonkey/vue-signalr';
import { HubConnectionBuilder } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
    .withUrl('http://localhost:5126/log-hub', { withCredentials: false })
    .build();

export { connection, VueSignalR };
