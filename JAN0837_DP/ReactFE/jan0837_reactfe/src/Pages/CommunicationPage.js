import React, { createContext, useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import '../App.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import TimeDate from '../Components/TimeDate.js';
import Picture from '../Components/Picture.js';
import Clock from '../Components/Clock.js';
import { useRefresh } from '../Communication/RefreshContext.js';
import { API_URL } from '../variables.js'; 

function CommunicationPage() {
    const [error, setError] = useState(null);
    const { interval, setInterval: setRefreshInterval } = useRefresh(); // alias!
    const [number, setNumber] = useState(0);
    const [toggle, setToggle] = useState(false);
    const [text, setText] = useState('');

    const fetchState = async () => {
        try {
            const res = await fetch(API_URL);           // GET /api/data
            if (!res.ok) throw new Error(res.statusText);
            const json = await res.json();              // { number, text, toggle }
            setNumber(Number(json.number) || 0);
            setText(typeof json.text === 'string' ? json.text : '');
            
            // backend posílá toggle jako string ("true"/"false" nebo "ON"/"OFF")
            const t = String(json.toggle || '').toLowerCase();
            setToggle(t === 'true' || t === 'on' || t === '1');
            setError(null);
        } 
        catch (e) {
            setError(e.message);
        }
    };

    useEffect(() => {
        const fetchState = async () => {
            try {
                const res = await fetch(API_URL);
                if (!res.ok) throw new Error(res.statusText);
                    const json = await res.json();
                    // ... setNumber / setText / setToggle ...
                } catch (e) {
                setError('Failed to fetch: ' + e.message);
                console.error(e);
            }
        };

        fetchState();
        const ms = Number.isFinite(interval) && interval > 0 ? interval : 2000;
        const id = window.setInterval(fetchState, ms);
        return () => window.clearInterval(id);
    }, [interval]);

    // POST helper
    const postJson = async (payload) => {
        console.log('POST', API_URL, payload)
        const res = await fetch(API_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (!res.ok) {
            const txt = await res.text().catch(() => '');
            throw new Error(`${res.status} ${res.statusText} ${txt}`);
        }
        const len = res.headers.get('content-length');
        if (len && len !== '0') {
            return await res.json();
        }
        if (res.headers.get('content-type')?.includes('application/json')) {
            try { return await res.json(); } catch {}
        }
        return null;
    };

    //
    const inc = async () => {
        try {
            const newVal = number + 1;
            await postJson({ number: newVal });
            setNumber(newVal);
        } 
        catch (e) {
            setError(e.message);
        }
    };

    const dec = async () => {
        try {
            const newVal = number - 1;
            await postJson({ number: newVal });
            setNumber(newVal);
        } 
        catch (e) {
            setError(e.message);
        }
    };

    const toggleAction  = async () => {
        try {
            const newToggle = !toggle;
            await postJson({ toggle: newToggle ? 'true' : 'false' });
            setToggle(newToggle);
        }  
        catch (e) {
            setError(e.message);
        }
    };

    return (
        <div>
            <h1>Communication Page</h1> 
            {error && <div style={{color:'red'}}>Chyba: {error}</div>}
            
            <label>Obnovovat každých{' '} <input type="number" value={interval} onChange={e => setInterval(Number(e.target.value) || 2000)} style={{ width: 80, margin: '0 0.5rem' }}/> ms</label>

            <div style={{ marginTop: 12 }}><strong>Number:</strong> {number}</div>

            <button style={{ marginTop: '0.5rem' }} onClick={inc}>
                Zvýšit o 1
            </button>

            <button style={{ marginTop: '0.5rem' }} onClick={dec}>
                Snížit o 1
            </button>

            <div><strong>Status:</strong> {toggle}</div>

            <button style={{ marginTop: '0.5rem' }} onClick={toggleAction}>
                Přepnout status
            </button>

            <div style={{ marginTop: 12 }}><strong>Text:</strong> {text}</div>

            <button onClick={fetchState} style={{ marginTop: '1rem' }}>
                Obnovit teď
            </button>

        </div>
    );
}

export default CommunicationPage;