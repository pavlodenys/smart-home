<script lang="ts">
  import { createEventDispatcher } from "svelte";
  import { httpFetch } from "../../../api/httpServise";
  import { ScenarioActionType, type ScenarioData } from "../../../types";

  export let scenario: ScenarioData;

  const dispatch = createEventDispatcher<{
    deleted: { id: number };
    edit: { scenario: ScenarioData };
  }>();
  let deleteError = "";

  const operatorNames = [
    "greater than",
    "less than",
    "equal to",
    "not equal to",
    "greater than or equal to",
    "less than or equal to",
  ];

  const removeScenario = async () => {
    deleteError = "";
    const response = await httpFetch.delete(`api/scenario/${scenario.id}`);
    if (typeof response === "string" && response !== scenario.id.toString()) {
      deleteError = response;
      return;
    }

    dispatch("deleted", { id: scenario.id });
  };
</script>

<article class="scenario-item">
  <div>
    <p>
      If <strong>{scenario.sensors?.[0]?.sensor?.name ?? "sensor"}</strong> is
      <strong>{operatorNames[scenario.operator] ?? "compared with"}</strong>
      <strong>{scenario.threshold}</strong>
    </p>

    {#if scenario.actionType === ScenarioActionType.Notification}
      <p>Notify: <strong>{scenario.command}</strong></p>
    {:else}
      <p>
        Toggle device:
        <strong>{scenario.devices?.[0]?.device?.description ?? "device"}</strong>
      </p>
    {/if}

    <p class="scenario-state">
      {scenario.isConditionActive ? "Triggered, waiting for recovery" : "Armed"}
    </p>

    {#if deleteError}
      <p class="error-message" role="alert">{deleteError}</p>
    {/if}
  </div>

  <div class="scenario-actions">
    <button type="button" class="edit-b" on:click={() => dispatch("edit", { scenario })}>Edit</button>
    <button class="remove-b" aria-label="Delete automation" on:click={removeScenario}>×</button>
  </div>
</article>

<style>
  .scenario-actions {
    display: flex;
    gap: 0.5rem;
  }

  .edit-b,
  .remove-b {
    min-height: 38px;
    padding: 0.45rem 0.7rem;
  }

  .scenario-item {
    align-items: flex-start;
    display: flex;
    justify-content: space-between;
    padding: 1rem;
    text-align: left;
  }

  .scenario-item p {
    margin: 0 0 0.5rem;
  }

  .scenario-state {
    color: #52606d;
    font-size: 0.875rem;
  }
</style>
