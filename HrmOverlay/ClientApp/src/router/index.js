import Vue from 'vue'
import Router from 'vue-router'
import Home from '@/views/Home'
import Overlay from '@/views/Overlay'

Vue.use(Router)

const router = new Router({
  mode: 'history',
  base: './',
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/overlay',
      name: 'overlay',
      component: Overlay
    }
  ]
})

export default router
