import { baseApiUrl } from '@/constants';
import { VueSignalR } from '@dreamonkey/vue-signalr';
import { HubConnectionBuilder } from '@microsoft/signalr';

const connection = new HubConnectionBuilder()
    .withUrl(`${baseApiUrl}log-hub`, { withCredentials: false })
    .build();

export { connection, VueSignalR };
