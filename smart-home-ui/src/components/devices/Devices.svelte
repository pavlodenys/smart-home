<script lang="ts">
  import { onMount } from "svelte";
  import type { DeviceData } from "../../types";

export let devices: DeviceData[] = [];

</script>

<div class="flex">
 <h2>Devices</h2>
<div class="devices-container">
 
  {#each devices as device}
    <div class="device">
      <h3>{device.name}</h3>
      <p>ID: {device.id}</p>
      <p>Status: {device.isActive}</p>
    </div>
  {/each}
</div>
</div>


<style>
  @import "./Devices.scss";
</style>
