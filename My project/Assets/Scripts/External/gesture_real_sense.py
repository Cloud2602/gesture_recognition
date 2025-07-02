import pyrealsense2 as rs
import cv2
import numpy as np
import mediapipe as mp
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

# === CONFIG ===
model_path = '../with_real_z/best_model.h5'
mean = np.load('../with_real_z/scaler_mean.npy')
scale = np.load('../with_real_z/scaler_scale.npy')
confidence_threshold = 0.6
radius = 40  # per smoothing profondità
depth_scale = 0.0001  # solitamente 1mm

model = load_model(model_path)
print(" Modello caricato!")
label_map = {
    0: "stop",
    1: "zoom",
    2: "rotation",
    3: "traslation"
}


# === MediaPipe Setup ===
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=2,
                       min_detection_confidence=0.2, min_tracking_confidence=0.5)
mp_drawing = mp.solutions.drawing_utils

# === RealSense Setup ===
pipeline = rs.pipeline()
config = rs.config()
config.enable_stream(rs.stream.depth, 640, 480, rs.format.z16, 30)
config.enable_stream(rs.stream.color, 640, 480, rs.format.bgr8, 30)
profile = pipeline.start(config)
align = rs.align(rs.stream.color)

depth_sensor = profile.get_device().first_depth_sensor()
depth_scale = depth_sensor.get_depth_scale()
print(" Scala profondità:", depth_scale)

# === Stato ===
last_prediction = None
stable_count = 0
display_label = "..."
mode = None
previous_mode = None
flag = 0
prev_pos = None

def get_valid_depth_avg(depth_map, x, y, radius=40):
    h, w = depth_map.shape
    values = []
    for dy in range(-radius, radius + 1):
        for dx in range(-radius, radius + 1):
            ny, nx = y + dy, x + dx
            if 0 <= ny < h and 0 <= nx < w:
                d = depth_map[ny, nx]
                if d > 0:
                    values.append(d)
    return np.mean(values) if values else 0.0

def get_landmark_xyz(landmarks, depth_img, h, w):
    xyz = []
    for lm in landmarks.landmark:
        x_px = int(lm.x * w)
        y_px = int(lm.y * h)
        
        z = get_valid_depth_avg(depth_img, x_px, y_px, radius) * depth_scale
        
        xyz.extend([lm.x, lm.y, z])
    return xyz

def predict_gesture(frame, results, right_hand, left_hand, last_prediction, stable_count, depth_image, h, w, display_label=None):
    stable_threshold = 20

    for hand_landmarks, handedness in zip(results.multi_hand_landmarks, results.multi_handedness):
        mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)
        label = handedness.classification[0].label
        label = "Left" if label == "Right" else "Right"

        xyz_landmarks = get_landmark_xyz(hand_landmarks, depth_image, h, w)
        if label == "Right":
            right_hand = xyz_landmarks
        else:
            left_hand = xyz_landmarks
        dominant_hand_label = label
    combined = right_hand + left_hand
    input_data = np.array([combined])
    input_data = (input_data - mean) / scale
    prediction = model.predict(input_data, verbose=0)
    predicted_index = int(np.argmax(prediction))
    confidence = float(np.max(prediction))

    if confidence >= confidence_threshold:
            current_label = label_map.get(predicted_index, "other")
    else:
        current_label = "other"

    
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

    return display_label, last_prediction, stable_count, dominant_hand_label 

