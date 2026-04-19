import React, { useEffect, useState } from 'react';
import moment from 'moment';

class Clock extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
        time: new Date().toLocaleString()
    };
  }
    
  componentDidMount() {
    this.intervalID = setInterval(() => this.tick(), 1000);
  }

  componentWillUnmount() {
    clearInterval(this.intervalID);
  }

  tick() {
    this.setState({ // Automatically call render after state change
      time: new Date().toLocaleString()
    });
  }

  render() {
    return (this.state.time);
  }
}

export default Clock;