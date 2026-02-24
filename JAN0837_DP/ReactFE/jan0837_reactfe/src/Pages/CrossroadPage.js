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
import { useData, useSectionData } from '../Communication/DataProvider';

/*
 * ── CrossroadData variable map ────────────────────
 *
 * INPUTS  (FE → PLC):
 *   btnCrossroadStart    Bool – start crossroad
 *   btnCrossroadPause    Bool – pause crossroad
 *   btnCrossroadStop     Bool – stop crossroad
 *   btnCrosswalk1        Bool – pedestrian button N
 *   btnCrosswalk2        Bool – pedestrian button S
 *
 * OUTPUTS (PLC → FE):
 *   crossroadType            Bool – night / day
 *   trafficLight1_green      Bool
 *   trafficLight1_yellow     Bool
 *   trafficLight1_red        Bool
 *   trafficLight2_green      Bool
 *   trafficLight2_yellow     Bool
 *   trafficLight2_red        Bool
 *   pedestrian1_green        Bool
 *   pedestrian1_red          Bool
 *   pedestrian2_green        Bool
 *   pedestrian2_red          Bool
 */

const toBool = (v) => {
  if (typeof v === 'boolean') return v;
  const s = String(v ?? '').trim().toLowerCase();
  return s === 'true' || s === '1' || s === 'on';
};

const names = ['crossroad_basic', 'crossroad_day', 'crossroad_night', 'crosswalk_ped_green_1920x1080_169', 'crosswalk_vehicle_yellow_1920x1080_169', 'crosswalk_vehicle_green_1920x1080_169', 'crosswalk_ped_green_800x600_43', 'crosswalk_vehicle_yellow_800x600_43', 'crosswalk_vehicle_green_800x600_43'];
const ext = 'png';         
const folder = 'images'; 

const LIGHT_SOURCES = {
  car: {
    green0: 'traffic_light_0_green.png',
    green90: 'traffic_light_90_green.png',
    green180: 'traffic_light_180_green.png',
    green270: 'traffic_light_270_green.png',

    yellow0: 'traffic_light_0_yellow.png',
    yellow90: 'traffic_light_90_yellow.png',
    yellow180: 'traffic_light_180_yellow.png',
    yellow270: 'traffic_light_270_yellow.png',

    red0: 'traffic_light_0_red.png',
    red90: 'traffic_light_90_red.png',
    red180: 'traffic_light_180_red.png',
    red270: 'traffic_light_270_red.png',

    blank0: 'traffic_light_0_blank.png',
    blank90: 'traffic_light_90_blank.png',
    blank180: 'traffic_light_180_blank.png',
    blank270: 'traffic_light_270_blank.png'
  },
  ped: {
    green0: 'pedestrian_light_0_green.png',
    green90: 'pedestrian_light_90_green.png',
    green180: 'pedestrian_light_180_green.png',
    green270: 'pedestrian_light_270_green.png',

    red0: 'pedestrian_light_0_red.png',
    red90: 'pedestrian_light_90_red.png',
    red180: 'pedestrian_light_180_red.png',
    red270: 'pedestrian_light_270_red.png',

    greenblank0: 'pedestrian_light_0_green_blank.png',
    greenblank90: 'pedestrian_light_90_green_blank.png',
    greenblank180: 'pedestrian_light_180_green_blank.png',
    greenblank270: 'pedestrian_light_270_green_blank.png',

    redblank0: 'pedestrian_light_0_red_blank.png',
    redblank90: 'pedestrian_light_90_red_blank.png',
    redblank180: 'pedestrian_light_180_red_blank.png',
    redblank270: 'pedestrian_light_270_red_blank.png'
  }
};

function pickCarSrc({ green, yellow, red }, dir = 0) {
  const d = String(dir);
  const r = toBool(red);
  const y = toBool(yellow);
  const g = toBool(green);
  if (r) return `/images/${LIGHT_SOURCES.car['red' + d]}`;
  if (y) return `/images/${LIGHT_SOURCES.car['yellow' + d]}`;
  if (g) return `/images/${LIGHT_SOURCES.car['green' + d]}`;
  return `/images/${LIGHT_SOURCES.car['blank' + d]}`;
}

function pickPedRedSrc(red, dir = 0) {
  const d = String(dir);
  const r = toBool(red);
  return r
    ? `/images/${LIGHT_SOURCES.ped['red' + d]}`
    : `/images/${LIGHT_SOURCES.ped['redblank' + d]}`;
}

function pickPedGreenSrc(green, dir = 0) {
  const d = String(dir);
  const g = toBool(green);
  return g
    ? `/images/${LIGHT_SOURCES.ped['green' + d]}`
    : `/images/${LIGHT_SOURCES.ped['greenblank' + d]}`;
}

