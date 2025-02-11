import socket
import time

class UnityTCPClient:
    def __init__(self, host="127.0.0.1", port=5005):
        self.host = host
        self.port = port
        self.sock = None

    def connect(self):
        """Connect to the Unity TCP server."""
        if self.sock is not None:
            self.sock.close()
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.connect((self.host, self.port))
        print(f"Connected to Unity server at {self.host}:{self.port}")

    def send(self, message):
        """Send a message to the Unity server."""
        if self.sock is None:
            self.connect()
        # Convert the string to bytes and send
        data = message.encode('ascii')
        self.sock.sendall(data)

    def close(self):
        """Close the connection."""
        if self.sock:
            self.sock.close()
            self.sock = None
