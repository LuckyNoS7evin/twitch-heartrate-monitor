import Vue from 'vue'
import Vuex from 'vuex'
const signalR = require('@aspnet/signalr')

Vue.use(Vuex)

export default new Vuex.Store({
  state: {
    connection: null,
    heartrate: 0,
    batteryPercentage: '100%'
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
    }
  },
  getters: {
    heartrate (state) {
      return state.heartrate
    },
    batteryPercentage (state) {
      return state.batteryPercentage
    }
  },
  actions: {
    load (context) {
      const connection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5000/heartrateHub')
        .build()

      connection.on('battey-level', data => {
        context.commit('batteryPercentage', data)
      })

      connection.on('heartrate', data => {
        context.commit('heartrate', data)
      })

      connection.start()
      context.commit('connection', connection)
    }
  }
})
