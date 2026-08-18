<script lang="ts">
  import { createEventDispatcher } from "svelte";
  import { httpFetch } from "../../api/httpServise";
  import type { DeviceData } from "../../types";

export let devices: DeviceData[] = [];
export let manage = false;
export let showHeading = true;
const dispatch = createEventDispatcher();
let busyId: number | null = null;
let error = "";

const toggleDevice = async (device: DeviceData) => {
  busyId = device.id;
  error = "";
  const isActive = await httpFetch.patch(`api/device/${device.id}`);
  busyId = null;
  if (typeof isActive === "boolean") {
    devices = devices.map((item) => item.id === device.id ? { ...item, isActive } : item);
    dispatch("changed", { device: { ...device, isActive } });
  } else {
    error = "Could not update the device. Please try again.";
  }
};

const deleteDevice = async (device: DeviceData) => {
  if (!window.confirm(`Delete ${device.name}? This cannot be undone.`)) return;
  busyId = device.id;
  error = "";
  const result = await httpFetch.delete(`api/device/${device.id}`);
  busyId = null;
  if (typeof result === "number" && result > 0) {
    devices = devices.filter((item) => item.id !== device.id);
    dispatch("deleted", { id: device.id });
  } else {
    error = "Could not delete the device. Please try again.";
  }
};

</script>

<div class="flex">
 {#if showHeading}<h2>Devices</h2>{/if}
<div class="devices-container">
  {#if devices.length === 0}
    <div class="empty-state">
      <strong>No devices yet</strong>
      <span>Add your first switch, light, or relay to start controlling it.</span>
    </div>
  {/if}
  {#each devices as device (device.id)}
    <div class="device">
      <div class="device-heading">
        <div>
          <h3>{device.name}</h3>
          {#if device.description}<p>{device.description}</p>{/if}
        </div>
        <span class:online={device.isActive} class="status-dot">{device.isActive ? "On" : "Off"}</span>
      </div>
      {#if manage}
        <div class="device-actions">
          <button disabled={busyId === device.id} on:click={() => toggleDevice(device)}>
            Turn {device.isActive ? "off" : "on"}
          </button>
          <button class="danger secondary" disabled={busyId === device.id} on:click={() => deleteDevice(device)}>Delete</button>
        </div>
      {/if}
    </div>
  {/each}
</div>
{#if error}<p class="inline-error" role="alert">{error}</p>{/if}
</div>


<style>
  @import "./Devices.scss";
</style>
