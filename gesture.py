import cv2
import os
import mediapipe as mp
import numpy as np
import socket
from tensorflow.keras.models import load_model
from sklearn.preprocessing import StandardScaler


# === UDP SETUP ===
UDP_IP = "127.0.0.1"
UDP_PORT = 5065
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def send_command(command):
    message = command.encode('utf-8')
    sock.sendto(message, (UDP_IP, UDP_PORT))
    print(f"[Python] Inviato comando: {command}")

model = load_model("best_model.h5")
label_map = {
    1: "zoom",
    2: "rotazione",
    3: "traslazione"
}
stable_mode = None
predicted_mode = None
same_prediction_count = 0
PREDICTION_THRESHOLD = 20

def normalize_hand(landmarks):
    wrist = landmarks[0]
    landmarks -= wrist
    scale = np.linalg.norm(landmarks[9] - wrist)
    return landmarks / scale if scale > 0 else landmarks

def extract_keypoints_multi(results):
    lh = np.zeros((21, 3))
    rh = np.zeros((21, 3))
    if results.multi_hand_landmarks and results.multi_handedness:
        for hand_landmarks, handedness in zip(results.multi_hand_landmarks, results.multi_handedness):
            label = handedness.classification[0].label  # 'Left' o 'Right'
            print (f"Rilevata mano: {label}")
            landmarks = np.array([[lm.x, lm.y, lm.z] for lm in hand_landmarks.landmark])
            #landmarks = normalize_hand(landmarks)
            if label == "Left":
                rh = landmarks
            elif label == "Right":
                lh = landmarks
    return np.concatenate([lh.flatten(), rh.flatten()])



mean = np.load("scaler_mean.npy")
scale = np.load("scaler_scale.npy")
# === MediaPipe SETUP ===
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(min_detection_confidence=0.7)
mp_draw = mp.solutions.drawing_utils

cap = cv2.VideoCapture(0)

prev_pos = None
mode = None

while True:
    success, img = cap.read()
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    result = hands.process(img_rgb)

    if result.multi_hand_landmarks:
        for handLms in result.multi_hand_landmarks:
            mp_draw.draw_landmarks(img, handLms, mp_hands.HAND_CONNECTIONS)
            # === Predizione modalità ===
            keypoints = extract_keypoints_multi(result)
            keypoints = (keypoints - mean) / scale
            prediction = model.predict(np.expand_dims(keypoints, axis=0))
            print(f"Predizione grezza: {prediction}")
            raw_prediction = int(np.argmax(prediction))  # Per ottenere 1,2,3
            print(f"Predizione grezza (intero): {raw_prediction}")
            predicted_mode = label_map.get(raw_prediction)

            if predicted_mode == stable_mode:
                same_prediction_count += 1
            else:
                same_prediction_count = 1
                stable_mode = predicted_mode

            if same_prediction_count >= PREDICTION_THRESHOLD:
                mode = stable_mode
            """# Coordinate Landmark
            thumb = handLms.landmark[4]
            index = handLms.landmark[8]

            h, w, _ = img.shape
            thumb_pos = (int(thumb.x * w), int(thumb.y * h))
            index_pos = (int(index.x * w), int(index.y * h))
            center_pos = (int((thumb.x + index.x)/2 * w), int((thumb.y + index.y)/2 * h))
            t = 5
            if mode == "zoom":
                dist = ((thumb_pos[0] - index_pos[0])**2 + (thumb_pos[1] - index_pos[1])**2) ** 0.5
                if dist < 40:
                    send_command("zoom_in")
                elif dist > 100:
                    send_command("zoom_out")

            elif mode == "traslazione":
                if prev_pos:
                    dx = center_pos[0] - prev_pos[0]
                    dy = center_pos[1] - prev_pos[1]
                    
                    if abs(dx) > abs(dy):
                        if dx > t:
                            send_command("translate_right")
                        elif dx < -t:
                            send_command("translate_left")
                    else:
                        if dy > t:
                            send_command("translate_down")
                        elif dy < -t:
                            send_command("translate_up")

                prev_pos = center_pos
            elif mode == "rotazione":
                if prev_pos:
                    dx = center_pos[0] - prev_pos[0]
                    dy = center_pos[1] - prev_pos[1]

                    if abs(dx) > abs(dy):
                        if dx > t:
                            send_command("rotate_right")
                        elif dx < -t:
                            send_command("rotate_left")
                    else:
                        if dy > t:
                            send_command("rotate_down")
                        elif dy < -t:
                            send_command("rotate_up")

                prev_pos = center_pos"""
            cv2.putText(img, f"Modalita: {mode}", (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 1, (0,255,0), 2)

    cv2.imshow("Gesture Control", img)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break
