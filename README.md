#FlashMyPi

Remotely set the LEDs on a Raspberry Pi Sense HAT.

#Camera

Pipe raspvid into VLC to create an rtsp stream on port 1234.

   raspivid -o - -t 0 -n -vf -w 960 -h 540 | cvlc -vvv stream:///dev/stdin --sout '#rtp{sdp=rtsp://:1234/}' :demux=h264
   
Play this on your desktop with VLC:

   rtsp://<raspberry pi hostname or ip address>>:1234/
