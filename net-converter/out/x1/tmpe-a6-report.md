# TM:PE -> Traffic import tmpe-20261020-124220

Traffic 0.2.13+ce1551f19d0341d7a4a6bb126daa515c6c021b43 (pinned 0.2.13+ce1551f). Nodes matched 20/27.

- Connections: {"skipped":59,"created":9} of 68
- Arrow lanes: {"superseded":37,"skipped":2} of 39
- Traffic data: 5 nodes, 9 source lanes, 9 connections. written; Traffic regenerates the node lanes this frame. Save the map to keep them.

## Lane connections

| node | CS2 node | id | CS1 from → CS2 | CS1 to → CS2 | status | reason |
|---|---|---|---|---|---|---|
| n527 |  | c59 |  |  | skipped | no CS2 net node within 60 m (x/z) of (59.1, -97.5) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n527 |  | c60 |  |  | skipped | no CS2 net node within 60 m (x/z) of (59.1, -97.5) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n1635 | 55182:1 | c9 | 10900:1 → 65169:11:0 | 20430:4 → 66690:19:2 | skipped | Traffic validation: source lane index 0 is not a road/track lane of edge 65169:11's composition (valid: 2,3,6) |
| n1635 | 55182:1 | c14 | 10900:0 → 65169:11:1 | 20430:1 → 66690:19:3 | skipped | Traffic validation: source lane index 1 is not a road/track lane of edge 65169:11's composition (valid: 2,3,6) |
| n1635 | 55182:1 | c15 | 10900:0 → 65169:11:1 | 20430:0 → 66690:19:4 | skipped | Traffic validation: source lane index 1 is not a road/track lane of edge 65169:11's composition (valid: 2,3,6) |
| n1884 |  | c34 |  |  | skipped | no CS2 net node within 60 m (x/z) of (-397.9, 3910.7) whose edges carry this node's CS1 segments |
| n1884 |  | c35 |  |  | skipped | no CS2 net node within 60 m (x/z) of (-397.9, 3910.7) whose edges carry this node's CS1 segments |
| n1884 |  | c36 |  |  | skipped | no CS2 net node within 60 m (x/z) of (-397.9, 3910.7) whose edges carry this node's CS1 segments |
| n4291 | 55048:1 | c16 | 22564:0 → 65841:5:1 | 22842:1 → 66108:3:3 | skipped | Traffic validation: source lane index 1 is not a road/track lane of edge 65841:5's composition (valid: 2,3,6) |
| n4291 | 55048:1 | c17 | 22564:1 → 65841:5:0 | 22842:4 → 66108:3:2 | skipped | Traffic validation: source lane index 0 is not a road/track lane of edge 65841:5's composition (valid: 2,3,6) |
| n4291 | 55048:1 | c18 | 9919:0 → 65001:9:0 | 22842:0 → 66108:3:4 | skipped | Traffic validation: source lane index 0 is not a road/track lane of edge 65001:9's composition (valid: 2) |
| n4378 | 54722:1 | c37 | 7738:0 | 26633:0 | skipped | no edge at CS2 node 54722:1 matches CS1 segment 7738 (nearest 13.93 m off at 20 m from the node) |
| n4378 | 54722:1 | c38 | 7738:1 | 26633:1 | skipped | no edge at CS2 node 54722:1 matches CS1 segment 7738 (nearest 13.93 m off at 20 m from the node) |
| n4378 | 54722:1 | c39 | 7738:4 | 26633:4 | skipped | no edge at CS2 node 54722:1 matches CS1 segment 7738 (nearest 13.93 m off at 20 m from the node) |
| n4801 |  | c64 |  |  | skipped | no CS2 net node within 60 m (x/z) of (62.8, 333.4) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n4801 |  | c65 |  |  | skipped | no CS2 net node within 60 m (x/z) of (62.8, 333.4) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n6802 |  | c52 |  |  | skipped | no CS2 net node within 60 m (x/z) of (267.4, 3395.2) whose edges carry this node's CS1 segments (best: 1/2 CS1 segments matched by edges) |
| n6802 |  | c53 |  |  | skipped | no CS2 net node within 60 m (x/z) of (267.4, 3395.2) whose edges carry this node's CS1 segments (best: 1/2 CS1 segments matched by edges) |
| n7849 | 56387:1 | c43 | 20430:0 → 66691:17:2 | 20118:0 → 66319:21:2 | created |  |
| n7849 | 56387:1 | c44 | 20430:1 → 66691:17:1 | 20118:1 → 66319:21:1 | skipped | Traffic validation: source lane index 1 is not a road/track lane of edge 66691:17's composition (valid: 2,3,4,7) |
| n7849 | 56387:1 | c45 | 20430:4 → 66691:17:0 | 20118:4 → 66319:21:0 | skipped | Traffic validation: source lane index 0 is not a road/track lane of edge 66691:17's composition (valid: 2,3,4,7) |
| n9988 | 56274:1 | c4 | 24116:0 | 24828:1 | skipped | lane-count mismatch on CS1 segment 24116: CS1 has 2 car lanes into the node, CS2 edge 97351:11 has 4 |
| n9988 | 56274:1 | c5 | 24116:1 | 24828:0 | skipped | lane-count mismatch on CS1 segment 24116: CS1 has 2 car lanes into the node, CS2 edge 97351:11 has 4 |
| n9988 | 56274:1 | c6 | 19095:0 | 24116:3 | skipped | lane-count mismatch on CS1 segment 19095: CS1 has 2 car lanes into the node, CS2 edge 66415:5 has 4 |
| n9988 | 56274:1 | c7 | 19095:1 | 24116:2 | skipped | lane-count mismatch on CS1 segment 19095: CS1 has 2 car lanes into the node, CS2 edge 66415:5 has 4 |
| n10346 |  | c31 |  |  | skipped | no CS2 net node within 60 m (x/z) of (-336.4, 3873.4) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n10346 |  | c32 |  |  | skipped | no CS2 net node within 60 m (x/z) of (-336.4, 3873.4) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n10346 |  | c33 |  |  | skipped | no CS2 net node within 60 m (x/z) of (-336.4, 3873.4) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n10611 |  | c57 |  |  | skipped | no CS2 net node within 60 m (x/z) of (63.7, -36.1) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n10611 |  | c58 |  |  | skipped | no CS2 net node within 60 m (x/z) of (63.7, -36.1) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n11611 | 55794:1 | c0 | 34976:2 | 15154:1 | skipped | lane-count mismatch on CS1 segment 34976: CS1 has 2 car lanes into the node, CS2 edge 96943:17 has 4 |
| n11611 | 55794:1 | c1 | 34976:3 | 15154:0 | skipped | lane-count mismatch on CS1 segment 34976: CS1 has 2 car lanes into the node, CS2 edge 96943:17 has 4 |
| n11611 | 55794:1 | c2 | 31696:0 | 34976:1 | skipped | no edge at CS2 node 55794:1 matches CS1 segment 31696 (nearest 15.45 m off at 20 m from the node) |
| n11611 | 55794:1 | c3 | 31696:1 | 34976:0 | skipped | no edge at CS2 node 55794:1 matches CS1 segment 31696 (nearest 15.45 m off at 20 m from the node) |
| n12156 | 55295:1 | c66 | 32183:0 | 11276:0 | skipped | no edge at CS2 node 55295:1 matches CS1 segment 32183 (nearest 24.2 m off at 20 m from the node) |
| n12156 | 55295:1 | c67 | 32183:1 | 11276:1 | skipped | no edge at CS2 node 55295:1 matches CS1 segment 32183 (nearest 24.2 m off at 20 m from the node) |
| n12773 | 55164:1 | c55 | 25952:0 | 10600:0 | skipped | lane-count mismatch on CS1 segment 25952: CS1 has 2 car lanes into the node, CS2 edge 66529:15 has 4 |
| n12773 | 55164:1 | c56 | 25952:1 | 31983:0 | skipped | lane-count mismatch on CS1 segment 25952: CS1 has 2 car lanes into the node, CS2 edge 66529:15 has 4 |
| n12950 | 55969:1 | c40 | 22842:0 → 66108:3:4 | 16466:0 → 65976:5:2 | created |  |
| n12950 | 55969:1 | c41 | 22842:1 → 66108:3:3 | 16466:1 → 65976:5:1 | skipped | Traffic validation: target lane index 1 is not a road/track lane of edge 65976:5's composition (valid: 2,3,4,7) |
| n12950 | 55969:1 | c42 | 22842:4 → 66108:3:2 | 16466:4 → 65976:5:0 | skipped | Traffic validation: target lane index 0 is not a road/track lane of edge 65976:5's composition (valid: 2,3,4,7) |
| n13892 | 53684:1 | c50 | 35734:0 → 64326:15:1 | 1054:0 → 61752:1:1 | skipped | Traffic validation: source lane index 1 is not a road/track lane of edge 64326:15's composition (valid: 2,3,6) |
| n13892 | 53684:1 | c51 | 35734:1 → 64326:15:0 | 1054:1 → 61752:1:0 | skipped | Traffic validation: source lane index 0 is not a road/track lane of edge 64326:15's composition (valid: 2,3,6) |
| n14204 | 53685:1 | c48 | 1054:0 → 61752:1:1 | 31965:0 → 64377:25:2 | skipped | Traffic validation: source lane index 1 is not a road/track lane of edge 61752:1's composition (valid: 2,3,6) |
| n14204 | 53685:1 | c49 | 1054:1 → 61752:1:0 | 28080:0 | skipped | lane-count mismatch on CS1 segment 28080: CS1 has 1 car lanes out of the node, CS2 edge 65680:3 has 2 |
| n14407 | 54723:1 | c46 | 7747:0 | 12566:0 | skipped | no edge at CS2 node 54723:1 matches CS1 segment 7747 (nearest 12.32 m off at 20 m from the node) |
| n14407 | 54723:1 | c47 | 23246:0 | 12566:1 | skipped | no edge at CS2 node 54723:1 matches CS1 segment 23246 (nearest 11.93 m off at 20 m from the node) |
| n15971 |  | c28 |  |  | skipped | no CS2 net node within 60 m (x/z) of (-270.0, 3850.9) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n15971 |  | c29 |  |  | skipped | no CS2 net node within 60 m (x/z) of (-270.0, 3850.9) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n15971 |  | c30 |  |  | skipped | no CS2 net node within 60 m (x/z) of (-270.0, 3850.9) whose edges carry this node's CS1 segments (best: 0/2 CS1 segments matched by edges) |
| n17002 | 53679:1 | c10 | 2218:0 → 61918:3:0 | 32183:1 → 64427:3:2 | skipped | Traffic validation: source lane index 0 is not a road/track lane of edge 61918:3's composition (valid: 2) |
| n17002 | 53679:1 | c61 | 1009:0 → 61749:1:0 | 32183:0 → 64427:3:3 | skipped | Traffic validation: source lane index 0 is not a road/track lane of edge 61749:1's composition (valid: 2) |
| n17387 | 53667:1 | c22 | 822:0 | 14787:0 | skipped | lane-count mismatch on CS1 segment 822: CS1 has 3 car lanes into the node, CS2 edge 65500:7 has 6 |
| n17387 | 53667:1 | c23 | 822:1 | 9948:0 | skipped | lane-count mismatch on CS1 segment 822: CS1 has 3 car lanes into the node, CS2 edge 65500:7 has 6 |
| n17387 | 53667:1 | c24 | 822:4 | 9948:1 | skipped | lane-count mismatch on CS1 segment 822: CS1 has 3 car lanes into the node, CS2 edge 65500:7 has 6 |
| n21201 | 53661:1 | c62 | 784:0 | 16556:0 | skipped | lane-count mismatch on CS1 segment 784: CS1 has 2 car lanes into the node, CS2 edge 65293:3 has 4 |
| n21201 | 53661:1 | c63 | 784:1 | 15937:0 | skipped | lane-count mismatch on CS1 segment 784: CS1 has 2 car lanes into the node, CS2 edge 65293:3 has 4 |
| n21320 | 54904:1 | c25 | 35218:0 → 65756:13:3 | 8915:1 → 64209:11:3 | created |  |
| n21320 | 54904:1 | c26 | 35218:1 → 65756:13:2 | 8915:4 → 64209:11:2 | created |  |
| n21320 | 54904:1 | c27 | 12088:0 → 64835:3:2 | 8915:0 → 64209:11:4 | created |  |
| n23859 | 56389:1 | c19 | 20118:0 → 66320:17:4 | 26394:0 → 67001:19:2 | created |  |
| n23859 | 56389:1 | c20 | 20118:1 → 66320:17:3 | 25731:0 → 67060:27:3 | created |  |
| n23859 | 56389:1 | c21 | 20118:4 → 66320:17:2 | 25731:1 → 67060:27:2 | created |  |
| n27056 | 53541:1 | c13 | 13450:0 → 64812:7:0 | 14444:1 | skipped | lane-count mismatch on CS1 segment 14444: CS1 has 2 car lanes out of the node, CS2 edge 67009:29 has 4 |
| n27056 | 53541:1 | c54 | 501:0 → 61723:1:0 | 14444:0 | skipped | lane-count mismatch on CS1 segment 14444: CS1 has 2 car lanes out of the node, CS2 edge 67009:29 has 4 |
| n27789 | 55971:1 | c8 | 16466:0 → 65976:5:2 | 25138:0 → 67002:17:3 | created |  |
| n27789 | 55971:1 | c11 | 16466:1 → 65976:5:1 | 25138:0 → 67002:17:3 | skipped | Traffic validation: source lane index 1 is not a road/track lane of edge 65976:5's composition (valid: 2,3,4,7) |
| n27789 | 55971:1 | c12 | 16466:4 → 65976:5:0 | 25138:1 → 67002:17:2 | skipped | Traffic validation: source lane index 0 is not a road/track lane of edge 65976:5's composition (valid: 2,3,4,7) |

