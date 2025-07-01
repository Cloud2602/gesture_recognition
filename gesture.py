import cv2
import mediapipe as mp
import numpy as np
import socket
from tensorflow.keras.models import load_model


# === UDP SETUP ===
UDP_IP = "127.0.0.1"
UDP_PORT = 5065
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def send_command(command):
    message = command.encode('utf-8')
    sock.sendto(message, (UDP_IP, UDP_PORT))
    print(f"[Python] Inviato comando: {command}")

# === CONFIGURAZIONE ===
model_path = './with_mediapipe_z/best_model.h5'
mean = np.load('./with_mediapipe_z/scaler_mean.npy')
scale = np.load('./with_mediapipe_z/scaler_scale.npy')

confidence_threshold = 0.9  # sotto questa soglia → "altro"


# === MAPPATURA CLASSI ===
label_map = {
    0: "stop",
    1: "zoom",
    2: "rotazione",
    3: "traslazione"
}

# === CARICA MODELLO ===
model = load_model(model_path)
print("✅ Modello caricato!")

# === SETUP MEDIAPIPE ===
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=2,
                       min_detection_confidence=0.7, min_tracking_confidence=0.5)
mp_drawing = mp.solutions.drawing_utils

# === WEBCAM ===
cap = cv2.VideoCapture(0)

# === STABILIZZATORE DI PREDIZIONE ===
last_prediction = None
stable_count = 0
display_label = "..."

def track_movement(mode, results, frame, prev_pos):
    # === GESTIONE MOVIMENTO IN BASE ALLA MODALITÀ STABILE ===
    
        for hand_landmarks in results.multi_hand_landmarks:
            thumb = hand_landmarks.landmark[4]
            index = hand_landmarks.landmark[8]

            h, w, _ = frame.shape
            thumb_pos = (int(thumb.x * w), int(thumb.y * h))
            index_pos = (int(index.x * w), int(index.y * h))
            center_pos = (int((thumb.x + index.x)/2 * w), int((thumb.y + index.y)/2 * h))
            t = 3  # soglia movimento

            if mode == "zoom":
                send_command("mode_zoom")
                dist = ((thumb_pos[0] - index_pos[0])**2 + (thumb_pos[1] - index_pos[1])**2) ** 0.5
                if dist < 40:
                    send_command("zoom_in")
                elif dist > 100:
                    send_command("zoom_out")

            elif mode == "traslazione":
                send_command("mode_translate")
                if prev_pos is not None:
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
                send_command("mode_rotate")
                if prev_pos is not None:
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
                prev_pos = center_pos
        return prev_pos
def predict_gesture(frame, right_hand, left_hand, last_prediction,stable_count, display_label= None):
        stable_threshold = 20  # numero minimo di frame consecutivi per mostrare la predizione
        for hand_landmarks, handedness in zip(results.multi_hand_landmarks, results.multi_handedness):
            mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
            
            label = handedness.classification[0].label
            label = "Left" if label == "Right" else "Right"  # inversione logica

            landmarks = [coord for lm in hand_landmarks.landmark for coord in (lm.x, lm.y, lm.z)]

            if label == "Right":
                right_hand = landmarks
            elif label == "Left":
                left_hand = landmarks

        # === COMBINA, STANDARDIZZA, PREDICI ===
        combined = right_hand + left_hand
        input_data = np.array([combined])
        input_data = (input_data - mean) / scale
        prediction = model.predict(input_data, verbose=0)
        predicted_index = int(np.argmax(prediction))
        confidence = float(np.max(prediction))
        #print(f"Predizione grezza: {prediction}, Indice: {predicted_index}, Confidenza: {confidence:.2f}")

        if confidence >= confidence_threshold:
            current_label = label_map.get(predicted_index, "altro")
        else:
            current_label = "altro"

        
        """# === VISUALIZZA ===
        cv2.putText(frame, f"{display_label}", (10, 30),
                cv2.FONT_HERSHEY_SIMPLEX, 1, (0,255,0), 2)  """
        # === STABILIZZAZIONE ===
        #print(f"Predizione corrente: {current_label}, Ultima predizione: {last_prediction}, Stabilità: {stable_count}")
        if current_label == last_prediction:
            stable_count += 1
            #print("Stable count:", stable_count)
        else:
            stable_count = 1
            last_prediction = current_label

        if stable_count >= stable_threshold:
            display_label = current_label
            #print(f"Predizione stabile: {display_label}")
            stable_count = 0    
        return display_label, last_prediction, stable_count
mode = None
previous_mode = None
flag = 0  
prev_pos = None  # posizione centrale mano precedente
while True:
    ret, frame = cap.read()
    if not ret:
        break

    frame = cv2.flip(frame, 1)  # effetto specchio
    image_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(image_rgb)

    # Landmark inizializzati a zero
    right_hand = [0.0] * (21 * 3)
    left_hand = [0.0] * (21 * 3)
    if results.multi_hand_landmarks and results.multi_handedness:
        mode, last_prediction, stable_count = predict_gesture(frame, right_hand, left_hand, last_prediction, stable_count)
        print(f"Modalità corrente: {mode},Conteggio stabile: {stable_count}")
        "flag [0,1,2] 0=attesa, 1=modalità scelta, 2=modalità attiva"
        if mode == "stop" :
            if flag == 0 or flag ==2:
                flag = 1
                mode = None
                previous_mode = None
                prev_pos = None
                print("Modalità attesa")
                send_command("choose_mode")
        if flag == 1:
                if mode != None:
                    flag = 2
                    print("Modalità scelta : ", mode)
                    
                    previous_mode= mode
                    mode = None
        if flag == 2:
            if previous_mode != None:
                if previous_mode == "stop":
                    print("Modalità stop attiva")
                    send_command("default")
                else:
                    print("Modalità attiva : ", previous_mode)
                    prev_pos = track_movement(previous_mode, results, frame, prev_pos)

    else:
        display_label = "🖐️ Nessuna mano"

    cv2.putText(frame, f"Modalita: {previous_mode}", (10, 70),
                cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

    cv2.imshow("Predizione in tempo reale", frame)

    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
