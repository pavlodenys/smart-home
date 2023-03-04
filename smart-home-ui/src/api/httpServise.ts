
const baseUrl = "https://localhost:7138/"; //TODO: Get from config
const getConfig = (): HeadersInit => {
    const token = localStorage.getItem('accessToken');
    if (token) {
        return {
            "Authorization": token,
            "Content-Type": "application/json"
        };
    }
    return {};
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

    return await response.json();
}

export const httpFetch = {
    get: async (url) => {
        return await httpMethod("GET", url);
    },
    post: async (url, data) => {
        return await httpMethod("POST", url, data);
    },
    patch: async (url) => {
        return await httpMethod("POST", url);
    },
    put: async (url) => {
        return await httpMethod("PUT", url);
    },
    delete: async (url) => {
        return await httpMethod("DELETE", url);
    }
}