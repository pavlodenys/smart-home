<script>
  let email = '';

  function resetPassword() {
    // Send a request to the server to reset the password for the given email
    console.log(`Resetting password for email: ${email}`);
  }
</script>

<div class="forgot-password">
  <h1>Forgot Password</h1>
  <form on:submit|preventDefault={resetPassword}>
    <label>
      Email:
      <input type="email" bind:value={email} required />
    </label>
    <button type="submit">Reset Password</button>
  </form>
</div>

<style>
  .forgot-password {
    max-width: 400px;
    margin: 0 auto;
    padding: 2rem;
    border: 1px solid #ccc;
    border-radius: 0.5rem;
  }

  .forgot-password h1 {
    margin-top: 0;
    margin-bottom: 1rem;
  }

  .forgot-password label {
    display: block;
    margin-bottom: 0.5rem;
  }

  .forgot-password input[type="email"] {
    width: 100%;
    padding: 0.5rem;
    font-size: 1rem;
    border: 1px solid #ccc;
    border-radius: 0.25rem;
  }

  .forgot-password button[type="submit"] {
    display: block;
    width: 100%;
    padding: 0.5rem;
    font-size: 1rem;
    background-color: #007aff;
    color: #fff;
    border: none;
    border-radius: 0.25rem;
    cursor: pointer;
  }

  .forgot-password button[type="submit"]:hover {
    background-color: #0062cc;
  }
</style>
