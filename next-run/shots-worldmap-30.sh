#!/bin/bash
# World map 10270758 (hmap-ext ticket 30) review shots: the east exterior from the playable border.
# Divide range (11.9 km, bearing 122 deg, 158 m) between the east river and the main river; two buttes east within 12 km of the border.
# Output next-run/shots-worldmap-30.
B=http://127.0.0.1:8642; O=next-run/shots-worldmap-30; mkdir -p "$O"
shot(){ curl -s -m 20 "$B/camera/set?x=$1&z=$2&angleX=$3&angleY=$4&zoom=$5" >/dev/null; sleep 2.5; curl -s -m 90 "$B/screenshot" -o "$O/$6.png"; echo "$6 $(stat -c %s "$O/$6.png")"; }
shot 7000 0 90 15 6000 01-east-flats-low            # same camera as shot 25 of the a9 series, for a before/after
shot 7000 0 90 8 3000 02-east-horizon
shot 7000 -3000 90 12 4000 03-southeast-low
shot 7000 3000 90 12 4000 04-northeast-low
shot 12000 -2000 0 70 9000 05-divide-range-top
shot 12000 -2000 120 10 2500 06-divide-range-along
shot 14000 1500 90 20 3000 07-buttes
shot 4000 -4000 135 12 5000 08-from-inside-southeast
shot 0 0 90 35 14000 09-whole-east-half
