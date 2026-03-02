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
 *   btnWestCrosswalk1    Bool – pedestrian button N
 *   btnWestCrosswalk2    Bool – pedestrian button S
 *   btnSouthCrosswalk1    Bool – pedestrian button W
 *   btnSouthCrosswalk2    Bool – pedestrian button E
 *
 * OUTPUTS (PLC → FE):
 *   crossroadType               Bool – night / day
 *   trafficLightNorth_green     Bool
 *   trafficLightNorth_yellow    Bool
 *   trafficLightNorth_red       Bool
 *   trafficLightSouth_green     Bool
 *   trafficLightSouth_yellow    Bool
 *   trafficLightSouth_red       Bool
 *   trafficLightWest_green      Bool
 *   trafficLightWest_yellow     Bool
 *   trafficLightWest_red        Bool
 *   trafficLightEast_green      Bool
 *   trafficLightEast_yellow     Bool
 *   trafficLightEast_red        Bool
 *   pedestrianNorth_green       Bool -> not now
 *   pedestrianNorth_red         Bool -> not now
 *   pedestrianSouth_green       Bool
 *   pedestrianSouth_red         Bool
 *   pedestrianWest_green        Bool
 *   pedestrianWest_red          Bool
 *   pedestrianEast_green        Bool -> not now
 *   pedestrianEast_red          Bool -> not now 
 */

const toBool = (v) => {
  if (typeof v === 'boolean') return v;
  const s = String(v ?? '').trim().toLowerCase();
  return s === 'true' || s === '1' || s === 'on';
};

const names = ['crossroad_day', 'crossroad_day_blank', 'crossroad_night', 'crossroad_night_blank'];
const ext = 'png';         
const folder = 'images/crossroad'; 

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
  if (r) return `/images/headlights/${LIGHT_SOURCES.car['red' + d]}`;
  if (y) return `/images/headlights/${LIGHT_SOURCES.car['yellow' + d]}`;
  if (g) return `/images/headlights/${LIGHT_SOURCES.car['green' + d]}`;
  return `/images/headlights/${LIGHT_SOURCES.car['blank' + d]}`;
}

function pickPedRedSrc(red, dir = 0) {
  const d = String(dir);
  const r = toBool(red);
  return r
    ? `/images/headlights/${LIGHT_SOURCES.ped['red' + d]}`
    : `/images/headlights/${LIGHT_SOURCES.ped['redblank' + d]}`;
}

function pickPedGreenSrc(green, dir = 0) {
  const d = String(dir);
  const g = toBool(green);
  return g
    ? `/images/headlights/${LIGHT_SOURCES.ped['green' + d]}`
    : `/images/headlights/${LIGHT_SOURCES.ped['greenblank' + d]}`;
}

function pickCarLensSrc(color, state, dir = 0) {
  const d = String(dir);
  const on = toBool(state?.[color]); // green, yellow, red
  const key = on ? `${color}${d}` : `blank${d}`;
  return `/images/headlights/${LIGHT_SOURCES.car[key]}`;
}

