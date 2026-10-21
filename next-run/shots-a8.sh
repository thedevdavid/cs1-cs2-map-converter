#!/bin/bash
# A-v8 review shots: Detroit opt2 import, the 26 marked spots (A-v7 cameras), Detroit comparison, water A/B, season test.
B=http://127.0.0.1:8642; O=next-run/shots-a8; mkdir -p "$O"
shot(){ curl -s -m 20 "$B/camera/set?x=$1&z=$2&angleX=$3&angleY=$4&zoom=$5" >/dev/null; sleep 3; curl -s -m 90 "$B/screenshot" -o "$O/$6.png"; echo "$6 $(stat -c %s "$O/$6.png")"; }
echo "== opt2 import"; curl -s -m 180 -H "Content-Type: application/json" --data-binary @net-converter/out/x1/import-detroit-opt2.json "$B/import/network" | head -c 300; echo; sleep 5
echo "== marked spots"
shot -3525 -3181 144 12 380 01-br006-truss-arch-side
shot -4420 50 150 25 650 02-br009-truss-arch-pair-and-approach
shot 437 200 60 20 650 03-br005-truss-arch-pair
shot 1533 -3498 127 10 1100 04-detroit-suspension-side
shot 1288 -3820 127 15 380 05-detroit-approach-piers
shot -1620 -1672 130 15 380 06-br003-piers
shot 778 2666 100 15 420 07-ourense-rail-truss-arch
shot -325 -2733 80 15 420 08-pedrido-rail-truss-arch
shot 3775 2919 0 20 220 09-covered-bridge-gravel
shot 1300 4680 180 25 700 10-dam-and-power-lines
shot 1600 4544 0 35 260 11-substation
shot 2995 820 0 50 700 12-interchange-oneway-joins
shot 3493 2152 0 50 700 30-interchange-05-stack
shot 3350 2000 45 25 500 30b-interchange-05-stack-low
shot -4224 -60 0 50 500 13-crossing-restored
shot 817 4013 0 45 420 14-dead-end-fix
shot 608 -4721 0 35 450 15-interchange-umber-driftwood
shot 634 -1505 0 45 900 16-sand-paint-river
shot 4096 -6127 0 45 1500 17-sand-paint-southeast
shot -1666 -1852 0 45 1500 18-sand-paint-lake
shot -6150 4397 0 45 1000 19-dry-grass-paint-west
shot 4565 -6194 0 45 1000 20-dry-grass-paint-southeast
shot -34 -2346 0 40 220 21-bare-earth-patch
shot 5 -2619 0 30 320 22-fort-ruins
shot -3000 -5500 0 30 900 23-po-rocks-cliffs-south
shot -6500 1000 0 30 900 24-po-rocks-cliffs-west
shot 7000 0 90 15 6000 25-world-map-east-flats
shot -7000 0 270 15 6000 26-world-map-west-range
echo "== detroit comparison"
shot 1532 -3498 37 12 1600 27-detroit-current-and-opt2-side
shot 1400 -3350 127 25 900 28-detroit-opt2-deck
shot 1532 -3498 217 45 1800 29-detroit-both-from-above
echo "== water A/B"
shot 600 -2750 0 35 700 W1-water-v1
curl -s -m 60 -H "Content-Type: application/json" --data-binary @look/water-v2a.json "$B/map/settings" >/dev/null; sleep 4
shot 600 -2750 0 35 700 W1-water-v2a
curl -s -m 60 -H "Content-Type: application/json" --data-binary @look/water-v2b.json "$B/map/settings" >/dev/null; sleep 4
shot 600 -2750 0 35 700 W1-water-v2b
curl -s -m 60 -H "Content-Type: application/json" --data-binary @look/water.json "$B/map/settings" >/dev/null; sleep 2
echo "== season test"
season(){ curl -s -m 20 -H "Content-Type: application/json" -d "{\"month\":$1,\"day\":$2,\"hour\":$3}" "$B/map/time" | head -c 200; echo; sleep 6; shot 600 -2750 270 20 900 "S$4-$1-$2-h$3-A-river"; shot -6200 1200 270 10 1500 "S$4-$1-$2-h$3-C-peak"; }
season 1 15 12 1; season 1 15 6 2; season 3 1 12 3; season 5 1 12 4; season 7 15 12 5; season 7 15 20 6; season 9 15 12 7; season 11 15 12 8; season 12 15 12 9
curl -s -m 20 -H "Content-Type: application/json" -d '{"reset":true}' "$B/map/time" | head -c 120; echo
echo "== done"
