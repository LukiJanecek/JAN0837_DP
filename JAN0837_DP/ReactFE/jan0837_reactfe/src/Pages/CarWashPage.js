import React, { useState, useEffect } from 'react';
import { Row, Col, Button, Badge } from 'react-bootstrap';

import '../App.css';
import './CarWashPage.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import { useRefresh } from '../Communication/RefreshContext.js';
import { useData, useSectionData } from '../Communication/DataProvider.js';

/*
 * ── CarWash variable map ─────────────────────────
 *
 * INPUTS  (FE → PLC):
 *   btnCarWashEmergencyStop  Bool
 *   btnStartCarWash          Bool
 *   btnStopCarWash           Bool
 *   CarWashErrorSystem       Bool
 *   CarWashCarPosition       Bool
 *   CarWashShowerPosition    Bool
 *   CarWashMode              Int
 *
 * OUTPUTS (PLC → FE):
 *   CarWashLight_green       Bool
 *   CarWashLight_yellow      Bool
 *   CarWashLight_red         Bool
 *   CarWashDoor1_Up          Bool
 *   CarWashDoor1_Down        Bool
 *   CarWashDoor2_Up          Bool
 *   CarWashDoor2_Down        Bool
 *   CarWashChemicalsFront    Bool
 *   CarWashChemicalsSides    Bool
 *   CarWashChemicalsBack     Bool
 *   CarWashPrewash           Bool
 *   CarWashWater             Bool
 *   CarWashWax               Bool
 *   CarWashDry               Bool
 *   CarWashBrushes           Bool
 *   CarWashSoap              Bool
 *   CarWashActiveFoam        Bool
 *   CarWashTimeDoorMovement  Int
 *   CarWashMEMDoor           Bool
 *   CarWashMEMDoorTrig       Bool
 *   CarWashMEMDoorClosingtrig Bool
 */

const toBool = (v) => {
  if (typeof v === 'boolean') return v;
  const s = String(v ?? '').trim().toLowerCase();
  return s === 'true' || s === '1' || s === 'on';
};

function CarWashParamsSidebar() {
  const { interval, setInterval } = useRefresh();
  const { section: d, saveSection, data, error, isFetching, refresh } = useSectionData('CarWash');

  const btnEmergencyStop = toBool(d?.btnCarWashEmergencyStop);
  const btnStart = toBool(d?.btnStartCarWash);
  const btnStop = toBool(d?.btnStopCarWash);
  const errorSystem = toBool(d?.CarWashErrorSystem);
  const carPosition = toBool(d?.CarWashCarPosition);
  const showerPosition = toBool(d?.CarWashShowerPosition);
  const mode = String(d?.CarWashMode ?? '');
  const lightGreen = toBool(d?.CarWashLight_green);
  const lightYellow = toBool(d?.CarWashLight_yellow);
  const lightRed = toBool(d?.CarWashLight_red);

  const setEmergencyStop = () => saveSection({ btnCarWashEmergencyStop: !btnEmergencyStop });
  const setStartAsync = () => saveSection({ btnStartCarWash: !btnStart });
  const setStopAsync = () => saveSection({ btnStopCarWash: !btnStop });

  return (
    <div>
      <h3>Parameters:</h3>

      <div>
        <Col>
          <div className="gap-2 mb-2">
            <Button className="btn--start" onClick={setStartAsync}>
              Start ({String(btnStart)})
            </Button>
            <Button className="btn--stop" onClick={setStopAsync}>
              Stop ({String(btnStop)})
            </Button>
            <Button variant="danger" onClick={setEmergencyStop}>
              Emergency Stop ({String(btnEmergencyStop)})
            </Button>
          </div>

          <div className="gap-2 mb-2">
            <div><strong>Mode:</strong> {mode}</div>
            <div><strong>Error:</strong> {String(errorSystem)}</div>
            <div><strong>Car Position:</strong> {String(carPosition)}</div>
            <div><strong>Shower Position:</strong> {String(showerPosition)}</div>
          </div>

          <div className="gap-2 mb-2">
            <div><strong>Light:</strong> G={String(lightGreen)} Y={String(lightYellow)} R={String(lightRed)}</div>
          </div>
        </Col>

        <pre style={{background:'#f6f8fa', padding:8, borderRadius:6, marginTop:12}}>
          {JSON.stringify(data, null, 2)}
        </pre>
      </div>
    </div>
  );
}

