import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import '../App.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import TimeDate from './TimeDate.js';
import Picture from './Picture.js';
import Clock from './Clock.js';

function CommunicationPage() {
    const [parameter1, setParameter1] = useState(42);
    const [status, setStatus] = useState('neznámý');

    const toggleStatus = () => {
        setStatus(prev => (prev === 'ON' ? 'OFF' : 'ON'));
    };

    return (
    <div>
        <h1>Communication Page</h1>
        <div>
            <strong>Parameter1:</strong> {parameter1}
        </div>

        <button style={{ marginTop: '0.5rem' }} onClick={() => setParameter1(prev => prev + 1)}>
            Zvýšit o 1
        </button>

        <div>
          <strong>Status:</strong> {status}
        </div>

        <button style={{ marginTop: '0.5rem' }} onClick={toggleStatus}>
            Přepnout status
        </button>
    </div>
  );
}

export default CommunicationPage;