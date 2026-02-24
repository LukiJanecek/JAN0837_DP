import React, { useState, useEffect } from 'react';
import { Row, Col, Button, Form, Badge } from 'react-bootstrap';

import '../App.css';
import './CarLightPage.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import { useRefresh } from '../Communication/RefreshContext.js';
import { useData, useSectionData } from '../Communication/DataProvider.js';

const toBool = (v) => {
  if (typeof v === 'boolean') return v;
  const s = String(v ?? '').trim().toLowerCase();
  return s === 'true' || s === '1' || s === 'on';
};

/*
 * ── CarLight variable map ──────────────────────────────
 *
 * INPUTS  (FE → PLC):
 *   btnStart               Bool   – start test sequence
 *   btnReset               Bool   – reset test
 *   markerLight            Bool   – marker light on/off
 *   brakeLight             Bool   – brake light on/off
 *   turnLight              Bool   – turn signal on/off
 *   markerBlinksPerSec     Real   – blinks/s for marker
 *   brakeBlinksPerSec      Real   – blinks/s for brake
 *   turnBlinksPerSec       Real   – blinks/s for turn
 *   sensorPositionDelay    Real   – delay (s) before sensorPosition → TRUE
 *   sensorConnectorDelay   Real   – delay (s) before sensorConnector → TRUE
 *   blockSensorPosition    Bool   – block position sensor (simulate fault)
 *   blockSensorConnector   Bool   – block connector sensor (simulate fault)
 *
 * OUTPUTS (PLC → FE):
 *   sensorPosition              Bool – position sensor state
 *   sensorConnectorConnected    Bool – connector sensor state
 *   done                        Bool – sequence completed correctly
 */

/* ── Visual car-light panel ───────────────────────────── */
function CarLightCanvas({ d }) {
  const marker = toBool(d?.markerLight);
  const brake  = toBool(d?.brakeLight);
  const turn   = toBool(d?.turnLight);
  const done   = toBool(d?.done);

  return (
    <div className="carlight-canvas">
      <div className="carlight-row">
        <div className={`carlight-bulb carlight-bulb--marker ${marker ? 'on' : ''}`}>
          <i className="bi bi-lightbulb-fill" />
          <span>Marker</span>
        </div>
        <div className={`carlight-bulb carlight-bulb--brake ${brake ? 'on' : ''}`}>
          <i className="bi bi-exclamation-triangle-fill" />
          <span>Brake</span>
        </div>
        <div className={`carlight-bulb carlight-bulb--turn ${turn ? 'on' : ''}`}>
          <i className="bi bi-arrow-left-right" />
          <span>Turn</span>
        </div>
      </div>
      {done && (
        <div className="carlight-done mt-3">
          <Badge bg="success" className="fs-5 px-4 py-2">DONE ✔</Badge>
        </div>
      )}
    </div>
  );
}

