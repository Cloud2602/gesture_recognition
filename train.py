import os
import numpy as np
import matplotlib.pyplot as plt
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import confusion_matrix, ConfusionMatrixDisplay, classification_report

from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.callbacks import ModelCheckpoint

# === Funzione di normalizzazione ===
def normalize_hand(landmarks):
    wrist = landmarks[0]
    landmarks -= wrist
    scale = np.linalg.norm(landmarks[9] - wrist)  # indice MCP
    return landmarks / scale if scale > 0 else landmarks

# === Config ===
DATA_PATH = "/content/gesture_recognition/dataset_landmarks_augumantation"
LABELS = ["0", "1", "2", "3"]
NUM_FEATURES = 126  # 21 punti * 3 coordinate * 2 mani

# === Caricamento dati ===
X, Y = [], []
for file_name in os.listdir(DATA_PATH):
    if file_name.endswith(".npy"):
        file_path = os.path.join(DATA_PATH, file_name)
        frame = np.load(file_path)
        for sample in frame:
            sample = normalize_hand(sample)
            sample = sample.reshape(-1)
            X.append(sample)
            Y.append(int(file_name.split(".")[0].split("_")[1]))

X = np.array(X)
Y = to_categorical(np.array(Y), num_classes=len(LABELS))

# === Preprocessing ===
scaler = StandardScaler()
X = scaler.fit_transform(X)

# === Train/test split ===
X_train, X_test, y_train, y_test = train_test_split(X, Y, test_size=0.2, random_state=42)

# === Modello MLP ===
model = Sequential([
    Dense(64, activation='relu', input_shape=(NUM_FEATURES,)),
    Dropout(0.4),
    Dense(32, activation='relu'),
    Dropout(0.3),
    Dense(len(LABELS), activation='softmax')
])

model.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])


# Callback per salvare il modello con la miglior accuratezza di validazione
checkpoint_cb = ModelCheckpoint(
    "best_model.h5",               # Nome del file da salvare
    monitor='val_accuracy',        # Metri da monitorare
    save_best_only=True,           # Salva solo il modello migliore
    mode='max',                    # Massimizzare la metrica
    verbose=1                      # Output testuale al salvataggio
)


# === Allenamento ===
history = model.fit(X_train, y_train, epochs=100, batch_size=16, validation_split=0.1, callbacks=[checkpoint_cb])

# === Plot Accuratezza e Loss ===
plt.figure(figsize=(12, 5))

plt.subplot(1, 2, 1)
plt.plot(history.history['accuracy'], label='Train acc')
plt.plot(history.history['val_accuracy'], label='Val acc')
plt.title('Accuracy over epochs')
plt.xlabel('Epoch')
plt.ylabel('Accuracy')
plt.legend()

plt.subplot(1, 2, 2)
plt.plot(history.history['loss'], label='Train loss')
plt.plot(history.history['val_loss'], label='Val loss')
plt.title('Loss over epochs')
plt.xlabel('Epoch')
plt.ylabel('Loss')
plt.legend()

plt.tight_layout()
plt.savefig("accuracy_loss_plot.png")
plt.close()


# === Valutazione ===
loss, acc = model.evaluate(X_test, y_test)
print(f"\n✅ Accuracy test: {acc:.2f}")

# === Predizione e Confusion Matrix ===
y_pred = model.predict(X_test)
cm = confusion_matrix(np.argmax(y_test, axis=1), np.argmax(y_pred, axis=1))

fig, ax = plt.subplots(figsize=(6, 6))
ConfusionMatrixDisplay(cm, display_labels=LABELS).plot(cmap='Blues', ax=ax)
plt.title("Confusion Matrix")
plt.savefig("confusion_matrix.png")
plt.close()


# === Classification Report ===
print("\n📋 Classification Report:")
print(classification_report(np.argmax(y_test, axis=1), np.argmax(y_pred, axis=1), target_names=LABELS))

# === Salva modello e scaler ===
model.save("mlp_static_gesture_model.h5")
np.save("scaler_mean.npy", scaler.mean_)
np.save("scaler_scale.npy", scaler.scale_)
