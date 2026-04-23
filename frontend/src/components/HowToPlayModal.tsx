import React from 'react';

interface HowToPlayModalProps {
  onClose: () => void;
}

const HowToPlayModal: React.FC<HowToPlayModalProps> = ({ onClose }) => {
  const steps = [
    'A word will appear on screen. Your goal is to spell it out using ASL hand signs.',
    'Sign each letter one at a time in front of your webcam. Hold the sign steady until it registers.',
    'The active letter pulses purple. Green means correct. Complete the word to move on.',
    'Build a streak! The longer your streak, the harder the words get.'
  ];

  return (
    <div className="fixed inset-0 bg-black/75 flex items-center justify-center z-[200] backdrop-blur-sm">
      <div className="bg-brand-700 border border-brand-600 rounded-2xl p-10 max-w-md w-[90%]">
        <h2 className="font-display text-2xl font-bold text-center mb-7">How to Play</h2>

        <div className="flex flex-col gap-5 mb-8">
          {steps.map((text, i) => (
            <div key={i} className="flex items-start gap-4">
              <span className="flex items-center justify-center min-w-[28px] h-7 rounded-full bg-brand-500 text-white font-display text-xs font-bold">
                {i + 1}
              </span>
              <p className="text-sm leading-relaxed text-muted pt-0.5">{text}</p>
            </div>
          ))}
        </div>

        <button
          onClick={onClose}
          className="w-full py-3.5 rounded-xl bg-brand-500 text-white font-semibold text-sm hover:-translate-y-0.5 transition-all shadow-lg shadow-brand-500/30 hover:shadow-xl hover:shadow-brand-500/40"
        >
          Let's Go
        </button>
      </div>
    </div>
  );
};

export default HowToPlayModal;