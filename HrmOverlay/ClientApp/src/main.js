import Vue from 'vue'
import App from '@/App.vue'
import store from '@/store'
import router from '@/router'

import { library } from '@fortawesome/fontawesome-svg-core'
import { faHeartbeat, faBatteryFull, faRedoAlt } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

library.add(faHeartbeat)
library.add(faBatteryFull)
library.add(faRedoAlt)

Vue.component('font-awesome-icon', FontAwesomeIcon)

Vue.config.productionTip = false

new Vue({
  store,
  router,
  render: h => h(App)
}).$mount('#app')
