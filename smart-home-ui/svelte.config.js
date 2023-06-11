import { svelte } from '@sveltejs/vite-plugin-svelte';
import preprocess from 'svelte-preprocess';

export default {
  // Use preprocess() for Svelte preprocessor
  preprocess: [
    preprocess({
      scss: {
       // prependData: `@import 'src/styles.scss';`,
      },
    }),
    svelte(),
  ],
};
