<script lang="ts">
  import Sensors from "../sensors/Sensors.svelte";
  import Devices from "../devices/Devices.svelte";
  import Scenario from "../scenario/Scenario.svelte";
  import { onMount } from "svelte";
  import type { DeviceData, SensorData } from "../../types";
  import TopMenu from "../top-menu/TopMenu.svelte";
  import { httpFetch } from "../../api/httpServise";
  export let message: string;

  let sensors: SensorData[] = [];
  let devices: DeviceData[] = [];

  onMount(async () => {
    const response = await httpFetch.get('api/home/sensors')
    sensors = response;

    const responseDevices = await httpFetch.get('api/home/device');
    devices = responseDevices;
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
  @import "../../app.css";
</style>
