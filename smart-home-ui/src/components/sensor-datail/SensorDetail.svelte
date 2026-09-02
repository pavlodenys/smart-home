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
  let isEditingSensor = !params?.id;
  let sensorBeforeEdit: SensorData | null = null;

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
        if (!window.confirm(`Delete MQTT data source ${index} and its readings? This cannot be undone. Update and reflash any device publishing with Id=${index} before it publishes again.`)) return;
  const updateChartData = async (e) => {
    const result = await httpFetch.get(
      `api/sensor/${e.detail.dataId}/data/${e.detail.page}/${count}`
    );

    if (!Array.isArray(result) || !result.length) {
      return;
    }

    // reassign sensor (rather than mutating in place) so Svelte propagates the new points to Chart
    sensor = {
      ...sensor,
      chartData: sensor.chartData.map((chartData) => {
        if (chartData.id !== e.detail.dataId) {
          return chartData;
        }

        const existingIds = new Set((chartData.data ?? []).map((point) => point.id));
        const newPoints = result.filter((point) => !existingIds.has(point.id));

        return { ...chartData, data: [...(chartData.data ?? []), ...newPoints] };
      }),
    };
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

      if (result) {
        sensor = result;
        sensorBeforeEdit = null;
        isEditingSensor = false;
      }
    } else {
      const result = await httpFetch.post(`api/sensor`, sensor);
      console.log(result);
      if (result) {
        sensor = result;
        sensorBeforeEdit = null;
        isEditingSensor = false;
      }
    }
  };

  const editSensor = () => {
    sensorBeforeEdit = { ...sensor };
    isEditingSensor = true;
  };

  const cancelSensorEdit = () => {
    if (sensorBeforeEdit) {
      sensor = { ...sensorBeforeEdit };
    }

    sensorBeforeEdit = null;
    isEditingSensor = false;
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
    if (!window.confirm("Delete this data source and its readings? This cannot be undone.")) return;
    const result = await httpFetch.delete(`api/sensor/${index}/data`);
    if (typeof result === "number" && result > 0) {
      sensor = { ...sensor, chartData: sensor.chartData.filter((data) => data.id !== index) };
    }
  };
</script>

