<script>
  let sensorData = [
    { name: "Temperature", value: "25°C" },
    { name: "Humidity", value: "50%" },
    { name: "Motion", value: "Detected" }
  ];

  let deviceData = [
    { name: "Light 1", status: "On" },
    { name: "Thermostat", status: "Cooling" },
    { name: "Security Camera", status: "Active" }
  ];
</script>

<style>
  .report-card {
    background-color: #f1f1f1;
    padding: 1rem;
    margin-bottom: 1rem;
    border-radius: 4px;
  }

  .report-card h3 {
    margin: 0;
    margin-bottom: 0.5rem;
  }

  .report-card p {
    margin: 0;
  }
</style>

<div>
  <h2>Reports</h2>

  <div class="report-card">
    <h3>Sensor Data</h3>
    {#each sensorData as sensor}
      <p>{sensor.name}: {sensor.value}</p>
    {/each}
  </div>

  <div class="report-card">
    <h3>Device Data</h3>
    {#each deviceData as device}
      <p>{device.name}: {device.status}</p>
    {/each}
  </div>
</div>
