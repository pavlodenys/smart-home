<script lang="ts">
  import { onMount } from "svelte";
  import { httpFetch } from "../../api/httpServise";
  import type { DeviceData } from "../../types";
  import Devices from "./Devices.svelte";

  let devices: DeviceData[] = [];
  let name = "";
  let description = "";
  let isActive = false;
  let saving = false;
  let loading = true;
  let error = "";
  let success = "";

  onMount(loadDevices);

  async function loadDevices() {
    loading = true;
    error = "";
    const response = await httpFetch.get("api/device");
    loading = false;
    if (Array.isArray(response)) devices = response;
    else error = "Could not load devices. Check that the API is running.";
  }

  async function addDevice() {
    error = "";
    success = "";
    if (!name.trim()) {
      error = "Enter a device name.";
      return;
    }

    saving = true;
    const response = await httpFetch.post("api/device", {
      id: 0,
      name: name.trim(),
      description: description.trim(),
      isActive,
    });
    saving = false;

    if (response && typeof response.id === "number") {
      devices = [...devices, response];
      success = `${response.name} was added.`;
      name = "";
      description = "";
      isActive = false;
    } else {
      error = "Could not add the device. Please try again.";
    }
  }

  const removeDevice = (event) => {
    devices = devices.filter((device) => device.id !== event.detail.id);
    success = "Device deleted.";
  };
</script>

<header class="page-header split-header">
  <div>
    <p class="eyebrow">Control center</p>
    <h1>Devices</h1>
    <p class="page-summary">Add switches, lights, relays, and other controllable equipment.</p>
  </div>
</header>

<div class="management-layout">
  <section class="panel form-panel" aria-labelledby="add-device-title">
    <h2 id="add-device-title">Add a device</h2>
    <form on:submit|preventDefault={addDevice}>
      <label>
        <span>Name</span>
        <input bind:value={name} autocomplete="off" placeholder="Greenhouse pump" />
      </label>
      <label>
        <span>Description</span>
        <textarea bind:value={description} rows="3" placeholder="Waters the raised bed"></textarea>
      </label>
      <label class="checkbox-row">
        <input type="checkbox" bind:checked={isActive} />
        <span>Start in the on state</span>
      </label>
      <button class="primary" type="submit" disabled={saving}>{saving ? "Adding…" : "Add device"}</button>
      {#if error}<p class="inline-error" role="alert">{error}</p>{/if}
      {#if success}<p class="inline-success" role="status">{success}</p>{/if}
    </form>
  </section>

  <section class="panel" aria-labelledby="device-list-title">
    <div class="section-heading">
      <h2 id="device-list-title">Your devices</h2>
      <span class="count-badge">{devices.length}</span>
    </div>
    {#if loading}
      <p class="muted">Loading devices…</p>
    {:else}
      <Devices {devices} manage={true} showHeading={false} on:deleted={removeDevice} />
    {/if}
  </section>
</div>
