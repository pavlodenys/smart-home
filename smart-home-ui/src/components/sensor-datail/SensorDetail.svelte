<script lang="ts">
  import { onMount } from "svelte";
  import Chart from "../chart/Chart.svelte";
  import { httpFetch } from "../../api/httpServise";
  import { location } from "svelte-spa-router";
  import type { ChartData, SensorData } from "../../types";
  import moment from "moment";

  const getInitialChartData = () => {
    return {
      // id: "",
      name: "",
      type: "",
      description: "",
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
  let page = 0;
  let count = 50;
  export let params = null;

  onMount(async () => {
    if (params?.id) {
      let date = moment().format('YYYY-MM-DD');
      sensor = await httpFetch.get(`api/home/sensors/${params.id}/${date}`);
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

  const updateChartData = async (e) => {
    const result = await httpFetch.get(
      `api/home/sensors/${e.detail.dataId}/data/${e.detail.page}/${count}`
    );

    let chartData = sensor.chartData.find((x) => x.id === e.detail.dataId);

    chartData.data = result;
  };

  const connectToData = () => {
    showNewData = true;
    newChartData = getInitialChartData();
  };

  const saveSensor = async () => {
    if (sensor.id) {
      const result = await httpFetch.put(
        `api/home/sensor/${sensor.id}`,
        sensor
      );
      console.log(result);
    } else {
      const result = await httpFetch.post(`api/home/sensors`, sensor);
      console.log(result);
    }
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
  <textarea id="description-input" bind:value={sensor.description} />
</div>

<div class="sensor-info">
  <label for="type-input">Type:</label>
  <input type="text" id="type-input" bind:value={sensor.type} />
</div>

<button on:click={connectToData}>+ Connect Data</button>
<button on:click={saveSensor}>Save</button>

{#if sensor?.chartData}
  {#each sensor.chartData as data, index}
    <Chart chart={data} chartId={index} on:chartEvent={updateChartData} />
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
      <textarea bind:value={newChartData.description} />
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
    display: flex;
    align-items: center;
    margin-bottom: 1rem;
  }

  .sensor-title label {
    margin-right: 1rem;
    font-weight: bold;
  }

  .sensor-title input[type="text"] {
    flex: 1;
    padding: 0.5rem;
    border-radius: 0.5rem;
    border: 2px solid #ccc;
  }

  .sensor-info {
    margin-bottom: 1rem;
  }

  .sensor-info label {
    display: block;
    font-weight: bold;
    margin-bottom: 0.5rem;
  }

  .sensor-info textarea {
    width: 100%;
    padding: 0.5rem;
    border-radius: 0.5rem;
    border: 2px solid #ccc;
  }

  .sensor-info input[type="text"] {
    width: 100%;
    padding: 0.5rem;
    border-radius: 0.5rem;
    border: 2px solid #ccc;
  }

  button {
    background-color: #4caf50;
    border: none;
    color: white;
    padding: 0.5rem 1rem;
    border-radius: 0.5rem;
    cursor: pointer;
    transition: all 0.3s ease-in-out;
  }

  button:hover {
    background-color: #3e8e41;
  }

  form {
    display: flex;
    flex-direction: column;
    margin-bottom: 1rem;
  }

  form label {
    margin-bottom: 0.5rem;
  }

  form input[type="text"],
  form textarea,
  form select {
    padding: 0.5rem;
    border-radius: 0.5rem;
    border: 2px solid #ccc;
    margin-bottom: 1rem;
  }

  form button[type="submit"] {
    background-color: #4caf50;
    border: none;
    color: white;
    padding: 0.5rem 1rem;
    border-radius: 0.5rem;
    cursor: pointer;
    transition: all 0.3s ease-in-out;
  }

  form button[type="submit"]:hover {
    background-color: #3e8e41;
  }

  form button[type="button"] {
    background-color: #f44336;
    border: none;
    color: white;
    padding: 0.5rem 1rem;
  }

  /* .sensor-title {
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
  } */
</style>
