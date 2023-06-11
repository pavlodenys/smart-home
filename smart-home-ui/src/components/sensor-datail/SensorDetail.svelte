<script lang="ts">
  import { onMount } from "svelte";
  import Chart from "../chart/Chart.svelte";
  import { httpFetch } from "../../api/httpServise";
  import type { SensorData } from "../../types";
  import moment from "moment";
  import Modal from "../modal/Modal.svelte";

  const getInitialChartData = (id) => {
    return {
      // id: "",
      name: `Sensor ${id}`,
      type: "",
      description: `Sensor ${id} data`,
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
  let isDataEdit = false;
  //let newChartData = getInitialChartData();
  let newChartData;
  // let newChartDataArray = [];
  let page = 0;
  let count = 50;
  let showModal = false;
  export let params = null;

  onMount(async () => {
    if (params?.id) {
      let date = moment().format("YYYY-MM-DD");
      sensor = await httpFetch.get(`api/sensor/${params.id}/${date}`);
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
    //showNewData = true;
    showModal = true;

    newChartData = getInitialChartData(sensor.chartData.length + 1);

    // newChartDataArray = [
    //   ...newChartDataArray,
    //   getInitialChartData(newChartDataArray.length + 1),
    // ];
  };

  const saveSensor = async () => {
    if (sensor.id) {
      const result = await httpFetch.put(`api/sensor/${sensor.id}`, sensor);
      console.log(result);

      sensor = result;
    } else {
      const result = await httpFetch.post(`api/sensor`, sensor);
      console.log(result);
      sensor = result;
    }
  };

  const cancelConnect = () => {
    //showNewData = false;

    newChartData = undefined;
  };

  const saveDataSource = () => {
    if (!sensor.chartData) {
      sensor.chartData = [];
    }

    sensor.chartData = [...sensor.chartData, newChartData];

    cancelConnect();
  };

  const closeModal = () => {
    showModal = false;
    cancelConnect();
  };

  const isValid = (sensorData) => {
    return !(sensorData.name === "" || sensorData.type === "");
  };

  const saveData = async () => {
    if (isValid(sensor)) {
      if (!sensor.chartData) {
        sensor.chartData = [];
      }

      sensor.chartData = [...sensor.chartData, newChartData];

      //todo: save data
      let result = await httpFetch.put(`api/sensor/${sensor.id}`, sensor);
      if (result) {
        closeModal();
      } else {
        showError();
      }
    }
    closeModal();
  };

  const editData = (i) => {
    showModal = true;

    newChartData = sensor.chartData[i];
    // newChartData = sensor.chartData[i];
  };

  function showError() {
    console.log("error");
  }

  const deleteData = async (index: number) => {
    let result = await httpFetch.delete(`api/sensor/${index}/data`);
    if (result) console.log(result);
  };
</script>

<div class="sensor-container">
  <div class="sensor-setup">
    <div class="sensor-title">
      <label for="name-input">Name:</label>
      <input type="text" id="name-input" bind:value={sensor.name} />
    </div>

    <div class="sensor-title">
      <label for="description-input">Description:</label>
      <input
        type="text"
        id="description-input"
        bind:value={sensor.description}
      />
    </div>

    <div class="sensor-title">
      <label for="type-input">Type:</label>
      <input type="text" id="type-input" bind:value={sensor.type} />
      <!-- todo: change it to dropdown -->
    </div>

    <div class="d-flex justify-between">
      <button on:click={saveSensor}>Save</button>
      {#if sensor?.id}
        <button on:click={connectToData}>+ Connect Data</button>
      {/if}
    </div>
  </div>

  <div class="d-flex">
    {#if sensor?.chartData}
      {#each sensor.chartData as data, index}
        <div class="m-l-1">
          <button
            class="edit-button"
            on:click={() => {
              editData(index);
            }}>Edit {index}</button
          >
          <button
            class="c-warn"
            on:click={() => {
              deleteData(data.id);
            }}>Delete {index}</button
          >
          <Chart chart={data} chartId={index} on:chartEvent={updateChartData} />
        </div>
      {/each}
    {:else}
      No data available
    {/if}
  </div>
</div>

<Modal
  show={showModal}
  {closeModal}
  ok={saveData}
  title={"Edit Sensor Data Source"}
  titleClass={"modal-title"}
>
  <div class="new-data">
    <!-- {#each newChartDataArray as newChartData} -->
    <form>
      <label class="modal-label">
        <span> Name:</span>
        <input class="w-100" type="text" bind:value={newChartData.name} />
      </label>

      <label class="modal-label">
        <span> Description:</span>
        <input
          class="w-100"
          type="text"
          bind:value={newChartData.description}
        />
      </label>

      <label class="modal-label">
        <span> Type:</span>
        <select bind:value={newChartData.type}>
          <option value="">-- Select Type --</option>
          <option value="Bar">Bar</option>
          <option value="Line">Line</option>
          <option value="Pie">Pie</option>
        </select>
      </label>

      <!-- <button type="submit" on:click={saveDataSource}>Submit</button>
      <button on:click={cancelConnect}>Cancel</button> -->
    </form>
    <!-- {/each} -->
  </div>
</Modal>

<style>
  .edit-button {
    position: relative;
     background-color: darkslateblue;
  }

  .sensor-container {
    display: flex;
    flex-direction: column;
  }

  .sensor-setup {
    display: flex;
    flex-direction: column;
    max-width: 40%;
    min-width: 20rem;
    margin-bottom: 1rem;
  }
  .new-data {
    display: flex;
  }
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

    padding: 1rem;
    margin: 1rem;
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

  /* form button[type="button"] {
    background-color: #f44336;
    border: none;
    color: white;
    padding: 0.5rem 1rem;
  } */

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
