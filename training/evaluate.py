"""
evaluate.py
Tests the trained ONNX model against the separate Kaggle test set
or individual images. Uses the new MediaPipe Tasks API.

SETUP: Requires hand_landmarker.task in the training/ directory.
"""

import numpy as np
import json
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from pathlib import Path
import onnxruntime as ort

# ── Config ──────────────────────────────────────────────────────
ONNX_MODEL_PATH = Path("models/asl_classifier.onnx")
LABEL_MAP_PATH = Path("models/label_map.json")
TEST_DATA_DIR = Path("data/raw/asl_alphabet_test")
HAND_MODEL_PATH = Path("hand_landmarker.task")

# ── Load ONNX model and label map ──────────────────────────────
session = ort.InferenceSession(str(ONNX_MODEL_PATH))
input_name = session.get_inputs()[0].name

with open(LABEL_MAP_PATH) as f:
    label_map = {int(k): v for k, v in json.load(f).items()}

# ── Setup MediaPipe HandLandmarker ──────────────────────────────
base_options = python.BaseOptions(model_asset_path=str(HAND_MODEL_PATH))
options = vision.HandLandmarkerOptions(
    base_options=base_options,
    num_hands=1
)
detector = vision.HandLandmarker.create_from_options(options)


def extract_landmarks(image_path: str) -> np.ndarray | None:
    """Extract 63 landmark features from an image."""
    try:
        mp_image = mp.Image.create_from_file(str(image_path))
    except Exception:
        return None

    result = detector.detect(mp_image)

    if not result.hand_landmarks:
        return None

    hand = result.hand_landmarks[0]
    landmarks = []
    for lm in hand:
        landmarks.extend([lm.x, lm.y, lm.z])

    return np.array(landmarks, dtype=np.float32).reshape(1, -1)


def predict(landmarks: np.ndarray) -> str:
    """Run ONNX inference and return predicted label."""
    result = session.run(None, {input_name: landmarks})
    predicted_index = int(result[0][0])
    return label_map[predicted_index]


def test_kaggle_set():
    """Run predictions against the Kaggle test set."""
    if not TEST_DATA_DIR.exists():
        print(f"Test directory not found: {TEST_DATA_DIR}")
        print("Download the test set from Kaggle and extract to data/raw/asl_alphabet_test/")
        return

    correct = 0
    total = 0
    skipped = 0
    misses = []

    test_images = list(TEST_DATA_DIR.glob("*.jpg")) + list(TEST_DATA_DIR.glob("*.png"))

    if not test_images:
        # Subdirectory structure (class folders)
        for class_dir in sorted(TEST_DATA_DIR.iterdir()):
            if not class_dir.is_dir():
                continue
            expected_label = class_dir.name
            for img_path in class_dir.glob("*"):
                landmarks = extract_landmarks(str(img_path))
                if landmarks is None:
                    skipped += 1
                    continue

                prediction = predict(landmarks)
                total += 1
                if prediction == expected_label:
                    correct += 1
                else:
                    misses.append((expected_label, prediction, img_path.name))
    else:
        # Flat structure — filename contains the label (e.g., "A_test.jpg")
        for img_path in test_images:
            expected_label = img_path.stem.split("_")[0]

            landmarks = extract_landmarks(str(img_path))
            if landmarks is None:
                skipped += 1
                continue

            prediction = predict(landmarks)
            total += 1
            if prediction == expected_label:
                correct += 1
            else:
                misses.append((expected_label, prediction, img_path.name))

    print(f"\n{'='*50}")
    print(f"TEST RESULTS")
    print(f"{'='*50}")
    if total > 0:
        print(f"Correct:  {correct}/{total} ({correct/total*100:.1f}%)")
    else:
        print("No images processed")
    print(f"Skipped:  {skipped} (no hand detected)")

    if misses:
        print(f"\nMisclassifications ({len(misses)}):")
        for expected, got, filename in misses[:20]:
            print(f"  {filename}: expected '{expected}', got '{got}'")
        if len(misses) > 20:
            print(f"  ... and {len(misses) - 20} more")


def test_single_image(image_path: str):
    """Test a single image — useful for quick sanity checks."""
    landmarks = extract_landmarks(image_path)
    if landmarks is None:
        print(f"No hand detected in {image_path}")
        return

    prediction = predict(landmarks)
    print(f"Prediction: {prediction}")


if __name__ == "__main__":
    import sys

    if len(sys.argv) > 1:
        test_single_image(sys.argv[1])
    else:
        test_kaggle_set()