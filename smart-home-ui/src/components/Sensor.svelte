<script lang="ts">
  import type { SensorData } from "../types";
  import Chart from "./Chart.svelte";

  export let sensor: SensorData;
</script>

<style>
    @import "../styles/Sensor.scss";
</style>

<div class="sensor-card">
  <div class="sensor-title">{sensor.name}</div>
  <div class="sensor-info">{sensor.description}</div>
  <div class="sensor-info">Type: {sensor.type}</div>
  {#if sensor.chartData}
   <Chart chart={sensor.chartData} />
  {:else}
    No data available
  {/if}
</div>

