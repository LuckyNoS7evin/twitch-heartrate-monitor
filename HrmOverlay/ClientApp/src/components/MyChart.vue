<script>
import { mapGetters } from 'vuex'
import { Line } from 'vue-chartjs'
import 'chartjs-plugin-streaming'

export default {
  extends: Line,
  computed: {
    ...mapGetters([
      'heartrate'
    ])
  },
  mounted () {
    this.renderChart({
      datasets: [
        {
          label: '',
          borderColor: 'rgb(54, 162, 235)',
          backgroundColor: 'rgba(54, 162, 235, 0.5)',
          pointHitRadius: 0
        }
      ]
    }, {
      legend: {
        display: false
      },
      scales: {
        yAxes: [{
          ticks: {
            min: 60,
            max: 100
          }
        }],
        xAxes: [{
          type: 'realtime',
          display: false,
          realtime: {
            onRefresh: (chart) => {
              chart.data.datasets.forEach((dataset) => {
                dataset.data.push({
                  x: Date.now(),
                  y: this.heartrate
                })
              })
            },
            delay: 1000
          }
        }]
      }
    })
  }
}
</script>

<style >
#line-chart {
  height: 100px;
  width: 400px;
}
</style>
