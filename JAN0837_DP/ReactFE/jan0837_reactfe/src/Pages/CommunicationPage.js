import React, { createContext, useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image, Form, Card, Badge  } from 'react-bootstrap';

import '../App.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import { useRefresh } from '../Communication/RefreshContext.js';
import { useData, useSectionData } from '../Communication/DataProvider';
import { API_URL } from '../variables.js'; 

function CommunicationParamsSidebar({data, refresh, isFetching}) {
  const [lastUpdated, setLastUpdated] = useState(null);
  useEffect(() => {
    setLastUpdated(new Date());
  }, [JSON.stringify(data)]);
  
  return (
    <div className="p-3 border-start h-100">
      <Card>
        <Card.Header>
          <span>Aktuální data z API</span>
            <Badge bg="light" text="dark">
              {lastUpdated ? lastUpdated.toLocaleTimeString() : '—'}
            </Badge>
        </Card.Header>
        <Card.Body style={{ overflow: 'auto' }}>
          <div className="gap-2 mb-2">
            <small className="text-muted">Endpoint: <code>{API_URL}</code></small>
          </div>
          <pre
            style={{
              background: '#f6f8fa',
              padding: 8,
              borderRadius: 6,
              marginTop: 8,
              whiteSpace: 'pre-wrap',
              wordBreak: 'break-word',
              fontSize: 13,
              lineHeight: 1.4,
              maxHeight: 420,
              overflow: 'auto',
            }}
          >
            {JSON.stringify(data, null, 2)}
          </pre>
        </Card.Body>
        <Card.Footer className="d-flex justify-content-between">
          <small className="text-muted">
            Stav: {isFetching ? 'Načítám…' : 'Hotovo'}
          </small>
          <Button size="sm" variant="outline-secondary" onClick={refresh}>
            Refresh
          </Button>
        </Card.Footer>
      </Card>
    </div>
  );
}

function CommunicationPage() {
    //const { number, text, toggle, error, isFetching, inc, dec, setToggleAsync, setTextAsync, refresh } = useData();
    const { interval, setInterval } = useRefresh();
    const { section: d, saveSection, data, error, isFetching, refresh } = useSectionData('TestData');

    const number = Number(d?.number ?? 0);
    const text = typeof d?.text === 'string' ? d.text : String(d?.text ?? '');
    const toggle = (() => {
        const t = String(d?.toggle ?? '').toLowerCase();
        return t === 'true' || t === 'on' || t === '1';
    })();
    const inc = () => saveSection({ number: number + 1 });
    const dec = () => saveSection({ number: number - 1 });
    const setToggleAsync = () => saveSection({ toggle: !toggle ? 'true' : 'false' });

    return (
        <Row className="g-0">
            <Col xs={12} lg={8} className="p-3">
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

                    <button style={{ marginTop: '0.5rem' }} onClick={() => setToggleAsync(!toggle)} /*disabled={isFetching}*/>Přepnout status ({String(toggle)})</button>

                    <div style={{ marginTop: 12 }}><strong>Text:</strong> {text}</div>

                    <div>
                        <button onClick={refresh} style={{ marginTop: '1rem' }} disabled={isFetching}>Obnovit teď</button>
                        <span style={{ marginLeft: 8 }}>{isFetching ? 'Fetching…' : 'Idle'}</span>
                    </div>
                </div>
            </Col>

            <Col lg={4}>
                <CommunicationParamsSidebar data={data} refresh={refresh} isFetching={isFetching}/>
            </Col>

        </Row>
    );
}

export default CommunicationPage;