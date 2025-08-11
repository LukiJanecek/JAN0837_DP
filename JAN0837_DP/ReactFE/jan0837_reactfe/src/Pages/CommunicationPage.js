import React, { createContext, useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import '../App.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import TimeDate from '../Components/TimeDate.js';
import Picture from '../Components/Picture.js';
import Clock from '../Components/Clock.js';
import { useRefresh } from '../Communication/RefreshContext.js';

function CommunicationPage() {
    const [error, setError] = useState(null);
    const {interval, setInterval } = useRefresh();
    const [parameter1, setParameter1] = useState(0);
    const [status, setStatus] = useState(false);

    const fetchState = async () => {
        try {
            // get status
            const status = await fetch('http://localhost:5000/api/status');
            if (!status.ok) throw new Error(`Status fetch error ${status.status}`);
            const statusState = await status.json();
            setStatus(Boolean(statusState));

            // get parameter1
            const value = await fetch('http://localhost:5000/api/parameter1');
            if (!value.ok) throw new Error(`Param fetch error ${value.status}`);
            const paramVal = await value.json();
            setParameter1(paramVal);

            setError(null);
        } 
        catch (e) {
            setError(e.message);
        }
    };

    useEffect(() => {
    // Načti hned a pak každé 2 s
        fetchState();
        const intervalId = setInterval(fetchState, interval);
        return () => clearInterval(intervalId);
    }, [interval]);

    // POST helper
    const postJson = async (url, payload) => {
        const res = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (!res.ok) throw new Error(`Failed ${url}: ${res.status}`);
        return res;
    };

    //
    const inc = async () => {
        try {
            const newVal = parameter1 + 1;
            await postJson('http://localhost:5000/api/parameter1', newVal);
            setParameter1(newVal);
        } 
        catch (e) {
            setError(e.message);
        }
    };

    const dec = async () => {
        try {
            const newVal = parameter1 - 1;
            await postJson('http://localhost:5000/api/parameter1', newVal);
            setParameter1(newVal);
        } 
        catch (e) {
            setError(e.message);
        }
    };

    const toggle = async () => {
        try {
            const newStatus = !status;
            await postJson('http://localhost:5000/api/status', newStatus);
            setStatus(newStatus);
        }  
        catch (e) {
            setError(e.message);
        }
    };

    const toggleStatus = () => {
        setStatus(prev => (prev === 'ON' ? 'OFF' : 'ON'));
    };

    return (
    <div>
        <h1>Communication Page</h1> 
        {error && <div style={{color:'red'}}>Chyba: {error}</div>}
        
        <label>Obnovovat každých <input type="number" value={interval} onChange={e => setInterval(Number(e.target.value))} style={{ width: 80, margin: '0 0.5rem' }}/> ms</label>
        <div><strong>Parameter1:</strong> {parameter1}</div>

        <button style={{ marginTop: '0.5rem' }} onClick={inc}>
            Zvýšit o 1
        </button>

        <button style={{ marginTop: '0.5rem' }} onClick={dec}>
            Snížit o 1
        </button>

        <div><strong>Status:</strong> {status}</div>

        <button style={{ marginTop: '0.5rem' }} onClick={toggle}>
            Přepnout status
        </button>

        <button onClick={fetchState} style={{ marginTop: '1rem' }}>
            Obnovit teď
        </button>

    </div>
  );
}

export default CommunicationPage;