import React, { createContext, useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image, Form  } from 'react-bootstrap';

import '../App.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import { useRefresh } from '../Communication/RefreshContext.js';
import { useData } from '../Communication/DataProvider';
import { API_URL } from '../variables.js'; 

function CommunicationParamsSidebar() {
  return (
    <div className="p-3 border-start h-100">
      <div className="fw-semibold mb-3">Parametry</div>
      <div className="d-grid gap-2">
        <Button variant="primary">Start</Button>
        <Button variant="outline-secondary">Stop</Button>
        <Form.Group className="mt-3">
          <Form.Label>Název</Form.Label>
          <Form.Control type="text" placeholder="Zadej název…" />
        </Form.Group>
        <Form.Group>
          <Form.Label>Poznámky</Form.Label>
          <Form.Control as="textarea" rows={4} placeholder="…" />
        </Form.Group>
        <Button variant="success">Uložit</Button>
      </div>
    </div>
  );
}

function CommunicationPage() {
    //const { number, text, toggle, error, isFetching, inc, dec, setToggleAsync, setTextAsync, refresh } = useData();
    const { interval, setInterval } = useRefresh();
    const { data, saveData, error, isFetching, refresh } = useData();

    const number = Number(data?.number ?? 0);
    const text = typeof data?.text === 'string' ? data.text : String(data?.text ?? '');
    const toggle = (() => {
        const t = String(data?.toggle ?? '').toLowerCase();
        return t === 'true' || t === 'on' || t === '1';
    })();
    const inc = () => saveData({ number: number + 1 });
    const dec = () => saveData({ number: number - 1 });
    const setToggleAsync = () => saveData({ toggle: !toggle ? 'true' : 'false' });

    return (
        <Row className="g-0">
            <Col xs={12} lg={10} className="p-3">
                <div>
                    <h1>Communication Page</h1> 
                    {error && <div style={{color:'red'}}>Chyba: {error}</div>}
                    
                    <div style={{ marginTop: 12 }}><strong>Obnovovat každých:</strong> {interval}</div>
                    {/*
                    <label>Obnovovat každých{' '} <input type="number" value={interval} onChange={e => setInterval(Number(e.target.value) || 2000)} style={{ width: 80, margin: '0 0.5rem' }}/> ms</label>
                    */}
                    <div style={{ marginTop: 12 }}><strong>Number:</strong> {number}</div>

                    <button style={{ marginTop: '0.5rem' }} onClick={inc} disabled={isFetching}>Zvýšit o 1</button>

                    <button style={{ marginTop: '0.5rem' }} onClick={dec} disabled={isFetching}>Snížit o 1</button>

                    <div><strong>Status:</strong> {String(toggle)}</div>

                    <button style={{ marginTop: '0.5rem' }} onClick={() => setToggleAsync(!toggle)} disabled={isFetching}>Přepnout status</button>

                    <div style={{ marginTop: 12 }}><strong>Text:</strong> {text}</div>

                    <div>
                        <button onClick={refresh} style={{ marginTop: '1rem' }} disabled={isFetching}>Obnovit teď</button>
                        <span style={{ marginLeft: 8 }}>{isFetching ? 'Fetching…' : 'Idle'}</span>
                    </div>
                </div>
            </Col>

            {/* RIGHT: 2/12 na lg (≈ 16.7 %) nebo přepni na lg={1} (≈ 8.33 %) */}
            <Col xs={12} lg={2}>
                <CommunicationParamsSidebar />
            </Col>
        </Row>
    );
}

export default CommunicationPage;