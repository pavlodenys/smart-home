<script lang="ts">
  import jwtDecode from "jwt-decode";
  import { getJwtToken } from "../../api/auth";

  let username = "Signed-in user";
  let email = "";

  try {
    const stored = getJwtToken();
    const token = stored ? JSON.parse(stored).accessToken : "";
    const claims: Record<string, string> = token ? jwtDecode(token) : {};
    username = claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || username;
    email = claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || "";
  } catch {
    // Keep the safe fallback when an old or invalid token is present.
  }
</script>

<header class="page-header">
  <div>
    <p class="eyebrow">Account</p>
    <h1>Profile</h1>
    <p class="page-summary">Your local smart-home account.</p>
  </div>
</header>

<section class="panel profile-card">
  <div class="profile-avatar" aria-hidden="true">{username.slice(0, 1).toUpperCase()}</div>
  <div>
    <h2>{username}</h2>
    {#if email}<p>{email}</p>{/if}
    <span class="status-dot online">Authenticated</span>
  </div>
</section>
