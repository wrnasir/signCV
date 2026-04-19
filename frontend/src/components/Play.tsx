import React, { useState, useEffect, useCallback, useRef } from 'react';
import WebcamFeed from '../components/WebcamFeed';
import HowToPlayModal from '../components/HowToPlayModal';

const BACKEND_URL = 'http://localhost:5000';

interface Challenge {
  targetWord: string;
  hint: string;
  difficulty: string;
}

const difficultyStyles: Record<string, string> = {
  easy: 'text-green-400 bg-green-400/15',
  medium: 'text-yellow-400 bg-yellow-400/15',
  hard: 'text-red-400 bg-red-400/15',
};

const Play: React.FC = () => {
  const [challenge, setChallenge] = useState<Challenge | null>(null);
  const [currentLetterIndex, setCurrentLetterIndex] = useState<number>(0);
  const [letterStatuses, setLetterStatuses] = useState<('pending' | 'correct')[]>([]);
  const [streak, setStreak] = useState<number>(0);
  const [skillLevel, setSkillLevel] = useState<number>(5);
  const [masteredSigns, setMasteredSigns] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [wordComplete, setWordComplete] = useState<boolean>(false);
  const [showHowToPlay, setShowHowToPlay] = useState<boolean>(true);
  const [prediction, setPrediction] = useState<string>('');
  const [confidence, setConfidence] = useState<number>(0);

  const correctHoldCount = useRef<number>(0);
  const HOLD_THRESHOLD = 8;

  const fetchChallenge = useCallback(async () => {
    setIsLoading(true);
    setWordComplete(false);
    setCurrentLetterIndex(0);
    correctHoldCount.current = 0;

    try {
      const response = await fetch(`${BACKEND_URL}/api/challenge/generate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ skillLevel, masteredSigns, streak })
      });

      if (response.ok) {
        const data = await response.json();
        setChallenge(data);
        setLetterStatuses(new Array(data.targetWord.length).fill('pending'));
      }
    } catch (err) {
      console.error('Failed to fetch challenge:', err);
    } finally {
      setIsLoading(false);
    }
  }, [skillLevel, masteredSigns, streak]);

  useEffect(() => {
    if (!showHowToPlay) {
      fetchChallenge();
    }
  }, [showHowToPlay, fetchChallenge]);

  const handlePrediction = useCallback((predictedSign: string, conf: number) => {
    setPrediction(predictedSign);
    setConfidence(conf);

    if (!challenge || wordComplete) return;

    const targetLetter = challenge.targetWord[currentLetterIndex];

    if (predictedSign === targetLetter) {
      correctHoldCount.current += 1;

      if (correctHoldCount.current >= HOLD_THRESHOLD) {
        correctHoldCount.current = 0;

        setLetterStatuses(prev => {
          const updated = [...prev];
          updated[currentLetterIndex] = 'correct';
          return updated;
        });

        const nextIndex = currentLetterIndex + 1;

        if (nextIndex >= challenge.targetWord.length) {
          setWordComplete(true);
          setStreak(prev => prev + 1);

          const newMastered = new Set(masteredSigns);
          challenge.targetWord.split('').forEach(letter => newMastered.add(letter));
          setMasteredSigns(Array.from(newMastered));
        } else {
          setCurrentLetterIndex(nextIndex);
        }
      }
    } else {
      correctHoldCount.current = 0;
    }
  }, [challenge, currentLetterIndex, wordComplete, masteredSigns]);

  return (
    <div className="flex-1 flex items-center justify-center p-6 md:p-10">
      {showHowToPlay && (
        <HowToPlayModal onClose={() => setShowHowToPlay(false)} />
      )}

      <div className="flex flex-col-reverse md:flex-row gap-8 max-w-[1100px] w-full items-start">
        {/* Left — Challenge Panel */}
        <div className="flex-1 flex flex-col gap-6 p-8 bg-brand-700 border border-brand-600 rounded-2xl min-h-[400px]">
          {/* Streak */}
          <div className="flex items-center gap-3">
            <span className="text-xs font-medium text-muted uppercase tracking-widest">Streak</span>
            <span className="font-display text-2xl font-bold text-yellow-400">{streak}</span>
          </div>

          {isLoading ? (
            <div className="text-sm text-muted italic">Generating challenge...</div>
          ) : challenge ? (
            <>
              {/* Difficulty */}
              <span className={`font-display text-[11px] font-bold uppercase tracking-widest px-3.5 py-1 rounded-md w-fit ${difficultyStyles[challenge.difficulty] || 'text-muted bg-brand-600'}`}>
                {challenge.difficulty}
              </span>

              {/* Word Display */}
              <div className="flex gap-2.5 flex-wrap">
                {challenge.targetWord.split('').map((letter, index) => {
                  const isActive = index === currentLetterIndex && !wordComplete;
                  const isCorrect = letterStatuses[index] === 'correct';

                  return (
                    <div
                      key={index}
                      className={`
                        w-14 h-16 flex items-center justify-center
                        font-display text-2xl font-bold rounded-xl
                        border-2 transition-all duration-300
                        ${isCorrect
                          ? 'border-green-400 text-green-400 bg-green-400/10'
                          : isActive
                            ? 'border-brand-500 text-gray-100 bg-brand-800 shadow-lg shadow-brand-500/25 animate-pulse'
                            : 'border-brand-600 text-muted bg-brand-800'
                        }
                      `}
                    >
                      {letter}
                    </div>
                  );
                })}
              </div>

              {/* Hint */}
              <p className="text-sm text-muted leading-relaxed">
                <span className="font-semibold text-gray-100">Hint:</span> {challenge.hint}
              </p>

              {/* Current Prediction */}
              {prediction && !wordComplete && (
                <div className="flex items-center gap-3">
                  <span className="text-sm text-muted">You're signing:</span>
                  <span className={`font-display text-3xl font-bold transition-colors ${
                    prediction === challenge.targetWord[currentLetterIndex]
                      ? 'text-green-400'
                      : 'text-red-400'
                  }`}>
                    {prediction}
                  </span>
                </div>
              )}

              {/* Word Complete */}
              {wordComplete && (
                <div className="flex flex-col items-center gap-4 pt-2">
                  <span className="font-display text-xl font-bold text-green-400">
                    Nice! 🎉
                  </span>
                  <button
                    onClick={fetchChallenge}
                    className="px-8 py-3 rounded-xl bg-brand-500 text-white font-semibold text-sm hover:-translate-y-0.5 transition-all shadow-lg shadow-brand-500/30 hover:shadow-xl hover:shadow-brand-500/40"
                  >
                    Next Word
                  </button>
                </div>
              )}
            </>
          ) : null}
        </div>

        {/* Right — Webcam */}
        <div className="flex-shrink-0">
          <WebcamFeed onPrediction={handlePrediction} />
        </div>
      </div>
    </div>
  );
};

export default Play;