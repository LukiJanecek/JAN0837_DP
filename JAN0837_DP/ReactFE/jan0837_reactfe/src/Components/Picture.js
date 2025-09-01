import React, { useState, useEffect } from 'react';
import { Link, NavLink } from "react-router-dom";
import { Container, Row, Col, Button, Nav, Image } from 'react-bootstrap';

const getBase = () => {
  const vite = (typeof import.meta !== 'undefined' && import.meta.env && import.meta.env.BASE_URL) || '';
  const cra  = (typeof process !== 'undefined' && process.env && process.env.PUBLIC_URL) || '';
  const base = vite || cra || '/';
  return base.endsWith('/') ? base.slice(0, -1) : base; 
};

function Picture({name, ext = 'png', folder = 'images', alt, rounded = true, fluid = true, aspect = '16 / 9', className = '', ...rest}) 
{
  const base = getBase();
  const cleanFolder = folder.replace(/^\/+/, ''); 
  const src = `${base}/${cleanFolder}/${name}.${ext}`;
  return (
    <Image
      src={src}
      alt={alt ?? name}
      rounded={rounded}
      fluid={fluid}
      className={className}
      onError={(e) => {
        //  fallback when picture not found 
        e.currentTarget.onerror = null;
        e.currentTarget.src = `${base}/${cleanFolder}/placeholder.${ext}`;
      }}
      {...rest}
    />
  );
}

export default Picture;