<div class="sensor-page">
  <header class="page-header sensor-page-header">
    <div>
      <a class="back-link" href="/#/dashboard/sensors" aria-label="Back to sensors">
        <span aria-hidden="true">&#8592;</span> Sensors
      </a>
      <p class="eyebrow">Sensor details</p>
      <h1>{sensor.name || "New sensor"}</h1>
      <p class="page-summary">
        {sensor.description || "Add a name, description, and type for this sensor."}
      </p>
    </div>
    <div class="page-actions">
      {#if sensor?.id && !isEditingSensor}
        <button type="button" on:click={editSensor}>Edit sensor</button>
      {/if}
      {#if sensor?.id}
        <button type="button" class="primary" on:click={connectToData}>Connect data</button>
      {/if}
    </div>
  </header>

  <div class:editing={isEditingSensor || !sensor?.id} class="sensor-detail-layout">
    {#if isEditingSensor || !sensor?.id}
      <aside class="panel sensor-setup">
        <div class="section-heading settings-heading">
          <div>
            <h2>Configuration</h2>
            <p>Identification used across your smart home.</p>
          </div>
        </div>

        <form on:submit|preventDefault={saveSensor}>
          <label for="name-input">
            <span>Name</span>
            <input type="text" id="name-input" bind:value={sensor.name} />
          </label>

          <label for="description-input">
            <span>Description</span>
            <textarea
              id="description-input"
              rows="3"
              bind:value={sensor.description}
            />
          </label>

          <label for="type-input">
            <span>Type</span>
            <input type="text" id="type-input" bind:value={sensor.type} />
          </label>

          <div class="sensor-form-actions">
            <button class="primary" type="submit">Save changes</button>
            {#if sensor?.id}
              <button type="button" on:click={cancelSensorEdit}>Cancel</button>
            {/if}
          </div>
        </form>
      </aside>
    {/if}

    <section class="sensor-readings" aria-labelledby="readings-heading">
      <div class="readings-heading">
        <div>
          <p class="eyebrow">Live history</p>
          <h2 id="readings-heading">Connected data</h2>
        </div>
        <span class="count-badge" aria-label={`${sensor.chartData?.length ?? 0} data sources`}>
          {sensor.chartData?.length ?? 0}
        </span>
      </div>

      {#if sensor?.chartData?.length}
        <div class="data-source-list">
          {#each sensor.chartData as data, index}
            <article class="panel data-source-card">
              <p class="mqtt-data-id">MQTT data ID: {data.id}</p>
              <div class="data-source-actions" aria-label={`Actions for ${data.name || `data source ${index + 1}`}`}>
                <button
                  type="button"
                  on:click={() => {
                    editData(index);
                  }}>Edit source</button
                >
                <button
                  type="button"
                  class="danger secondary"
                  on:click={() => {
                    deleteData(data.id);
                  }}>Delete</button
                >
              </div>
              <Chart
                chart={data}
                chartId={index}
                sensorId={sensor.id}
                on:chartEvent={updateChartData}
              />
            </article>
          {/each}
        </div>
      {:else}
        <div class="panel empty-state readings-empty-state">
          <strong>No data sources connected</strong>
          <span>Connect a source to start collecting readings for this sensor.</span>
          {#if sensor?.id}
            <button type="button" class="primary" on:click={connectToData}>Connect data</button>
          {/if}
        </div>
      {/if}
    </section>
  </div>
</div>

<Modal
  show={showModal}
  {closeModal}
  ok={saveData}
  title={newChartData?.id ? "Edit data source" : "Connect data source"}
  titleClass={"modal-title"}
  description="Choose how this sensor's readings are identified and presented."
  okLabel={newChartData?.id ? "Save changes" : "Connect source"}
  cancelLabel="Cancel"
>
  <div class="new-data">
    <!-- {#each newChartDataArray as newChartData} -->
    <form>
      <label class="modal-label">
        <span>Name</span>
        <input class="w-100" type="text" bind:value={newChartData.name} />
      </label>

      <label class="modal-label">
        <span>Description</span>
        <input
          class="w-100"
          type="text"
          bind:value={newChartData.description}
        />
      </label>

      <label class="modal-label">
        <span>Chart type</span>
        <select bind:value={newChartData.type}>
          <option value="">Select a chart type</option>
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
  .sensor-page-header {
    align-items: flex-end;
  }

  .back-link {
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
    margin-bottom: 1.5rem;
    font-size: 0.88rem;
  }

  .page-actions {
    display: flex;
    flex-wrap: wrap;
    justify-content: flex-end;
    gap: 0.65rem;
  }

  .sensor-detail-layout {
    display: grid;
    grid-template-columns: minmax(0, 1fr);
    gap: 1.5rem;
    align-items: start;
  }

  .sensor-detail-layout.editing {
    grid-template-columns: minmax(280px, 0.72fr) minmax(0, 1.55fr);
  }

  .sensor-setup {
    position: sticky;
    top: 100px;
  }

  .settings-heading p {
    margin: 0.3rem 0 0;
    color: var(--muted);
    font-size: 0.9rem;
    line-height: 1.5;
  }

  .sensor-form-actions,
  .data-source-actions {
    display: flex;
    flex-wrap: wrap;
    gap: 0.65rem;
  }

  .sensor-form-actions {
    padding-top: 0.35rem;
  }

  .sensor-form-actions button:first-child {
    flex: 1;
  }

  .readings-heading {
    display: flex;
    align-items: center;
    justify-content: space-between;
    min-height: 44px;
    margin-bottom: 1rem;
  }

  .readings-heading h2,
  .readings-heading .eyebrow {
    margin-bottom: 0;
  }

  .readings-heading .eyebrow {
    margin-bottom: 0.25rem;
  }

  .data-source-list {
    display: grid;
    gap: 1rem;
  }

  .data-source-card {
    position: relative;
    min-width: 0;
    overflow: hidden;
  }

  .data-source-actions {
    position: absolute;
    z-index: 2;
    top: 1.25rem;
    right: 1.25rem;
  }

  .data-source-actions button {
    min-height: 38px;
    padding: 0.45rem 0.7rem;
    font-size: 0.82rem;
  }

  .mqtt-data-id {
    margin: 0 0 0.75rem;
    color: var(--muted);
    font-size: 0.82rem;
    font-weight: 700;
  }

  .readings-empty-state {
    min-height: 240px;
    place-content: center;
  }

  .readings-empty-state button {
    justify-self: center;
    margin-top: 0.75rem;
  }

  .new-data {
    width: 100%;
  }

  .new-data form {
    width: 100%;
    gap: 1rem;
  }

  @media (max-width: 900px) {
    .sensor-detail-layout.editing {
      grid-template-columns: 1fr;
    }

    .sensor-setup {
      position: static;
    }
  }

  @media (max-width: 600px) {
    .sensor-page-header {
      align-items: flex-start;
    }

    .page-actions {
      width: 100%;
      justify-content: flex-start;
    }

    .page-actions button {
      flex: 1;
    }

    .data-source-card {
      padding-top: 4.75rem;
    }

    .data-source-actions {
      top: 1rem;
      right: 1rem;
      left: 1rem;
    }
  }
</style>
