import os
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout
from tensorflow.keras.utils import to_categorical

# === Config ===
DATA_PATH = "data_static"  # cartella con sottocartelle per ogni gesto
LABELS = ["stop","1", "2", "3"]
NUM_FEATURES = 126  # 21 punti * 3 coordinate * 2 mani

# === Caricamento dati ===
X, y = [], []

for label_index, label in enumerate(LABELS):
    folder = os.path.join(DATA_PATH, label)
    for file in os.listdir(folder):
        if file.endswith(".npy"):
            frame = np.load(os.path.join(folder, file))  # shape (126,)
            if frame.shape == (NUM_FEATURES,):
                X.append(frame)
                y.append(label_index)

X = np.array(X)
#formato categorical perchè usiamo categorical loss
y = to_categorical(np.array(y), num_classes=len(LABELS))

# === Preprocessing ===

#Normalizzazione
scaler = StandardScaler()
X = scaler.fit_transform(X)

# === Train/test split ===
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

# === Modello MLP ===
model = Sequential([
    Dense(256, activation='relu', input_shape=(NUM_FEATURES,)),
    Dropout(0.4),
    Dense(128, activation='relu'),
    Dropout(0.3),
    Dense(len(LABELS), activation='softmax')
])

model.compile(optimizer='adam', loss='categorical_crossentropy', metrics=['accuracy'])

# === Allenamento ===
history = model.fit(X_train, y_train, epochs=40, batch_size=16, validation_split=0.1)

# === Valutazione ===
loss, acc = model.evaluate(X_test, y_test)
print(f"\n✅ Accuracy test: {acc:.2f}")

# === Salva modello e scaler ===
model.save("mlp_static_gesture_model.h5")
np.save("scaler_mean.npy", scaler.mean_)
np.save("scaler_scale.npy", scaler.scale_)
