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
import ResponsiveImage from '../Components/ResponsiveImage.js';

import { useRefresh } from '../Communication/RefreshContext.js';
import { useData } from '../Communication/DataProvider';

const names = ['crosswalk_ped_green_1920x1080_169', 'crosswalk_vehicle_yellow_1920x1080_169', 'crosswalk_vehicle_green_1920x1080_169', 'crosswalk_ped_green_800x600_43', 'crosswalk_vehicle_yellow_800x600_43', 'crosswalk_vehicle_green_800x600_43'];
const ext = 'png';         
const folder = 'images'; 

function CrossroadParamsSidebar({names, idx, onPrev, onNext, onJump,}) 
{
  const { interval, setInterval } = useRefresh();
  const { data, saveData, error, isFetching, refresh } = useData();

  const bool = (() => {const t = String(data?.toggle ?? '').toLowerCase();return t === 'true' || t === 'on' || t === '1';})();
  const trafficLight1_green = (() => {const t = String(data?.trafficLight1_green ?? '').toLowerCase();})();
  const trafficLight1_yellow = Boolean(data?.trafficLight1_yellow ?? false);
  const trafficLight1_red = Boolean(data?.trafficLight1_red ?? true); 
  const trafficLight2_green = Boolean(data?.trafficLight2_green ?? false);
  const trafficLight2_yellow = Boolean(data?.trafficLight2_yellow ?? false);
  const trafficLight2_red = Boolean(data?.trafficLight2_red ?? true); 
  const pedestrian1_green = Boolean(data?.pedestrian1_green ?? false);
  const pedestrian1_red = Boolean(data?.pedestrian1_red ?? true); 
  const pedestrian2_green = Boolean(data?.pedestrian2_green ?? false);
  const pedestrian2_red = Boolean(data?.pedestrian2_red ?? true);

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

      <div>
        <div><strong>TL1_G:</strong>{trafficLight1_green}</div>
        <div><strong>TL1_Y:</strong>{trafficLight1_yellow}</div>
        <div><strong>TL1_R:</strong>{trafficLight1_red}</div>
        <div><strong>TL2_G:</strong>{trafficLight2_green}</div>
        <div><strong>TL2_Y:</strong>{trafficLight2_yellow}</div>
        <div><strong>TL2_R:</strong>{trafficLight2_red}</div>
        <div><strong>PL1_G:</strong>{pedestrian1_green}</div>
        <div><strong>PL1_R:</strong>{pedestrian1_red}</div>
        <div><strong>PL2_G:</strong>{pedestrian2_green}</div>
        <div><strong>PL2_R:</strong>{pedestrian2_red}</div>
      </div>
    </div>
  );
}

function CrossroadPage({ setAside }) {
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

  {/*
  useEffect(()=>{
      setAside(
        <div className="stack">
          <strong>Parametry</strong>
          <label>Limit <input type="number" defaultValue={50}/></label>
          <button>Start</button>
        </div>
      );
      return ()=> setAside(null);
    }, [setAside]);
  */}

  return (
    <Row className="crossroadpage">
      <Col /*xs={12} lg={10}*/>
        
        <div className="mt-3">
          <Picture name={names[idx]} ext={ext} folder={folder} />
        </div>
        
        {/*}
        */}
        
        <div style={{display:"grid", gap:12}}>
          <ResponsiveImage name="/crosswalk_ped_green_1920x1080_169" alt="Schema křižovatky 16:9" aspect="16 / 9" fit="contain"/>
        </div>
        
        
        <div style={{display:"grid", gap:12}}>
          <ResponsiveImage name="/crosswalk_ped_green_800x600_43" alt="Schema křižovatky 4:3" aspect="4 / 3" fit="contain"/>
        </div>
        
        
        <div>
          <PictureSwitcher names={names} ext="png" imgClassName="shadow-sm" />
        </div>
        
      </Col>
      
      <Col /*xs={12} lg={2}*/>
        <CrossroadParamsSidebar names={names} idx={idx} onPrev={prev} onNext={next} onJump={jump}/>
      </Col>
      
    </Row>
  );
}

export default CrossroadPage;

/*
      <Col xs={12} lg={2}>
        <CrossroadParamsSidebar names={names} idx={idx} onPrev={prev} onNext={next} onJump={jump}/>
      </Col>
*/