import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

function Picture() {
  return (
    <Container>
      <Row>
        <Col xs={6} md={4}>
          <Image src="name.jpg" alt="picture" rounded />
        </Col>
      </Row>
    </Container>
  );
}

export default Picture;