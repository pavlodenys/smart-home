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
        <p>If </p>

        {#each scenario.sensors as sensor}
          <p class="device-title">{sensor.sensor.name}</p>
          <p> Value is</p>
          <p class="device-title">{">"}</p>
          <p> then </p>
          <p class="device-title"> {scenario.value} </p>
        {/each}
        <p> then </p>
        <p class="device-title">{device.device.description}</p>
        <p> is </p>
        <p class={device.device.isActive ? "btn-active" : "btn-inactive"}>
          {device.device.isActive ? " Active " : " Disabled "}
        </p>
        <!-- <button
          on:click={() => toggleDevice(device)}
          class={device.device.isActive ? "btn-active" : "btn-inactive"}
        >
          {device.device.isActive ? "Turn Off" : "Turn On"}
        </button> -->
      </div>
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
    flex-wrap: wrap;
  }

  .device-title {
    font-weight: bold;
    border: 1px solid black;
    background-color: rgba(151, 190, 150, 0.822);
    border-radius: 0.5rem;
    padding: 0.25rem;
    margin: 0.25rem
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
