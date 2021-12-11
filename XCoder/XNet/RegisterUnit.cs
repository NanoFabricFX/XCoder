﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCoder.XNet
{
    internal class RegisterUnit
    {
        [ReadOnly(true)]
        public Int32 Address { get; set; }

        public UInt16 Value { get; set; }

        public String ValueHex => Value.ToString("X4");
    }
}
