import * as React from 'react';
import * as ReactDOMClient from 'react-dom/client';
import App from './App.tsx';
import { fetchUiConfig } from './utils/loadBundle';

import * as ReactRouterDOM from 'react-router-dom';
import * as QuasarUiReact from './index';

async function start() {
  const w = window as any;
  w.React = React;
  w.ReactDOM = ReactDOMClient;
  w.ReactDOMClient = ReactDOMClient;
  w.ReactRouterDOM = ReactRouterDOM;
  w.QuasarUiReact = QuasarUiReact;

  await fetchUiConfig();
  ReactDOMClient.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
      <App />
    </React.StrictMode>,
  );
}

start();
