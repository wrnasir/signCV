<a id="readme-top"></a>

[![Live Site][site-shield]][site-url]



<br />
<div align="center">
  <h1>🤟 SignLearn</h1>

  <p align="center">
    Learn ASL through real-time computer vision challenges. Powered by a custom-trained ML model and LLM-generated content.
    <br />
    <br />
  </p>
</div>



<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#how-it-works">How It Works</a></li>
        <li><a href="#architecture">Architecture</a></li>
        <li><a href="#model-performance">Model Performance</a></li>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



## About The Project

[![SignLearn Screenshot][product-screenshot]](https://github.com/your-username/signlearn)

SignLearn is a full-stack ASL (American Sign Language) learning platform that teaches users to sign through interactive, real-time webcam challenges. A custom-trained RandomForest classifier recognizes 26 ASL alphabet signs at 98% accuracy, while a Groq-powered LLM generates adaptive spelling challenges that scale to the user's skill level.

No video ever leaves the browser — MediaPipe extracts hand landmarks client-side and sends only 500 bytes of coordinate data to the backend per prediction. The ML model was trained in Python on 87,000 images, exported to ONNX, and runs natively in C# via ONNX Runtime with zero Python dependency in production.



### How It Works

1. **Get a Word** — the LLM generates a word matched to your skill level, streak, and mastered signs
2. **Sign Each Letter** — hold up each ASL letter in front of your webcam. The camera reads your hand in real time
3. **Get Instant Feedback** — green for correct, move to the next letter. Complete the word to keep your streak alive

### Core Features

**ML Training Pipeline**
* Extracts 21 hand landmarks (63 features) from 87k ASL images via MediaPipe HandLandmarker
* Trains a RandomForest classifier (100 estimators, ~98% accuracy) on landmark geometry
* Exports to ONNX format for cross-platform inference — bridges Python training to C# production

**ASL Recognition Backend**
* Loads ONNX model as a singleton via ONNX Runtime — single load, zero cold starts
* REST endpoint accepts 63-float landmark vectors and returns predicted sign in <10ms
* Input validation, custom exception hierarchy, and structured error responses

**LLM Challenge Engine**
* GroqService wraps the Groq API (Llama 3.1 8B) as a generic prompt-response client
* ChallengeService builds context-aware prompts factoring skill level, streak, mastered signs, and used word blacklist
* Parses structured JSON from LLM responses with backtick stripping and case-insensitive deserialization

**Real-Time Frontend**
* MediaPipe Hands JS runs landmark detection client-side at 30fps — no video leaves the browser
* Canvas overlay renders hand skeleton (21 landmarks + connections) on the webcam feed
* Prediction callback architecture decouples webcam detection from game state via refs to avoid stale closures
* Throttled to ~5 backend calls/sec to balance responsiveness with server load




### Architecture

```
```



### Built With

* [![Python][Python-badge]][Python-url]
* [![CSharp][CSharp-badge]][CSharp-url]
* [![React][React.js]][React-url]
* [![TypeScript][TypeScript-badge]][TypeScript-url]
* [![TailwindCSS][Tailwind-badge]][Tailwind-url]



## Getting Started

To get a local copy up and running, follow these steps.

### Prerequisites

* Python 3.10+
* .NET 8 SDK
* Node.js 18+
* A [Groq API key](https://console.groq.com) (free tier)

### Installation

1. Clone the repo
   ```sh
   git clone https://github.com/your-username/signlearn.git
   cd signlearn
   ```

2. **Training Pipeline** (optional — pretrained model included)
   ```sh
   cd training
   pip install -r requirements.txt
   wget -O hand_landmarker.task https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/1/hand_landmarker.task
   # Download dataset from https://www.kaggle.com/datasets/grassknoted/asl-alphabet
   # Extract to data/raw/asl_alphabet_train/
   python extract_landmarks.py
   python train_model.py
   python evaluate.py
   ```

3. **Backend**
   ```sh
   cd backend/SignLearn.Api
   dotnet restore
   ```

4. Set your Groq API key in `Properties/launchSettings.json`
   ```json
   "environmentVariables": {
     "GROQ_API_KEY": "your-key-here"
   }
   ```

5. Run the backend
   ```sh
   dotnet run
   ```

6. **Frontend** (new terminal)
   ```sh
   cd frontend
   npm install
   cp ../training/hand_landmarker.task public/
   npm start
   ```

7. Open `http://localhost:3000` and start signing



## Usage

Start both the backend and frontend, then navigate to the app in your browser.

- **Landing Page** (`/`) — overview of the platform and how it works
- **Play** (`/play`) — the main game. A how-to-play modal appears on first visit. Sign each letter of the generated word to progress. Build a streak for harder challenges.

The difficulty scales automatically. The LLM considers your skill level, current streak, mastered signs, and previously used words to keep challenges fresh and appropriately difficult.



## Roadmap

- [x] ML training pipeline (MediaPipe → RandomForest → ONNX)
- [x] C# backend with ONNX Runtime inference
- [x] React frontend with real-time hand tracking
- [x] Groq LLM integration for adaptive challenge generation
- [x] Tailwind CSS frontend overhaul
- [ ] User authentication (JWT)
- [ ] Student progress tracking (EF Core + SQL Server)
- [ ] SignalR for WebSocket-based real-time predictions
- [ ] Docker + GitHub Actions CI/CD
- [ ] Azure deployment

See the [open issues](https://github.com/your-username/signlearn/issues) for a full list of proposed features and known issues.



<!-- MARKDOWN LINKS & IMAGES -->
[site-shield]: https://img.shields.io/website?url=https%3A%2F%2Fyourdomain.com&label=Live%20Site&style=for-the-badge&up_message=Online&down_message=Offline
[site-url]: https://linkedin.com/in/your-linkedin
[product-screenshot]: images/screenshot.png
[Python-badge]: https://img.shields.io/badge/Python-3776AB?style=for-the-badge&logo=python&logoColor=white
[Python-url]: https://python.org
[CSharp-badge]: https://img.shields.io/badge/C%23-512BD4?style=for-the-badge&logo=dotnet&logoColor=white
[CSharp-url]: https://dotnet.microsoft.com
[React.js]: https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB
[React-url]: https://reactjs.org/
[TypeScript-badge]: https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white
[TypeScript-url]: https://typescriptlang.org
[Tailwind-badge]: https://img.shields.io/badge/Tailwind_CSS-06B6D4?style=for-the-badge&logo=tailwindcss&logoColor=white
[Tailwind-url]: https://tailwindcss.com
