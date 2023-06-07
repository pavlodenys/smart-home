<script lang="ts">
  import { onMount } from "svelte";
  import Router from "svelte-spa-router";

  import { getJwtToken, cleanJwtToken } from "./api/auth";
  import "./styles.scss";
  import { redirect } from "./redirects";
  import { push } from "svelte-spa-router";
  import { routes } from "./routes";

  let authenticated;

  onMount(() => {
    const session = getJwtToken();

    authenticated = !!session;

    console.log(authenticated);
  });

  const requireAuth = () => {
    redirect(authenticated);
  };

  const logout = () => {
    authenticated = false;
    cleanJwtToken();
      push(`/login`);
  };

  const routeLoading = (event) => {
    console.log(event);

    if (
      event.detail.route === "/forgot-password" ||
      event.detail.route === "/register"
    ) {
      return true;
    }

    const jwtToken = getJwtToken();

    if (jwtToken) {
      // const token = jwtDecode(jwtToken);
      authenticated = !!jwtToken;
      console.log(jwtToken);

      return true;
    }
    // else {
    //   requireAuth();
    // }

    return false;
  };

  const conditionsFailed = (event) => {
    console.log(event);

    // Perform any action, for example replacing the current route
    if (!event.detail.userData) {
      let returnUrl;
      returnUrl = event.detail.route ? event.detail.route : "";
      push(`/login?returnUrl=${returnUrl}`);
    }
  };
</script>

<nav class="topbar">
  <a href="/" class="logo">
    <svg viewBox="0 0 200 200" xmlns="http://www.w3.org/2000/svg">
      <rect width="100%" height="100%" fill="#646cff5a" rx="10" ry="10" />
      <rect x="25%" y="25%" width="50%" height="50%" fill="#007aff" />
      <circle cx="50%" cy="50%" r="40%" fill="#fff" />
      <circle cx="50%" cy="50%" r="30%" fill="#007aff" />
      <rect x="35%" y="40%" width="30%" height="20%" fill="#fff" />
    </svg>
  </a>
  <div class="links">
    <a href="/#/dashboard">Dashboard</a>

    {#if !authenticated}
      <a href="/#/login">Login</a>
    {/if}
    {#if authenticated}
      <a href="/#/profile">Profile</a>
      <button  on:click={logout}>Logout</button>
    {/if}
  </div>
</nav>

<!-- <Router
  {routes}
  on:routeLoading={routeLoading}
  on:conditionsFailed={conditionsFailed}
/> -->
<Router
  {routes}
  on:routeLoading={routeLoading}
  on:conditionsFailed={conditionsFailed}
/>

<style>
  @import "./styles/App.scss";
</style>
