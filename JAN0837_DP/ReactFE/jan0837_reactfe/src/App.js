import React from 'react';
import logo from './logo.svg';
import './App.css';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';

import SideBar from './Components/SideBar';
import MainPage from './Components/MainPage';
import CrossroadPage from './Components/CrossroadPage';

function App() {
  return (
    <Router>
      <div style={{ display: "flex" }}>
        <SideBar />
        <div style={{ flex: 1, padding: "20px" }}>
          <Routes>
            <Route path="/mainpage" element={<MainPage />} />
            <Route path="/crossroad" element={<CrossroadPage />} />
          </Routes>
        </div>
      </div>
    </Router>
  );
}

export default App;
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