function pickCarLensSrc(color, state, dir = 0) {
  const d = String(dir);
  const on = toBool(state?.[color]); // green, yellow, red
  const key = on ? `${color}${d}` : `blank${d}`;
  return `/images/${LIGHT_SOURCES.car[key]}`;
}

function pickPedLensSrc(color, state, dir = 0) {
  const d = String(dir);
  const on = toBool(state?.[color]); // green, red
  const key = on ? `${color}${d}` : `${color}blank${d}`;
  return `/images/${LIGHT_SOURCES.ped[key]}`;
}

function PedLens({ color, state, dir=0, x, y, alt }) {
  const src = pickPedLensSrc(color, state, dir);
  const style = { left: x, top: y };
  return <img className="light light--uniform" src={src} alt={alt ?? `ped-${color}`} style={style} />;
}

function CarLens({ color, state, dir=0, x, y, alt }) {
  const src = pickCarLensSrc(color, state, dir);
  const style = { left: x, top: y };
  return <img className="light light--uniform" src={src} alt={alt ?? `car-${color}`} style={style} />;
}

function TrafficLight({ state, dir = 0, x, y, alt }) {
  const src = pickCarSrc(state, dir);
  const style = { left: x, top: y };
  return (
    <img className="light light--uniform" src={src} alt={alt ?? "car light"} style={style}/>
  );
}

function PedestrianLight({ state, dir = 0, x, y, alt }) {
  const redSrc = pickPedRedSrc(state.red, dir);
  const greenSrc = pickPedGreenSrc(state.green, dir);
  const style = { left: x, top: y };

  return (
    <>
      <img className="light light--uniform" src={redSrc} alt={alt + ' red'} style={style} />
      <img className="light light--uniform" src={greenSrc} alt={alt + ' green'} style={style} />
    </>
  );
}

function CrossroadCanvas({ background, lights, pedControls }) {
  const style = { backgroundImage: `url(${background})` };
  return (
    <div className="crossroad" style={style}>
      {lights.map(l => l.kind === 'car' ? (
          <CarLens 
            key={l.id}
            kind={l.kind}
            state={l.state}
            dir={l.dir ?? 0}
            x={l.x}
            y={l.y}
            w={l.w}
            alt={l.id}
            color = {l.color}
          />
        ) : (
          <PedLens
            key={l.id}
            kind={l.kind}
            state={l.state}
            dir={l.dir ?? 0}
            x={l.x}
            y={l.y}
            w={l.w}
            alt={l.id}
            color = {l.color}
          />
        )
      )}

      <>
        <button
          type="button"
          className="ped-btn ped-btn--north btn btn-sm btn-light"
          onClick={pedControls.onNorth}
          aria-label="Toggle pedestrian North"
        >
          N {pedControls.btnPedN ? '🟢' : '⚪'}
        </button>

        <button
          type="button"
          className="ped-btn ped-btn--south btn btn-sm btn-light"
          onClick={pedControls.onSouth}
          aria-label="Toggle pedestrian South"
        >
          S {pedControls.btnPedS ? '🟢' : '⚪'}
        </button>
      </>
    </div>
  );
}

