import Vue from 'vue'
import Vuex from 'vuex'
import axios from 'axios'
const signalR = require('@aspnet/signalr')

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    connection: null,
    heartrate: 0,
    batteryPercentage: '100%',
    listeners: [],
    selected: '',
    device: null,
    loadingDevice: false,
    active: false,
    received: false,
    timer: null
  },
  mutations: {
    connection (state, value) {
      state.connection = value
    },
    heartrate (state, value) {
      state.heartrate = value
    },
    batteryPercentage (state, value) {
      state.batteryPercentage = value
    },
    listeners (state, value) {
      state.listeners = value
    },
    selected (state, value) {
      state.selected = value
    },
    device (state, value) {
      state.device = value
    },
    loadingDevice (state, value) {
      state.loadingDevice = value
    },
    timer (state, value) {
      state.timer = value
    },
    active (state, value) {
      state.active = value
    },
    received (state, value) {
      state.received = value
    }
  },
  getters: {
    heartrate (state) {
      return state.heartrate
    },
    batteryPercentage (state) {
      return state.batteryPercentage
    },
    listeners (state) {
      return state.listeners
    },
    selected (state) {
      return state.selected
    },
    device (state) {
      return state.device
    },
    loadingDevice (state) {
      return state.loadingDevice
    },
    active (state) {
      return state.active
    }
  },
  actions: {
    changeSelected (context, event) {
      context.commit('loadingDevice', true)
      context.commit('selected', event.target.value)
      return axios.get(`/api/Device/${encodeURIComponent(event.target.value)}`)
        .then(resp => {
          context.commit('device', resp.data)
          context.commit('loadingDevice', false)
        })
    },
    load (context) {
      const connection = new signalR.HubConnectionBuilder()
        .withUrl('/heartrateHub')
        .build()

      connection.on('battery-level', data => {
        context.commit('batteryPercentage', data)
      })

      connection.on('heartrate', data => {
        context.commit('heartrate', data)
        context.commit('received', true)
        context.commit('active', true)
      })

      connection.start()
      context.commit('connection', connection)

      context.commit('timer', setInterval(() => {
        if (!context.state.received) {
          context.commit('heartrate', 0)
          context.commit('active', false)
        }
        context.commit('received', false)
      }, 10000))

      return axios.get('/api/Device/Listeners')
        .then(resp => {
          context.commit('listeners', resp.data)
        })
    },
    loadListeners (context) {
      return axios.get('/api/Device/Listeners')
        .then(resp => {
          context.commit('listeners', resp.data)
        })
    }
  }
})
