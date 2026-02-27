import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Row, Col, Button, Form, Badge } from 'react-bootstrap';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

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

/* ── RC circuit diagram (image only, no overlays) ────── */
function RegulatorCanvas({ background }) {
  return (
    <div className="regulator-img-wrap">
      <img src={background} alt="RC circuit" className="regulator-img" />
    </div>
  );
}

/* ── Uc chart with rolling history ───────────────────── */
const MAX_POINTS = 200;

function UcChart({ Uc }) {
  const [history, setHistory] = useState([]);
  const startRef = useRef(Date.now());

  useEffect(() => {
    setHistory(prev => {
      const t = ((Date.now() - startRef.current) / 1000).toFixed(1);
      const next = [...prev, { t: Number(t), Uc: Number(Uc) }];
      return next.length > MAX_POINTS ? next.slice(next.length - MAX_POINTS) : next;
    });
  }, [Uc]);

  const clearHistory = () => {
    setHistory([]);
    startRef.current = Date.now();
  };

  return (
    <div className="uc-chart-wrap">
      <div className="d-flex align-items-center justify-content-between mb-2">
        <h5 className="mb-0">U<sub>c</sub> v čase</h5>
        <Button size="sm" variant="outline-secondary" onClick={clearHistory}>Reset graf</Button>
      </div>
      <ResponsiveContainer width="100%" height={280}>
        <LineChart data={history}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="t"
            label={{ value: 't (s)', position: 'insideBottomRight', offset: -5 }}
            type="number"
            domain={['dataMin', 'dataMax']}
          />
          <YAxis
            label={{ value: 'Uc (V)', angle: -90, position: 'insideLeft' }}
          />
          <Tooltip formatter={(v) => [Number(v).toFixed(3) + ' V', 'Uc']} />
          <Line type="monotone" dataKey="Uc" stroke="#0d6efd" dot={false} isAnimationActive={false} />
        </LineChart>
      </ResponsiveContainer>
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
          <Form.Label>C (uF)</Form.Label>
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
          <Form.Label>U<sub>in</sub> (V)</Form.Label>
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
  const Uc = Number(d?.Uc ?? 0);

  const background = '/images/regulator/regulator_RC.PNG';

  return (
    <Row className="regulatorpage">
      <Col xs={12} lg={8}>
        <div className="mt-3">
          <RegulatorCanvas background={background} />
          <UcChart Uc={Uc} />
        </div>
      </Col>

      <Col lg={4}>
        <RegulatorParamsSidebar />
      </Col>
    </Row>
  );
}

export default RegulatorPage;
