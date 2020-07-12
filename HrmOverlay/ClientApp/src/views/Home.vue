<template>
  <div>
    <select v-model="selected">
      <option disabled value="">Please select one</option>
      <option v-for="(device, index) in devices" :value="device.id" :key="index">
        {{ device.name }}
      </option>
    </select>
    <span>Selected: {{ selected }}</span>
    <span>Device: {{ device }}</span>

  </div>
</template>

<script>
import axios from 'axios'

export default {
  data () {
    return {
      devices: [],
      selected: '',
      device: null
    }
  },
  watch: {
    selected (newVal) {
      console.log(newVal)
      axios.get(`/api/Device/${encodeURIComponent(newVal)}`)
        .then(resp => {
          this.device = resp.data
        })
    }
  },
  mounted () {
    axios.get('/api/Device')
      .then(resp => {
        resp.data.forEach(element => {
          this.devices.push(element)
        })
      })
  }
}
</script>

<style lang="scss">

</style>
