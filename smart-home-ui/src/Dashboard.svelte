<script lang="ts">
  import Sensors from "./Sensors.svelte";
  import Devices from "./components/devices/Devices.svelte";
  import Scenario from "./components/scenario/Scenario.svelte";
  import { onMount } from "svelte";
  import type { DeviceData, SensorData } from "./types";
  import TopMenu from "./components/top-menu/TopMenu.svelte";
  export let message: string;

  let sensors: SensorData[] = [];
  let devices: DeviceData[] = [];

  onMount(async () => {
    const response = await fetch("https://localhost:7138/api/home/sensors");
    sensors = await response.json();

    const responseDevices = await fetch(
      "https://localhost:7138/api/home/device"
    );
    devices = await responseDevices.json();
  });
</script>

<h1>Dashboard</h1>
<p>{message}</p>

<div class="dahsboard">
  <Scenario {sensors} {devices} />
  <Sensors {sensors} />
  <Devices {devices} />
</div>

<style>
  @import "./app.css";
</style>
