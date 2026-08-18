<script lang="ts">
  import { createEventDispatcher } from "svelte";
  import { push } from "svelte-spa-router";
  import Sensor from "../sensor/Sensor.svelte";
  import type { SensorData } from "../../types";

  export let sensors: SensorData[];
  export let showHeading = true;
  const dispatch = createEventDispatcher();

  const createNew = () => {
    push("/sensor");
  }

  const sensorDeleted = (event) => {
    sensors = sensors.filter((sensor) => sensor.id !== event.detail.id);
    dispatch("deleted", event.detail);
  };
</script>

<div class="sensors-wrap">
  {#if showHeading}<h2>Sensors</h2>{/if}

  {#if (!sensors || sensors.length === 0)}
    <p>No sensors found</p>
  {:else}
    <ul>
      {#each sensors as sensor (sensor.id)}
        <li><Sensor {sensor} on:deleted={sensorDeleted} /></li>
      {/each}
    </ul>
  {/if}
  <button class="add-button" on:click={createNew}>+ Add sensor</button>
</div>

<style>
    @import "./Sensors.scss";
</style>

