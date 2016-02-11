using Orleans;

namespace SimpleGrains
{
    public class MyGrainState : GrainState
    {
        public int Thing1 { get; set; }
        public string Thing2 { get; set; }
    }
}