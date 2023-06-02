<script lang="ts">
  import { onMount } from "svelte";
  import Sensors from "../sensors/Sensors.svelte";
  import {
    ComparisonOperator,
    type DeviceData,
    type SensorData,
  } from "../../types";
  import Modal from "../modal/Modal.svelte";
  import ScenarioItem from './scenario-item/ScenarioItem.svelte';
  import { httpFetch } from "../../api/httpServise";
  let showModal = false;
  let selectedSensor;
  let selectedDevice;
  let inputValue;
  let commandValue;
  let selectedOperator;
  let scenarios = [];

  let operators = ComparisonOperator;

  export let sensors: SensorData[];
  export let devices: DeviceData[];

  const openModal = () => {
    showModal = true;
  };

  const closeModal = () => {
    showModal = false;
  };

  const saveScenario = async () => {
    const response = await httpFetch.post('api/scenario', {
        sensors: [{sensorId:selectedSensor}],
        devices: [{deviceId:selectedDevice}],
        value: inputValue,
        operator: selectedOperator,
        command: commandValue
      });
    sensors = response;
  };

  onMount(async () => {
     scenarios = await httpFetch.get(`api/scenario`);

    console.log(scenarios);

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        closeModal();
      }
    });
  });
</script>

<div class="sensor-card">
  Create scenario?
  <button on:click={openModal}> Yes! </button>

<ul>
  {#each scenarios as scenario}
    <ScenarioItem scenario={scenario} />
  {/each}
</ul>

</div>

<Modal show={showModal} {closeModal} ok={saveScenario}>
  <div class="sensor-device-container">
    <div class="title">Choose Sensor:</div>
    <select id="sensor-select" class="select" bind:value={selectedSensor}>
      {#each sensors as sensor}
        <option value={sensor.id}>{sensor.name}</option>
      {/each}
    </select>

    <div class="title">Choose Device:</div>
    <select id="device-select" class="select" bind:value={selectedDevice}>
      {#each devices as device}
        <option value={device.id}>{device.name}</option>
      {/each}
    </select>

    <div class="title">Choose Operator:</div>
    <select id="device-select" class="select" bind:value={selectedOperator}>
      {#each Object.entries(operators) as operator}
        <option value={operator[1]}>{operator[0]}</option>
      {/each}
    </select>

    
    <div class="title">Add Command:</div>
    <input
      type="text"
      id="value-input"
      placeholder="Enter a value"
      class="input"
      bind:value={commandValue}
    />

    <div class="title">Add Value:</div>
    <input
      type="text"
      id="value-input"
      placeholder="Enter a value"
      class="input"
      bind:value={inputValue}
    />

    <button id="submit-btn" class="button" on:click={saveScenario}
      >Launch Scenario</button
    >
  </div>
</Modal>

<style>
  @import "./Scenario.scss";
</style>
