import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter, Route, Routes } from "react-router";
import "./index.css";
import App from "./App.tsx";
import Layout from "./Layout.tsx";
import SystemsGame from "./SystemsGame.tsx";
import LinesGame from "./LinesGame.tsx";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<App />} />
          <Route path="/systems" element={<SystemsGame />} />
          <Route path="/lines" element={<LinesGame />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>,
);
