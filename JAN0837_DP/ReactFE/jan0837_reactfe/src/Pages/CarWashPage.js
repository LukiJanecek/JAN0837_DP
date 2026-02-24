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

function CarWashPage() {
  const { section: d } = useSectionData('CarWash');

  return (
    <Row className="carwashpage">
      <Col xs={12} lg={8}>
        <div className="mt-3">
          {/* TODO: CarWash visualisation placeholder */}
          <div className="carwash-placeholder d-flex align-items-center justify-content-center"
               style={{ width:'100%', aspectRatio:'16/9', background:'#e9ecef', borderRadius:8 }}>
            <span className="text-muted fs-5">Car Wash – vizualizace</span>
          </div>
        </div>
      </Col>

      <Col lg={4}>
        <CarWashParamsSidebar />
      </Col>
    </Row>
  );
}

export default CarWashPage;
