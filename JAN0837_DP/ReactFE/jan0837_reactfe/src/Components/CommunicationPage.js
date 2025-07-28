import React, { createContext, useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import '../App.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import TimeDate from './TimeDate.js';
import Picture from './Picture.js';
import Clock from './Clock.js';
import { useRefresh } from './RefreshContext';

function CommunicationPage() {
    const [error, setError] = useState(null);
    const {interval, setInterval } = useRefresh();
    const [parameter1, setParameter1] = useState(42);
    const [status, setStatus] = useState('neznámý');

    const fetchState = async () => {
        try {
            //  status
            const s = await fetch('http://localhost:5000/api/status');
            if (!s.ok) throw new Error(`Status fetch error ${s.status}`);
            const statusText = await s.text();
            setStatus(statusText.replace(/^"|"$/g, ''));

            //  parameter1
            const p = await fetch('http://localhost:5000/api/parameter1');
            if (!p.ok) throw new Error(`Param fetch error ${p.status}`);
            const paramVal = await p.json();
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
    }, []);

    const toggleStatus = () => {
        setStatus(prev => (prev === 'ON' ? 'OFF' : 'ON'));
    };

    return (
    <div>
        <h1>Communication Page</h1>
        <label>Obnovovat každých <input type="number" value={interval} onChange={e => setInterval(Number(e.target.value))} style={{ width: 80, margin: '0 0.5rem' }}/> ms</label>
        <div><strong>Parameter1:</strong> {parameter1}</div>

        <button style={{ marginTop: '0.5rem' }} onClick={() => setParameter1(prev => prev + 1)}>
            Zvýšit o 1
        </button>

        <div><strong>Status:</strong> {status}</div>

        <button style={{ marginTop: '0.5rem' }} onClick={toggleStatus}>
            Přepnout status
        </button>

        <button onClick={fetchState} style={{ marginTop: '1rem' }}>
            Obnovit teď
        </button>

    </div>
  );
}

export default CommunicationPage;