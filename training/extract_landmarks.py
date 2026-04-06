"""
extract_landmarks.py
Processes ASL alphabet images through MediaPipe HandLandmarker (Tasks API)
to extract 21 hand landmarks (63 features: x, y, z per landmark) per image.
Outputs a CSV ready for model training.

SETUP: Before running, download the model file:
  wget -q https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/1/hand_landmarker.task
  Place it in the training/ directory (same level as this script).
"""

import csv
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import vision
from pathlib import Path
import time

# ── Config ──────────────────────────────────────────────────────
RAW_DATA_DIR = Path("data/raw/asl_alphabet_train")
OUTPUT_CSV = Path("data/processed/landmarks.csv")
MODEL_PATH = Path("hand_landmarker.task")
NUM_LANDMARKS = 21

# ── Setup MediaPipe HandLandmarker (Tasks API) ──────────────────
base_options = python.BaseOptions(model_asset_path=str(MODEL_PATH))
options = vision.HandLandmarkerOptions(
    base_options=base_options,
    num_hands=1
)
detector = vision.HandLandmarker.create_from_options(options)


def extract_landmarks(image_path: str) -> list[float] | None:
    """
    Run MediaPipe HandLandmarker on an image and return a flat list
    of 63 floats (21 landmarks * 3 coords) or None if no hand detected.
    """
    try:
        mp_image = mp.Image.create_from_file(str(image_path))
    except Exception:
        return None

    result = detector.detect(mp_image)

    if not result.hand_landmarks:
        return None

    hand_landmarks = result.hand_landmarks[0]

    landmarks = []
    for lm in hand_landmarks:
        landmarks.extend([lm.x, lm.y, lm.z])

    return landmarks


def build_csv_header() -> list[str]:
    header = []
    for i in range(NUM_LANDMARKS):
        header.extend([f"x{i}", f"y{i}", f"z{i}"])
    header.append("label")
    return header


def main():
    if not MODEL_PATH.exists():
        print(f"ERROR: Model file not found at {MODEL_PATH}")
        print("Download it with:")
        print("  wget https://storage.googleapis.com/mediapipe-models/hand_landmarker/hand_landmarker/float16/1/hand_landmarker.task")
        return

    if not RAW_DATA_DIR.exists():
        print(f"ERROR: Dataset not found at {RAW_DATA_DIR}")
        print("Download from: https://www.kaggle.com/datasets/grassknoted/asl-alphabet")
        return

    OUTPUT_CSV.parent.mkdir(parents=True, exist_ok=True)

    class_dirs = sorted([
        d for d in RAW_DATA_DIR.iterdir()
        if d.is_dir()
    ])

    print(f"Found {len(class_dirs)} classes: {[d.name for d in class_dirs]}")

    total_processed = 0
    total_skipped = 0

    with open(OUTPUT_CSV, "w", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(build_csv_header())

        for class_dir in class_dirs:
            label = class_dir.name
            image_files = list(class_dir.glob("*.jpg")) + list(class_dir.glob("*.png"))

            class_processed = 0
            class_skipped = 0
            start_time = time.time()

            for img_path in image_files:
                landmarks = extract_landmarks(img_path)

                if landmarks is not None:
                    writer.writerow(landmarks + [label])
                    class_processed += 1
                else:
                    class_skipped += 1

            elapsed = time.time() - start_time
            total_processed += class_processed
            total_skipped += class_skipped

            print(
                f"  [{label:>10}] "
                f"Extracted: {class_processed:>4} | "
                f"Skipped (no hand): {class_skipped:>4} | "
                f"Time: {elapsed:.1f}s"
            )

    print(f"\n{'='*60}")
    print(f"DONE. Total extracted: {total_processed} | Skipped: {total_skipped}")
    print(f"Output: {OUTPUT_CSV}")

    detector.close()


if __name__ == "__main__":
    main()