/* ── Sidebar: inputs + outputs ────────────────────────── */
function CarLightParamsSidebar() {
  const { interval, setInterval } = useRefresh();
  const { section: d, saveSection, data, error, isFetching, refresh } = useSectionData('CarLight');

  // ── Inputs – buttons ──
  const btnStart    = toBool(d?.btnStart);
  const btnReset    = toBool(d?.btnReset);
  const markerLight = toBool(d?.markerLight);
  const brakeLight  = toBool(d?.brakeLight);
  const turnLight   = toBool(d?.turnLight);

  // ── Inputs – block sensors ──
  const blockPos  = toBool(d?.blockSensorPosition);
  const blockConn = toBool(d?.blockSensorConnector);

  // ── Inputs – numeric (local state for controlled inputs) ──
  const [localMarkerBps, setLocalMarkerBps] = useState('');
  const [localBrakeBps,  setLocalBrakeBps]  = useState('');
  const [localTurnBps,   setLocalTurnBps]   = useState('');
  const [localPosDelay,  setLocalPosDelay]  = useState('');
  const [localConnDelay, setLocalConnDelay] = useState('');

  useEffect(() => {
    if (d) {
      setLocalMarkerBps(String(d.markerBlinksPerSec ?? 0));
      setLocalBrakeBps(String(d.brakeBlinksPerSec ?? 0));
      setLocalTurnBps(String(d.turnBlinksPerSec ?? 0));
      setLocalPosDelay(String(d.sensorPositionDelay ?? 3));
      setLocalConnDelay(String(d.sensorConnectorDelay ?? 4));
    }
  }, [d?.markerBlinksPerSec, d?.brakeBlinksPerSec, d?.turnBlinksPerSec,
      d?.sensorPositionDelay, d?.sensorConnectorDelay]);

  // ── Outputs ──
  const sensorPosition           = toBool(d?.sensorPosition);
  const sensorConnectorConnected = toBool(d?.sensorConnectorConnected);
  const done                     = toBool(d?.done);

  const toggle = (key, cur) => saveSection({ [key]: !cur });

  const sendNum = (key, raw) => {
    const num = parseFloat(raw);
    if (!isNaN(num)) saveSection({ [key]: num });
  };

  return (
    <div>
      <h3>Car Light – vstupy</h3>

      {/* Start / Reset */}
      <div className="gap-2 mb-2">
        <Button className="btn--start" onClick={() => toggle('btnStart', btnStart)}>
          Start ({String(btnStart)})
        </Button>
        <Button className="btn--stop" onClick={() => toggle('btnReset', btnReset)}>
          Reset ({String(btnReset)})
        </Button>
      </div>

      {/* Light toggles */}
      <div className="gap-2 mb-3">
        <Button variant={markerLight ? 'warning' : 'outline-warning'}
                onClick={() => toggle('markerLight', markerLight)}>
          Marker Light ({String(markerLight)})
        </Button>
        <Button variant={brakeLight ? 'danger' : 'outline-danger'}
                onClick={() => toggle('brakeLight', brakeLight)}>
          Brake Light ({String(brakeLight)})
        </Button>
        <Button variant={turnLight ? 'info' : 'outline-info'}
                onClick={() => toggle('turnLight', turnLight)}>
          Turn Light ({String(turnLight)})
        </Button>
      </div>

      {/* Blink config */}
      <h5>Blikání (blinks/s)</h5>
      <Form>
        <Form.Group className="mb-2">
          <Form.Label>Marker blinks/s</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localMarkerBps}
            onChange={e => setLocalMarkerBps(e.target.value)}
            onBlur={() => sendNum('markerBlinksPerSec', localMarkerBps)}
            onKeyDown={e => e.key === 'Enter' && sendNum('markerBlinksPerSec', localMarkerBps)}
          />
        </Form.Group>
        <Form.Group className="mb-2">
          <Form.Label>Brake blinks/s</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localBrakeBps}
            onChange={e => setLocalBrakeBps(e.target.value)}
            onBlur={() => sendNum('brakeBlinksPerSec', localBrakeBps)}
            onKeyDown={e => e.key === 'Enter' && sendNum('brakeBlinksPerSec', localBrakeBps)}
          />
        </Form.Group>
        <Form.Group className="mb-3">
          <Form.Label>Turn blinks/s</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localTurnBps}
            onChange={e => setLocalTurnBps(e.target.value)}
            onBlur={() => sendNum('turnBlinksPerSec', localTurnBps)}
            onKeyDown={e => e.key === 'Enter' && sendNum('turnBlinksPerSec', localTurnBps)}
          />
        </Form.Group>
      </Form>

      {/* Sensor delays */}
      <h5>Simulace čidel</h5>
      <Form>
        <Form.Group className="mb-2">
          <Form.Label>Čidlo poloha – delay (s)</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localPosDelay}
            onChange={e => setLocalPosDelay(e.target.value)}
            onBlur={() => sendNum('sensorPositionDelay', localPosDelay)}
            onKeyDown={e => e.key === 'Enter' && sendNum('sensorPositionDelay', localPosDelay)}
          />
        </Form.Group>
        <Form.Group className="mb-2">
          <Form.Label>Čidlo konektor – delay (s)</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localConnDelay}
            onChange={e => setLocalConnDelay(e.target.value)}
            onBlur={() => sendNum('sensorConnectorDelay', localConnDelay)}
            onKeyDown={e => e.key === 'Enter' && sendNum('sensorConnectorDelay', localConnDelay)}
          />
        </Form.Group>
      </Form>

      {/* Block sensors – simulate faults */}
      <div className="gap-2 mb-3">
        <Button size="sm" variant={blockPos ? 'danger' : 'outline-secondary'}
                onClick={() => toggle('blockSensorPosition', blockPos)}>
          Blokovat čidlo poloha ({String(blockPos)})
        </Button>
        <Button size="sm" variant={blockConn ? 'danger' : 'outline-secondary'}
                onClick={() => toggle('blockSensorConnector', blockConn)}>
          Blokovat čidlo konektor ({String(blockConn)})
        </Button>
      </div>

      <h3>Car Light – výstupy</h3>
      <div className="gap-2 mb-2">
        <div>
          <strong>Sensor Position:</strong>{' '}
          <Badge bg={sensorPosition ? 'success' : 'secondary'}>{String(sensorPosition)}</Badge>
        </div>
        <div>
          <strong>Connector Connected:</strong>{' '}
          <Badge bg={sensorConnectorConnected ? 'success' : 'secondary'}>{String(sensorConnectorConnected)}</Badge>
        </div>
        <div>
          <strong>Done:</strong>{' '}
          <Badge bg={done ? 'success' : 'secondary'}>{String(done)}</Badge>
        </div>
      </div>

      <pre style={{ background: '#f6f8fa', padding: 8, borderRadius: 6, marginTop: 12 }}>
        {JSON.stringify(data, null, 2)}
      </pre>
    </div>
  );
}

/* ── Page ─────────────────────────────────────────────── */
function CarLightPage() {
  const { section: d } = useSectionData('CarLight');

  return (
    <Row className="carlightpage">
      <Col xs={12} lg={8}>
        <div className="mt-3">
          <CarLightCanvas d={d} />
        </div>
      </Col>

      <Col lg={4}>
        <CarLightParamsSidebar />
      </Col>
    </Row>
  );
}

export default CarLightPage;
