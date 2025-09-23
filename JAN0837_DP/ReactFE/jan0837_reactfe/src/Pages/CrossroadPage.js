import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image as RBImage, Form, Card, Badge } from 'react-bootstrap';

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

const names = ['crossroad_basic', 'crossroad_day', 'crossroad_night', 'crosswalk_ped_green_1920x1080_169', 'crosswalk_vehicle_yellow_1920x1080_169', 'crosswalk_vehicle_green_1920x1080_169', 'crosswalk_ped_green_800x600_43', 'crosswalk_vehicle_yellow_800x600_43', 'crosswalk_vehicle_green_800x600_43'];
const ext = 'png';         
const folder = 'images'; 

const crossroad = ['crossroad_basic', 'crossroad_day', 'crossroad_night']
const crosswalkLights = ['crosswalk_red', 'crosswalk_green', 'crosswalk_blank']
const trafficLights = ['traffic_lights_red', 'traffic_lights_yellow', 'traffic_lights_green', 'traffic_lights_yellow_red', 'traffic_lights_blank']

const crossroad_light_green = ["traffic_light_green_0", "traffic_light_green_90", "traffic_light_green_180", "traffic_light_green_270"]
const crossroad_light_yellow = ["traffic_light_yellow_0", "traffic_light_yellow_90", "traffic_light_yellow_180", "traffic_light_yellow_270"]
const crossroad_light_red = ["traffic_light_red_0", "traffic_light_red_90", "traffic_light_red_180", "traffic_light_red_270"]
const crossroad_light_blank = ["traffic_light_blank_0", "traffic_light_blank_90", "traffic_light_blank_180", "traffic_light_blank_270"]
const pedestrian_light_green = ["crosswalk_light_green_0", "crosswalk_light_green_90", "crosswalk_light_green_180", "crosswalk_light_green_270"]
const pedestrian_light_red = ["crosswalk_light_red_0", "crosswalk_light_red_90", "crosswalk_light_red_180", "crosswalk_light_red_270"]
const pedestrian_light_red_blank = ["crosswalk_light_red_blank_0", "crosswalk_light_red_blank_90", "crosswalk_light_red_blank_180", "crosswalk_light_red_blank_270"]
const pedestrian_light_green_blank = ["crosswalk_light_green_blank_0", "crosswalk_light_green_blank_90", "crosswalk_light_green_blank_180", "crosswalk_light_green_blank_270"]

const switcherGroups = [
  { key: 'crossroad', label: 'Křižovatka',          names: crossroad },
  { key: 'crosswalk', label: 'Přechod pro chodce', names: crosswalkLights },
  { key: 'traffic',  label: 'Dopravní světla',     names: trafficLights },
  { key: 'tl_green',  label: 'Auto – zelená',          names: crossroad_light_green },
  { key: 'tl_yellow', label: 'Auto – žlutá',           names: crossroad_light_yellow },
  { key: 'tl_red',    label: 'Auto – červená',         names: crossroad_light_red },
  { key: 'tl_blank',  label: 'Auto – prázdná',         names: crossroad_light_blank },
  { key: 'ped_green', label: 'Chodec – zelená',        names: pedestrian_light_green },
  { key: 'ped_red',   label: 'Chodec – červená',       names: pedestrian_light_red },
  { key: 'ped_r_b',   label: 'Chodec – červená (blik)', names: pedestrian_light_red_blank },
  { key: 'ped_g_b',   label: 'Chodec – zelená (blik)',  names: pedestrian_light_green_blank },
];

