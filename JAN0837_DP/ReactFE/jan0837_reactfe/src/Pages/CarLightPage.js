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

function CarLightCanvas({ d }) {
  const errorActive = toBool(d?.error) || toBool(d?.err);
  const marker = toBool(d?.markerLight) && !errorActive;
  const brake  = toBool(d?.brakeLight) && !errorActive;
  const turn   = toBool(d?.turnLight) && !errorActive;
  const done   = toBool(d?.done);

  return (
    <div className="carlight-canvas">
      <div className="carlight-headlamp">
        <div className={`carlight-cell carlight-cell--turn ${turn ? 'on' : ''}`}>
          <i className="bi bi-arrow-left-right" />
          <span>Blinkr</span>
        </div>
        <div className={`carlight-cell carlight-cell--brake ${brake ? 'on' : ''}`}>
          <i className="bi bi-sun-fill" />
          <span>Dálkové</span>
        </div>
        <div className={`carlight-cell carlight-cell--marker ${marker ? 'on' : ''}`}>
          <i className="bi bi-lightbulb-fill" />
          <span>Obrysové</span>
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

function CarLightParamsSidebar() {
  const { interval, setInterval } = useRefresh();
  const { section: d, saveSection, data, error: fetchError, isFetching, refresh } = useSectionData('CarLight');

  // Inputs
  const btnStart    = toBool(d?.btnStart);
  const btnReset    = toBool(d?.btnReset);
  const markerLight = toBool(d?.markerLight);
  const brakeLight  = toBool(d?.brakeLight);
  const turnLight   = toBool(d?.turnLight);
  const errorState = toBool(d?.error) || toBool(d?.err);
  const [localMarkerBps, setLocalMarkerBps] = useState('');
  const [localBrakeBps,  setLocalBrakeBps]  = useState('');
  const [localTurnBps,   setLocalTurnBps]   = useState('');
  const [localMarkerDelta, setLocalMarkerDelta] = useState('');
  const [localBrakeDelta,  setLocalBrakeDelta]  = useState('');
  const [localTurnDelta,   setLocalTurnDelta]   = useState('');

  useEffect(() => {
    if (d) {
      setLocalMarkerBps(String(d.markerBlinksPerSec ?? 0));
      setLocalBrakeBps(String(d.brakeBlinksPerSec ?? 0));
      setLocalTurnBps(String(d.turnBlinksPerSec ?? 0));
      setLocalMarkerDelta(String(d.markerTimeDelta ?? 0));
      setLocalBrakeDelta(String(d.brakeTimeDelta ?? 0));
      setLocalTurnDelta(String(d.turnTimeDelta ?? 0));
    }
  }, [d?.markerBlinksPerSec, d?.brakeBlinksPerSec, d?.turnBlinksPerSec,
      d?.markerTimeDelta, d?.brakeTimeDelta, d?.turnTimeDelta]);

  const connectorConnected = markerLight || brakeLight || turnLight;
  const sensorLight = (markerLight || brakeLight || turnLight) && !errorState;

  // Outputs
  const sensorPosition = sensorLight;
  const sensorConnectorConnected = connectorConnected;
  const done  = toBool(d?.done);

  useEffect(() => {
    const updates = {};
    if (toBool(d?.sensorConnectorConnected) !== connectorConnected) {
      updates.sensorConnectorConnected = connectorConnected;
    }
    if (toBool(d?.sensorPosition) !== sensorLight) {
      updates.sensorPosition = sensorLight;
    }
    if (Object.keys(updates).length > 0) {
      saveSection(updates);
    }
  }, [d?.sensorConnectorConnected, d?.sensorPosition, connectorConnected, sensorLight, saveSection]);

  const toggle = async (key, cur) => {
    try {
      console.log(`toggle: ${key} = ${!cur}`);
      await saveSection({ [key]: !cur });
      console.log(`toggle: ${key} sent successfully`);
    } catch (e) {
      console.error(`toggle ${key} error:`, e);
    }
  };

  const sendNum = async (key, raw) => {
    const num = parseFloat(raw);
    if (!isNaN(num)) {
      try {
        console.log(`sendNum: ${key} = ${num}`);
        await saveSection({ [key]: num });
        console.log(`sendNum: ${key} sent successfully`);
      } catch (e) {
        console.error(`sendNum ${key} error:`, e);
      }
    }
  };

  const sendReset = async () => {
    try {
      await saveSection({
        btnReset: true,
        markerLight: false,
        brakeLight: false,
        turnLight: false,
        error: false,
        err: false,
        sensorConnectorConnected: false,
        sensorPosition: false,
      });
      setTimeout(() => {
        saveSection({ btnReset: false });
      }, 150);
    } catch (e) {
      console.error('sendReset error:', e);
    }
  };

  return (
    <div>
      <h3>Parameters:</h3>

      <div className="gap-2 mb-2">
        <Button className="btn--start" onClick={() => toggle('btnStart', btnStart)}>
          Start ({String(btnStart)})
        </Button>
        <Button className="btn--stop" onClick={sendReset}>
          Reset ({String(btnReset)})
        </Button>
      </div>

      <div className="gap-2 mb-3">
        <Button variant={markerLight ? 'warning' : 'outline-warning'}
                onClick={() => toggle('markerLight', markerLight)}>
          Obrysové ({String(markerLight)})
        </Button>
        <Button variant={brakeLight ? 'primary' : 'outline-primary'}
                onClick={() => toggle('brakeLight', brakeLight)}>
          Dálkové ({String(brakeLight)})
        </Button>
        <Button variant={turnLight ? 'info' : 'outline-info'}
                onClick={() => toggle('turnLight', turnLight)}>
          Blinkr ({String(turnLight)})
        </Button>
      </div>

      <Form.Group className="mb-3">
        <Form.Check
          type="checkbox"
          id="carlight-error"
          label={`Error (${String(errorState)})`}
          checked={errorState}
          onChange={e => saveSection({ error: e.target.checked, err: e.target.checked })}
        />
      </Form.Group>

      <Form>
        <Form.Group className="mb-2">
          <Form.Label>Obrysové – frekvence (Hz)</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localMarkerBps}
            onChange={e => setLocalMarkerBps(e.target.value)}
            onBlur={() => sendNum('markerBlinksPerSec', localMarkerBps)}
            onKeyDown={e => e.key === 'Enter' && sendNum('markerBlinksPerSec', localMarkerBps)}
          />
        </Form.Group>
        <Form.Group className="mb-2">
          <Form.Label>Dálkové – frekvence (Hz)</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localBrakeBps}
            onChange={e => setLocalBrakeBps(e.target.value)}
            onBlur={() => sendNum('brakeBlinksPerSec', localBrakeBps)}
            onKeyDown={e => e.key === 'Enter' && sendNum('brakeBlinksPerSec', localBrakeBps)}
          />
        </Form.Group>
        <Form.Group className="mb-3">
          <Form.Label>Blinkr – frekvence (Hz)</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localTurnBps}
            onChange={e => setLocalTurnBps(e.target.value)}
            onBlur={() => sendNum('turnBlinksPerSec', localTurnBps)}
            onKeyDown={e => e.key === 'Enter' && sendNum('turnBlinksPerSec', localTurnBps)}
          />
        </Form.Group>
      </Form>

      <Form>
        <Form.Group className="mb-2">
          <Form.Label>Obrysové – časová delta (s)</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localMarkerDelta}
            onChange={e => setLocalMarkerDelta(e.target.value)}
            onBlur={() => sendNum('markerTimeDelta', localMarkerDelta)}
            onKeyDown={e => e.key === 'Enter' && sendNum('markerTimeDelta', localMarkerDelta)}
          />
        </Form.Group>
        <Form.Group className="mb-2">
          <Form.Label>Dálkové – časová delta (s)</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localBrakeDelta}
            onChange={e => setLocalBrakeDelta(e.target.value)}
            onBlur={() => sendNum('brakeTimeDelta', localBrakeDelta)}
            onKeyDown={e => e.key === 'Enter' && sendNum('brakeTimeDelta', localBrakeDelta)}
          />
        </Form.Group>
        <Form.Group className="mb-3">
          <Form.Label>Blinkr – časová delta (s)</Form.Label>
          <Form.Control type="number" step="any" min="0"
            value={localTurnDelta}
            onChange={e => setLocalTurnDelta(e.target.value)}
            onBlur={() => sendNum('turnTimeDelta', localTurnDelta)}
            onKeyDown={e => e.key === 'Enter' && sendNum('turnTimeDelta', localTurnDelta)}
          />
        </Form.Group>
      </Form>

      <div className="gap-2 mb-2">
        <div>
          <strong>Čidlo světla:</strong>{' '}
          <Badge bg={sensorPosition ? 'success' : 'secondary'}>{String(sensorPosition)}</Badge>
        </div>
        <div>
          <strong>Čidlo konektoru:</strong>{' '}
          <Badge bg={sensorConnectorConnected ? 'success' : 'secondary'}>{String(sensorConnectorConnected)}</Badge>
        </div>
        <div>
          <strong>Error:</strong>{' '}
          <Badge bg={errorState ? 'danger' : 'secondary'}>{String(errorState)}</Badge>
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
