import React, { useRef, useEffect, useState, useCallback } from 'react';
import {
  HandLandmarker,
  FilesetResolver,
  NormalizedLandmark
} from '@mediapipe/tasks-vision';

const BACKEND_URL = 'http://localhost:5264';

// Connections between landmarks for drawing the hand skeleton
const HAND_CONNECTIONS: [number, number][] = [
  [0, 1], [1, 2], [2, 3], [3, 4],        // thumb
  [0, 5], [5, 6], [6, 7], [7, 8],        // index finger
  [0, 9], [9, 10], [10, 11], [11, 12],   // middle finger
  [0, 13], [13, 14], [14, 15], [15, 16], // ring finger
  [0, 17], [17, 18], [18, 19], [19, 20], // pinky
  [5, 9], [9, 13], [13, 17]              // palm
];

// Throttle predictions — don't send every single frame to the backend
const PREDICTION_INTERVAL_MS = 200; // ~5 predictions per second

const WebcamFeed: React.FC = () => {
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const handLandmarkerRef = useRef<HandLandmarker | null>(null);
  const animationFrameRef = useRef<number>(0);
  const lastPredictionTime = useRef<number>(0);

  const [prediction, setPrediction] = useState<string>('');
  const [confidence, setConfidence] = useState<number>(0);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');

  // ── Initialize MediaPipe HandLandmarker ──────────────────────
  const initializeHandLandmarker = useCallback(async () => {
    try {
      // Load WASM files needed by MediaPipe
      const vision = await FilesetResolver.forVisionTasks(
        'https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@latest/wasm'
      );

      // Create the hand landmarker with the .task model file
      // The model file must be in your public/ folder
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

  // ── Start webcam stream ──────────────────────────────────────
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

  // ── Send landmarks to backend for ASL prediction ─────────────
  const sendForPrediction = async (landmarks: NormalizedLandmark[]) => {
    const now = performance.now();

    // Throttle: skip if we sent a prediction too recently
    if (now - lastPredictionTime.current < PREDICTION_INTERVAL_MS) {
      return;
    }
    lastPredictionTime.current = now;

    // Flatten landmarks into the 63-float array the backend expects
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
        setPrediction(result.recognizedSign);
        setConfidence(result.confidence);
      }
    } catch (err) {
      // Don't interrupt the video feed for prediction errors
      console.error('Prediction failed:', err);
    }
  };

  // ── Draw landmarks and connections on the canvas ─────────────
  const drawLandmarks = (
    ctx: CanvasRenderingContext2D,
    landmarks: NormalizedLandmark[],
    width: number,
    height: number
  ) => {
    // Draw skeleton lines
    ctx.strokeStyle = '#00FF88';
    ctx.lineWidth = 2;
    for (const [start, end] of HAND_CONNECTIONS) {
      const startLm = landmarks[start];
      const endLm = landmarks[end];
      ctx.beginPath();
      ctx.moveTo(startLm.x * width, startLm.y * height);
      ctx.lineTo(endLm.x * width, endLm.y * height);
      ctx.stroke();
    }

    // Draw landmark dots
    for (const lm of landmarks) {
      ctx.beginPath();
      ctx.arc(lm.x * width, lm.y * height, 5, 0, 2 * Math.PI);
      ctx.fillStyle = '#FF4444';
      ctx.fill();
      ctx.strokeStyle = '#FFFFFF';
      ctx.lineWidth = 1;
      ctx.stroke();
    }
  };

  // ── Main detection loop — runs every animation frame ─────────
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

    // Run MediaPipe hand detection on the current video frame
    const results = handLandmarker.detectForVideo(video, performance.now());

    // Clear previous frame's drawings
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    if (results.landmarks && results.landmarks.length > 0) {
      const landmarks = results.landmarks[0];
      drawLandmarks(ctx, landmarks, canvas.width, canvas.height);
      sendForPrediction(landmarks);
    } else {
      setPrediction('');
      setConfidence(0);
    }

    // Schedule next frame
    animationFrameRef.current = requestAnimationFrame(detectFrame);
  }, []);

  // ── Lifecycle: init on mount, cleanup on unmount ─────────────
  useEffect(() => {
    initializeHandLandmarker();
    startWebcam();

    return () => {
      if (animationFrameRef.current) {
        cancelAnimationFrame(animationFrameRef.current);
      }
    };
  }, [initializeHandLandmarker, startWebcam]);

  // Start detection loop once model is loaded
  useEffect(() => {
    if (!isLoading) {
      animationFrameRef.current = requestAnimationFrame(detectFrame);
    }
  }, [isLoading, detectFrame]);

  // ── Render ───────────────────────────────────────────────────
  if (error) {
    return <div style={{ color: '#FF4444', fontSize: '18px' }}>{error}</div>;
  }

  return (
    <div style={{ position: 'relative', display: 'inline-block' }}>
      {isLoading && (
        <div style={{
          position: 'absolute',
          top: '50%',
          left: '50%',
          transform: 'translate(-50%, -50%)',
          zIndex: 10,
          fontSize: '18px'
        }}>
          Loading hand detection model...
        </div>
      )}

      {/* Raw webcam feed */}
      <video
        ref={videoRef}
        autoPlay
        playsInline
        muted
        width={640}
        height={480}
        style={{
          borderRadius: '12px',
          transform: 'scaleX(-1)' // Mirror the feed so it feels natural
        }}
      />

      {/* Canvas overlaid on video for drawing landmarks */}
      <canvas
        ref={canvasRef}
        width={640}
        height={480}
        style={{
          position: 'absolute',
          top: 0,
          left: 0,
          borderRadius: '12px',
          transform: 'scaleX(-1)' // Mirror to match video
        }}
      />

      {/* Prediction overlay */}
      {prediction && (
        <div style={{
          position: 'absolute',
          bottom: '20px',
          left: '50%',
          transform: 'translateX(-50%)',
          backgroundColor: 'rgba(0, 0, 0, 0.7)',
          padding: '12px 24px',
          borderRadius: '8px',
          textAlign: 'center',
          minWidth: '120px'
        }}>
          <div style={{ fontSize: '36px', fontWeight: 'bold' }}>
            {prediction}
          </div>
          <div style={{ fontSize: '14px', color: '#aaa', marginTop: '4px' }}>
            {(confidence * 100).toFixed(1)}% confidence
          </div>
        </div>
      )}
    </div>
  );
};

export default WebcamFeed;