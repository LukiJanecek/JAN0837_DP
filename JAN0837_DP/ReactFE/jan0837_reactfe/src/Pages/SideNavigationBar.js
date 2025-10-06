import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import '../App.css';
import './SideNavigationBar.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import TimeDate from '../Components/TimeDate.js';
import Picture from '../Components/Picture.js';
import Clock from '../Components/Clock.js';

import PictureSwitcher from '../Components/PictureSwitcher.js';
import ResponsiveImage from '../Components/ResponsiveImage.js';

function SideNavigationBar() {
  return (
    <>
      <Nav className="nav_text">
        <Nav.Link as={NavLink} to="/mainpage">
          Main Page
        </Nav.Link>
        <Nav.Link as={NavLink} to="/crossroad">
           Crossroad
        </Nav.Link>
        <Nav.Link as={NavLink} to="/communication">
           Communication
        </Nav.Link>
      </Nav>

      <div className="sidebar-footer">
        <Clock />
        <ResponsiveImage name="/KKBI" /*alt=""*/ aspect="1 / 1" fit="contain"/>
        <ResponsiveImage name="/450 FEI-CZ_edited" /*alt=""*/ aspect="80 / 15" fit="contain"/>
      </div>
    </>
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