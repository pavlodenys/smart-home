import { wrap } from "svelte-spa-router/wrap";
import { push } from "svelte-spa-router";
import { getJwtToken } from "../api/auth";

function checkAuthentication() {
    // Perform authentication check here
    // Return true if authenticated, false otherwise
    const session = getJwtToken();

    return !!session;
}

export const wrapAuth = (asyncComponent)=> {
    return wrap({
        asyncComponent: asyncComponent,
        conditions: [
            // Pre-condition to check if the user is authenticated
            (detail) => {
                
                const isAuthenticated = checkAuthentication(); // Replace with your authentication check logic
                if (!isAuthenticated) {
                    // Redirect to the login page if not authenticated
                    push('/login');
                }
                return isAuthenticated;
            },
        ]
    });
}