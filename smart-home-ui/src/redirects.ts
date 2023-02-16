import {navigate} from 'svelte-routing';

export function redirect(node:  HTMLElement, authenticated: boolean){
    if(!authenticated){
        navigate("/login");
    }
}