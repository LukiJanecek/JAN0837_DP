import logo from './logo.svg';

import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';

import TimeDate from './Components/TimeDate.js';
import Picture from './Components/Picture.js';
import Clock from './Components/Clock.js';

import SideNavigationBar from './Components/SideNavigationBar.js';
import MainPage from './Components/MainPage.js';
import CrossroadPage from './Components/CrossroadPage.js';
import CommunicationPage from './Components/CommunicationPage.js';

function App() {
  return (
    <Container fluid className="app-container p-0">
      <Row className="g-0"> 
        <Col xs={12} md={3} lg={2}>
          <SideNavigationBar />
        </Col>


        <Col xs={12} md={9} lg={10} className="custom-content">
          <Routes>
            <Route path="/" element={<Navigate to="/mainpage" replace />} />
            <Route path="/mainpage" element={<div className="default-content"><MainPage /></div>}/>
            <Route path="/crossroad" element={<div className="default-content"><CrossroadPage /></div>}/>
            <Route path="/communication" element={<div className="default-content"><CommunicationPage /></div>}/>
          </Routes>
        </Col>
      </Row>
    </Container>
  );
}

export default App;

/*
    <div>
      <Container fluid>
        <Col xxl={3} xl={3} lg={4} md={5} sm={5.5} xs={6} className='custom-sidebar'>
          <SideNavigationBar />
        </Col>
        <Col xxl={9} xl={9} lg={8} md={7} sm={6.5} xs={6} className='custom-content custom-content-text'>
          <div className>
            <Routes>
                <Route path="/mainpage" element={<div className='default-content'><MainPage /></div>} />
                <Route path="/crossroad" element={<div className='default-content'><CrossroadPage /></div>}></Route>
            </Routes>
          </div>
        </Col>
      </Container>
    </div>
*/

/*
    <div className="App">
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.js</code> and save to reload.
        </p>
        <a
          className="App-link"
          href="https://reactjs.org"
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
      </header>
    </div>
*/