function CrossroadParamsSidebar({names, idx, onPrev, onNext, onJump,}) 
{
  const { interval, setInterval } = useRefresh();
  const { data, saveData, error, isFetching, refresh } = useData();

  const toBool = (v) => {
    const s = String(v ?? '').trim().toLowerCase();
    return s === 'true' || s === '1' || s === 'on';
  };

  const toggleBtn = (key, value) => {
    saveData({
      btnCrossroadStart: key === 'btnCrossroadStart' ? (value ? 'true' : 'false') : 'false',
      btnCrossroadPause: key === 'btnCrossroadPause' ? (value ? 'true' : 'false') : 'false',
      btnCrossroadStop:  key === 'btnCrossroadStop'  ? (value ? 'true' : 'false') : 'false',
    });
  };
  
  const number = Number(data?.number ?? 0);
  const text = typeof data?.text === 'string' ? data.text : String(data?.text ?? '');
  const toggle = (() => {
        const t = String(data?.toggle ?? '').toLowerCase();
        return t === 'true' || t === 'on' || t === '1';
  })();

  const btnCrossroadStart = toBool(data?.btnCrossroadStart);
  const btnCrossroadPause = toBool(data?.btnCrossroadPause);
  const btnCrossroadStop = toBool(data?.btnCrossroadStop);

  const trafficLight1_green = toBool(data?.trafficLight1_green);
  const trafficLight1_yellow = toBool(data?.trafficLight1_yellow);
  const trafficLight1_red = toBool(data?.trafficLight1_red); 
  const trafficLight2_green = toBool(data?.trafficLight2_green);
  const trafficLight2_yellow = toBool(data?.trafficLight2_yellow);
  const trafficLight2_red = toBool(data?.trafficLight2_red); 
  const pedestrian1_green = toBool(data?.pedestrian1_green);
  const pedestrian1_red = toBool(data?.pedestrian1_red); 
  const pedestrian2_green = toBool(data?.pedestrian2_green);
  const pedestrian2_red = toBool(data?.pedestrian2_red);

  const setFlag = (key, value) => saveData({ [key]: value ? 'true' : 'false' });

  const setStartAsync = () => saveData({ btnCrossroadStart: !btnCrossroadStart });
  const setPauseAsync = () => saveData({ btnCrossroadPause: !btnCrossroadPause });
  const setStopAsync  = () => saveData({ btnCrossroadStop: !btnCrossroadStop });

  {/*const toggleBtn = (key, value) => saveData({ [key]: !value ? 'true' : 'false' });
  */}
  return (
    <div>
      <h3>Parametry:</h3>
      <div className="gap-2 mb-2">
        <div className="text-muted small text-center">
          Obrázek {idx + 1} / {names.length}
        </div>
        
        <Button variant="outline-secondary" onClick={onPrev}>
          &laquo; Předchozí
        </Button>
        
        <Button variant="primary" onClick={onNext}>
          Další &raquo;
        </Button>

        <Form.Select value={idx} onChange={(e) => onJump(Number(e.target.value))} className="mb-3">          
          {names.map((n, i) => (
            <option key={n} value={i}>
              {n}
            </option>
          ))}
        </Form.Select>

      </div>
      
      <div>
        <div><strong>Number:</strong> {number}</div>
        <div><strong>Status:</strong> {String(toggle)}</div>
        <div><strong>Text:</strong> {text}</div>
        
        <div><strong>btnCrossroadStart:</strong> {String(btnCrossroadStart)}</div>
        <div><strong>btnCrossroadPause:</strong> {String(btnCrossroadPause)}</div>
        <div><strong>btnCrossroadStop:</strong> {String(btnCrossroadStop)}</div>

        <div><strong>TL1_G:</strong> {String(trafficLight1_green)}</div>
        <div><strong>TL1_Y:</strong> {String(trafficLight1_yellow)}</div>
        <div><strong>TL1_R:</strong> {String(trafficLight1_red)}</div>
        <div><strong>TL2_G:</strong> {String(trafficLight2_green)}</div>
        <div><strong>TL2_Y:</strong> {String(trafficLight2_yellow)}</div>
        <div><strong>TL2_R:</strong> {String(trafficLight2_red)}</div>
        <div><strong>PL1_G:</strong> {String(pedestrian1_green)}</div>
        <div><strong>PL1_R:</strong> {String(pedestrian1_red)}</div>
        <div><strong>PL2_G:</strong> {String(pedestrian2_green)}</div>
        <div><strong>PL2_R:</strong> {String(pedestrian2_red)}</div>

        <Col>
          <div className="gap-2 mb-2">
            <Button className="btn--start" onClick={() => setStartAsync(!btnCrossroadStart) /*toggleBtn("btnCrossroadStart", !btnCrossroadStart)*/} /*disabled={isFetching}*/>
              Start ({String(btnCrossroadStart)})
            </Button>
          </div>
          <div className="gap-2 mb-2">
            <Button className="btn--pause" onClick={() => setPauseAsync(!btnCrossroadPause) /*toggleBtn("btnCrossroadPause", !btnCrossroadPause)*/} /*disabled={isFetching}*/>
              Pause ({String(btnCrossroadPause)})
            </Button>
          </div>
          <div className="gap-2 mb-2">
            <Button className="btn--stop" onClick={() => setStopAsync(!btnCrossroadStop) /*toggleBtn("btnCrossroadStop", !btnCrossroadStop)*/} /*disabled={isFetching}*/>
              Stop ({String(btnCrossroadStop)})
            </Button>  
          </div>
        </Col>

        {/*}
        */}
        <pre style={{background:'#f6f8fa', padding:8, borderRadius:6, marginTop:12}}>
          {JSON.stringify(data, null, 2)}
        </pre>
        
        
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
      <Col xs={12} lg={8}>
        
        <div className="mt-3">
          <Picture name={names[idx]} ext={ext} folder={folder} />
        </div>
        
        <div className="switchers-grid mt-4">
          {switcherGroups.map(g => (
            <Card key={g.key} className="h-100">
              <Card.Header className="py-2">{g.label}</Card.Header>
                <Card.Body>
                  <PictureSwitcher
                    names={g.names}
                    ext="png"
                    folder="images"
                    imgClassName="shadow-sm w-100"
                  />
                </Card.Body>
              </Card>
          ))}
        </div>

        {/*}
        */}
        {/*}
        <div style={{display:"grid", gap:12}}>
          <ResponsiveImage name="/crosswalk_ped_green_1920x1080_169" alt="Schema křižovatky 16:9" aspect="16 / 9" fit="contain"/>
        </div>
        */}
        {/*}
        <div style={{display:"grid", gap:12}}>
          <ResponsiveImage name="/crosswalk_ped_green_800x600_43" alt="Schema křižovatky 4:3" aspect="4 / 3" fit="contain"/>
        </div>
        */}
        {/*}
        <div>
          <PictureSwitcher names={names} ext="png" imgClassName="shadow-sm" />
        </div>
        */}
      </Col>
      
      <Col lg={4}>
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