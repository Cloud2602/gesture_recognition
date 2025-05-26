
import socket
import time

UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

def send_command(command):
    message = command.encode('utf-8')
    sock.sendto(message, (UDP_IP, UDP_PORT))
    print(f"[Python] Inviato comando: {command}")

if __name__ == "__main__":
    # Esempio: invia comandi finti ogni secondo
    comandi = ["zoom", "rotate", "translate", "stop"]
    for comando in comandi:
        send_command(comando)
        time.sleep(1)
