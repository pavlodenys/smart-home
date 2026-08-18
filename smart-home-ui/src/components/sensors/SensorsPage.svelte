<script lang="ts">
  import { onMount } from "svelte";
  import { httpFetch } from "../../api/httpServise";
  import type { SensorData } from "../../types";
  import Sensors from "./Sensors.svelte";

  let sensors: SensorData[] = [];
  let loading = true;
  let error = "";

  onMount(async () => {
    const response = await httpFetch.get("api/sensor");
    loading = false;
    if (Array.isArray(response)) sensors = response;
    else error = "Could not load sensors. Check that the API is running.";
  });

  const removeSensor = (event) => {
    sensors = sensors.filter((sensor) => sensor.id !== event.detail.id);
  };
</script>

<header class="page-header">
  <div>
    <p class="eyebrow">Live measurements</p>
    <h1>Sensors</h1>
    <p class="page-summary">Review every sensor, open its history, or connect a new data source.</p>
  </div>
</header>

<section class="panel page-panel">
  {#if loading}
    <p class="muted">Loading sensors…</p>
  {:else if error}
    <p class="inline-error" role="alert">{error}</p>
  {:else}
    <Sensors {sensors} showHeading={false} on:deleted={removeSensor} />
  {/if}
</section>
