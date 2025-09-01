import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image as RBImage, Form } from 'react-bootstrap';

import '../App.css';
import './CrossroadPage.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import TimeDate from '../Components/TimeDate.js';
import Picture from '../Components/Picture.js';
import Clock from '../Components/Clock.js';
import PictureSwitcher from '../Components/PictureSwitcher.js';

const names = ['crosswalk_ped_green19201080', 'crosswalk_vehicle_yellow19201080', 'crosswalk_vehicle_green19201080', 'crosswalk_800x600_ped_green43', 'crosswalk_800x600_vehicle_yellow43', 'crosswalk_800x600_vehicle_green43'];
const ext = 'png';         
const folder = 'images'; 

function CrossroadParamsSidebar({names, idx, onPrev, onNext, onJump,}) 
{
  return (
    <div className="p-3 border-start h-100">
      <div className="fw-semibold mb-3">Parametry</div>
      
      <div className="d-grid gap-2 mb-2">
        <div className="text-muted small text-center">
          Obrázek {idx + 1} / {names.length}
        </div>
        <Button variant="outline-secondary" onClick={onPrev}>
          &laquo; Předchozí
        </Button>
        <Button variant="primary" onClick={onNext}>
          Další &raquo;
        </Button>
      </div>

      <Form.Select
        value={idx}
        onChange={(e) => onJump(Number(e.target.value))}
        className="mb-3"
      >
        {names.map((n, i) => (
          <option key={n} value={i}>
            {n}
          </option>
        ))}
      </Form.Select>
      
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

function CrossroadPage() {
  const [idx, setIdx] = useState(0);

  const prev = () => setIdx((i) => (i - 1 + names.length) % names.length);
  const next = () => setIdx((i) => (i + 1) % names.length);
  const jump = (i) => setIdx(i);

  useEffect(() => {
    const preload = (name) => {
      const img = new Image();
      const viteBase = (typeof import.meta !== 'undefined' && import.meta.env?.BASE_URL) || '';
      const craBase  = (typeof process !== 'undefined' && process.env?.PUBLIC_URL) || '';
      const base = (viteBase || craBase || '/').replace(/\/$/, '');
      img.src = `${base}/${folder}/${name}.${ext}`;
    };
    preload(names[(idx + 1) % names.length]);
    preload(names[(idx - 1 + names.length) % names.length]);
  }, [idx]);
  
  return (
    <Row className="g-0">
      <Col xs={12} lg={10} className="p-3">
        <div className="mt-3">
          <Picture name={names[idx]} ext={ext} folder={folder} />
        </div>

        {/*}
        <div className="mt-4">
          <PictureSwitcher names={names} ext="png" imgClassName="shadow-sm" />
        </div>
        */}
      </Col>

      <Col xs={12} lg={2}>
        <CrossroadParamsSidebar names={names} idx={idx} onPrev={prev} onNext={next} onJump={jump}/>
      </Col>
    </Row>
  );
}

export default CrossroadPage;