function CrossroadParamsSidebar({names, idx, onPrev, onNext, onJump,}) 
{
  const { interval, setInterval } = useRefresh();
  const { section: d, saveSection, data, error, isFetching, refresh } = useSectionData('CrossroadData');

  const [status, setStatus] = React.useState('');

  const crossroadType = toBool(d?.crossroadType);

  const btnCrossroadStart = toBool(d?.btnCrossroadStart);
  const btnCrossroadPause = toBool(d?.btnCrossroadPause);
  const btnCrossroadStop = toBool(d?.btnCrossroadStop);

  const trafficLight1_green = toBool(d?.trafficLight1_green);
  const trafficLight1_yellow = toBool(d?.trafficLight1_yellow);
  const trafficLight1_red = toBool(d?.trafficLight1_red); 
  const trafficLight2_green = toBool(d?.trafficLight2_green);
  const trafficLight2_yellow = toBool(d?.trafficLight2_yellow);
  const trafficLight2_red = toBool(d?.trafficLight2_red); 
  const pedestrian1_green = toBool(d?.pedestrian1_green);
  const pedestrian1_red = toBool(d?.pedestrian1_red); 
  const pedestrian2_green = toBool(d?.pedestrian2_green);
  const pedestrian2_red = toBool(d?.pedestrian2_red);

  const setCrossroadType = () => saveSection({ crossroadType: !crossroadType });

  const setStartAsync = () => saveSection({ btnCrossroadStart: !btnCrossroadStart });
  const setPauseAsync = () => saveSection({ btnCrossroadPause: !btnCrossroadPause });
  const setStopAsync  = () => saveSection({ btnCrossroadStop: !btnCrossroadStop });

  const setCrossroadLightGreen1 = () => saveSection({ trafficLight1_green: !trafficLight1_green });
  const setCrossroadLightYellow1 = () => saveSection({ trafficLight1_yellow: !trafficLight1_yellow });
  const setCrossroadLightRed1 = () => saveSection({ trafficLight1_red: !trafficLight1_red });
  const setCrossroadLightGreen2 = () => saveSection({ trafficLight2_green: !trafficLight2_green });
  const setCrossroadLightYellow2 = () => saveSection({ trafficLight2_yellow: !trafficLight2_yellow });
  const setCrossroadLightRed2 = () => saveSection({ trafficLight2_red: !trafficLight2_red });
  const setPedestrianLightGreen1 = () => saveSection({ pedestrian1_green: !pedestrian1_green });
  const setPedestrianLightRed1 = () => saveSection({ pedestrian1_red: !pedestrian1_red });
  const setPedestrianLightGreen2 = () => saveSection({ pedestrian2_green: !pedestrian2_green });
  const setPedestrianLightRed2 = () => saveSection({ pedestrian2_red: !pedestrian2_red });

  const toggleCrossroadType = async () => {
    try {
      const current = toBool(d?.crossroadType);
      const next = !current;

      setStatus(`sending… (current=${String(current)} → next=${String(next)})`);
      console.log('toggleCrossroadType', { current, next, data });

      await saveSection({ crossroadType: next });

      setStatus(`ok ✔ (store now: ${String(toBool(d?.crossroadType))})`);
    } catch (e) {
      console.error(e);
      setStatus(`error ✖ ${e?.message ?? e}`);
    }
  };

  return (
    <div>
      <h3>Parameters:</h3>
      {/*
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
      */}
      
      <div className="gap-2 mb-3">
        <Button onClick={setCrossroadType}>
          CrossroadType ({String(crossroadType)})
        </Button>
      </div>

      <div>
        <Col>
          <div className="gap-2 mb-2">
            <Button className="btn--start" onClick={() => setStartAsync(!btnCrossroadStart) /*toggleBtn("btnCrossroadStart", !btnCrossroadStart)*/} /*disabled={isFetching}*/>
              Start ({String(btnCrossroadStart)})
            </Button>
          
            <Button className="btn--pause" onClick={() => setPauseAsync(!btnCrossroadPause) /*toggleBtn("btnCrossroadPause", !btnCrossroadPause)*/} /*disabled={isFetching}*/>
              Pause ({String(btnCrossroadPause)})
            </Button>
          
            <Button className="btn--stop" onClick={() => setStopAsync(!btnCrossroadStop) /*toggleBtn("btnCrossroadStop", !btnCrossroadStop)*/} /*disabled={isFetching}*/>
              Stop ({String(btnCrossroadStop)})
            </Button>  
          </div>

          <div className="gap-2 mb-2">
            <Button onClick={() => setCrossroadLightGreen1(!trafficLight1_green)}>
              Crossroad green 1 ({String(trafficLight1_green)})
            </Button>
            <Button onClick={() => setCrossroadLightYellow1(!trafficLight1_yellow)}>
              Crossroad yellow 1 ({String(trafficLight1_yellow)})
            </Button>
            <Button onClick={() => setCrossroadLightRed1(!trafficLight1_red)}>
              Crossroad red 1 ({String(trafficLight1_red)})
            </Button>
          </div>

          <div className="gap-2 mb-2">
            <Button onClick={() => setCrossroadLightGreen2(!trafficLight2_green)}>
              Crossroad green 2 ({String(trafficLight2_green)})
            </Button>
            <Button onClick={() => setCrossroadLightYellow2(!trafficLight2_yellow)}>
              Crossroad yellow 2 ({String(trafficLight2_yellow)})
            </Button>
            <Button onClick={() => setCrossroadLightRed2(!trafficLight2_red)}>
              Crossroad red 2 ({String(trafficLight2_red)})
            </Button>
          </div>

          <div className="gap-2 mb-2">
            <Button onClick={() => setPedestrianLightGreen1(!pedestrian1_green)}>
              Pedestrian green 1 ({String(pedestrian1_green)})
            </Button>
            <Button onClick={() => setPedestrianLightRed1(!pedestrian1_red)}>
              Pedestrian red 1 ({String(pedestrian1_red)})
            </Button>
          </div>

          <div className="gap-2 mb-2">
            <Button onClick={() => setPedestrianLightGreen2(!pedestrian2_green)}>
              Pedestrian green 2 ({String(pedestrian2_green)})
            </Button>
            <Button onClick={() => setPedestrianLightRed2(!pedestrian2_red)}>
              Pedestrian red 2 ({String(pedestrian2_red)})
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
  const { section: d, saveSection, data } = useSectionData('CrossroadData');

    useEffect(() => {
    if (
      d?.trafficLight1_green === undefined &&
      d?.trafficLight1_yellow === undefined &&
      d?.trafficLight1_red === undefined &&
      d?.trafficLight2_green === undefined &&
      d?.trafficLight2_yellow === undefined &&
      d?.trafficLight2_red === undefined &&
      d?.pedestrian1_green === undefined &&
      d?.pedestrian1_red === undefined &&
      d?.pedestrian2_green === undefined &&
      d?.pedestrian2_red === undefined
    ) {
      saveSection({
        trafficLight1_green: 'false',
        trafficLight1_yellow: 'false',
        trafficLight1_red: 'false',
        trafficLight2_green: 'false',
        trafficLight2_yellow: 'false',
        trafficLight2_red: 'false',
        pedestrian1_green: 'false',
        pedestrian1_red: 'false',
        pedestrian2_green: 'false',
        pedestrian2_red: 'false',
      });
    }
  }, [d, saveSection]);

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

  const prev = () => setIdx((i) => (i - 1 + names.length) % names.length);
  const next = () => setIdx((i) => (i + 1) % names.length);
  const jump = (i) => setIdx(i);

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

  const isNight = toBool(d?.crossroadType);
  const background = isNight
    ? '/images/crossroad_night_blank.png'
    : '/images/crossroad_day_blank.png';

  useEffect(() => {
    ['/images/crossroad_day_blank.png', '/images/crossroad_night_blank.png'].forEach(src => {
      const img = new Image();
      img.src = src;
    });
  }, []);

  const CARW = {
    green: d?.trafficLight1_green ?? false,
    yellow: d?.trafficLight1_yellow ?? false,
    red: d?.trafficLight1_red ?? false,
  };

  const CARE = {
    green: d?.trafficLight2_green ?? false,
    yellow: d?.trafficLight2_yellow ?? false,
    red: d?.trafficLight2_red ?? false,
  };

  const PEDN = {
    green: d?.pedestrian1_green ?? false,
    red: d?.pedestrian1_red ?? false,
  };

  const PEDS = {
    green: d?.pedestrian2_green ?? false,
    red: d?.pedestrian2_red ?? false,
  };

  const lights = [
    // car – west (W) and east (E)
    { id: 'car-W-green', kind: 'car', color: 'green', state: CARW, dir: 90, x: '37.6%', y: '77.6%' }, // W = 90° 
    { id: 'car-W-yellow', kind: 'car', color: 'yellow', state: CARW, dir: 90, x: '41.5%', y: '77.6%' }, // W = 90° 
    { id: 'car-W-red', kind: 'car', color: 'red', state: CARW, dir: 90, x: '45.4%', y: '77.6%' }, // W = 90° 
    { id: 'car-E-green', kind: 'car', color: 'green', state: CARE,  dir: 270, x: '72%', y: '7.8%' }, // E = 270°
    { id: 'car-E-yellow', kind: 'car', color: 'yellow', state: CARE, dir: 270, x: '68.1%', y: '7.8%' }, // E = 270° 
    { id: 'car-E-red', kind: 'car', color: 'red', state: CARE, dir: 270, x: '64.2%', y: '7.8%' }, // E = 270° 

    // pedestrians – north (N) and south (S)
    { id: 'ped-N-green', kind: 'ped', color: 'green', state: PEDN, dir: 180, x: '45.6%', y: '2.5%' }, // N = 180°  
    { id: 'ped-N-red', kind: 'ped', color: 'red', state: PEDN, dir: 180, x: '45.6%', y: '9.5%' }, // N = 180° 
    { id: 'ped-S-green', kind: 'ped', color: 'green', state: PEDS, dir: 0, x: '63.4%', y: '82.5%' }, // S = 0° 
    { id: 'ped-S-red', kind: 'ped', color: 'red', state: PEDS, dir: 0, x: '63.4%', y: '75.7%' }, // S = 0° 
  ];

  const btnPed1 = toBool(d?.btnCrosswalk1);
  const btnPed2 = toBool(d?.btnCrosswalk2);

  const togglePedN = () => saveSection({ btnCrosswalk1: !btnPed1 });
  const togglePedS = () => saveSection({ btnCrosswalk2: !btnPed2 });

  return (
    <Row className="crossroadpage">
      <Col xs={12} lg={8}>
        
        <div className="mt-3">
          {/*<Picture name={names[idx]} ext={ext} folder={folder} />*/}
          <CrossroadCanvas background = {background} lights = {lights} pedControls = {{ btnPedN: btnPed1, btnPedS: btnPed2, onNorth: togglePedN, onSouth: togglePedS }} /> 
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
