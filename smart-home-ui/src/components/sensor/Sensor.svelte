<script lang="ts">
  import {link} from 'svelte-spa-router';
  import type { SensorData } from "../../types";
  import Chart from "../chart/Chart.svelte";
  import { httpFetch } from '../../api/httpServise';

  export let sensor: SensorData;
  const deleteSensor = async (id) => {
    let result = await httpFetch.delete(`api/home/sensors/${id}`);
    console.log(result);
  }
</script>

<style>
    @import "./Sensor.scss";
</style>

<div class="sensor-card">
  <div class="sensor-title">{sensor.name}</div>
  <div class="sensor-info">{sensor.description}</div>
  <div class="sensor-info">Type: {sensor.type}</div>

  <a href='{'/sensor/'+sensor.id}' use:link>Details</a>
  <button on:click={() => deleteSensor(sensor.id)}>Delete</button>
</div>

