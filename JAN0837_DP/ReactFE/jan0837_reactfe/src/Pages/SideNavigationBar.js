import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import '../App.css';
import './SideNavigationBar.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import TimeDate from '../Components/TimeDate.js';
import Picture from '../Components/Picture.js';
import Clock from '../Components/Clock.js'

function SideNavigationBar() {
  return (
    <aside className="custom-sidebar">
      <Nav className="flex-column">
        <Nav.Link as={NavLink} to="/mainpage" className="custom-nav-link" end>
          <i className="bi bi-house-door-fill" /> Main Page
        </Nav.Link>
        <Nav.Link as={NavLink} to="/crossroad" className="custom-nav-link">
          <i className="bi bi-geo-alt-fill" /> Crossroad
        </Nav.Link>
        <Nav.Link as={NavLink} to="/communication" className="custom-nav-link">
          <i className="bi bi-geo-alt-fill" /> Communication
        </Nav.Link>
      </Nav>

      <div className="sidebar-footer">
        <Clock />
      </div>
    </aside>
  );
}

export default SideNavigationBar;

/*
    <div>
      <nav>
        <div>
          <Link to="/mainpage" className="custom-nav-link" style={{ marginTop: '30px' }}>main page</Link>
          <Link to="/crossroad" className="custom-nav-link">crossroad</Link>

          <div style={{marginLeft: '50px', marginTop:'50%', color: 'white', fontSize: '15px', fontWeight: 'Bold'}}>
            <Clock />
          </div>

          <Nav fill variant="tabs" defaultActiveKey="/home">
            <Nav.Item>
              <Nav.Link href="/mainpage" className="custom-nav-link">home page</Nav.Link>
            </Nav.Item>
            <Nav.Item>
              <Nav.Link href="/crossroad" className="custom-nav-link">crossroad</Nav.Link>
            </Nav.Item>
          </Nav>
        </div>
      </nav>
    </div>
*/