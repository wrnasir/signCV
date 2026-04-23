import React from 'react';
import { Link, useLocation } from 'react-router-dom';

const Navbar: React.FC = () => {
  const location = useLocation();

  return (
    <nav className="flex items-center justify-between px-10 py-4 bg-brand-800/80 border-b border-brand-600 sticky top-0 z-50 backdrop-blur-md">
      <Link to="/" className="flex items-center gap-2.5 font-display font-bold text-xl text-gray-100 hover:opacity-80 transition-opacity">
        <span className="text-2xl">🤟</span>
        <span className="tracking-tight">SignLearn</span>
      </Link>
      <div className="flex items-center gap-2">
        <Link
          to="/play"
          className={`px-5 py-2 rounded-lg text-sm font-medium transition-all ${
            location.pathname === '/play'
              ? 'text-brand-500 bg-brand-500/20'
              : 'text-muted hover:text-gray-100 hover:bg-brand-700'
          }`}
        >
          Play
        </Link>
      </div>
    </nav>
  );
};

export default Navbar;