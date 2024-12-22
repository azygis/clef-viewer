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
            beforeEnter: (to) => {
                const { setId } = useLogSessionStore();
                setId(to.params.sessionId as string);
            },
        },
    ],
});

export default router;
