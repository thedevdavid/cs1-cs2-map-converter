#!/bin/bash
# A-v23 review shots: same cameras as A-v9 (sheets.ps1 -Run a11); season test runs separately via look/season-test.mjs.
B=http://127.0.0.1:8642; O=next-run/shots-a23; mkdir -p "$O"
echo "settle pause 25 s before the first camera jump (crash #4/#5 pattern)"; sleep 25
shot(){ curl -s -m 20 "$B/camera/set?x=$1&z=$2&angleX=$3&angleY=$4&zoom=$5" >/dev/null; sleep 3; curl -s -m 90 "$B/screenshot" -o "$O/$6.png"; echo "$6 $(stat -c %s "$O/$6.png")"; }
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
shot 3493 2152 45 30 600 30b-interchange-05-stack-levels
shot -4224 -60 0 50 500 13-crossing-restored
shot 817 4013 0 45 420 14-dead-end-fix
shot 608 -4721 0 35 450 15-interchange-umber-driftwood
shot 662 -4494 0 40 400 15b-interchange-08-heights
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
shot 5000 2500 0 55 300 00-textures-ground-closeup
shot 600 -2750 0 35 700 W1-water-v1
echo "== done"
