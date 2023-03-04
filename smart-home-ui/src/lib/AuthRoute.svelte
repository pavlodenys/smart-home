<script lang="ts">
 // import jwt_decode from "jwt_decode";
  import { navigate } from "svelte-routing";
  import { getJwtToken } from "../api/auth";

  export let component;

  let user;

  const token = getJwtToken();

  // if (token) {
  //   //const { sub } = jwt_decode(token);
  // }
</script>

{#if user}
  <svelte:component this={component} />
{:else}
  <p>Redirect to login...</p>
  {#await navigate("/#/login")}{/await}
{/if}
