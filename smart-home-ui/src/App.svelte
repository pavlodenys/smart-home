<script lang="ts">
  import { onMount } from "svelte";
  import Router from "svelte-spa-router";
  import Dashboard from "./Dashboard.svelte";
  import Login from "./components/Login.svelte";
  import Home from "./components/Home.svelte";
  import {getSession} from "./api/auth";
  import "./app.css";
  import { redirect } from "./redirects";

  let authenticated = false;

  onMount(() => {
    const session = getSession();


    authenticated == !!session;

        console.log(authenticated);
  });

  const requireAuth = (node: HTMLElement) => {
    redirect(node, authenticated);
  };

  const routes = {
    "/": Home,
    "/dashboard": Dashboard,
    "/login": Login,
  };
</script>

<!-- <main>

  <div class="card">
    <Dashboard message={'Hello!'} />
  </div>

</main> -->

<nav>
  <a href="/">Home</a>
  <a href="/#/dashboard">Dashboard</a>
  <a href="/#/login">Login</a>
</nav>

<Router {routes} />

<style>
  @import "./styles/App.scss";
</style>
