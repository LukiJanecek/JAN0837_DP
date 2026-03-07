import React, { useState, useEffect, useRef, useCallback } from 'react';
import { Row, Col, Button, Form, Badge } from 'react-bootstrap';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';

import '../App.css';
import './RegulatorPage.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import { useRefresh } from '../Communication/RefreshContext.js';
import { useData, useSectionData } from '../Communication/DataProvider.js';

const toBool = (v) => {
  if (typeof v === 'boolean') return v;
  const s = String(v ?? '').trim().toLowerCase();
  return s === 'true' || s === '1' || s === 'on';
};

function RegulatorCanvas({ background }) {
  return (
    <div className="regulator-img-wrap">
      <img src={background} alt="RC circuit" className="regulator-img" />
    </div>
  );
}

const MAX_POINTS = 200;

function UcChart({ Uc1, Uc2, order }) {
  const [history, setHistory] = useState([]);
  const startRef = useRef(Date.now());
  const isSecondOrder = order === 2;

  useEffect(() => {
    setHistory(prev => {
      const t = ((Date.now() - startRef.current) / 1000).toFixed(1);
      const point = { 
        t: Number(t), 
        Uc1: Number(Uc1)
      };
      if (isSecondOrder) {
        point.Uc2 = Number(Uc2);
      }
      const next = [...prev, point];
      return next.length > MAX_POINTS ? next.slice(next.length - MAX_POINTS) : next;
    });
  }, [Uc1, Uc2, isSecondOrder]);

  const clearHistory = () => {
    setHistory([]);
    startRef.current = Date.now();
  };

  return (
    <div className="uc-chart-wrap">
      <div className="d-flex align-items-center justify-content-between mb-2">
        <h5 className="mb-0">{isSecondOrder ? 'Uc₁, Uc₂' : 'Uc'} in time</h5>
        <Button size="sm" variant="outline-secondary" onClick={clearHistory}>Reset chart/graph</Button>
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
            label={{ value: 'U (V)', angle: -90, position: 'insideLeft' }}
          />
          <Tooltip formatter={(v, name) => [Number(v).toFixed(3) + ' V', name]} />
          <Legend />
          <Line type="monotone" dataKey="Uc1" stroke="#0d6efd" dot={false} isAnimationActive={false} name={isSecondOrder ? "Uc₁" : "Uc"} />
          {isSecondOrder && (
            <Line type="monotone" dataKey="Uc2" stroke="#6c757d" dot={false} isAnimationActive={false} name="Uc₂" />
          )}
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}

function RegulatorParamsSidebar() {
  const { interval, setInterval } = useRefresh();
  const { section: d, saveSection, data, error, isFetching, refresh } = useSectionData('RegulatorData');

  const btnReset = toBool(d?.btnReset);
  const switchstate = toBool(d?.switchstate);
  const order = Number(d?.order ?? 1); // 1 = 1. řád, 2 = 2. řád
  const isSecondOrder = order === 2;

  const [localR1,  setLocalR1]  = useState('');
  const [localR2,  setLocalR2]  = useState('');
  const [localC1,  setLocalC1]  = useState('');
  const [localC2,  setLocalC2]  = useState('');
  const [localTd,  setLocalTd]  = useState('');
  const [localTs,  setLocalTs]  = useState('');
  
  // Track which field is currently being edited to prevent server overwrite
  const editingRef = useRef(null);

  useEffect(() => {
    if (d) {
      if (editingRef.current !== 'R1') setLocalR1(String(d.R1  ?? 0));
      if (editingRef.current !== 'R2') setLocalR2(String(d.R2  ?? 0));
      if (editingRef.current !== 'C1') setLocalC1(String(d.C1  ?? 0));
      if (editingRef.current !== 'C2') setLocalC2(String(d.C2  ?? 0));
      if (editingRef.current !== 'Td') setLocalTd(String(d.Td ?? 0));
      if (editingRef.current !== 'Ts') setLocalTs(String(d.Ts ?? 0));
    }
  }, [d?.R1, d?.R2, d?.C1, d?.C2, d?.Td, d?.Ts]);

  const Uin = Number(d?.Uin ?? 0);
  const Uc1 = Number(d?.Uc1 ?? 0);
  const Uc2 = Number(d?.Uc2 ?? 0);
  const toggleSwitch = async () => {
    try {
      await saveSection({ switchstate: !switchstate });
    } catch (e) {
      console.error('toggleSwitch error:', e);
    }
  };

  const toggleReset = async () => {
    try {
      await saveSection({ btnReset: !btnReset });
    } catch (e) {
      console.error('toggleReset error:', e);
    }
  };

  const sendField = async (key, raw, isInt = false) => {
    const num = isInt ? parseInt(raw, 10) : parseFloat(raw);
    if (!isNaN(num)) {
      try {
        console.log(`sendField: ${key} = ${num}`);
        await saveSection({ [key]: num });
        console.log(`sendField: ${key} sent successfully`);
      } catch (e) {
        console.error(`sendField ${key} error:`, e);
      }
    }
    // Clear editing lock after send completes
    editingRef.current = null;
  };

  const handleFocus = (key) => {
    editingRef.current = key;
  };

  const handleBlur = (key, value, isInt = false) => {
    sendField(key, value, isInt);
  };

  return (
    <div>
      <h3>Parameters:</h3>

      <div className="d-flex gap-2 mb-3">
        <Button variant="outline-danger" onClick={toggleReset}>
          Reset ({String(btnReset)})
        </Button>
        <Button
          className={switchstate ? 'btn--stop' : 'btn--start'}
          onClick={toggleSwitch}
        >
          Switch: {switchstate ? 'ON' : 'OFF'}
        </Button>
        <Button
          variant="outline-primary"
          onClick={() => saveSection({ order: isSecondOrder ? 1 : 2 })}
        >
          Order: {isSecondOrder ? '2nd (RC-RC)' : '1st (RC)'}
        </Button>
      </div>

      <Form>
        <Form.Group className="mb-2">
          <Form.Label>{isSecondOrder ? 'R₁' : 'R'} (Ω)</Form.Label>
          <Form.Control
            type="number"
            step="any"
            value={localR1}
            onChange={e => setLocalR1(e.target.value)}
            onFocus={() => handleFocus('R1')}
            onBlur={() => handleBlur('R1', localR1)}
            onKeyDown={e => e.key === 'Enter' && sendField('R1', localR1)}
          />
        </Form.Group>

        {isSecondOrder && (
          <Form.Group className="mb-2">
            <Form.Label>R₂ (Ω)</Form.Label>
            <Form.Control
              type="number"
              step="any"
              value={localR2}
              onChange={e => setLocalR2(e.target.value)}
              onFocus={() => handleFocus('R2')}
              onBlur={() => handleBlur('R2', localR2)}
              onKeyDown={e => e.key === 'Enter' && sendField('R2', localR2)}
            />
          </Form.Group>
        )}

        <Form.Group className="mb-2">
          <Form.Label>{isSecondOrder ? 'C₁' : 'C'} (µF)</Form.Label>
          <Form.Control
            type="number"
            step="any"
            value={localC1}
            onChange={e => setLocalC1(e.target.value)}
            onFocus={() => handleFocus('C1')}
            onBlur={() => handleBlur('C1', localC1)}
            onKeyDown={e => e.key === 'Enter' && sendField('C1', localC1)}
          />
        </Form.Group>

        {isSecondOrder && (
          <Form.Group className="mb-2">
            <Form.Label>C₂ (µF)</Form.Label>
            <Form.Control
              type="number"
              step="any"
              value={localC2}
              onChange={e => setLocalC2(e.target.value)}
              onFocus={() => handleFocus('C2')}
              onBlur={() => handleBlur('C2', localC2)}
              onKeyDown={e => e.key === 'Enter' && sendField('C2', localC2)}
            />
          </Form.Group>
        )}

        <Form.Group className="mb-2">
          <Form.Label>T<sub>d</sub> (s) – transport delay</Form.Label>
          <Form.Control
            type="number"
            step="any"
            value={localTd}
            onChange={e => setLocalTd(e.target.value)}
            onFocus={() => handleFocus('Td')}
            onBlur={() => handleBlur('Td', localTd)}
            onKeyDown={e => e.key === 'Enter' && sendField('Td', localTd)}
          />
        </Form.Group>

        <Form.Group className="mb-3">
          <Form.Label>T<sub>s</sub> (s) – sampling time</Form.Label>
          <Form.Control
            type="number"
            step="any"
            value={localTs}
            onChange={e => setLocalTs(e.target.value)}
            onFocus={() => handleFocus('Ts')}
            onBlur={() => handleBlur('Ts', localTs)}
            onKeyDown={e => e.key === 'Enter' && sendField('Ts', localTs)}
          />
        </Form.Group>
      </Form>

      <div className="reg-output mb-3">
        <Badge bg="secondary" className="fs-6 me-2">
          U<sub>in</sub> = {Uin.toFixed(2)} V
        </Badge>
        <Badge bg="info" className="fs-6 me-2">
          U<sub>{isSecondOrder ? 'c1' : 'c'}</sub> = {Uc1.toFixed(2)} V
        </Badge>
        {isSecondOrder && (
          <Badge bg="info" className="fs-6 me-2">
            U<sub>c2</sub> = {Uc2.toFixed(2)} V
          </Badge>
        )}
      </div>

      <pre style={{ background: '#f6f8fa', padding: 8, borderRadius: 6, marginTop: 12 }}>
        {JSON.stringify(data, null, 2)}
      </pre>
    </div>
  );
}

function RegulatorPage() {
  const { section: d } = useSectionData('RegulatorData');
  
  const switchstate = toBool(d?.switchstate);
  const order = Number(d?.order ?? 1); // 1 = 1. řád, 2 = 2. řád
  const isSecondOrder = order === 2;
  
  const Uc1 = Number(d?.Uc1 ?? 0);
  const Uc2 = Number(d?.Uc2 ?? 0);
  // Dynamický výběr obrázku podle řádu a stavu spínače
  const getBackgroundImage = () => {
    const circuitType = isSecondOrder ? 'RCRC' : 'RC';
    const switchState = switchstate ? 'closed' : 'open';
    return `/images/regulator/regulator_${circuitType}_${switchState}.PNG`;
  };

  const background = getBackgroundImage();

  return (
    <Row className="regulatorpage">
      <Col xs={12} lg={8}>
        <div className="mt-3">
          <RegulatorCanvas background={background} />
          <UcChart Uc1={Uc1} Uc2={Uc2} order={order} />
        </div>
      </Col>

      <Col lg={4}>
        <RegulatorParamsSidebar />
      </Col>
    </Row>
  );
}

export default RegulatorPage;
