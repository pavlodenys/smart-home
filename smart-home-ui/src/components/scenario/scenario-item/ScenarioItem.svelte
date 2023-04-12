<script lang="ts">
  
    export let scenario;
  
</script>

<div class="scenario-item">
  <h2>{scenario.title}</h2>
  <p>{scenario.description}</p>
</div>

<style>
  .scenario-item {
    text-align: center;
    padding: 1rem;
  }

  /* .scenario-item img {
    width: 100%;
    max-width: 400px;
    margin: 0 auto;
    display: block;
  } */

  .scenario-item h2 {
    margin: 1rem 0;
  }

  .scenario-item p {
    margin: 0;
  }
</style>
