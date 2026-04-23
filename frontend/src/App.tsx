import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar';
import Landing from './components/Landing';
import Play from './components/Play';

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-brand-900 text-gray-100 flex flex-col font-body">
        <Navbar />
        <Routes>
          <Route path="/" element={<Landing />} />
          <Route path="/play" element={<Play />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;