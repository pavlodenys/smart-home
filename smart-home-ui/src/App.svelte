<script lang="ts">
  import { onMount } from "svelte";
  import Router from "svelte-spa-router";
  import Dashboard from "./Dashboard.svelte";
  import Login from "./components/Login.svelte";
  import Home from "./components/Home.svelte";
  import NotFound from "./components/NotFound.svelte";

  import SensorDetail from "./components/sensor-datail/SensorDetail.svelte";
  import { getJwtToken, getSession } from "./api/auth";
  import "./app.css";
  import { redirect } from "./redirects";
  import AuthRoute from "./lib/AuthRoute.svelte";
  import { wrap } from "svelte-spa-router/wrap";
  import jwtDecode from "jwt-decode";
  import { navigate } from "svelte-routing";
  import { push, pop, replace } from "svelte-spa-router";

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
  };

  const routes = {
    "/": Home,
    "/dashboard": wrap({
      component: Dashboard,
      conditions: [(detail) => routeLoading(detail)],
    }),
    "/sensor": wrap({
      asyncComponent: () =>
        import("./components/sensor-datail/SensorDetail.svelte"),
    }),
    "/sensor/:id": wrap({
      asyncComponent: () =>
        import("./components/sensor-datail/SensorDetail.svelte"),
    }),
    "/login": Login,
    "*": NotFound,
  };

  const routeLoading = (event) => {
    console.log(event);

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

<nav>
  <a href="/">Home</a>
  <a href="/#/dashboard">Dashboard</a>
  {#if !authenticated}
    <a href="/#/login">Login</a>
  {/if}
  {#if authenticated}
    <a href="/#/login" on:click={logout}>Logout</a>
  {/if}
</nav>

<Router
  {routes}
  on:routeLoading={routeLoading}
  on:conditionsFailed={conditionsFailed}
/>

<style>
  @import "./styles/App.scss";
</style>
