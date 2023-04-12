import { httpFetch } from './httpServise';
// src/lib/auth.ts
export interface SessionData {
    accessToken: string;
    // add other session data properties as needed
}

export function getSession(): SessionData | null {
    const sessionDataString = localStorage.getItem('sessionData');
    if (sessionDataString) {
        try {
            const sessionData = {accessToken:sessionDataString}; //JSON.parse(sessionDataString);
            return sessionData;
        } catch (err) {
            console.error('Failed to parse session data:', err);
        }
    }
    return null;
}

export const getJwtToken = () => {
    const token = localStorage.getItem('accessToken');
    if (token) {
        return token;
    }
    return null;
}

export const cleanJwtToken = () => {
    localStorage.removeItem('accessToken');
}

export const loginApi = async (email, password) => {
    try {
        const response = await httpFetch.post('api/auth/login', {
           username: email,
            password,
        });

        if (response.token) {
           // const { token } = await response.json();
            setToken(response.token);

            return { success: true };
        } else {
            let error = JSON.parse(response);
            throw new Error(error ? error.message : 'Login failed');
        }
    } catch (err) {
        return { success: false, error: err.message };
    }
}

const setToken = (accessCode) => {
    localStorage.setItem('accessToken', JSON.stringify({ accessToken: accessCode }));
}
