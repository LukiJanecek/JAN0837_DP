import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image as RBImage, Form } from 'react-bootstrap';

import '../App.css';
import './MainPage.css';
import 'bootstrap/dist/css/bootstrap.min.css';

import TimeDate from '../Components/TimeDate.js';
import Picture from '../Components/Picture.js';
import Clock from '../Components/Clock.js';

function MainPage({ setAside }) {
  {/*
  useEffect(()=>{
    setAside(
      <div className="stack">
        <strong>Parameters:</strong>
        <label>Parameter1: <input type="number" defaultValue={100}/></label>
        <button>Start</button>
      </div>
    );
    return ()=> setAside(null);
  }, [setAside]);
  */}
  return (
    <div className="default-content p-3">
      <h1>Hey, this is my main page.</h1>
    </div>
  )
}

export default MainPage;