<script lang="ts">
  import { onMount } from "svelte";
  import { push } from "svelte-spa-router";
  import Sensor from "../sensor/Sensor.svelte";
  import type { SensorData } from "../../types";
  //import { get } from 'svelte/fetch';

  export let sensors: SensorData[];

  const createNew = () => {
    push("/sensor");
  }
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

  {#if (!sensors || sensors.length === 0)}
    <p>No sensors found</p>
  {:else}
    <ul>
      {#each sensors as sensor}
        <li><Sensor {sensor} /></li>
      {/each}
    </ul>
  {/if}
  <button on:click={createNew}>+</button>
</div>