## Lane arrows

| node | id | arrows | CS1 lane → CS2 | status | kept / CS2 default | reason |
|---|---|---|---|---|---|---|
| n1635 | a3 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n1635 | a20 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n4291 | a7 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n4291 | a31 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n4291 | a34 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n9988 | a15 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n9988 | a25 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n9988 | a29 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n9988 | a37 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n11611 | a0 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n11611 | a8 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n11611 | a16 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n11611 | a28 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n12773 | a6 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n12773 | a24 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n14204 | a19 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n14204 | a21 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n14407 | a11 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n14407 | a38 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n17002 | a4 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n17002 | a18 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n17387 | a14 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n17387 | a17 | Right |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n17387 | a33 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n18884 | a22 | Forward | 36804:0 → 63919:9:2 | skipped |  | no CS2 edge matches a CS1 segment in the arrow direction |
| n18884 | a26 | Forward | 3338:0 → 63919:9:2 | skipped |  | no CS2 edge matches a CS1 segment in the arrow direction |
| n21201 | a5 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n21201 | a36 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n21320 | a9 | Right |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n21320 | a10 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n21320 | a27 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n23859 | a13 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n23859 | a30 | Right |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n23859 | a32 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n27056 | a12 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n27056 | a35 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n27789 | a1 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n27789 | a2 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |
| n27789 | a23 | Forward |  | superseded |  | the lane has TM:PE lane connections, which TM:PE applies instead of its arrows |

Request: 27 nodes, height factor 1. Checklist for the manual fallback: cs1-offline/out/tmpe-summary.md.
