<script lang="ts">
  import { onMount } from "svelte";
  import { push } from "svelte-spa-router";
  import Sensor from "../sensor/Sensor.svelte";
  import type { SensorData } from "../../types";

  export let sensors: SensorData[];

  const createNew = () => {
    push("/sensor");
  }
</script>

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

