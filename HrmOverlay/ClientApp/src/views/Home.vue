<template>
  <div>
    <select @change="changeSelected">
      <option disabled value="" :selected="selected === ''">Please select one</option>
      <option v-for="(device, index) in devices" :value="device.id" :key="index" :selected="selected === device.id">
        {{ device.name }}
      </option>
    </select>
    <button v-if="selected !== ''" @click="changeSelected({ target: { value: selected } })">
      <font-awesome-icon icon="redo-alt" />
    </button><br/>
    <div v-if="device !== null && !loadingDevice">
      <h1>Name: {{device.name}}</h1>
      <ul>
        <li v-for="service in device.services" :key="service.name">
          {{service.name}}
          <ul v-if="service.characteristics.length > 0">
            <li v-for="characteristic in service.characteristics" :key="characteristic">
              {{characteristic}}
              <button
                v-if="isListeningHeartRate(selected, service.name, characteristic) === false || isListening('HeartRate', selected, service.name, characteristic)"
                :style="{ backgroundColor: isListening('HeartRate', selected, service.name, characteristic) ? 'red' : 'green', color: 'white' }"
                @click="actionHeartRate(selected, service.name, characteristic)"
              >
                <font-awesome-icon icon="heartbeat" />
              </button>
              <button
                v-if="isListeningBatteryLevel(selected, service.name, characteristic) === false || isListening('BatteryLevel', selected, service.name, characteristic)"
                :style="{ backgroundColor: isListening('BatteryLevel', selected, service.name, characteristic) ? 'red' : 'green', color: 'white' }"
                @click="actionBatteryLevel(selected, service.name, characteristic)"
              >
                <font-awesome-icon icon="battery-full" />
              </button>
            </li>
          </ul>
        </li>
      </ul>
    </div>
    <p v-if="loadingDevice">Loading...</p>
  </div>
</template>

<script>
import axios from 'axios'
import { mapGetters, mapActions } from 'vuex'

export default {
  data () {
    return {
      devices: []
    }
  },
  computed: {
    ...mapGetters([
      'listeners',
      'selected',
      'device',
      'loadingDevice'
    ])
  },
  methods: {
    ...mapActions([
      'load',
      'loadListeners',
      'changeSelected'
    ]),
    actionHeartRate (id, service, characteristic) {
      if (this.isListening('HeartRate', id, service, characteristic)) {
        this.unlistenHeartRate(id, service, characteristic)
      } else {
        this.listenHeartRate(id, service, characteristic)
      }
    },
    listenHeartRate (id, service, characteristic) {
      axios.post(`/api/Device/HeartRate/${encodeURIComponent(id)}/${encodeURIComponent(service)}/${encodeURIComponent(characteristic)}`, '')
        .then(resp => this.loadListeners())
    },
    unlistenHeartRate (id, service, characteristic) {
      axios.delete(`/api/Device/HeartRate/${encodeURIComponent(id)}/${encodeURIComponent(service)}/${encodeURIComponent(characteristic)}`, '')
        .then(resp => this.loadListeners())
    },
    actionBatteryLevel (id, service, characteristic) {
      if (this.isListening('BatteryLevel', id, service, characteristic)) {
        this.unlistenBatteryLevel(id, service, characteristic)
      } else {
        this.listenBatteryLevel(id, service, characteristic)
      }
    },
    listenBatteryLevel (id, service, characteristic) {
      axios.post(`/api/Device/BatteryLevel/${encodeURIComponent(id)}/${encodeURIComponent(service)}/${encodeURIComponent(characteristic)}`, '')
        .then(resp => this.loadListeners())
    },
    unlistenBatteryLevel (id, service, characteristic) {
      axios.delete(`/api/Device/BatteryLevel/${encodeURIComponent(id)}/${encodeURIComponent(service)}/${encodeURIComponent(characteristic)}`, '')
        .then(resp => this.loadListeners())
    },
    isListening (type, id, service, characteristic) {
      return this.listeners.find(x => x.type === type && x.id === id && x.service === service && x.characteristic === characteristic)
    },
    isListeningHeartRate (id, service, characteristic) {
      const item = this.listeners.find(x => x.type === 'HeartRate')
      return item !== undefined
    },
    isListeningBatteryLevel (id, service, characteristic) {
      const item = this.listeners.find(x => x.type === 'BatteryLevel')
      return item !== undefined
    }
  },
  mounted () {
    axios.get('/api/Device')
      .then(resp => {
        resp.data.forEach(element => {
          this.devices.push(element)
          this.load()
        })
      })
  }
}
</script>

<style lang="scss">

body, select {
  font-size: 20px;
  font-weight: normal;
}
</style>
