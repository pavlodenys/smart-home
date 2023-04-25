<script lang="ts">
  export let scenario;
   function toggleDevice(device) {
    device.device = { ...device.device, isActive: !device.device.isActive };
scenario = scenario;
  }

</script>

<div>
  <h2>{scenario.command}</h2>
  <div class="scenario-item">
    {#each scenario.devices as device}
      <div class="device-item">
        <p>{device.device.description}</p>
        <button
          on:click={() => toggleDevice(device)}
          class={device.device.isActive ? "btn-active" : "btn-inactive"}
        >
          {device.device.isActive ? "Turn Off" : "Turn On"}
        </button>
      </div>
    {/each}
    {#each scenario.sensors as sensor}
      <p>{sensor.sensor.name}</p>
    {/each}
  </div>
</div>

<style>
  .scenario-item {
    text-align: center;
    padding: 1rem;
    display: flex;
    justify-content: space-between;
  }

  .device-item {
    display: flex;
    align-items: center;
  }

  button {
    margin-left: 0.5rem;
  }

  .btn-active {
    background-color: green;
    color: white;
  }

  .btn-inactive {
    background-color: red;
    color: white;
  }

  /* Add space between elements */
  h2 {
    margin-bottom: 1rem;
  }
</style>
