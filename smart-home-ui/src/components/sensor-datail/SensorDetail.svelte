<script lang="ts">
  import { onMount } from "svelte";
  import Chart from "../Chart.svelte";
  import { httpFetch } from "../../api/httpServise";
  import { location } from "svelte-spa-router";
  import type { ChartData, SensorData } from "../../types";

  const getInitialChartData = () => {
    return {
     // id: "",
      name: "",
      type: "",
      description: ""
    };
  };

  let sensor: SensorData = {
    //id: "",
    name: "",
    type: "",
    description: "",
    chartData: [],
  };
  let showNewData = false;
  let newChartData = getInitialChartData();
  export let params = null;

  onMount(async () => {
    if (params?.id) {
      sensor = await httpFetch.get(`api/home/sensors/${params.id}`);
    } else {
      sensor = {
        //id: "",
        name: "",
        type: "",
        description: "",
        chartData: [],
      };
    }
  });

  const connectToData = () => {
    showNewData = true;
    newChartData = getInitialChartData();
  };

  const saveSensor = async () => {
    const result = await httpFetch.post(`api/home/sensors`, sensor);

    console.log(result);
  };

  const cancelConnect = () => {
    showNewData = false;
  };

  const saveDataSource = () => {
    if (!sensor.chartData) {
      sensor.chartData = [];
    }

    sensor.chartData = [...sensor.chartData, newChartData];

    cancelConnect();
  };
</script>

<div class="sensor-title">
  <label for="name-input">Name:</label>
  <input type="text" id="name-input" bind:value={sensor.name} />
</div>

<div class="sensor-info">
  <label for="description-input">Description:</label>
  <textarea id="description-input" bind:value={sensor.description} ></textarea>
</div>

<div class="sensor-info">
  <label for="type-input">Type:</label>
  <input type="text" id="type-input" bind:value={sensor.type} />
</div>

<button on:click={connectToData}>+ Connect Data</button>
<button on:click={saveSensor}>Save</button>

{#if sensor?.chartData}
  {#each sensor.chartData as data, index}
    <Chart chart={data} chartId={index} />
  {/each}
{:else}
  No data available
{/if}

{#if showNewData}
  <form>
    <label>
      Name:
      <input type="text" bind:value={newChartData.name} />
    </label>

     <label>
      Description:
      <textarea bind:value={newChartData.description} ></textarea>
    </label> 

    <label>
      Type:
      <select bind:value={newChartData.type}>
        <option value="">-- Select Type --</option>
        <option value="Bar">Bar</option>
        <option value="Line">Line</option>
        <option value="Pie">Pie</option>
      </select>
    </label>

    <button type="submit" on:click={saveDataSource}>Submit</button>
    <button on:click={cancelConnect}>Cancel</button>
  </form>
{/if}

<style>
  .sensor-title {
    font-size: 1.5rem;
    font-weight: bold;
  }

  .sensor-info {
    margin-top: 0.5rem;
    font-size: 1rem;
    color: gray;
  }

  input,
  textarea {
    padding: 0.5rem;
    font-size: 1rem;
    border: 1px solid gray;
    border-radius: 0.25rem;
  }
</style>
