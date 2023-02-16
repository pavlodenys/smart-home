<script lang="ts">
  export let onSubmit;
  let email = '';
  let password = '';
  let error;

  async function login() {
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email,
          password,
        }),
      });

      if (response.ok) {
        const { access_token } = await response.json();
        onSubmit(access_token);
      } else {
        throw new Error('Login failed');
      }
    } catch (err) {
      error = err.message;
    }
  }
</script>


  <div class="login">
    <h1>Login</h1>
    <form on:submit|preventDefault={login}>
      <label>
        Email:
        <input type="email" bind:value={email}>
      </label>
      <label>
        Password:
        <input type="password" bind:value={password}>
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
    padding: 1rem;
  }
  form {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    width: 100%;
    max-width: 300px;
    margin-top: 1rem;
  }
  label {
    display: flex;
    flex-direction: column;
  }
  input[type='email'],
  input[type='password'] {
    padding: 0.5rem;
    font-size: 1rem;
    border: 1px solid #ccc;
    border-radius: 0.25rem;
  }
  button {
    padding: 0.5rem;
    font-size: 1rem;
    background-color: #000;
    color: #fff;
    border: none;
    border-radius: 0.25rem;
    cursor: pointer;
  }
  .error {
    color: red;
  }
</style>
