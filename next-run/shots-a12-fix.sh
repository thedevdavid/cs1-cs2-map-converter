#!/bin/bash
# A-v12 fix shots: the 19 cameras of the A-v11 owner review of 15 (Umber/Driftwood + Detroit approach) and 24 (gallery). Output next-run/shots-a12/fix.
B=http://127.0.0.1:8642; O=next-run/shots-a12/fix; mkdir -p "$O"
shot(){ curl -s -m 20 "$B/camera/set?x=$1&z=$2&angleX=$3&angleY=$4&zoom=$5" >/dev/null; sleep 2.5; curl -s -m 90 "$B/screenshot" -o "$O/$6.png"; echo "$6 $(stat -c %s "$O/$6.png")"; }
shot 662 -4494 0 60 600 15-01-umber-top
shot 662 -4494 0 20 450 15-02-umber-low-s
shot 662 -4494 90 20 450 15-03-umber-low-e
shot 662 -4494 180 20 450 15-04-umber-low-n
shot 662 -4494 270 20 450 15-05-umber-low-w
shot 662 -4494 0 50 280 15-06-umber-close
shot 920 -4310 0 60 450 15-07-approach-top
shot 920 -4310 45 20 400 15-08-approach-low-a
shot 920 -4310 225 20 400 15-09-approach-low-b
shot 1100 -4080 0 35 300 15-10-deck-start-top
shot 1100 -4080 90 15 320 15-11-deck-start-side-e
shot 1100 -4080 270 15 320 15-12-deck-start-side-w
shot -6348 873 0 60 500 24-01-gallery-top
shot -6348 873 90 15 400 24-02-gallery-low-e
shot -6348 873 270 15 400 24-03-gallery-low-w
shot -6440 700 45 10 250 24-04-gallery-along-sw
shot -6240 1050 225 10 250 24-05-gallery-along-ne
shot -6348 873 135 25 200 24-06-gallery-close
shot -6348 873 0 35 900 24-07-gallery-context
echo "== done"
