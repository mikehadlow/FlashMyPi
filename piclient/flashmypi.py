#!/usr/bin/python
from sense_hat import SenseHat
import socket


sense = SenseHat()
white = (50, 50, 50)
black = (0, 0, 0)

def display(chunk):
    for x in range(0, 8):
        for y in range(0, 8):
            val = ord(chunk[x * 8 + y]) 
            
            if (val == 0):
                sense.set_pixel(x, y, black)
            else:
                sense.set_pixel(x, y, white)

def recieveMsg(sock):
    chunk = b'x'
    while chunk != b'':
        chunk = sock.recv(64)
        if chunk != b'':
            display(chunk)
            print('message\n')

    return b''.join(chunks)

def interact():
    svr = ('mikeh-nuc', 11000)
    msg = bytearray('Hello from Python!<EOF>', 'ascii')

    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.connect(svr)

    recieveMsg(sock)

interact()


