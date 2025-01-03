import { useLogSessionStore } from '@/stores/log-session';
import { createRouter, createWebHistory } from 'vue-router';
import HomeView from '../views/HomeView.vue';

const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes: [
        {
            path: '/',
            name: 'home',
            component: HomeView,
        },
        {
            path: '/events/:sessionId',
            name: 'events',
            component: () => import('../views/EventsView.vue'),
            beforeEnter: async (to, _from, next) => {
                const { setId } = useLogSessionStore();
                await setId(to.params.sessionId as string);
                next();
            },
        },
        {
            path: '/:catchAll(.*)',
            redirect: '/',
        },
    ],
});

export default router;
