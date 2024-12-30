import { createPinia } from 'pinia';
import { createApp } from 'vue';

import axios from 'axios';
import App from './App.vue';
import { VueSignalR, connection, router, vuetify } from './plugins';

axios.defaults.baseURL = 'http://localhost:5126/';

const app = createApp(App);

app.use(createPinia());
app.use(vuetify);
app.use(router);
app.use(VueSignalR, { connection, autoOffInsideComponentScope: false, failFn: () => {} });

app.mount('#app');
