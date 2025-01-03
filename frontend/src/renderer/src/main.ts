import { createPinia } from 'pinia';
import { createApp } from 'vue';

import axios from 'axios';
import App from './App.vue';
import { baseApiUrl } from './constants';
import { VueSignalR, connection, router, vuetify } from './plugins';

axios.defaults.baseURL = baseApiUrl;

const app = createApp(App);

app.use(createPinia());
app.use(vuetify);
app.use(router);
app.use(VueSignalR, { connection, autoOffInsideComponentScope: false, failFn: () => {} });

app.mount('#app');
