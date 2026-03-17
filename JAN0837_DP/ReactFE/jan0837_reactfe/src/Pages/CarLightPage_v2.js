import React, { useEffect, useMemo, useState } from 'react';
import { Row, Col, Form, Badge, Button } from 'react-bootstrap';

import '../App.css';
import './CarLightPage.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import { useSectionData } from '../Communication/DataProvider.js';

const toBool = (v) => {
  if (typeof v === 'boolean') return v;
  const s = String(v ?? '').trim().toLowerCase();
  return s === 'true' || s === '1' || s === 'on';
};

const toBoolString = (v) => (v ? 'true' : 'false');

function CarLightCanvas({ d }) {
  const connectorConnected = toBool(d?.sensorConnectorConnected);
  const sensorLight = toBool(d?.sensorLight);
  const errorActive = toBool(d?.error);
  const marker = connectorConnected && toBool(d?.lowBeamLight) && !errorActive;
  const brake = connectorConnected && toBool(d?.highBeamLight) && !errorActive;
  const turn = connectorConnected && toBool(d?.turnLight) && !errorActive;
  const source = connectorConnected
    ? '/images/carlight/carlight_connected.png'
    : '/images/carlight/carlight_disconnected.png';

  return (
    <div className="carlight-canvas">
      <div className="carlight-image-wrap">
        <img className="carlight-image" src={source} alt="Car light" />
        
        <span className={`carlight-glow carlight-glow--turn ${turn ? 'on' : ''}`} />
        <span className={`carlight-glow carlight-glow--high ${brake ? 'on' : ''}`} />
        <span className={`carlight-glow carlight-glow--low ${marker ? 'on' : ''}`} />

        <div className="carlight-overlay-status">
          <Badge bg={connectorConnected ? 'success' : 'danger'}>
            {connectorConnected ? '✅' : '❌'} Connector
          </Badge>
          {' '}
          <Badge bg={sensorLight ? 'success' : 'danger'}>
            {sensorLight ? '✅' : '❌'} Sensor light
          </Badge>
          {errorActive && (
            <>
              {' '}
              <Badge bg="danger">
                ❌ Error
              </Badge>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

function CarLightParamsSidebar() {
  const { section: d, saveSection, data } = useSectionData('CarLight');

  const btnReset = toBool(d?.btnReset);
  const lowBeamLight = toBool(d?.lowBeamLight);
  const highBeamLight = toBool(d?.highBeamLight);
  const turnLight   = toBool(d?.turnLight);
  //const sensorLight = toBool(d?.sensorLight);
  const sensorConnectorConnected = toBool(d?.sensorConnectorConnected);
  const errorState = toBool(d?.error);
  const [cfg, setCfg] = useState({
    aBps: '1',
    bBps: '1',
    cBps: '1',
  });

  const numCfg = useMemo(() => {
    const toNum = (v, fallback = 0) => {
      const parsed = parseFloat(String(v ?? '').replace(',', '.'));
      return Number.isFinite(parsed) && parsed >= 0 ? parsed : fallback;
    };
    const deltaFromBps = (bps) => (bps > 0 ? 1 / bps : 0);
    return {
      A: { bps: toNum(cfg.aBps, 1), delta: deltaFromBps(toNum(cfg.aBps, 1)) },
      B: { bps: toNum(cfg.bBps, 1), delta: deltaFromBps(toNum(cfg.bBps, 1)) },
      C: { bps: toNum(cfg.cBps, 1), delta: deltaFromBps(toNum(cfg.cBps, 1)) },
    };
  }, [cfg]);

  const totalSumTime = useMemo(
    () => numCfg.A.delta + numCfg.B.delta + numCfg.C.delta,
    [numCfg]
  );

  const normalizeNonNegative = (value) => {
    const normalized = String(value).replace(',', '.');
    if (normalized === '' || normalized === '.') return normalized;
    const parsed = parseFloat(normalized);
    if (!Number.isFinite(parsed)) return '';
    return String(Math.max(0, parsed));
  };

  const toggleReset = async () => {
    try {
      await saveSection({ btnReset: !btnReset });
    } catch (e) {
      console.error('toggleReset error:', e);
    }
  };

  const toggleError = async () => {
    try {
      await saveSection({ error: toBoolString(!errorState) });
    } catch (e) {
      console.error('toggleError error:', e);
    }
  };

  return (
    <div>
      <h3>Parameters:</h3>

      <div className="gap-2 mb-3">
        <Button variant="outline-danger" onClick={toggleReset}>
          Reset ({String(btnReset)})
        </Button>
      </div>

      <Form>
        <Row className="g-2 mb-2">
          <Col xs={6}><strong>Blinks per second</strong></Col>
          <Col xs={6}><strong>Delta seconds</strong></Col>
        </Row>

        <div className="mb-2"><strong>Low Beam</strong></div>
        <Row className="g-2 mb-3">
          <Col xs={6}>
            <Form.Control
              type="number"
              step="any"
              min="0"
              value={cfg.aBps}
              onChange={e => setCfg(prev => ({ ...prev, aBps: normalizeNonNegative(e.target.value) }))}
              placeholder="A - blik/s"
            />
          </Col>
          <Col xs={6}>
            <Form.Control
              type="text"
              readOnly
              value={`${numCfg.A.delta.toFixed(3)} s`}
              placeholder="A - delta (s)"
            />
          </Col>
        </Row>

        <div className="mb-2"><strong>High Beam</strong></div>
        <Row className="g-2 mb-3">
          <Col xs={6}>
            <Form.Control
              type="number"
              step="any"
              min="0"
              value={cfg.bBps}
              onChange={e => setCfg(prev => ({ ...prev, bBps: normalizeNonNegative(e.target.value) }))}
              placeholder="B - blik/s"
            />
          </Col>
          <Col xs={6}>
            <Form.Control
              type="text"
              readOnly
              value={`${numCfg.B.delta.toFixed(3)} s`}
              placeholder="B - delta (s)"
            />
          </Col>
        </Row>

        <div className="mb-2"><strong>Turn</strong></div>
        <Row className="g-2 mb-3">
          <Col xs={6}>
            <Form.Control
              type="number"
              step="any"
              min="0"
              value={cfg.cBps}
              onChange={e => setCfg(prev => ({ ...prev, cBps: normalizeNonNegative(e.target.value) }))}
              placeholder="C - blik/s"
            />
          </Col>
          <Col xs={6}>
            <Form.Control
              type="text"
              readOnly
              value={`${numCfg.C.delta.toFixed(3)} s`}
              placeholder="C - delta (s)"
            />
          </Col>
        </Row>
      </Form>

      <div className="mb-3">
        <Row>
          <Col xs={6}>Total: </Col>
          <Col xs={6}>{totalSumTime.toFixed(5)} s</Col>
        </Row>
      </div>

      <Form.Group className="mb-3">
        <Form.Check
          type="checkbox"
          id="carlight-connector"
          label={`Connector connected (${String(sensorConnectorConnected)})`}
          checked={sensorConnectorConnected}
          onChange={e => saveSection({ sensorConnectorConnected: e.target.checked })}
        />
      </Form.Group>

      <Form.Group className="mb-3">
        <Form.Check
          type="checkbox"
          id="carlight-sensor-light"
          label={`Sensor light (${String(toBool(d?.sensorLight))})`}
          checked={toBool(d?.sensorLight)}
          onChange={e => saveSection({ sensorLight: toBoolString(e.target.checked) })}
        />
      </Form.Group>

      <Form.Group className="mb-3">
        <Form.Check
          type="checkbox"
          id="carlight-manual-error"
          label={`Manual error (${String(errorState)})`}
          checked={errorState}
          onChange={toggleError}
        />
      </Form.Group>

      <div className="gap-2 mb-2">
        <div className="d-flex align-items-center gap-2 mb-1">
          <strong>Low Beam (A):</strong>
          <Badge bg={lowBeamLight ? 'success' : 'secondary'}>{String(lowBeamLight)}</Badge>
          <Button
            size="sm"
            variant={lowBeamLight ? 'success' : 'outline-secondary'}
            onClick={() => saveSection({ lowBeamLight: toBoolString(!lowBeamLight) })}
          >
            {lowBeamLight ? 'ON' : 'OFF'}
          </Button>
        </div>
        <div className="d-flex align-items-center gap-2 mb-1">
          <strong>High Beam (B):</strong>
          <Badge bg={highBeamLight ? 'success' : 'secondary'}>{String(highBeamLight)}</Badge>
          <Button
            size="sm"
            variant={highBeamLight ? 'success' : 'outline-secondary'}
            onClick={() => saveSection({ highBeamLight: toBoolString(!highBeamLight) })}
          >
            {highBeamLight ? 'ON' : 'OFF'}
          </Button>
        </div>
        <div className="d-flex align-items-center gap-2 mb-1">
          <strong>Turn (C):</strong>
          <Badge bg={turnLight ? 'success' : 'secondary'}>{String(turnLight)}</Badge>
          <Button
            size="sm"
            variant={turnLight ? 'success' : 'outline-secondary'}
            onClick={() => saveSection({ turnLight: toBoolString(!turnLight) })}
          >
            {turnLight ? 'ON' : 'OFF'}
          </Button>
        </div>
        <div>
          <strong>Result:</strong>{' '}
          <Badge bg={toBool(d?.result) ? 'success' : 'danger'}>{String(toBool(d?.result))}</Badge>
        </div>
      </div>

      <div className="gap-2 mb-2">
        <div>
          <strong>Sensor light:</strong>{' '}
          <Badge bg={toBool(d?.sensorLight) ? 'success' : 'secondary'}>{String(toBool(d?.sensorLight))}</Badge>
        </div>
        <div>
          <strong>Sensor connector:</strong>{' '}
          <Badge bg={sensorConnectorConnected ? 'success' : 'secondary'}>{String(sensorConnectorConnected)}</Badge>
        </div>
        <div>
          <strong>Error:</strong>{' '}
          <Badge bg={errorState ? 'danger' : 'secondary'}>{String(errorState)}</Badge>
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
