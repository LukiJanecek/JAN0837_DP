import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

import '../App.css';
import 'bootstrap/dist/css/bootstrap.min.css';

import TimeDate from './TimeDate.js';
import Picture from './Picture.js';
import Clock from './Clock.js';

function MainPage() {
  return (
    <div>
      <h1>Hey, this is my main page. xD</h1>
    </div>
  )
}

export default MainPage;