<script lang="ts">
  import { onMount } from "svelte";
  import type { DeviceData } from "../../types";

export let devices: DeviceData[] = [];

</script>

<div class="devices-container">
  <h2>Devices</h2>
  {#each devices as device}
    <div class="device">
      <h3>{device.name}</h3>
      <p>ID: {device.id}</p>
      <p>Status: {device.status}</p>
    </div>
  {/each}
</div>

<style>
  @import "./Devices.scss";
</style>
