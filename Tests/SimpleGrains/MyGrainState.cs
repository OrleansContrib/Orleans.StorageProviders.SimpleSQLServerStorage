using Orleans;
using System;
using System.Collections.Generic;

namespace SimpleGrains
{
    public class MyGrainState : GrainState
    {
        public int Thing1 { get; set; }
        public string Thing2 { get; set; }
        public Guid Thing3 { get; set; }
        public DateTime Thing4 { get; set; }
        public IEnumerable<int> Things1 { get; set; }
    }
}