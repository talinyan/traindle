import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter, Route, Routes } from 'react-router';
import './index.css';
import App from './App.tsx';
import Layout from './components/Layout.tsx';
import SystemsGame from './components/SystemsGame.tsx';
import LinesGame from './components/LinesGame.tsx';
import ChangeLog from './components/ChangeLog.tsx';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path='/' element={<App />} />
          <Route path='/daily/systems' element={<SystemsGame key='daily' mode='daily' />} />
          <Route path='/daily/lines' element={<LinesGame key='daily' mode='daily' />} />
          <Route path='/endless/systems' element={<SystemsGame key='random' mode='random' />} />
          <Route path='/endless/lines' element={<LinesGame key='random' mode='random' />} />
          <Route path='/changelog' element={<ChangeLog />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>
);
