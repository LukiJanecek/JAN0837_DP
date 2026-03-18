import React, { useEffect, useRef, useState } from 'react';
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

const toNum = (v, fallback = 0) => {
  const parsed = parseFloat(String(v ?? '').replace(',', '.'));
  return Number.isFinite(parsed) && parsed >= 0 ? parsed : fallback;
};

const normalizeNonNegative = (value) => {
  const normalized = String(value).replace(',', '.');
  if (normalized === '' || normalized === '.') return normalized;
  const parsed = parseFloat(normalized);
  if (!Number.isFinite(parsed)) return '';
  return String(Math.max(0, parsed));
};

const TOLERANCE = 0.15; // 150 ms tolerance

function CarLightCanvas({ d }) {
  const connectorConnected = toBool(d?.sensorConnectorConnected);
  const sensorLight = toBool(d?.sensorLight);
  const errorActive = toBool(d?.error);
  const lowBeam = connectorConnected && toBool(d?.lowBeamLight) && !errorActive;
  const highBeam = connectorConnected && toBool(d?.highBeamLight) && !errorActive;
  const turn = connectorConnected && toBool(d?.turnLight) && !errorActive;
  const source = connectorConnected
    ? '/images/carlight/carlight_connected.png'
    : '/images/carlight/carlight_disconnected.png';

  return (
    <div className="carlight-canvas">
      <div className="carlight-image-wrap">
        <img className="carlight-image" src={source} alt="Car light" />
        
        <span className={`carlight-glow carlight-glow--turn ${turn ? 'on' : ''}`} />
        <span className={`carlight-glow carlight-glow--high ${highBeam ? 'on' : ''}`} />
        <span className={`carlight-glow carlight-glow--low ${lowBeam ? 'on' : ''}`} />

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
  const turnLight = toBool(d?.turnLight);
  const sensorConnectorConnected = toBool(d?.sensorConnectorConnected);
  const errorState = toBool(d?.error);

  const lowStart = useRef(null);
  const highStart = useRef(null);
  const turnStart = useRef(null);
  const [lowDuration, setLowDuration] = useState(null);
  const [highDuration, setHighDuration] = useState(null);
  const [turnDuration, setTurnDuration] = useState(null);

  const [turnCount, setTurnCount] = useState(0);
  const turnBlinkState = useRef(false);

  const getTestState = () => {
    if (!lowBeamLight && !highBeamLight && !turnLight) return 'Idle';
    const lights = [lowBeamLight, highBeamLight, turnLight].filter(Boolean).length;
    if (lights === 1) {
      if (lowBeamLight) return 'Testing Low Beam';
      if (highBeamLight) return 'Testing High Beam';
      if (turnLight) return 'Testing Turn';
    }
    if (lights > 1) return 'Testing';
    return 'Waiting';
  };
  const testState = getTestState();

  const [lowBeamDuration, setLowBeamDuration] = useState('1');
  const [highBeamDuration, setHighBeamDuration] = useState('1');
  const [turnBps, setTurnBps] = useState('1');

  const expectedLowDuration = toNum(lowBeamDuration, 1);
  const expectedHighDuration = toNum(highBeamDuration, 1);
  const expectedTurnBps = toNum(turnBps, 1);
  
  const [measuredLowDuration, setMeasuredLowDuration] = useState(null);
  const [measuredHighDuration, setMeasuredHighDuration] = useState(null);
  const [measuredTurnBps, setMeasuredTurnBps] = useState(null);

  const measurePhase = useRef('idle');
  const phaseStartTime = useRef(null);
  useEffect(() => {
    const now = Date.now();

    if (btnReset) {
      setLowDuration(null);
      setHighDuration(null);
      setTurnDuration(null);
      lowStart.current = null;
      highStart.current = null;
      turnStart.current = null;
      setTurnCount(0);
      turnBlinkState.current = false;
      return;
    }

    const lightsOn = [lowBeamLight, highBeamLight, turnLight].filter(Boolean).length;
    const isAllPhase = lightsOn > 1;

    // LOW BEAM light 
    if (lowBeamLight && !isAllPhase) {
      if (!lowStart.current) lowStart.current = now;
    } else {
      if (lowStart.current) {
        setLowDuration(((now - lowStart.current) / 1000).toFixed(3));
        lowStart.current = null;
      }
    }

    // HIGH BEAM light 
    if (highBeamLight && !isAllPhase) {
      if (!highStart.current) highStart.current = now;
    } else {
      if (highStart.current) {
        setHighDuration(((now - highStart.current) / 1000).toFixed(3));
        highStart.current = null;
      }
    }

    // TURN light measures blinks only in Testing Turn phase
    const testState = (() => {
      if (!lowBeamLight && !highBeamLight && !turnLight) return 'Idle';
      const lights = [lowBeamLight, highBeamLight, turnLight].filter(Boolean).length;
      if (lights === 1) {
        if (lowBeamLight) return 'Testing Low Beam';
        if (highBeamLight) return 'Testing High Beam';
        if (turnLight) return 'Testing Turn';
      }
      if (lights > 1) return 'Testing';
      return 'Waiting';
    })();

    if (testState === 'Testing Turn') {
      if (!turnStart.current) turnStart.current = now;
      if (turnLight !== turnBlinkState.current) {
        if (turnLight) {
          setTurnCount(c => c + 1);
        }
        turnBlinkState.current = turnLight;
      }
    } else {
      if (turnStart.current) {
        setTurnDuration(((now - turnStart.current) / 1000).toFixed(3));
        turnStart.current = null;
      }
      turnBlinkState.current = turnLight;
    }
  }, [lowBeamLight, highBeamLight, turnLight, btnReset]);

  // Result evaluation
  const withinTolerance = (measured, expected) => {
    if (measured === null || expected <= 0) return false;
    return Math.abs(measured - expected) <= TOLERANCE;
  };

  const lowOk = withinTolerance(Number(lowDuration), expectedLowDuration);
  const highOk = withinTolerance(Number(highDuration), expectedHighDuration);

  const expectedTurnCount = turnDuration !== null ? Math.round(expectedTurnBps * Number(turnDuration)) : null;
  const turnOk = turnDuration !== null && expectedTurnCount !== null && Math.abs(turnCount - expectedTurnCount) <= 1;

  const resultOk = lowOk && highOk && turnOk;

  useEffect(() => {
    saveSection({ result: resultOk ? 'true' : 'false' });
  }, [resultOk, saveSection]);

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
        <div className="mb-2"><strong>Low Beam — duration (s)</strong></div>
        <Row className="g-2 mb-3">
          <Col>
            <Form.Control
              type="number"
              step="any"
              min="0"
              value={lowBeamDuration}
              onChange={e => setLowBeamDuration(normalizeNonNegative(e.target.value))}
              placeholder="Expected duration (s)"
            />
          </Col>
        </Row>

        <div className="mb-2"><strong>High Beam — duration (s)</strong></div>
        <Row className="g-2 mb-3">
          <Col>
            <Form.Control
              type="number"
              step="any"
              min="0"
              value={highBeamDuration}
              onChange={e => setHighBeamDuration(normalizeNonNegative(e.target.value))}
              placeholder="Expected duration (s)"
            />
          </Col>
        </Row>

        <div className="mb-2"><strong>Turn — blinks per second</strong></div>
        <Row className="g-2 mb-3">
          <Col>
            <Form.Control
              type="number"
              step="any"
              min="0"
              value={turnBps}
              onChange={e => setTurnBps(normalizeNonNegative(e.target.value))}
              placeholder="Expected blinks/s"
            />
          </Col>
        </Row>
      </Form>

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

      {/* Light outputs (read-only from PLC) */}
      <div className="gap-2 mb-2">
        <div className="d-flex align-items-center gap-2 mb-1">
          <strong>Low Beam:</strong>
          <Badge bg={lowBeamLight ? 'success' : 'secondary'}>{lowBeamLight ? 'ON' : 'OFF'}</Badge>
          {lowDuration !== null ? (
            <Badge bg={Math.abs(lowDuration - expectedLowDuration) <= TOLERANCE ? 'success' : 'danger'}>
              {lowDuration}s / {expectedLowDuration}s
              {' '}
              <span style={{ fontSize: '0.9em', marginLeft: 4 }}>
                ({(lowDuration - expectedLowDuration >= 0 ? '+' : '')}{(lowDuration - expectedLowDuration).toFixed(3)}s)
              </span>
            </Badge>
          ) : (
            <Badge bg="secondary">--- / {expectedLowDuration}s</Badge>
          )}
        </div>
        <div className="d-flex align-items-center gap-2 mb-1">
          <strong>High Beam:</strong>
          <Badge bg={highBeamLight ? 'success' : 'secondary'}>{highBeamLight ? 'ON' : 'OFF'}</Badge>
          {highDuration !== null ? (
            <Badge bg={Math.abs(highDuration - expectedHighDuration) <= TOLERANCE ? 'success' : 'danger'}>
              {highDuration}s / {expectedHighDuration}s
              {' '}
              <span style={{ fontSize: '0.9em', marginLeft: 4 }}>
                ({(highDuration - expectedHighDuration >= 0 ? '+' : '')}{(highDuration - expectedHighDuration).toFixed(3)}s)
              </span>
            </Badge>
          ) : (
            <Badge bg="secondary">--- / {expectedHighDuration}s</Badge>
          )}
        </div>
        <div className="d-flex align-items-center gap-2 mb-1">
          <strong>Turn:</strong>
          <Badge bg={turnLight ? 'success' : 'secondary'}>{turnLight ? 'ON' : 'OFF'}</Badge>
          {/* Turn badge: šedá pokud test není ukončen, zelená při splnění, červená při nesplnění */}
          <Badge bg={turnDuration === null ? 'secondary' : (turnOk ? 'success' : 'danger')}>
            {turnCount} blinks{expectedTurnCount !== null ? ` / ${expectedTurnCount}` : ''}
            {' '}
            {turnDuration !== null && expectedTurnCount !== null && (
              <span style={{ fontSize: '0.9em', marginLeft: 4 }}>
                ({(turnCount - expectedTurnCount >= 0 ? '+' : '')}{(turnCount - expectedTurnCount)})
              </span>
            )}
          </Badge>
        </div>
        <div className="d-flex align-items-center gap-2 mb-1">
          <strong>Test state:</strong>
          <Badge bg="info">{testState}</Badge>
        </div>
        <div>
          <strong>Result:</strong>{' '}
          <Badge bg={resultOk ? 'success' : 'danger'}>
            {resultOk ? '✅ PASS' : '❌ FAIL'}
          </Badge>
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
