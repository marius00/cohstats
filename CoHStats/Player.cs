﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 0x194: Offset player 1 in 2v2
// 0x464: Offset player 2 in 2v2 (720b above p1)
// 0x464: Offset player 3 in 2v2 (720b above p1)
namespace CoHStats {
    public enum Player {
        One = 0x194, Two = 0x464, Three = 0x734, Four = 0xA04
    }
}
