#!/bin/bash
# A-v20 gallery shots: the 7 A-v11/A-v12 cameras plus reference-style views (inside along the road, from the road at each portal). Output next-run/shots-a20/fix.
B=http://127.0.0.1:8642; O=next-run/shots-a20/fix; mkdir -p "$O"
shot(){ curl -s -m 20 "$B/camera/set?x=$1&z=$2&angleX=$3&angleY=$4&zoom=$5" >/dev/null; sleep 2.5; curl -s -m 90 "$B/screenshot" -o "$O/$6.png"; echo "$6 $(stat -c %s "$O/$6.png")"; }
shot -6348 873 0 60 500 24-01-gallery-top
shot -6348 873 90 15 400 24-02-gallery-low-e
shot -6348 873 270 15 400 24-03-gallery-low-w
shot -6440 700 45 10 250 24-04-gallery-along-sw
shot -6240 1050 225 10 250 24-05-gallery-along-ne
shot -6348 873 135 25 200 24-06-gallery-close
shot -6348 873 0 35 900 24-07-gallery-context
shot -6348 873 331 4 60 24-08-inside-a
shot -6348 873 151 4 60 24-09-inside-b
shot -6235 675 331 6 120 24-10-portal-se-a
shot -6235 675 151 6 120 24-11-portal-se-b
shot -6460 1070 151 6 120 24-12-portal-nw-a
shot -6460 1070 331 6 120 24-13-portal-nw-b
shot -6420 820 60 12 260 24-14-valley-side-low
echo "== done"
shot -6500 1000 0 30 900 24-00-cs1-camera
shot -6262 700 150 20 140 24-15-pillars-se-low
shot -6350 880 150 20 140 24-16-pillars-mid-low
shot -6440 1050 330 20 140 24-17-pillars-nw-low
shot -6262 700 0 80 160 24-18-edge-se-top
shot -6350 880 0 80 160 24-19-edge-mid-top
shot -34 -2346 0 40 220 R1-bare-earth-road-now-highway
shot 3775 2919 0 20 220 R2-covered-bridge-road
shot -4224 -60 0 50 500 R3-crossing-highways
shot 0 0 0 60 1200 R4-starting-area-centre
