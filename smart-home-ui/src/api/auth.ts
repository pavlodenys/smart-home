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

export const loginApi = (email, password) => {

    setToken(email);
    return {success: true};
    // try {
    //     const response = await fetch('/api/auth/login', {
    //         method: 'POST',
    //         headers: {
    //             'Content-Type': 'application/json',
    //         },
    //         body: JSON.stringify({
    //             email,
    //             password,
    //         }),
    //     });

    //     if (response.ok) {
    //         const { access_token } = await response.json();
    //         setToken(access_token);

    //         return 'ok';
    //     } else {
    //         throw new Error('Login failed');
    //     }
    // } catch (err) {
    //     return err.message;
    // }
}

const setToken = (accessCode) => {
    localStorage.setItem('accessToken', JSON.stringify({ accessToken: accessCode }));
}


