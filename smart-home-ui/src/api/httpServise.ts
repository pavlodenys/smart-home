import { push } from "svelte-spa-router";
import { cleanJwtToken } from "./auth";

//const baseUrl = "https://localhost:7138/"; //TODO: Get from config
const baseUrl = import.meta.env.VITE_BASE_URL;
const getConfig = (): HeadersInit => {
    const token = localStorage.getItem('accessToken');
    if (token) {
        return {
            "Authorization": `Bearer ${JSON.parse(token).accessToken}`,
            "Content-Type": "application/json"
        };
    }
    return { "Content-Type": "application/json" };
}
const httpMethod = async (method, url, data = null) => {
    const config = getConfig();

    const response = await fetch(baseUrl + url, data !== null ? {
        method: method,
        headers: config,
        body: JSON.stringify(data)
    } : {
        method: method,
        headers: config
    });

    if (response.ok) {
        return await response.json();
    } else {
       if(response.status === 401) {
           cleanJwtToken();
           push(`/login`);
       } else {
        return await response.text();
       }

    }
}

export const httpFetch = {
    get: async (url) => {
        return await httpMethod("GET", url);
    },
    post: async (url, data) => {
        return await httpMethod("POST", url, data);
    },
    patch: async (url, data) => {
        return await httpMethod("POST", url, data);
    },
    put: async (url, data) => {
        return await httpMethod("PUT", url, data);
    },
    delete: async (url) => {
        return await httpMethod("DELETE", url);
    }
}