def track_movement(mode, results, frame, prev_pos, tracked_hand, hand_count):
    for hand_landmarks, handedness in zip(results.multi_hand_landmarks, results.multi_handedness):
        label = handedness.classification[0].label
        label = "Left" if label == "Right" else "Right"

        if label != tracked_hand:
            continue  
        thumb = hand_landmarks.landmark[4]
        index = hand_landmarks.landmark[8]
        h, w, _ = frame.shape

        thumb_pos = (int(thumb.x * w), int(thumb.y * h))
        index_pos = (int(index.x * w), int(index.y * h))
        center_pos = (int((thumb.x + index.x)/2 * w), int((thumb.y + index.y)/2 * h))
        t = 10

        if mode == "zoom" and hand_count == 1:
            send_command("mode_zoom")
            dist = ((thumb_pos[0] - index_pos[0])**2 + (thumb_pos[1] - index_pos[1])**2) ** 0.5
            if dist < 40:
                send_command("zoom_in")
            elif dist > 100:
                send_command("zoom_out")

        elif mode == "traslation" and hand_count == 1:
            send_command("mode_translate")
            if prev_pos is not None:
                dx = center_pos[0] - prev_pos[0]
                dy = center_pos[1] - prev_pos[1]
                if abs(dx) > abs(dy):
                    send_command("translate_right" if dx > t else "translate_left")
                else:
                    send_command("translate_down" if dy > t else "translate_up")
            prev_pos = center_pos

        elif mode == "rotation"and hand_count == 1:
            send_command("mode_rotate")
            if prev_pos is not None:
                dx = center_pos[0] - prev_pos[0]
                dy = center_pos[1] - prev_pos[1]
                if abs(dx) > abs(dy):
                    send_command("rotate_right" if dx > t else "rotate_left")
                else:
                    send_command("rotate_down" if dy > t else "rotate_up")
            prev_pos = center_pos
    return prev_pos

# === LOOP ===
try:
    while True:
        try:
            frames = pipeline.wait_for_frames()
            aligned = align.process(frames)
            color_frame = aligned.get_color_frame()
            depth_frame = aligned.get_depth_frame()

            if not color_frame or not depth_frame:
                continue

            color_image = np.asanyarray(color_frame.get_data())
            depth_image = np.asanyarray(depth_frame.get_data())

            frame = cv2.flip(color_image, 1)
        
            image_rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
            results = hands.process(image_rgb)

            right_hand = [0.0] * (21 * 3)
            left_hand = [0.0] * (21 * 3)
            h, w, _ = frame.shape

            if results.multi_hand_landmarks and results.multi_handedness:
                hand_count = len(results.multi_hand_landmarks) if results.multi_hand_landmarks else 0
                mode, last_prediction, stable_count, dominant_hand_label = predict_gesture(
                    frame, results, right_hand, left_hand,
                    last_prediction, stable_count, depth_image, h, w
                )
                #print(f"Modalità corrente: {mode},Conteggio stabile: {stable_count}")

                if mode == "stop" :
                    if flag == 0 or flag ==2:
                        flag = 1
                        mode = None
                        previous_mode = "Choice"
                        prev_pos = None
                        print(" Modalità attesa")
                        send_command("choose_mode")
                if flag == 1:
                    if mode != None:
                        flag = 2
                        print(f" Modalità scelta: {mode}")
                        previous_mode = mode
                        tracked_hand = dominant_hand_label
                        mode = None

                if flag == 2:
                    if previous_mode != None:
                        if previous_mode == "stop":
                            print("Modalità stop attiva")
                            send_command("default")
                        else:
                            print("Modalità attiva : ", previous_mode)
                            prev_pos = track_movement(previous_mode, results, frame, prev_pos, tracked_hand, hand_count)
            else:
                display_label = " Nessuna mano"

            cv2.putText(frame, f"Mode': {previous_mode}", (10, 30),
            cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)

            cv2.putText(frame, f"Stable count: {stable_count}", (10, 60),
                        cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 0), 2)

            cv2.imshow("RealSense Hand Tracking", frame)

            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
        
        except Exception as e:
            print(f"[Errore]: {e}")
            # Qui puoi decidere se vuoi continuare il loop o interrompere
            # Per esempio continua:
            continue

finally:
    pipeline.stop()
    cv2.destroyAllWindows()
    print(" Pipeline chiusa")