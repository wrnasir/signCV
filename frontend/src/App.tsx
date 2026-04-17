import React from 'react';
import WebcamFeed from './components/WebcamFeed';

function App() {
  return (
    <div style={{
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      minHeight: '100vh',
      backgroundColor: '#1a1a2e',
      color: '#ffffff',
      fontFamily: 'sans-serif'
    }}>
      <h1 style={{ marginBottom: '20px' }}>SignLearn</h1>
      <WebcamFeed />
    </div>
  );
}

export default App;