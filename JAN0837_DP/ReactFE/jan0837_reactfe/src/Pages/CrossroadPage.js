import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import '../App.css';
import './CrossroadPage.css';
import 'bootstrap-icons/font/bootstrap-icons.css';

import TimeDate from '../Components/TimeDate.js';
import Picture from '../Components/Picture.js';
import Clock from '../Components/Clock.js';

function CrossroadPage() {
  return (
    <div>
      <h1>Crossroad Page</h1>
      <p></p>
      <TimeDate />
      <p></p> 
    </div>
  ) 
}

export default CrossroadPage;