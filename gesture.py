import cv2
import mediapipe as mp
import socket

# === UDP SETUP ===
UDP_IP = "127.0.0.1"
UDP_PORT = 5065
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def send_command(command):
    message = command.encode('utf-8')
    sock.sendto(message, (UDP_IP, UDP_PORT))
    print(f"[Python] Inviato comando: {command}")

# === MediaPipe SETUP ===
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(min_detection_confidence=0.7)
mp_draw = mp.solutions.drawing_utils

cap = cv2.VideoCapture(0)

prev_pos = None
mode = "traslazione"  # cambia dinamicamente se vuoi
#mode = "zoom"  # o "rotazione"
#mode = "rotazione"  # per rotazione
while True:
    success, img = cap.read()
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    result = hands.process(img_rgb)

    if result.multi_hand_landmarks:
        for handLms in result.multi_hand_landmarks:
            mp_draw.draw_landmarks(img, handLms, mp_hands.HAND_CONNECTIONS)

            # Coordinate Landmark
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

                prev_pos = center_pos
    cv2.imshow("Gesture Control", img)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break
