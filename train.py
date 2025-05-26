import os
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout
from tensorflow.keras.utils import to_categorical

def normalize_hand(landmarks):
    wrist = landmarks[0]
    landmarks -= wrist
    scale = np.linalg.norm(landmarks[9] - wrist)  # indice MCP
    return landmarks / scale if scale > 0 else landmarks

# === Config ===
DATA_PATH = "/content/gesture_recognition/dataset_landmarks_augumantation"  # cartella con sottocartelle per ogni gesto
LABELS = ["0","1", "2", "3"] #0:stop 
NUM_FEATURES = 126  # 21 punti * 3 coordinate * 2 mani

# === Caricamento dati ===
X, Y = [], []



for file_name in os.listdir(DATA_PATH):
    if file_name.endswith(".npy"):
        # (490,(42,3) )
        file_path=os.path.join(DATA_PATH, file_name)
        frame = np.load(file_path)  
        for sample in frame:
            sample = normalize_hand(sample)
            sample= sample.reshape(-1)
            X.append(sample)
            Y.append(int(file_name.split(".")[0]))
                

X = np.array(X)  #(490,42)  quindi questo dovrebbe essere una lista di 280 array (70x4) ognuno contenente 126 elementi
#formato categorical perchè usiamo categorical loss
Y = to_categorical(np.array(Y), num_classes=len(LABELS))


# === Preprocessing ===
scaler = StandardScaler()      #normalizzazione -> standardizzazione z-score
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

# === Allenamento ===
history = model.fit(X_train, y_train, epochs=40, batch_size=16, validation_split=0.1)

# === Valutazione ===
loss, acc = model.evaluate(X_test, y_test)
print(f"\n✅ Accuracy test: {acc:.2f}")
y_pred = model.predict(X_test)
cm = confusion_matrix(np.argmax(y_test, axis=1), np.argmax(y_pred, axis=1))
ConfusionMatrixDisplay(cm).plot()
# === Salva modello e scaler ===
model.save("mlp_static_gesture_model.h5")
np.save("scaler_mean.npy", scaler.mean_)
np.save("scaler_scale.npy", scaler.scale_)
