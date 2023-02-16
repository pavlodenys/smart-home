<script lang="ts">
 // import { Link, navigate } from 'svelte-routing';

export let links = [];
</script>

<nav>
  <ul>
    {#each links as link}
      <li>
        <!-- <Link to={link.route}>
          <a href="/">{link.text}</a>
        </Link> -->
      </li>
    {/each}
  </ul>
</nav>

<style>
    @import './TopNav.scss';
</style>