function CarWashCanvas({ d }) {
  const b = (k) => toBool(d?.[k]);

  const lightG = b('CarWashLight_green');
  const lightY = b('CarWashLight_yellow');
  const lightR = b('CarWashLight_red');

  const door1Up = b('CarWashDoor1_Up');
  const door1Down = b('CarWashDoor1_Down');
  const door2Up = b('CarWashDoor2_Up');
  const door2Down = b('CarWashDoor2_Down');

  const brushes = b('CarWashBrushes');
  const water = b('CarWashWater');
  const wax = b('CarWashWax');
  const dry = b('CarWashDry');
  const soap = b('CarWashSoap');
  const prewash = b('CarWashPrewash');

  const carPresent = b('CarWashCarPosition');
  const showerPos = b('CarWashShowerPosition');

  const doorY1 = door1Down ? 220 : 40;
  const doorY2 = door2Down ? 220 : 40;

  return (
    <div className="carwash-canvas" style={{ width: '100%', marginTop: 16 }}>
      <svg viewBox="0 0 800 450" width="100%" style={{ borderRadius: 8, background: '#eef2f5' }}>
        {/* floor */}
        <rect x="0" y="300" width="800" height="150" fill="#cfd8dc" />

        {/* entrance doors (left/right) */}
        <rect x="40" y={doorY1} width="80" height="200" rx="8" fill="#7b8" stroke="#556" />
        <rect x="680" y={doorY2} width="80" height="200" rx="8" fill="#7b8" stroke="#556" />

        {/* car placeholder */}
        {carPresent ? (
          <g>
            <rect x="300" y="220" width="200" height="80" rx="20" fill="#2b6fb3" />
            <circle cx="350" cy="315" r="16" fill="#222" />
            <circle cx="450" cy="315" r="16" fill="#222" />
          </g>
        ) : (
          <g>
            <rect x="300" y="240" width="200" height="60" rx="14" fill="#9fb8d6" opacity="0.6" />
          </g>
        )}

        {/* brushes */}
        <g transform="translate(200,160)">
          <rect x="0" y="0" width="40" height="140" rx="6" fill={brushes ? '#ff6f61' : '#ddd'} />
        </g>
        <g transform="translate(560,160)">
          <rect x="0" y="0" width="40" height="140" rx="6" fill={brushes ? '#ff6f61' : '#ddd'} />
        </g>

        {/* water jets */}
        {water && (
          <g stroke="#4fc3f7" strokeWidth="4" strokeLinecap="round" opacity="0.9">
            <line x1="380" y1="200" x2="380" y2="260" />
            <line x1="420" y1="200" x2="420" y2="260" />
          </g>
        )}

        {/* soap / foam overlay */}
        {soap && (
          <g fill="#fff" opacity="0.7">
            <ellipse cx="400" cy="260" rx="160" ry="30" />
          </g>
        )}

        {/* shower position indicator */}
        {showerPos && <circle cx="400" cy="190" r="8" fill="#1976d2" />}

        {/* status lights */}
        <g transform="translate(700,20)">
          <circle cx="0" cy="0" r="12" fill={lightR ? '#d32f2f' : '#eee'} stroke="#333" />
          <circle cx="0" cy="30" r="12" fill={lightY ? '#fbc02d' : '#eee'} stroke="#333" />
          <circle cx="0" cy="60" r="12" fill={lightG ? '#388e3c' : '#eee'} stroke="#333" />
        </g>

        {/* labels */}
        <text x="20" y="30" fontSize="18" fill="#333">Car Wash</text>

        {/* small legend */}
        <g transform="translate(20,340)" fontSize="12" fill="#333">
          <text x="0" y="0">Prewash: {prewash ? 'ON' : 'OFF'}</text>
          <text x="0" y="16">Soap: {soap ? 'ON' : 'OFF'}</text>
          <text x="140" y="0">Wax: {wax ? 'ON' : 'OFF'}</text>
          <text x="140" y="16">Dry: {dry ? 'ON' : 'OFF'}</text>
        </g>
      </svg>
    </div>
  );
}

function CarWashPage() {
  const { section: d } = useSectionData?.('CarWash') ?? {};

  return (
    <div className="carwashpage">
      <Row className="carwashpage">
        <Col xs={12} lg={8}>
          <div className="mt-3">
            <CarWashCanvas d={d} />
          </div>
        </Col>

        <Col lg={4}>
          <CarWashParamsSidebar />
        </Col>
      </Row>

      {/* Canvas is now rendered inside the left column */}
    </div>
  );
}

export default CarWashPage;
