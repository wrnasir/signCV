# 🤟 SignLearn — Learn ASL, One Sign at a Time

![Python](https://img.shields.io/badge/Python-3.10+-blue?logo=python&logoColor=white)
![C#](https://img.shields.io/badge/C%23-.NET%208-purple?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-TypeScript-blue?logo=react&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green)

> *Breaking the communication barrier between the hearing and deaf communities — powered by computer vision and machine learning.*

## 🌟 Highlights

- **Real-time ASL recognition** — sign a letter, see it identified instantly on screen
- **Custom-trained ML model** — 98% accuracy across 26 ASL alphabet signs, trained on 87k images
- **No video leaves the browser** — MediaPipe runs client-side, sending only 500-byte landmark payloads to the backend
- **Cross-platform inference** — model trained in Python, served in C# via ONNX Runtime

## ℹ️ Overview

SignLearn is an AI-powered ASL (American Sign Language) learning platform that teaches users to sign through real-time webcam feedback. Point your camera at your hand, make a sign, and the system tells you what letter it sees — instantly.

Under the hood, a custom RandomForest classifier is trained on hand landmark geometry extracted from 87,000 ASL alphabet images using Google's MediaPipe. The trained model is exported to ONNX format and served by a C# ASP.NET Core backend, while a React frontend handles webcam capture, client-side hand tracking, and skeleton visualization — all in real time.

### System Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    React Frontend                        │
│  ┌──────────┐    ┌───────────────┐    ┌──────────────┐  │
│  │  Webcam   │───▶│  MediaPipe JS  │───▶│ Canvas Draw  │  │
│  │  Stream   │    │  (Landmarks)   │    │ (Skeleton)   │  │
│  └──────────┘    └──────┬────────┘    └──────────────┘  │
│                         │ 63 floats                      │
└─────────────────────────┼───────────────────────────────┘
                          ▼
┌─────────────────────────────────────────────────────────┐
│                 C# ASP.NET Core Backend                  │
│  ┌──────────────┐    ┌─────────────────┐                │
│  │  Analysis     │───▶│  ONNX Runtime    │                │
│  │  Controller   │    │  (Inference)     │                │
│  └──────────────┘    └────────┬────────┘                │
│                               │ Predicted Sign           │
│                               ▼                          │
│                      ┌─────────────────┐                │
│                      │   Label Map      │                │
│                      │  { 0: "A", ... } │                │
│                      └─────────────────┘                │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│              Python Training Pipeline                    │
│  ┌────────────┐   ┌────────────┐   ┌────────────────┐  │
│  │  87k Images │──▶│  MediaPipe  │──▶│  RandomForest   │  │
│  │  (Kaggle)   │   │  Landmarks  │   │  → ONNX Export  │  │
│  └────────────┘   └────────────┘   └────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Data Flow

| Step | Communication | Description |
|------|--------------|-------------|
| 1 | Webcam → MediaPipe JS | Browser captures video frames and extracts 21 hand landmarks client-side |
| 2 | Frontend → Backend | 63 landmark coordinates (x, y, z × 21 points) sent via REST |
| 3 | Backend → ONNX Runtime | Landmarks fed into trained model, returns predicted class index |
| 4 | Backend → Frontend | Predicted ASL letter + confidence score returned as JSON |

### Technologies

| Category | Stack |
|----------|-------|
| ML Training | Python, scikit-learn, MediaPipe, ONNX |
| Backend | C#, ASP.NET Core, ONNX Runtime |
| Frontend | React, TypeScript, MediaPipe JS |
| Model Format | ONNX (cross-platform) |
| Dataset | [ASL Alphabet](https://www.kaggle.com/datasets/grassknoted/asl-alphabet) (87k images, 29 classes) |

### ✍️ Authors

**Waleed** — CS student & software developer. Building tools at the intersection of AI and accessibility.
- [GitHub](https://github.com/your-username)

## 🚀 Usage

Start both the backend and frontend, then open your browser and show your hand to the camera:

```
# Terminal 1 — Backend
cd backend/SignLearn.Api
dotnet run

# Terminal 2 — Frontend
cd frontend
npm start
```

Sign a letter and see it recognized in real time with a confidence score overlay.

## ⬇️ Installation

### Prerequisites
- Python 3.10+
- .NET 8 SDK
- Node.js 18+

### Training Pipeline (optional — pretrained model included)
```bash
cd training
pip install -r requirements.txt

# Download MediaPipe model
wget -O hand_landmarker.task https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/1/hand_landmarker.task

# Download dataset from https://www.kaggle.com/datasets/grassknoted/asl-alphabet
# Extract to data/raw/asl_alphabet_train/

python extract_landmarks.py   # ~30-60 min
python train_model.py          # Trains + exports ONNX
python evaluate.py             # Validates against test set
```

### Backend
```bash
cd backend/SignLearn.Api
dotnet restore
dotnet run
```

### Frontend
```bash
cd frontend
npm install
cp ../training/hand_landmarker.task public/
npm start
```

## 📊 Model Performance

| Metric | Value |
|--------|-------|
| Overall Accuracy | ~98% |
| Best Performing | Most letters at 0.97–0.99 |
| Lowest Performing | M (0.91), N (0.91) |
| Input Features | 63 (21 landmarks × 3 coordinates) |
| Model Type | RandomForest (100 estimators) |
| Export Format | ONNX |

> M and N have the lowest accuracy because their hand shapes differ by only one finger position — a known challenge in ASL recognition.

## 🗺️ Roadmap

- [x] ML training pipeline (MediaPipe → RandomForest → ONNX)
- [x] C# backend with ONNX Runtime inference
- [x] React frontend with real-time hand tracking
- [ ] User authentication (JWT)
- [ ] Groq LLM integration for adaptive lesson generation
- [ ] Student progress tracking (EF Core + SQL Server)
- [ ] SignalR for WebSocket-based real-time predictions
- [ ] Docker + GitHub Actions CI/CD
- [ ] Azure deployment

## 💭 Feedback and Contributing

Found a bug or have a feature idea? [Open an issue](https://github.com/wrnasir/signcv/issues).

Contributions are welcome — whether it's improving model accuracy, adding new sign support, or enhancing the UI.