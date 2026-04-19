import React from 'react';
import { useNavigate } from 'react-router-dom';

const Landing: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div className="flex flex-col items-center px-6 md:px-10 pb-20">
      {/* Hero */}
      <div className="flex flex-col items-center text-center max-w-2xl pt-24 pb-20">
        <span className="font-display text-xs font-bold uppercase tracking-widest text-brand-500 bg-brand-500/15 px-4 py-1.5 rounded-full mb-6">
          AI-Powered ASL Learning
        </span>
        <h1 className="font-display text-4xl md:text-5xl font-bold leading-tight mb-6 tracking-tight">
          Learn Sign Language.
          <br />
          <span className="text-brand-500">One Sign at a Time.</span>
        </h1>
        <p className="text-base md:text-lg leading-relaxed text-muted max-w-xl mb-10">
          SignLearn uses real-time computer vision to watch your hands and teach you
          American Sign Language through interactive challenges. No sign language
          experience needed — just a webcam and your hands.
        </p>
        <button
          onClick={() => navigate('/play')}
          className="px-9 py-3.5 rounded-xl bg-brand-500 text-white font-semibold text-sm hover:-translate-y-0.5 transition-all shadow-lg shadow-brand-500/30 hover:shadow-xl hover:shadow-brand-500/40"
        >
          Start Signing
        </button>
      </div>

      {/* Features */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6 max-w-4xl w-full mb-24">
        {[
          {
            icon: '👁️',
            title: 'Real-Time Detection',
            desc: 'Your webcam tracks 21 hand landmarks in real time. Sign a letter and see it recognized instantly — no delay, no uploads.'
          },
          {
            icon: '🧠',
            title: 'AI-Scaled Difficulty',
            desc: "Challenges adapt to your skill level. The more you get right, the harder the words. Powered by LLM-generated content that keeps things fresh."
          },
          {
            icon: '🤟',
            title: '98% Accuracy',
            desc: 'Custom-trained ML model recognizes 26 ASL alphabet signs with 98% accuracy. Trained on 87,000 images and running natively in the browser.'
          }
        ].map((feature, i) => (
          <div
            key={i}
            className="bg-brand-700 border border-brand-600 rounded-2xl p-8 hover:border-brand-500 hover:-translate-y-1 transition-all duration-300 hover:shadow-lg hover:shadow-black/20"
          >
            <div className="text-3xl mb-4">{feature.icon}</div>
            <h3 className="font-display text-sm font-bold mb-2">{feature.title}</h3>
            <p className="text-sm leading-relaxed text-muted">{feature.desc}</p>
          </div>
        ))}
      </div>

      {/* How It Works */}
      <div className="max-w-lg w-full">
        <h2 className="font-display text-2xl font-bold text-center mb-12">How It Works</h2>
        <div className="flex flex-col">
          {[
            { num: '01', title: 'Get a Word', desc: 'AI generates a word matched to your skill level.' },
            { num: '02', title: 'Sign Each Letter', desc: 'Hold up each letter in ASL. The camera reads your hand in real time.' },
            { num: '03', title: 'Get Instant Feedback', desc: 'Green for correct, red for wrong. Keep your streak alive.' }
          ].map((step, i) => (
            <React.Fragment key={i}>
              <div className="flex items-start gap-6 py-5">
                <span className="font-display text-3xl font-bold text-brand-500 min-w-[56px]">
                  {step.num}
                </span>
                <div>
                  <h4 className="font-semibold mb-1">{step.title}</h4>
                  <p className="text-sm text-muted leading-relaxed">{step.desc}</p>
                </div>
              </div>
              {i < 2 && <div className="w-0.5 h-5 bg-brand-600 ml-7" />}
            </React.Fragment>
          ))}
        </div>
      </div>
    </div>
  );
};

export default Landing;