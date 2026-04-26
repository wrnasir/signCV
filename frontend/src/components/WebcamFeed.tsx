import React, { useRef, useEffect, useState, useCallback } from 'react';
import {
  HandLandmarker,
  FilesetResolver,
  NormalizedLandmark
} from '@mediapipe/tasks-vision';

import { BACKEND_URL } from '../config';

const HAND_CONNECTIONS: [number, number][] = [
  [0, 1], [1, 2], [2, 3], [3, 4],
  [0, 5], [5, 6], [6, 7], [7, 8],
  [0, 9], [9, 10], [10, 11], [11, 12],
  [0, 13], [13, 14], [14, 15], [15, 16],
  [0, 17], [17, 18], [18, 19], [19, 20],
  [5, 9], [9, 13], [13, 17]
];

const PREDICTION_INTERVAL_MS = 1500;

interface WebcamFeedProps {
  onPrediction?: (sign: string, confidence: number) => void;
}

const WebcamFeed: React.FC<WebcamFeedProps> = ({ onPrediction }) => {
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const handLandmarkerRef = useRef<HandLandmarker | null>(null);
  const animationFrameRef = useRef<number>(0);
  const lastPredictionTime = useRef<number>(0);

  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');

  const onPredictionRef = useRef(onPrediction);

  useEffect(() => {
    onPredictionRef.current = onPrediction;
  }, [onPrediction]);

  const initializeHandLandmarker = useCallback(async () => {
    try {
      const vision = await FilesetResolver.forVisionTasks(
        'https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@latest/wasm'
      );

      handLandmarkerRef.current = await HandLandmarker.createFromOptions(vision, {
        baseOptions: {
          modelAssetPath: '/hand_landmarker.task',
          delegate: 'GPU'
        },
        runningMode: 'VIDEO',
        numHands: 1
      });

      setIsLoading(false);
    } catch (err) {
      setError('Failed to load hand detection model');
      console.error(err);
    }
  }, []);

  const startWebcam = useCallback(async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { width: 640, height: 480, facingMode: 'user' }
      });

      if (videoRef.current) {
        videoRef.current.srcObject = stream;
      }
    } catch (err) {
      setError('Failed to access webcam. Check browser permissions.');
      console.error(err);
    }
  }, []);

  const sendForPrediction = async (landmarks: NormalizedLandmark[]) => {
    const now = performance.now();
    if (now - lastPredictionTime.current < PREDICTION_INTERVAL_MS) return;
    lastPredictionTime.current = now;

    const flatLandmarks: number[] = [];
    for (const lm of landmarks) {
      flatLandmarks.push(lm.x, lm.y, lm.z);
    }

    try {
      const response = await fetch(`${BACKEND_URL}/api/analysis`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ landmarks: flatLandmarks })
      });

      if (response.ok) {
        const result = await response.json();
        if (onPredictionRef.current) {
          onPredictionRef.current(result.recognizedSign, result.confidence);
        }
      }
    } catch (err) {
      console.error('Prediction failed:', err);
    }
  };

  const drawLandmarks = (
    ctx: CanvasRenderingContext2D,
    landmarks: NormalizedLandmark[],
    width: number,
    height: number
  ) => {
    ctx.strokeStyle = '#6c5ce7';
    ctx.lineWidth = 2;
    for (const [start, end] of HAND_CONNECTIONS) {
      const startLm = landmarks[start];
      const endLm = landmarks[end];
      ctx.beginPath();
      ctx.moveTo(startLm.x * width, startLm.y * height);
      ctx.lineTo(endLm.x * width, endLm.y * height);
      ctx.stroke();
    }

    for (const lm of landmarks) {
      ctx.beginPath();
      ctx.arc(lm.x * width, lm.y * height, 5, 0, 2 * Math.PI);
      ctx.fillStyle = '#e8e8ef';
      ctx.fill();
      ctx.strokeStyle = '#6c5ce7';
      ctx.lineWidth = 1.5;
      ctx.stroke();
    }
  };

  const detectFrame = useCallback(() => {
    const video = videoRef.current;
    const canvas = canvasRef.current;
    const handLandmarker = handLandmarkerRef.current;

    if (!video || !canvas || !handLandmarker || video.readyState < 2) {
      animationFrameRef.current = requestAnimationFrame(detectFrame);
      return;
    }

    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const results = handLandmarker.detectForVideo(video, performance.now());
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    if (results.landmarks && results.landmarks.length > 0) {
      const landmarks = results.landmarks[0];
      drawLandmarks(ctx, landmarks, canvas.width, canvas.height);
      sendForPrediction(landmarks);
    }

    animationFrameRef.current = requestAnimationFrame(detectFrame);
  }, []);

  useEffect(() => {
    initializeHandLandmarker();
    startWebcam();

    return () => {
      if (animationFrameRef.current) {
        cancelAnimationFrame(animationFrameRef.current);
      }
    };
  }, [initializeHandLandmarker, startWebcam]);

  useEffect(() => {
    if (!isLoading) {
      animationFrameRef.current = requestAnimationFrame(detectFrame);
    }
  }, [isLoading, detectFrame]);

  if (error) {
    return (
      <div className="text-red-400 text-sm p-10 text-center">{error}</div>
    );
  }

  return (
    <div className="relative inline-block rounded-2xl overflow-hidden border border-brand-600 bg-brand-800">
      {isLoading && (
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-10 text-sm text-muted">
          Loading hand detection...
        </div>
      )}

      <video
        ref={videoRef}
        autoPlay
        playsInline
        muted
        width={640}
        height={480}
        className="block -scale-x-100"
      />

      <canvas
        ref={canvasRef}
        width={640}
        height={480}
        className="absolute top-0 left-0 -scale-x-100 pointer-events-none"
      />
    </div>
  );
};

export default WebcamFeed;