import { API_URL } from '../variables';

export async function apiGet(endpoint) {
    const res = await fetch(`${API_URL}${endpoint}`);

    if (!res.ok) {
        const errorText = await res.text()
        console.error(`[API GET ${endpoint}] Error:`, res.status, errorText)
        throw new Error(`API ${endpoint} Failed: ${res.status}`)
    }

    if (res.status === 204) {    
        return null;
    }

    const ct = res.headers.get('content-type') || '';
    return ct.includes('application/json') ? await res.json() : await res.text();
    //return await res.json();
}

export async function apiPost(endpoint, body) {
    const res = await fetch(`${API_URL}${endpoint}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(body)
    })

    if (!res.ok) {
        const errorText = await res.text()
        console.error(`[API POST ${endpoint}] Error:`, res.status, errorText)
        throw new Error(`API ${endpoint} Failed: ${res.status}`)
    }

    if (res.status === 204) {    
        return null;
    }
    
    const ct = res.headers.get('content-type') || '';
    return ct.includes('application/json') ? await res.json() : await res.text();
    //return await res.json();
}

export async function readState(signal) {
    const res = await fetch(API_URL, {signal});
    
    if (!res.ok) {
        const errorText = await res.text()
        console.error(`[GET Error:`, res.status, errorText)
        throw new Error(`GET Failed ${res.status}`);
    } 

    return await parseBody(res);
}

export async function writeState(patch, signal) {
    const res = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(patch),
        signal
    });

    if (!res.ok) {
        const errorText = await res.text()
        console.error(`[POST Error:`, res.status, errorText)
        throw new Error(`POST Failed ${res.status}`);
    } 
    
    return await parseBody(res);
}

async function parseBody(res) {
    if (res.status === 204) {
        return null;
    } 

    const len = res.headers.get('content-length');
  
    if (len === '0') {
        return null;
    }
    
    const ct = (res.headers.get('content-type') || '').toLowerCase();
    const text = await res.text();
    
    if (!text) {
        return null;
    } 

    if (ct.includes('application/json')) {
        try { 
            return JSON.parse(text); 
        }
        catch { 
            console.warn('Invalid JSON – returning raw text'); 
            return text; 
        }
    }
    return text;
}