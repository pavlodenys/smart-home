<script lang="ts">
  import { onMount } from "svelte";
  import {
    ComparisonOperator,
    ScenarioActionType,
    type DeviceData,
    type ScenarioData,
    type SensorData,
  } from "../../types";
  import Modal from "../modal/Modal.svelte";
  import ScenarioItem from "./scenario-item/ScenarioItem.svelte";
  import { httpFetch } from "../../api/httpServise";
  import { buildScenarioPayload } from "./scenarioPayload";

  export let sensors: SensorData[];
  export let devices: DeviceData[];

  let showModal = false;
  let selectedSensor: number | undefined;
  let selectedDevice: number | undefined;
  let threshold = 30;
  let hysteresis = 2;
  let command = "Plant moisture is {value}%, below {threshold}%";
  let selectedOperator = ComparisonOperator.LessThan;
  let actionType: number = ScenarioActionType.Notification;
  let scenarios: ScenarioData[] = [];
  let errorMessage = "";
  let isSaving = false;

  const operators = ComparisonOperator;
  const actionTypes = ScenarioActionType;

  const loadScenarios = async () => {
    const response = await httpFetch.get("api/scenario");
    if (typeof response === "string") {
      throw new Error(response);
    }

    scenarios = response ?? [];
  };

  const openModal = () => {
    selectedSensor ??= sensors[0]?.id;
    selectedDevice ??= devices[0]?.id;
    errorMessage = "";
    showModal = true;
  };

  const closeModal = () => {
    showModal = false;
    errorMessage = "";
  };

  const saveScenario = async () => {
    if (isSaving) {
      return;
    }

    errorMessage = "";

    if (!selectedSensor) {
      errorMessage = "Choose a sensor.";
      return;
    }

    if (!Number.isFinite(threshold) || !Number.isFinite(hysteresis) || hysteresis < 0) {
      errorMessage = "Enter a valid threshold and a non-negative recovery margin.";
      return;
    }

    if (actionType === ScenarioActionType.Notification && !command.trim()) {
      errorMessage = "Enter a notification message.";
      return;
    }

    if (actionType === ScenarioActionType.Device && !selectedDevice) {
      errorMessage = "Choose a device.";
      return;
    }

    isSaving = true;
    try {
      const response = await httpFetch.post(
        "api/scenario",
        buildScenarioPayload({
          selectedSensor,
          selectedDevice,
          threshold,
          hysteresis,
          operator: selectedOperator,
          actionType,
          command,
        }),
      );

      if (typeof response === "string") {
        throw new Error(response);
      }

      await loadScenarios();
      closeModal();
    } catch (error) {
      errorMessage = error instanceof Error ? error.message : "Could not save automation.";
    } finally {
      isSaving = false;
    }
  };

  const removeScenario = (event: CustomEvent<{ id: number }>) => {
    scenarios = scenarios.filter((scenario) => scenario.id !== event.detail.id);
  };

  onMount(() => {
    loadScenarios().catch((error) => {
      errorMessage = error instanceof Error ? error.message : "Could not load automations.";
    });

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        closeModal();
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  });
</script>

<div class="sensor-card">
  <div class="scenario-heading">
    <p>Create an automation for a sensor threshold.</p>
    <button class="button" on:click={openModal}>Create automation</button>
  </div>

  {#if errorMessage && !showModal}
    <p class="error-message" role="alert">{errorMessage}</p>
  {/if}

  {#if scenarios.length === 0}
    <p class="empty-state">No automations yet.</p>
  {:else}
    <ul class="scenario-list">
      {#each scenarios as scenario (scenario.id)}
        <li>
          <ScenarioItem {scenario} on:deleted={removeScenario} />
        </li>
      {/each}
    </ul>
  {/if}
</div>

<Modal
  show={showModal}
  {closeModal}
  ok={saveScenario}
  title="Create automation"
  titleClass="modal-title"
>
  <div class="sensor-device-container">
    <label class="title" for="scenario-sensor">Sensor</label>
    <select id="scenario-sensor" class="select" bind:value={selectedSensor}>
      {#each sensors as sensor}
        <option value={sensor.id}>{sensor.name}</option>
      {/each}
    </select>

    <label class="title" for="scenario-operator">Condition</label>
    <select id="scenario-operator" class="select" bind:value={selectedOperator}>
      {#each Object.entries(operators) as [name, value]}
        <option {value}>{name}</option>
      {/each}
    </select>

    <label class="title" for="scenario-threshold">Threshold</label>
    <input
      id="scenario-threshold"
      class="input"
      type="number"
      step="0.1"
      bind:value={threshold}
    />

    <label class="title" for="scenario-action">Action</label>
    <select id="scenario-action" class="select" bind:value={actionType}>
      {#each Object.entries(actionTypes) as [name, value]}
        <option {value}>{name}</option>
      {/each}
    </select>

    {#if actionType === ScenarioActionType.Notification}
      <label class="title" for="scenario-hysteresis">Recovery margin</label>
      <input
        id="scenario-hysteresis"
        class="input"
        type="number"
        min="0"
        step="0.1"
        bind:value={hysteresis}
      />

      <label class="title" for="scenario-message">Notification message</label>
      <input
        id="scenario-message"
        class="input"
        type="text"
        maxlength="250"
        bind:value={command}
      />
      <p class="field-help">
        Use {"{value}"} and {"{threshold}"} in the message. One notification is sent,
        then the automation re-arms after moisture recovers.
      </p>
    {:else}
      <label class="title" for="scenario-device">Device</label>
      <select id="scenario-device" class="select" bind:value={selectedDevice}>
        {#each devices as device}
          <option value={device.id}>{device.name}</option>
        {/each}
      </select>
    {/if}

    {#if errorMessage}
      <p class="error-message" role="alert">{errorMessage}</p>
    {/if}

    {#if isSaving}
      <p class="field-help">Saving automation...</p>
    {/if}
  </div>
</Modal>

<style>
  @import "./Scenario.scss";
</style>
