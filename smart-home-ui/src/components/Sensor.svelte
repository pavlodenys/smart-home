<script lang="ts">
  import {link} from 'svelte-spa-router';
  import type { SensorData } from "../types";
  import Chart from "./Chart.svelte";

  export let sensor: SensorData;
  const deleteSensor = () => {
    
  }
</script>

<style>
    @import "../styles/Sensor.scss";
</style>

<div class="sensor-card">
  <div class="sensor-title">{sensor.name}</div>
  <div class="sensor-info">{sensor.description}</div>
  <div class="sensor-info">Type: {sensor.type}</div>

  <a href='{'/sensor/'+sensor.id}' use:link>Details</a>
  <button on:click={deleteSensor}>Delete</button>
</div>

