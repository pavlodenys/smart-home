<script lang="ts">
  import { onMount } from "svelte";
  import Sensor from "./components/Sensor.svelte";
  import type { SensorData } from "./types";
  //import { get } from 'svelte/fetch';

  export let sensors: SensorData[];
</script>

<!-- <h2>Sensors</h2>

<table>
  <thead>
    <tr>
      <th>ID</th>
      <th>Name</th>
      <th>Description</th>
    </tr>
  </thead>
  <tbody>
    {#each sensors as sensor}
      <tr>
        <td>{sensor.id}</td>
        <td>{sensor.name}</td>
        <td>{sensor.description}</td>
      </tr>
    {/each}
  </tbody>
</table> -->

<div>
  <h1>Sensors</h1>

  {#if sensors.length === 0}
    <p>No sensors found</p>
  {:else}
    <ul>
      {#each sensors as sensor}
        <li><Sensor {sensor} /></li>
      {/each}
    </ul>
  {/if}
</div>

<style>
  /* table {
    width: 100%;
    border-collapse: collapse;
  }

  th, td {
    border: 1px solid #dddddd;
    padding: 8px;
    text-align: left;
  }

  tr:nth-child(even) {
    background-color: #dddddd;
  } */
</style>
