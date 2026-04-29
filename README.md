<a id="readme-top"></a>

[![Live Site](https://img.shields.io/website?url=https%3A%2F%2Fsignlearn-web.azurewebsites.net&label=Live%20Site&up_message=Online&down_message=Offline)](https://signlearn-web.azurewebsites.net)
[![CI](https://github.com/wrnasir/signCV/actions/workflows/ci.yml/badge.svg)](https://github.com/wrnasir/signCV/actions/workflows/ci.yml)
[![CD](https://github.com/wrnasir/signCV/actions/workflows/cd.yml/badge.svg)](https://github.com/wrnasir/signCV/actions/workflows/cd.yml)



<br />
<div align="center">
  <h1>🤟 SignLearn</h1>

  <p align="center">
    Learn ASL through real-time computer vision challenges. Powered by a custom-trained ML model and LLM-generated content.
    <br />
    <br />
  </p>
</div>
    

## About The Project

![SignLearn Screenshot](https://github.com/wrnasir/signCV/blob/wrnasir/readme/diagrams/SignLearn_Home.png?raw=true)

SignLearn is a full-stack ASL (American Sign Language) learning platform that teaches users to sign through interactive, real-time webcam challenges. A custom-trained MLP neural network classifier recognizes 26 ASL alphabet signs at ~98% accuracy, while a Groq-powered LLM generates adaptive spelling challenges that scale to the user's skill level.

No video ever leaves the browser; MediaPipe extracts hand landmarks client-side and sends only 500 bytes of coordinate data to the backend per prediction. The ML model was trained in Python on 87,000 images, exported to ONNX, and runs natively in C# via ONNX Runtime with zero Python dependency in production.

## Core Features

**ML Training Pipeline**
* Extracts 21 hand landmarks (63 features) from 87k ASL images via MediaPipe HandLandmarker model
* Trains an MLP neural network classifier (128-64-32 hidden layers, ~98% accuracy) on landmark geometry
* Exports to ONNX format for cross-platform inference — bridges Python training to C# production
* Supports both left and right hand detection via real-time x-coordinate mirroring

**ASL Recognition Backend**
* Loads ONNX model as a singleton via ONNX Runtime
* REST endpoint accepts 63-float landmark vectors and returns predicted sign
* Service layer built on interfaces (`IAnalysisService`, `IGroqService`, `IChallengeService`) for testability and dependency injection
* Input validation, custom exception hierarchy, and structured error responses

**LLM Challenge Engine**
* GroqService wraps the Groq API (Llama 3.1 8B) as a generic prompt-response client
* ChallengeService builds context-aware prompts factoring skill level, streak, mastered signs, and used word blacklist
* Parses structured JSON from LLM responses with backtick stripping and case-insensitive deserialization

**Real-Time Frontend**
* MediaPipe Hands JS runs landmark detection client-side at 30fps
* Canvas overlay renders hand skeleton (21 landmarks + connections) on the webcam feed
* Interactive game loop: LLM generates a word → user signs each letter → instant per-letter feedback → streak tracking
* Built with React, TypeScript, and Tailwind CSS

**Testing & DevOps**
* xUnit test suite with Moq — covers services and controllers
* Dockerized backend and frontend with docker-compose for local development
* GitHub Actions CI pipeline — runs tests and validates Docker builds on every push/PR
* GitHub Actions CD pipeline — deploys to Azure App Service on merge to main


## Architecture

![diagram](https://github.com/wrnasir/signCV/blob/wrnasir/readme/diagrams/SignLearn_DataFlow.png?raw=true)


### Built With

* [![Python][Python-badge]][Python-url]
* [![CSharp][CSharp-badge]][CSharp-url]
* [![React][React.js]][React-url]
* [![TypeScript][TypeScript-badge]][TypeScript-url]
* [![TailwindCSS][Tailwind-badge]][Tailwind-url]
* [![Docker][Docker-badge]][Docker-url]



## Getting Started

### Prerequisites

* Docker
* A [Groq API key](https://console.groq.com) (free tier)

### Installation

1. Clone the repo
   ```sh
   git clone https://github.com/wrnasir/signCV.git
   cd signCV
   ```

2. Create a `.env` file in the project root with your Groq API key
   ```sh
   echo "GROQ_API_KEY=your-key-here" > .env
   ```

3. Build and run
   ```sh
   docker compose up --build
   ```

The backend runs on `http://localhost:5000` and frontend on `http://localhost:3000`.



<!-- MARKDOWN LINKS & IMAGES -->
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
[Docker-badge]: https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white
[Docker-url]: https://docker.com
