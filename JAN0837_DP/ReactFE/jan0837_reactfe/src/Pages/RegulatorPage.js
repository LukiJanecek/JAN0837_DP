import React, { useState, useEffect } from 'react';
import { Row, Col, Button, Form, Badge } from 'react-bootstrap';

import '../App.css';
import './RegulatorPage.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import { useRefresh } from '../Communication/RefreshContext.js';
import { useData, useSectionData } from '../Communication/DataProvider.js';

/*
 * ── RegulatorData variable map ───────────────────
 *
 * INPUTS  (FE → PLC):
 *   switchstate   Bool – spínač (on/off)
 *   R             Real – odpor (Ω)
 *   C             Real – kapacita (F)
 *   U             Real – napětí (V)
 *   Td            Real – časová konstanta (s)
 *
 * OUTPUTS (PLC → FE):
 *   Uc            Real – napětí na kondenzátoru (V)
 */

const toBool = (v) => {
  if (typeof v === 'boolean') return v;
  const s = String(v ?? '').trim().toLowerCase();
  return s === 'true' || s === '1' || s === 'on';
};

/* ── Overlay labels for the RC circuit diagram ────────── */
function RegulatorCanvas({ background, d }) {
  const Uc = Number(d?.Uc ?? 0);

  return (
    <div className="regulator" style={{ backgroundImage: `url(${background})` }}>
      {/* Descriptive labels positioned over the schematic */}
      <span className="reg-label reg-label--R">R</span>
      <span className="reg-label reg-label--C">C</span>
      <span className="reg-label reg-label--U">U</span>
      <span className="reg-label reg-label--Uc">
        U<sub>c</sub> = {Uc.toFixed(2)}
      </span>
    </div>
  );
}

function RegulatorParamsSidebar() {
  const { interval, setInterval } = useRefresh();
  const { section: d, saveSection, data, error, isFetching, refresh } = useSectionData('RegulatorData');

  const switchstate = toBool(d?.switchstate);
  const [localR,  setLocalR]  = useState('');
  const [localC,  setLocalC]  = useState('');
  const [localU,  setLocalU]  = useState('');
  const [localTd, setLocalTd] = useState('');

  useEffect(() => {
    if (d) {
      setLocalR(String(d.R  ?? 0));
      setLocalC(String(d.C  ?? 0));
      setLocalU(String(d.U  ?? 0));
      setLocalTd(String(d.Td ?? 0));
    }
  }, [d?.R, d?.C, d?.U, d?.Td]);

  const Uc = Number(d?.Uc ?? 0);

  const toggleSwitch = () => saveSection({ switchstate: !switchstate });

  const sendField = (key, raw) => {
    const num = parseFloat(raw);
    if (!isNaN(num)) saveSection({ [key]: num });
  };

  return (
    <div>
      <h3>Regulator – inputs</h3>

      <div className="gap-2 mb-3">
        <Button
          className={switchstate ? 'btn--stop' : 'btn--start'}
          onClick={toggleSwitch}
        >
          Spínač: {switchstate ? 'ON' : 'OFF'}
        </Button>
      </div>

      <Form>
        <Form.Group className="mb-2">
          <Form.Label>R (Ω)</Form.Label>
          <Form.Control
            type="number"
            step="any"
            value={localR}
            onChange={e => setLocalR(e.target.value)}
            onBlur={() => sendField('R', localR)}
            onKeyDown={e => e.key === 'Enter' && sendField('R', localR)}
          />
        </Form.Group>

        <Form.Group className="mb-2">
          <Form.Label>C (F)</Form.Label>
          <Form.Control
            type="number"
            step="any"
            value={localC}
            onChange={e => setLocalC(e.target.value)}
            onBlur={() => sendField('C', localC)}
            onKeyDown={e => e.key === 'Enter' && sendField('C', localC)}
          />
        </Form.Group>

        <Form.Group className="mb-2">
          <Form.Label>U (V)</Form.Label>
          <Form.Control
            type="number"
            step="any"
            value={localU}
            onChange={e => setLocalU(e.target.value)}
            onBlur={() => sendField('U', localU)}
            onKeyDown={e => e.key === 'Enter' && sendField('U', localU)}
          />
        </Form.Group>

        <Form.Group className="mb-3">
          <Form.Label>T<sub>d</sub> (s)</Form.Label>
          <Form.Control
            type="number"
            step="any"
            value={localTd}
            onChange={e => setLocalTd(e.target.value)}
            onBlur={() => sendField('Td', localTd)}
            onKeyDown={e => e.key === 'Enter' && sendField('Td', localTd)}
          />
        </Form.Group>
      </Form>

      <h3>Regulator – outputs</h3>
      <div className="reg-output mb-3">
        <Badge bg="info" className="fs-6">
          U<sub>c</sub> = {Uc.toFixed(2)} V
        </Badge>
      </div>

      <pre style={{ background: '#f6f8fa', padding: 8, borderRadius: 6, marginTop: 12 }}>
        {JSON.stringify(data, null, 2)}
      </pre>
    </div>
  );
}

function RegulatorPage() {
  const { section: d } = useSectionData('RegulatorData');

  const background = '/images/regulator_RC.PNG';

  useEffect(() => {
    const img = new Image();
    img.src = '/images/regulator_RC.PNG';
  }, []);

  return (
    <Row className="regulatorpage">
      <Col xs={12} lg={8}>
        <div className="mt-3">
          <RegulatorCanvas background={background} d={d} />
        </div>
      </Col>

      <Col lg={4}>
        <RegulatorParamsSidebar />
      </Col>
    </Row>
  );
}

export default RegulatorPage;
