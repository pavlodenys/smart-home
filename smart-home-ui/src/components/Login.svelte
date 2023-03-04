<script lang="ts">
  import { onMount } from "svelte";
  import { push } from "svelte-spa-router";
  import { loginApi } from "../api/auth";

  let email = "";
  let password = "";
  let error;

  async function login() {
    const response = loginApi(email, password);
    if (response.success) {
      // Redirect to the return URL
      push(returnUrl || "/");
    }
  }

  let returnUrl;
  onMount(() => {
   // returnUrl = new URLSearchParams(location.search).get("returnUrl");
    returnUrl = '/dashboard';
  });
</script>

<div class="login">
  <h1>Login</h1>
  <form on:submit|preventDefault={login}>
    <label>
      Email:
      <input type="email" bind:value={email} />
    </label>
    <label>
      Password:
      <input type="password" bind:value={password} />
    </label>
    <button type="submit">Login</button>
  </form>
  <!-- {error && <p class="error">{error}</p>} -->
</div>

<style>
  .login {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 1rem;
    max-width: 400px;
    margin: 0 auto;
    padding: 2rem;
    background-color: #fff;
    box-shadow: 0px 2px 6px rgba(0, 0, 0, 0.1);
    border-radius: 0.5rem;
  }

  .login h1 {
    margin: 0;
    font-size: 2rem;
    color: #333;
  }

  .login form {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    width: 100%;
  }

  .login form label {
    display: block;
    font-size: 1.2rem;
    color: #555;
  }

  .login form input {
    border: none;
    border-radius: 0.25rem;
    padding: 0.5rem 1rem;
    font-size: 1.2rem;
    background-color: #f5f5f5;
    color: black;
  }

  .login form button {
    border: none;
    border-radius: 0.25rem;
    padding: 0.5rem 1rem;
    font-size: 1.2rem;
    background-color: #333;
    color: #fff;
    cursor: pointer;
  }

  .login form button:hover {
    background-color: #555;
  }

  .error {
    color: red;
    margin-top: 0.5rem;
  }
</style>
