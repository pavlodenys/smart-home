<script lang="ts">
  import { onMount } from "svelte";
  import { push } from "svelte-spa-router";
  import { loginApi } from "../api/auth";

  let email = "";
  let password = "";
  let error;
  let emailInvalid = false;
  let passwordInvalid = false;

  async function login() {
    if (!email.trim()) {
      emailInvalid = true;
    }
    if (!password.trim()) {
      passwordInvalid = true;
    }

    if (emailInvalid || passwordInvalid) {
      return;
    }
    const response = await loginApi(email, password);
    if (response.success) {
      // Redirect to the return URL
      push(returnUrl || "/");
    } else {
      error = response.error;
    }
  }

  let returnUrl;
  onMount(() => {
    // returnUrl = new URLSearchParams(location.search).get("returnUrl");
    returnUrl = "/dashboard";
  });

  $: isFormInvalid = !email || !password;
</script>

<div class="login">
  <h1>Login</h1>
  <form on:submit={login} novalidate>
    <label>
      Username:
      <input
        type="text"
        bind:value={email}
        on:input={(e) => (email = e.target.value.trim())}
        class:invalid={emailInvalid}
        required
      />
      {#if emailInvalid}
        <span class="error-message">Email is required</span>
      {/if}
    </label>
    <label>
      Password:
      <input
        type="password"
        bind:value={password}
        on:input={(e) => (password = e.target.value.trim())}
        class:invalid={passwordInvalid}
        required
      />
      {#if passwordInvalid}
        <span class="error-message">Password is required</span>
      {/if}
    </label>
    <button type="submit">Login</button>
  </form>
  {#if error}
    <p class="error">{error}</p>
  {/if}
  <div class="links">
    <a href="/#/forgot-password">Forgot password?</a>
    <a href="/#/register">Register</a>
  </div>
</div>

<style>
  .login {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 1rem;
    max-width: 480px;
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

  input.invalid {
    border: 2px solid red;
  }

  .error-message {
    color: red;
    font-size: 0.8em;
    margin-left: 10px;
  }
</style>
