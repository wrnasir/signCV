"""
train_model.py
Loads extracted landmark data, trains a classifier, evaluates accuracy,
and exports the model to ONNX format for use in C# backend.
"""

import numpy as np
import pandas as pd
from pathlib import Path
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.neural_network import MLPClassifier
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import classification_report, confusion_matrix, accuracy_score
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType
import matplotlib.pyplot as plt
import seaborn as sns
import pickle
import time

# ── Config ──────────────────────────────────────────────────────
LANDMARKS_CSV = Path("data/processed/landmarks.csv")
MODELS_DIR = Path("models")
ONNX_OUTPUT = MODELS_DIR / "asl_classifier.onnx"
LABEL_ENCODER_OUTPUT = MODELS_DIR / "label_encoder.pkl"
TEST_SIZE = 0.2
RANDOM_STATE = 42

# Choose which model to train: "rf" (RandomForest) or "mlp" (Neural Net)
MODEL_TYPE = "rf"

def load_data():
    """Load landmarks CSV and split into features/labels."""
    print("Loading data...")
    df = pd.read_csv(LANDMARKS_CSV)
    
    print(f"  Total samples: {len(df)}")
    print(f"  Classes: {df['label'].nunique()}")
    print(f"  Samples per class:\n{df['label'].value_counts().to_string()}\n")
    # Drop classes with too few samples for train/test split
    min_samples = 10
    class_counts = df["label"].value_counts()
    small_classes = class_counts[class_counts < min_samples].index.tolist()
    if small_classes:
        print(f"  Dropping classes with <{min_samples} samples: {small_classes}")
        df = df[~df["label"].isin(small_classes)]
        
    X = df.drop("label", axis=1).values.astype(np.float32)
    y = df["label"].values
    
    return X, y

def train_random_forest(X_train, y_train):
    """Train a RandomForest classifier."""
    print("Training RandomForest...")
    model = RandomForestClassifier(
        n_estimators=100,
        max_depth=30,
        n_jobs=-1,            # Use all CPU cores
        random_state=RANDOM_STATE,
        verbose=1
    )
    model.fit(X_train, y_train)
    return model

def train_mlp(X_train, y_train):
    """Train an MLP (neural network) classifier."""
    print("Training MLP Neural Network...")
    model = MLPClassifier(
        hidden_layer_sizes=(128, 64, 32),
        activation="relu",
        max_iter=300,
        random_state=RANDOM_STATE,
        verbose=True
    )
    model.fit(X_train, y_train)
    return model

def evaluate(model, X_test, y_test, label_encoder):
    """Print classification metrics and save confusion matrix plot."""
    print("\nEvaluating model...")
    y_pred = model.predict(X_test)
    
    accuracy = accuracy_score(y_test, y_pred)
    print(f"\n  Accuracy: {accuracy:.4f} ({accuracy*100:.2f}%)\n")
    
    # Decode labels for readable report
    target_names = label_encoder.classes_
    print(classification_report(y_test, y_pred, target_names=target_names))
    
    # Confusion matrix
    cm = confusion_matrix(y_test, y_pred)
    plt.figure(figsize=(16, 14))
    sns.heatmap(
        cm, annot=True, fmt="d", cmap="Blues",
        xticklabels=target_names,
        yticklabels=target_names
    )
    plt.title(f"Confusion Matrix (Accuracy: {accuracy:.2%})")
    plt.xlabel("Predicted")
    plt.ylabel("Actual")
    plt.tight_layout()
    
    cm_path = MODELS_DIR / "confusion_matrix.png"
    plt.savefig(cm_path, dpi=150)
    print(f"  Confusion matrix saved to {cm_path}")
    
    return accuracy

def export_to_onnx(model, n_features):
    """Export sklearn model to ONNX format for C# consumption."""
    print("\nExporting to ONNX...")
    
    initial_type = [("input", FloatTensorType([None, n_features]))]
    onnx_model = convert_sklearn(model, initial_types=initial_type)
    
    ONNX_OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    with open(ONNX_OUTPUT, "wb") as f:
        f.write(onnx_model.SerializeToString())
    
    print(f"  ONNX model saved to {ONNX_OUTPUT}")
    print(f"  File size: {ONNX_OUTPUT.stat().st_size / 1024 / 1024:.1f} MB")

def main():
    MODELS_DIR.mkdir(parents=True, exist_ok=True)
    
    # Load data
    X, y = load_data()
    
    # Encode string labels to integers (needed for ONNX)
    label_encoder = LabelEncoder()
    y_encoded = label_encoder.fit_transform(y)
    
    print(f"Label mapping: {dict(zip(label_encoder.classes_, label_encoder.transform(label_encoder.classes_)))}\n")
    
    # Save label encoder for later use in C# (mapping predictions back to letters)
    with open(LABEL_ENCODER_OUTPUT, "wb") as f:
        pickle.dump(label_encoder, f)
    print(f"Label encoder saved to {LABEL_ENCODER_OUTPUT}")
    
    # Split
    X_train, X_test, y_train, y_test = train_test_split(
        X, y_encoded, test_size=TEST_SIZE, random_state=RANDOM_STATE, stratify=y_encoded
    )
    print(f"Train: {len(X_train)} | Test: {len(X_test)}\n")
    
    # Train
    start = time.time()
    if MODEL_TYPE == "rf":
        model = train_random_forest(X_train, y_train)
    elif MODEL_TYPE == "mlp":
        model = train_mlp(X_train, y_train)
    else:
        raise ValueError(f"Unknown model type: {MODEL_TYPE}")
    
    elapsed = time.time() - start
    print(f"\nTraining completed in {elapsed:.1f}s")
    
    # Evaluate
    accuracy = evaluate(model, X_test, y_test, label_encoder)
    
    # Export to ONNX
    n_features = X.shape[1]  # Should be 63
    export_to_onnx(model, n_features)
    
    # Save label classes as JSON for C# backend to load
    import json
    label_map = {int(i): label for i, label in enumerate(label_encoder.classes_)}
    label_map_path = MODELS_DIR / "label_map.json"
    with open(label_map_path, "w") as f:
        json.dump(label_map, f, indent=2)
    print(f"  Label map saved to {label_map_path}")
    
    print(f"\n{'='*60}")
    print(f"COMPLETE. Model accuracy: {accuracy:.2%}")
    print(f"ONNX model ready at: {ONNX_OUTPUT}")
    print(f"Next step: Copy {ONNX_OUTPUT} to backend/SignLearn.Api/wwwroot/models/")

if __name__ == "__main__":
    main()