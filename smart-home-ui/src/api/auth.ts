// src/lib/auth.ts
export interface SessionData {
    accessToken: string;
    // add other session data properties as needed
}

export function getSession(): SessionData | null {
    const sessionDataString = localStorage.getItem('sessionData');
    if (sessionDataString) {
        try {
            const sessionData = JSON.parse(sessionDataString);
            return sessionData;
        } catch (err) {
            console.error('Failed to parse session data:', err);
        }
    }
    return null;
}
