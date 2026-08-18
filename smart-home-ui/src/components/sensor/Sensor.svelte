<script lang="ts">
  import { createEventDispatcher } from "svelte";
  import {link} from 'svelte-spa-router';
  import type { SensorData } from "../../types";
  import { httpFetch } from '../../api/httpServise';

  export let sensor: SensorData;
  const dispatch = createEventDispatcher();
  let deleting = false;
  let error = "";

  const deleteSensor = async (id) => {
    if (!window.confirm(`Delete ${sensor.name || "this sensor"}? This cannot be undone.`)) {
      return;
    }

    deleting = true;
    error = "";
    const result = await httpFetch.delete(`api/sensor/${id}`);
    deleting = false;

    if (typeof result === "number" && result > 0) {
      dispatch("deleted", { id });
      return;
    }

    error = "Could not delete this sensor. Please try again.";
  }
</script>

<style>
    @import "./Sensor.scss";
</style>

<div class="sensor-card">
  <div class="sensor-title">{sensor.name}</div>
  <div class="sensor-info">{sensor.description}</div>
  <div class="sensor-info">Type: {sensor.type}</div>

  <div class="sensor-actions">
    <a class="button-link" href='{'/sensor/'+sensor.id}' use:link>Details</a>
    <button class="danger" disabled={deleting} on:click={() => deleteSensor(sensor.id)}>
      {deleting ? "Deleting…" : "Delete"}
    </button>
  </div>
  {#if error}<p class="inline-error" role="alert">{error}</p>{/if}
</div>

