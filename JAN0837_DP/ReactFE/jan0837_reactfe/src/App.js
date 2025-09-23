import logo from './logo.svg';

import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, NavLink, useLocation } from 'react-router-dom';
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';

import SideNavigationBar from './Pages/SideNavigationBar.js';
import MainPage from './Pages/MainPage.js';
import CrossroadPage from './Pages/CrossroadPage.js';
import CommunicationPage from './Pages/CommunicationPage.js';

function App() {
  const [aside, setAside] = useState(null);
  const location = useLocation();

  useEffect(()=>{ setAside(null); }, [location.pathname]);

  return (
    <div className="app" data-aside={aside ? "on" : "off"}>
      <div className="app__nav">
        <SideNavigationBar />
          {/* jednoduché odkazy */}
          {/* 
          <NavLink to="/mainpage" className={({isActive})=>`nav__link ${isActive?'is-active':''}`}>Main Page</NavLink>
          <NavLink to="/crossroad" className={({isActive})=>`nav__link ${isActive?'is-active':''}`}>Crossroad</NavLink>
          <NavLink to="/communication" className={({isActive})=>`nav__link ${isActive?'is-active':''}`}>Communication</NavLink>
          <div className="nav__footer">{new Date().toLocaleString()}</div>
          */}
      </div>
      
      
    
      <main className="app__main stack">
        <Routes>
          <Route path="/" element={<Navigate to="/mainpage" replace />} />
          <Route path="/mainpage" element={<MainPage setAside={setAside} />} />
          <Route path="/crossroad" element={<CrossroadPage setAside={setAside} />} />
          <Route path="/communication" element={<CommunicationPage setAside={setAside} />} />
        </Routes>
      </main>

      <aside className="app__aside">
        {aside}
      </aside>
    </div>
  );
}

export default App;

/*
    <Container fluid className="app-container p-0">
      <Row className="g-0 flex-nowrap"> 
        <Col xs={12} lg={2} className="bg-light border-end left-col">
          <SideNavigationBar />
        </Col>
        <Col xs={12} lg={10} className="custom-content">
          <Routes>
            <Route path="/" element={<Navigate to="/mainpage" replace />} />
            <Route path="/mainpage" element={<MainPage />}/>
            <Route path="/crossroad" element={<CrossroadPage />} />
            <Route path="/communication" element={<CommunicationPage />} />
          </Routes>
        </Col>
      </Row>
    </Container>
*/

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


