<script lang="ts">
  export let show = false;
  export let closeModal;
  export let ok;
  export let title;
  export let titleClass;
  export let description = "";
  export let okLabel = "Save";
  export let cancelLabel = "Cancel";

  function handleKeyDown(event) {
    if (show && event.key === "Escape") {
      closeModal();
    }
  }
</script>

<svelte:window on:keydown={handleKeyDown} />

{#if show}
  <div class="modal-overlay">
    <div class="modal" role="dialog" aria-modal="true" aria-labelledby="modal-title">
      <header class="modal-header">
        <div>
          <h2 id="modal-title" class="{titleClass}">{title}</h2>
          {#if description}
            <p>{description}</p>
          {/if}
        </div>
        <button class="modal-close" type="button" on:click={closeModal} aria-label="Close dialog">
          <span aria-hidden="true">&#215;</span>
        </button>
      </header>
      <div class="modal-body">
        <slot />
      </div>
      <footer class="modal-actions">
        <button type="button" on:click={closeModal}>{cancelLabel}</button>
        <button class="primary" type="button" on:click={ok}>{okLabel}</button>
      </footer>
    </div>
  </div>
{/if}

<style>
  @import "./Modal.scss";
</style>
