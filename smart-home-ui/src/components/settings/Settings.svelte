<script lang="ts">
  import { onMount } from "svelte";
  import { httpFetch } from "../../api/httpServise";

  let apiOnline = false;
  let checking = true;

  onMount(async () => {
    const response = await httpFetch.get("api/sensor");
    apiOnline = Array.isArray(response);
    checking = false;
  });
</script>

<header class="page-header">
  <div>
    <p class="eyebrow">System</p>
    <h1>Settings</h1>
    <p class="page-summary">See the local stack status and where device data is being sent.</p>
  </div>
</header>

<section class="panel settings-list">
  <div class="setting-row">
    <div>
      <h2>Smart Home API</h2>
      <p>The browser uses this service for sensors, devices, and automations.</p>
    </div>
    <span class:online={apiOnline} class="status-dot">{checking ? "Checking…" : apiOnline ? "Online" : "Offline"}</span>
  </div>
  <div class="setting-row">
    <div>
      <h2>Local UI</h2>
      <p>Available from this computer at <code>http://localhost:5173</code>.</p>
    </div>
    <span class="status-dot online">Online</span>
  </div>
</section>
