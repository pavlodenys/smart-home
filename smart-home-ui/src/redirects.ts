import {navigate} from 'svelte-routing';

export function redirect(authenticated: boolean){
    if(!authenticated){
        navigate("/#/login");
    }
}