function pickPedLensSrc(color, state, dir = 0) {
  const d = String(dir);
  const on = toBool(state?.[color]); // green, red
  const key = on ? `${color}${d}` : `${color}blank${d}`;
  return `/images/headlights/${LIGHT_SOURCES.ped[key]}`;
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
        {/* 
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
          className="ped-btn ped-btn--north-2 btn btn-sm btn-light"
          onClick={pedControls.onNorth}
          aria-label="Toggle pedestrian North second"
        >
          N {pedControls.btnPedN ? '🟢' : '⚪'}
        </button>
        */}

        <button
          type="button"
          className="ped-btn ped-btn--south btn btn-sm btn-light"
          onClick={pedControls.onSouth1}
          aria-label="Toggle pedestrian South"
        >
          S {pedControls.btnPedS1 ? '🟢' : '⚪'}
        </button>

        <button
          type="button"
          className="ped-btn ped-btn--south-2 btn btn-sm btn-light"
          onClick={pedControls.onSouth2}
          aria-label="Toggle pedestrian South second"
        >
          S {pedControls.btnPedS2 ? '🟢' : '⚪'}
        </button>

        <button
          type="button"
          className="ped-btn ped-btn--west btn btn-sm btn-light"
          onClick={pedControls.onWest1}
          aria-label="Toggle pedestrian West"
        >
          W {pedControls.btnPedW1 ? '🟢' : '⚪'}
        </button>

        <button
          type="button"
          className="ped-btn ped-btn--west-2 btn btn-sm btn-light"
          onClick={pedControls.onWest2}
          aria-label="Toggle pedestrian West second"
        >
          W {pedControls.btnPedW2 ? '🟢' : '⚪'}
        </button>

        {/* 
        <button
          type="button"
          className="ped-btn ped-btn--east btn btn-sm btn-light"
          onClick={pedControls.onEast}
          aria-label="Toggle pedestrian East"
        >
          E {pedControls.btnPedE ? '🟢' : '⚪'}
        </button>

        <button
          type="button"
          className="ped-btn ped-btn--east-2 btn btn-sm btn-light"
          onClick={pedControls.onEast}
          aria-label="Toggle pedestrian East second"
        >
          E {pedControls.btnPedE ? '🟢' : '⚪'}
        </button>
        */}
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

  const btnStart = toBool(d?.btnStart);
  const btnPause = toBool(d?.btnPause);
  const btnStop = toBool(d?.btnStop);

  const trafficLightNorth_green = toBool(d?.trafficLightNorth_green);
  const trafficLightNorth_yellow = toBool(d?.trafficLightNorth_yellow);
  const trafficLightNorth_red = toBool(d?.trafficLightNorth_red);
  const trafficLightSouth_green = toBool(d?.trafficLightSouth_green);
  const trafficLightSouth_yellow = toBool(d?.trafficLightSouth_yellow);
  const trafficLightSouth_red = toBool(d?.trafficLightSouth_red);
  const trafficLightWest_green = toBool(d?.trafficLightWest_green);
  const trafficLightWest_yellow = toBool(d?.trafficLightWest_yellow);
  const trafficLightWest_red = toBool(d?.trafficLightWest_red);
  const trafficLightEast_green = toBool(d?.trafficLightEast_green);
  const trafficLightEast_yellow = toBool(d?.trafficLightEast_yellow);
  const trafficLightEast_red = toBool(d?.trafficLightEast_red);

  //const pedestrianNorth_green = toBool(d?.pedestrianNorth_green);
  //const pedestrianNorth_red = toBool(d?.pedestrianNorth_red);
  const pedestrianSouth_green = toBool(d?.pedestrianSouth_green);
  const pedestrianSouth_red = toBool(d?.pedestrianSouth_red);
  const pedestrianWest_green = toBool(d?.pedestrianWest_green);
  const pedestrianWest_red = toBool(d?.pedestrianWest_red);
  //const pedestrianEast_green = toBool(d?.pedestrianEast_green);
  //const pedestrianEast_red = toBool(d?.pedestrianEast_red);

  const setCrossroadType = async () => {
    try {
      await saveSection({ crossroadType: !toBool(d?.crossroadType) });
    } catch (e) { console.error('setCrossroadType error:', e); }
  };

  const setStartAsync = async () => {
    try {
      await saveSection({ btnStart: !toBool(d?.btnStart) });
    } catch (e) { console.error('setStartAsync error:', e); }
  };
  const setPauseAsync = async () => {
    try {
      await saveSection({ btnPause: !toBool(d?.btnPause) });
    } catch (e) { console.error('setPauseAsync error:', e); }
  };
  const setStopAsync = async () => {
    try {
      await saveSection({ btnStop: !toBool(d?.btnStop) });
    } catch (e) { console.error('setStopAsync error:', e); }
  };

  const setCrossroadLightNorthGreen = async () => {
    try {
      await saveSection({ trafficLightNorth_green: !toBool(d?.trafficLightNorth_green) });
    } catch (e) { console.error('setCrossroadLightNorthGreen error:', e); }
  };
  const setCrossroadLightNorthYellow = async () => {
    try {
      await saveSection({ trafficLightNorth_yellow: !toBool(d?.trafficLightNorth_yellow) });
    } catch (e) { console.error('setCrossroadLightNorthYellow error:', e); }
  };
  const setCrossroadLightNorthRed = async () => {
    try {
      await saveSection({ trafficLightNorth_red: !toBool(d?.trafficLightNorth_red) });
    } catch (e) { console.error('setCrossroadLightNorthRed error:', e); }
  };
  const setCrossroadLightSouthGreen = async () => {
    try {
      await saveSection({ trafficLightSouth_green: !toBool(d?.trafficLightSouth_green) });
    } catch (e) { console.error('setCrossroadLightSouthGreen error:', e); }
  };
  const setCrossroadLightSouthYellow = async () => {
    try {
      await saveSection({ trafficLightSouth_yellow: !toBool(d?.trafficLightSouth_yellow) });
    } catch (e) { console.error('setCrossroadLightSouthYellow error:', e); }
  };
  const setCrossroadLightSouthRed = async () => {
    try {
      await saveSection({ trafficLightSouth_red: !toBool(d?.trafficLightSouth_red) });
    } catch (e) { console.error('setCrossroadLightSouthRed error:', e); }
  };
  const setCrossroadLightWestGreen = async () => {
    try {
      await saveSection({ trafficLightWest_green: !toBool(d?.trafficLightWest_green) });
    } catch (e) { console.error('setCrossroadLightWestGreen error:', e); }
  };
  const setCrossroadLightWestYellow = async () => {
    try {
      await saveSection({ trafficLightWest_yellow: !toBool(d?.trafficLightWest_yellow) });
    } catch (e) { console.error('setCrossroadLightWestYellow error:', e); }
  };
  const setCrossroadLightWestRed = async () => {
    try {
      await saveSection({ trafficLightWest_red: !toBool(d?.trafficLightWest_red) });
    } catch (e) { console.error('setCrossroadLightWestRed error:', e); }
  };
  const setCrossroadLightEastGreen = async () => {
    try {
      await saveSection({ trafficLightEast_green: !toBool(d?.trafficLightEast_green) });
    } catch (e) { console.error('setCrossroadLightEastGreen error:', e); }
  };
  const setCrossroadLightEastYellow = async () => {
    try {
      await saveSection({ trafficLightEast_yellow: !toBool(d?.trafficLightEast_yellow) });
    } catch (e) { console.error('setCrossroadLightEastYellow error:', e); }
  };
  const setCrossroadLightEastRed = async () => {
    try {
      await saveSection({ trafficLightEast_red: !toBool(d?.trafficLightEast_red) });
    } catch (e) { console.error('setCrossroadLightEastRed error:', e); }
  };

  const setPedestrianNorthGreen = async () => {
    try {
      await saveSection({ pedestrianNorth_green: !toBool(d?.pedestrianNorth_green) });
    } catch (e) { console.error('setPedestrianNorthGreen error:', e); }
  };
  const setPedestrianNorthRed = async () => {
    try {
      await saveSection({ pedestrianNorth_red: !toBool(d?.pedestrianNorth_red) });
    } catch (e) { console.error('setPedestrianNorthRed error:', e); }
  };
  const setPedestrianSouthGreen = async () => {
    try {
      await saveSection({ pedestrianSouth_green: !toBool(d?.pedestrianSouth_green) });
    } catch (e) { console.error('setPedestrianSouthGreen error:', e); }
  };
  const setPedestrianSouthRed = async () => {
    try {
      await saveSection({ pedestrianSouth_red: !toBool(d?.pedestrianSouth_red) });
    } catch (e) { console.error('setPedestrianSouthRed error:', e); }
  };
  const setPedestrianWestGreen = async () => {
    try {
      await saveSection({ pedestrianWest_green: !toBool(d?.pedestrianWest_green) });
    } catch (e) { console.error('setPedestrianWestGreen error:', e); }
  };
  const setPedestrianWestRed = async () => {
    try {
      await saveSection({ pedestrianWest_red: !toBool(d?.pedestrianWest_red) });
    } catch (e) { console.error('setPedestrianWestRed error:', e); }
  };
  const setPedestrianEastGreen = async () => {
    try {
      await saveSection({ pedestrianEast_green: !toBool(d?.pedestrianEast_green) });
    } catch (e) { console.error('setPedestrianEastGreen error:', e); }
  };
  const setPedestrianEastRed = async () => {
    try {
      await saveSection({ pedestrianEast_red: !toBool(d?.pedestrianEast_red) });
    } catch (e) { console.error('setPedestrianEastRed error:', e); }
  };

  const toggleCrossroadType = async () => {
    try {
      const current = toBool(d?.crossroadType);
      const next = !current;

      setStatus(`sending… (current=${String(current)} → next=${String(next)})`);
      console.log('toggleCrossroadType', { current, next, data });

      await saveSection({ crossroadType: next });

      setStatus(`ok (store now: ${String(toBool(d?.crossroadType))})`);
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
        <Button onClick={toggleCrossroadType}>
          CrossroadType ({String(crossroadType)})
        </Button>
      </div>

      <div>
        <Col>
          <div className="gap-2 mb-2">
            <Button className="btn--start" onClick={setStartAsync}>
              Start ({String(btnStart)})
            </Button>
          
            <Button className="btn--pause" onClick={setPauseAsync}>
              Pause ({String(btnPause)})
            </Button>
          
            <Button className="btn--stop" onClick={setStopAsync}>
              Stop ({String(btnStop)})
            </Button>  
          </div>

          <div className="gap-2 mb-2">
            <Button onClick={setCrossroadLightNorthGreen}>
              Crossroad North green ({String(trafficLightNorth_green)})
            </Button>
            <Button onClick={setCrossroadLightNorthYellow}>
              Crossroad North yellow ({String(trafficLightNorth_yellow)})
            </Button>
            <Button onClick={setCrossroadLightNorthRed}>
              Crossroad North red ({String(trafficLightNorth_red)})
            </Button>
          </div>

          <div className="gap-2 mb-2">
            <Button onClick={setCrossroadLightSouthGreen}>
              Crossroad South green ({String(trafficLightSouth_green)})
            </Button>
            <Button onClick={setCrossroadLightSouthYellow}>
              Crossroad South yellow ({String(trafficLightSouth_yellow)})
            </Button>
            <Button onClick={setCrossroadLightSouthRed}>
              Crossroad South red ({String(trafficLightSouth_red)})
            </Button>
          </div>

          <div className="gap-2 mb-2">
            <Button onClick={setCrossroadLightWestGreen}>
              Crossroad West green ({String(trafficLightWest_green)})
            </Button>
            <Button onClick={setCrossroadLightWestYellow}>
              Crossroad West yellow ({String(trafficLightWest_yellow)})
            </Button>
            <Button onClick={setCrossroadLightWestRed}>
              Crossroad West red ({String(trafficLightWest_red)})
            </Button>
          </div>

          <div className="gap-2 mb-2">
            <Button onClick={setCrossroadLightEastGreen}>
              Crossroad East green ({String(trafficLightEast_green)})
            </Button>
            <Button onClick={setCrossroadLightEastYellow}>
              Crossroad East yellow ({String(trafficLightEast_yellow)})
            </Button>
            <Button onClick={setCrossroadLightEastRed}>
              Crossroad East red ({String(trafficLightEast_red)})
            </Button>
          </div>

          {/*}
          <div className="gap-2 mb-2">
            <Button onClick={setPedestrianNorthGreen}>
              Pedestrian North green ({String(pedestrianNorth_green)})
            </Button>
            <Button onClick={setPedestrianNorthRed}>
              Pedestrian North red ({String(pedestrianNorth_red)})
            </Button>
          </div>
          */}

          <div className="gap-2 mb-2">
            <Button onClick={setPedestrianSouthGreen}>
              Pedestrian South green ({String(pedestrianSouth_green)})
            </Button>
            <Button onClick={setPedestrianSouthRed}>
              Pedestrian South red ({String(pedestrianSouth_red)})
            </Button>
          </div>

          <div className="gap-2 mb-2">
            <Button onClick={setPedestrianWestGreen}>
              Pedestrian West green ({String(pedestrianWest_green)})
            </Button>
            <Button onClick={setPedestrianWestRed}>
              Pedestrian West red ({String(pedestrianWest_red)})
            </Button>
          </div>
          
          {/*}
          <div className="gap-2 mb-2">
            <Button onClick={setPedestrianEastGreen}>
              Pedestrian East green ({String(pedestrianEast_green)})
            </Button>
            <Button onClick={setPedestrianEastRed}>
              Pedestrian East red ({String(pedestrianEast_red)})
            </Button>
          </div>
          */}
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
      d?.trafficLightNorth_green === undefined &&
      d?.trafficLightNorth_yellow === undefined &&
      d?.trafficLightNorth_red === undefined &&
      d?.trafficLightSouth_green === undefined &&
      d?.trafficLightSouth_yellow === undefined &&
      d?.trafficLightSouth_red === undefined &&
      d?.trafficLightWest_green === undefined &&
      d?.trafficLightWest_yellow === undefined &&
      d?.trafficLightWest_red === undefined &&
      d?.trafficLightEast_green === undefined &&
      d?.trafficLightEast_yellow === undefined &&
      d?.trafficLightEast_red === undefined &&
      //d?.pedestrianNorth_green === undefined &&
      //d?.pedestrianNorth_red === undefined &&
      d?.pedestrianSouth_green === undefined &&
      d?.pedestrianSouth_red === undefined &&
      d?.pedestrianWest_green === undefined &&
      d?.pedestrianWest_red === undefined 
      //d?.pedestrianEast_green === undefined &&
      //d?.pedestrianEast_red === undefined
    ) {
      saveSection({
        trafficLightNorth_green: 'false',
        trafficLightNorth_yellow: 'false',
        trafficLightNorth_red: 'false',
        trafficLightSouth_green: 'false',
        trafficLightSouth_yellow: 'false',
        trafficLightSouth_red: 'false',
        trafficLightWest_green: 'false',
        trafficLightWest_yellow: 'false',
        trafficLightWest_red: 'false',
        trafficLightEast_green: 'false',
        trafficLightEast_yellow: 'false',
        trafficLightEast_red: 'false',
        //pedestrianNorth_green: 'false',
        //pedestrianNorth_red: 'false',
        pedestrianSouth_green: 'false',
        pedestrianSouth_red: 'false',
        pedestrianWest_green: 'false',
        pedestrianWest_red: 'false',
        //pedestrianEast_green: 'false',
        //pedestrianEast_red: 'false',
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
    ? '/images/crossroad/crossroad_night_blank.png'
    : '/images/crossroad/crossroad_day_blank.png';

  useEffect(() => {
    ['/images/crossroad/crossroad_day_blank.png', '/images/crossroad/crossroad_night_blank.png'].forEach(src => {
      const img = new Image();
      img.src = src;
    });
  }, []);

  const CARN = {
    green: d?.trafficLightNorth_green ?? false,
    yellow: d?.trafficLightNorth_yellow ?? false,
    red: d?.trafficLightNorth_red ?? false,
  };

  const CARS = {
    green: d?.trafficLightSouth_green ?? false,
    yellow: d?.trafficLightSouth_yellow ?? false,
    red: d?.trafficLightSouth_red ?? false,
  };

  const CARW = {
    green: d?.trafficLightWest_green ?? false,
    yellow: d?.trafficLightWest_yellow ?? false,
    red: d?.trafficLightWest_red ?? false,
  };

  const CARE = {
    green: d?.trafficLightEast_green ?? false,
    yellow: d?.trafficLightEast_yellow ?? false,
    red: d?.trafficLightEast_red ?? false,
  };

  const PEDN = {
    green: d?.pedestrianNorth_green ?? false,
    red: d?.pedestrianNorth_red ?? false,
  };

  const PEDS = {
    green: d?.pedestrianSouth_green ?? false,
    red: d?.pedestrianSouth_red ?? false,
  };

  const PEDW = {
    green: d?.pedestrianWest_green ?? false,
    red: d?.pedestrianWest_red ?? false,
  };

  const PEDE = {
    green: d?.pedestrianEast_green ?? false,
    red: d?.pedestrianEast_red ?? false,
  };

  const lights = [
    // car – West
    { id: 'car-W-main-green', kind: 'car', color: 'green', state: CARW, dir: 90, x: '14%', y: '58.2%' },
    { id: 'car-W-main-yellow', kind: 'car', color: 'yellow', state: CARW, dir: 90, x: '16.2%', y: '58.2%' },
    { id: 'car-W-main-red', kind: 'car', color: 'red', state: CARW, dir: 90, x: '18.5%', y: '58.2%' },

    // car – East
    { id: 'car-E-main-green', kind: 'car', color: 'green', state: CARE, dir: 270, x: '41.6%', y: '24.1%' },
    { id: 'car-E-main-yellow', kind: 'car', color: 'yellow', state: CARE, dir: 270, x: '39.4%', y: '24.1%' },
    { id: 'car-E-main-red', kind: 'car', color: 'red', state: CARE, dir: 270, x: '37.2%', y: '24.1%' },

    // car – North
    { id: 'car-N-main-green', kind: 'car', color: 'green', state: CARN, dir: 180, x: '18.25%', y: '16.65%' },
    { id: 'car-N-main-yellow', kind: 'car', color: 'yellow', state: CARN, dir: 180, x: '18.25%', y: '20.65%' },
    { id: 'car-N-main-red', kind: 'car', color: 'red', state: CARN, dir: 180, x: '18.25%', y: '24.5%' },

    // car – South
    { id: 'car-S-main-green', kind: 'car', color: 'green', state: CARS, dir: 0, x: '37.3%', y: '58.0%' },
    { id: 'car-S-main-yellow', kind: 'car', color: 'yellow', state: CARS, dir: 0, x: '37.3%', y: '62%' },
    { id: 'car-S-main-red', kind: 'car', color: 'red', state: CARS, dir: 0, x: '37.3%', y: '65.8%' },

    // pedestrians N
    //{ id: 'ped-N-main-green', kind: 'ped', color: 'green', state: PEDN, dir: 180, x: '47.0%', y: '4.8%' },
    //{ id: 'ped-N-main-red', kind: 'ped', color: 'red', state: PEDN, dir: 180, x: '47.0%', y: '11.0%' },
    //{ id: 'ped-N-mirror-green', kind: 'ped', color: 'green', state: PEDN, dir: 180, x: '58.4%', y: '16.8%' },
    //{ id: 'ped-N-mirror-red', kind: 'ped', color: 'red', state: PEDN, dir: 180, x: '58.4%', y: '23.0%' },
    
    // pedestrians S
    { id: 'ped-S-main-green', kind: 'ped', color: 'green', state: PEDS, dir: 90, x: '17%', y: '83%' },
    { id: 'ped-S-main-red', kind: 'ped', color: 'red', state: PEDS, dir: 90, x: '19.3%', y: '83%' },
    { id: 'ped-S-mirror-green', kind: 'ped', color: 'green', state: PEDS, dir: 270, x: '39.15%', y: '83%' },
    { id: 'ped-S-mirror-red', kind: 'ped', color: 'red', state: PEDS, dir: 270, x: '36.9%', y: '83%' },

    // pedestrians W
    { id: 'ped-W-main-green', kind: 'ped', color: 'green', state: PEDW, dir: 0, x: '0.95%', y: '61%' },
    { id: 'ped-W-main-red', kind: 'ped', color: 'red', state: PEDW, dir: 0, x: '0.95%', y: '57%' },
    { id: 'ped-W-mirror-green', kind: 'ped', color: 'green', state: PEDW, dir: 180, x: '0.95%', y: '21.5%' },
    { id: 'ped-W-mirror-red', kind: 'ped', color: 'red', state: PEDW, dir: 180, x: '0.95%', y: '25.5%' },

    // pedestrians E
    //{ id: 'ped-E-main-green', kind: 'ped', color: 'green', state: PEDE, dir: 270, x: '89.2%', y: '29.5%' },
    //{ id: 'ped-E-main-red', kind: 'ped', color: 'red', state: PEDE, dir: 270, x: '94.0%', y: '29.5%' },
    //{ id: 'ped-E-mirror-green', kind: 'ped', color: 'green', state: PEDE, dir: 270, x: '77.6%', y: '41.0%' },
    //{ id: 'ped-E-mirror-red', kind: 'ped', color: 'red', state: PEDE, dir: 270, x: '82.4%', y: '41.0%' },
  ];

  const btnPedS1 = toBool(d?.btnWestCrosswalk1);
  const btnPedS2 = toBool(d?.btnWestCrosswalk2);
  const btnPedW1 = toBool(d?.btnSouthCrosswalk1);
  const btnPedW2 = toBool(d?.btnSouthCrosswalk2);

  const togglePedS1 = () => saveSection({ btnWestCrosswalk1: !btnPedS1 });
  const togglePedS2 = () => saveSection({ btnWestCrosswalk2: !btnPedS2 });
  const togglePedW1 = () => saveSection({ btnSouthCrosswalk1: !btnPedW1 });
  const togglePedW2 = () => saveSection({ btnSouthCrosswalk2: !btnPedW2 });

  return (
    <Row className="crossroadpage">
      <Col xs={12} lg={8}>
        
        <div className="mt-3">
          {/*<Picture name={names[idx]} ext={ext} folder={folder} />*/}
          <CrossroadCanvas
            background={background}
            lights={lights}
            pedControls={{
              btnPedS1,
              btnPedS2,
              btnPedW1,
              btnPedW2,
              onSouth1: togglePedS1,
              onSouth2: togglePedS2,
              onWest1: togglePedW1,
              onWest2: togglePedW2
            }}
          /> 
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
