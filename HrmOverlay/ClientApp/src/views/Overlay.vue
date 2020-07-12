<template>
  <div id="app">
    <div class="battery">
      <div class="batteryPercentage" v-text="batteryPercentage"></div>
      <div class="batteryContainer">
        <div class="batteryOuter"><div id="batteryLevel" :width="batteryPercentage"></div></div>
        <div class="batteryBump"></div>
      </div>
    </div>
    <div class="heart"><img src="Hearts-Vortex-Heart.png" ></div>
    <div class="heartrate" v-text="heartrate"></div>
    <MyChart :width="400" :height="100" />
  </div>
</template>

<script>
import { mapGetters, mapActions } from 'vuex'
import MyChart from '@/components/MyChart'

export default {
  components: {
    MyChart
  },
  computed: {
    ...mapGetters([
      'batteryPercentage',
      'heartrate'
    ])
  },
  methods: {
    ...mapActions([
      'load'
    ])
  },
  mounted () {
    this.load()
  }
}
</script>

<style>

@import url('https://fonts.googleapis.com/css2?family=Lexend+Tera&display=swap');

body, html, #app {
  background-color: transparent;
  font-family: 'Lexend Tera', sans-serif;
  padding:0;
  margin:0;
}

#app {
  display: flex;
}

.battery {
  color: #73AD21;
  display: -webkit-box;
  display: -moz-box;
  display: -ms-flexbox;
  display: -webkit-flex;
  display: flex;
  flex-direction: column;
  width: 60px;
  align-items: center;
}

.batteryContainer {
  display: -webkit-box;
  display: -moz-box;
  display: -ms-flexbox;
  display: -webkit-flex;
  display: flex;
  flex-direction: row;
  align-items: center;
}

.batteryOuter {
  border-radius: 3px;
  border: 1px solid #444;
  padding: 1px;
  width: 25px;
  height: 9px;
}

.batteryBump {
  border-radius: 2px;
  background-color: #444;
  margin: 2px;
  width: 1px;
  height: 3px;
}

#batteryLevel {
  border-radius: 2px;
  background-color: #73AD21;
  height: 9px;
}

.heart {
  height: 100px;
  width: 100px;
  margin: 0 10px;
}

.heartrate {
  width: 115px;
  color: #73AD21;
  font-size: 75px;
  font-weight: 800;
}

img {
  width: 100%;
  display: block;
  -webkit-animation-duration: 1s;
  -webkit-animation-name: heartbeat;
  -webkit-animation-iteration-count: infinite;
  -webkit-animation-direction: alternate;
  animation-duration: 0.5s;
  animation-name: heartbeat;
  animation-iteration-count: infinite;
  animation-direction: alternate;
}

@-moz-keyframes heartbeat {
  0% {
    -webkit-transform: scale(1);
    transform: scale(1);
  }
  100% {
    -webkit-transform: scale(0.6);
    transform: scale(0.6);
  }
}

@keyframes heartbeat {
  0% {
    -webkit-transform: scale(1);
    transform: scale(1);
  }
  100% {
    -webkit-transform: scale(0.6);
    transform: scale(0.6);
  }
}
</style>
