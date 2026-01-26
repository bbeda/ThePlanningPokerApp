import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@/views/HomeView.vue')
    },
    {
      path: '/create',
      name: 'create-session',
      component: () => import('@/views/CreateSessionView.vue')
    },
    {
      path: '/join/:code?',
      name: 'join-session',
      component: () => import('@/views/JoinSessionView.vue'),
      props: true
    },
    {
      path: '/session/:code',
      name: 'session',
      component: () => import('@/views/SessionView.vue'),
      props: true
    }
  ]
})

export default router
