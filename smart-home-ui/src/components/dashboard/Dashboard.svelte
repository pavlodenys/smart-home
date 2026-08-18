<script lang="ts">
  import Sensors from "../sensors/Sensors.svelte";
  import Devices from "../devices/Devices.svelte";
  import Scenario from "../scenario/Scenario.svelte";
  import { onMount } from "svelte";
  import type { DeviceData, SensorData } from "../../types";
  import { httpFetch } from "../../api/httpServise";

  let sensors: SensorData[] = [];
  let devices: DeviceData[] = [];

  onMount(async () => {
    const response = await httpFetch.get('api/sensor')
    sensors = response;

    const responseDevices = await httpFetch.get('api/device');
    devices = responseDevices;
  });

  const removeSensor = (event) => {
    sensors = sensors.filter((sensor) => sensor.id !== event.detail.id);
  };
</script>

<header class="page-header">
  <div>
    <p class="eyebrow">Smart home overview</p>
    <h1>Dashboard</h1>
    <p class="page-summary">Monitor sensors, control devices, and build automations from one place.</p>
  </div>
</header>

<div class="dashboard-grid">
  <section class="panel scenario-panel" aria-labelledby="automations-title">
    <h2 id="automations-title">Automations</h2>
    <Scenario {sensors} {devices} />
  </section>
  <section class="panel" aria-labelledby="sensors-title">
    <div class="section-heading">
      <h2 id="sensors-title">Sensors</h2>
      <a href="/#/dashboard/sensors">View all</a>
    </div>
    <Sensors {sensors} showHeading={false} on:deleted={removeSensor} />
  </section>
  <section class="panel" aria-labelledby="devices-title">
    <div class="section-heading">
      <h2 id="devices-title">Devices</h2>
      <a href="/#/dashboard/devices">Manage</a>
    </div>
    <Devices {devices} showHeading={false} />
  </section>
</div>
