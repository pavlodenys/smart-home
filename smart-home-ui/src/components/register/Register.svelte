<script lang="ts">
  import { httpFetch } from "../../api/httpServise";


  let username = "";
  let email = "";
  let firstName = "";
  let lastName = "";
  let password = "";
  let confirmPassword = "";

  const register = async () => {
     const response = await httpFetch.post('api/auth/register', {
           username,
            email,
            firstName,
            lastName,
            password,
            confirmPassword
        });

        if(response.success){
            console.log("OK!");
        } else {
            console.log(response);
        }
  };

  const validateForm = () => {
    // Add form validation logic here

    return true;
  };
</script>

<div class="register">
  <h1>Register</h1>
  <form on:submit|preventDefault={register}>
    <label>
      Username:
      <input type="text" bind:value={username} on:input={validateForm} />
    </label>
    <label>
      Email:
      <input type="email" bind:value={email} on:input={validateForm} />
    </label>
    <label>
      First Name:
      <input type="text" bind:value={firstName} on:input={validateForm} />
    </label>
    <label>
      Last Name:
      <input type="text" bind:value={lastName} on:input={validateForm} />
    </label>
    <label>
      Password:
      <input type="password" bind:value={password} on:input={validateForm} />
    </label>
    <label>
      Confirm Password:
      <input type="password" bind:value={confirmPassword} on:input={validateForm} />
    </label>
    <button type="submit" disabled={validateForm()}>Register</button>
  </form>
</div>

<style>
  .register {
    display: flex;
    flex-direction: column;
    align-items: center;
  }

  .register form {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    width: 100%;
    max-width: 400px;
  }

  .register label {
    display: block;
    margin-bottom: 10px;
    font-weight: bold;
  }

  .register input {
    display: block;
    width: 100%;
    padding: 8px;
    font-size: 16px;
    border: 1px solid #ccc;
    border-radius: 4px;
    box-sizing: border-box;
    margin-bottom: 20px;
  }

  .register button {
    padding: 10px 20px;
    font-size: 18px;
    background-color: #646cff;
    color: #fff;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: background-color 0.3s ease;
  }

  .register button:hover {
    background-color: #4b47ed;
  }

  /* .register .error {
    color: red;
    margin-top: 5px;
  } */
</style>
