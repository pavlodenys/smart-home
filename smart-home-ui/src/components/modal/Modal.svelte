<script lang="ts">
  export let show = false;
  export let closeModal;
  export let ok;

  function handleKeyDown(event) {
    if (event.key === "Escape") {
      closeModal();
    }
  }
</script>

{#if show}
  <div class="modal-overlay" on:keydown={handleKeyDown}>
    <div class="modal">
      <slot />
      <button on:click={ok}>Ok</button>
      <button on:click={closeModal}>Close modal</button>
    </div>
  </div>
{/if}

<style>
  @import "./Modal.scss";
</style>
