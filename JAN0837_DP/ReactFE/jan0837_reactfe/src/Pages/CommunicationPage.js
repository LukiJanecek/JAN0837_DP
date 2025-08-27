import React, { createContext, useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import '../App.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import { useRefresh } from '../Communication/RefreshContext.js';
import { useData } from '../Communication/DataProvider';
import { API_URL } from '../variables.js'; 

function CommunicationPage() {
    const { number, text, toggle, error, isFetching, inc, dec, setToggleAsync, setTextAsync, refresh } = useData();
    const { interval, setInterval } = useRefresh();

    return (
        <div>
            <h1>Communication Page</h1> 
            {error && <div style={{color:'red'}}>Chyba: {error}</div>}
            
            <label>Obnovovat každých{' '} <input type="number" value={interval} onChange={e => setInterval(Number(e.target.value) || 2000)} style={{ width: 80, margin: '0 0.5rem' }}/> ms</label>

            <div style={{ marginTop: 12 }}><strong>Number:</strong> {number}</div>

            <button style={{ marginTop: '0.5rem' }} onClick={inc} disabled={isFetching}>
                Zvýšit o 1
            </button>

            <button style={{ marginTop: '0.5rem' }} onClick={dec} disabled={isFetching}>Snížit o 1</button>

            <div><strong>Status:</strong> {toggle}</div>

            <button style={{ marginTop: '0.5rem' }} onClick={() => setToggleAsync(!toggle)} disabled={isFetching}>Přepnout status</button>

            <div style={{ marginTop: 12 }}><strong>Text:</strong> {text}</div>

            <div>
                <button onClick={refresh} style={{ marginTop: '1rem' }} disabled={isFetching}>Obnovit teď</button>
                <span style={{ marginLeft: 8 }}>{isFetching ? 'Fetching…' : 'Idle'}</span>
            </div>
        </div>
    );
}

export default CommunicationPage;