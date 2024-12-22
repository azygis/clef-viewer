import '@mdi/font/css/materialdesignicons.css';
import { createVuetify } from 'vuetify';
import { md3 } from 'vuetify/blueprints';
import 'vuetify/styles';

export default createVuetify({
    blueprint: md3,
    theme: {
        defaultTheme: 'dark',
    },
});
