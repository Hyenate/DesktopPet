using System;
using CSharpDictionary = System.Collections.Generic.Dictionary<string, DefaultAnimationsMetadata.AnimationMetadata>;


public partial class DefaultAnimationsMetadata
{
    public class AnimationMetadata(int weight, float animSpeed, int animEndDelay, bool looping)
    {
        public int Weight { get; set; } = weight;
        public float AnimSpeed { get; set; } = animSpeed;
        public int AnimEndDelay { get; set; } = animEndDelay;
        public bool Looping { get; set; } = looping;
    }

    private static readonly CSharpDictionary AnimationsMetadata = [];

    public static CSharpDictionary GetDefaultAnimationsMetadata()
    {
        AnimationsMetadata["Attack"] = new AnimationMetadata(5, 1, 0, false);
        AnimationsMetadata["Charge"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["Cringe"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["DeepBreath"] = new AnimationMetadata(5, 1, 0, false);
        AnimationsMetadata["Double"] = new AnimationMetadata(0, 1, 0, false);
        AnimationsMetadata["Eat"] = new AnimationMetadata(5, 1, 0, true);
        AnimationsMetadata["EventSleep"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["Faint"] = new AnimationMetadata(0, 1, 8, false);
        AnimationsMetadata["Float"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["Head"] = new AnimationMetadata(0, 1, 0, false);
        AnimationsMetadata["HitGround"] = new AnimationMetadata(5, 1, 4, false);
        AnimationsMetadata["Hop"] = new AnimationMetadata(15, 0.75f, 0, false);
        AnimationsMetadata["Hurt"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["Idle"] = new AnimationMetadata(40, 1, 0, true);
        AnimationsMetadata["Laying"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["LeapForth"] = new AnimationMetadata(0, 1, 0, false);
        AnimationsMetadata["LookUp"] = new AnimationMetadata(0, 1, 16, false);
        AnimationsMetadata["LostBalance"] = new AnimationMetadata(5, 1, 0, true);
        AnimationsMetadata["Nod"] = new AnimationMetadata(0, 1, 8, false);
        AnimationsMetadata["Pain"] = new AnimationMetadata(5, 1, 8, false);
        AnimationsMetadata["Pose"] = new AnimationMetadata(5, 1, 16, false);
        AnimationsMetadata["Pull"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["Rotate"] = new AnimationMetadata(5, 0.75f, 0, true);
        AnimationsMetadata["Shoot"] = new AnimationMetadata(5, 1, 0, false);
        AnimationsMetadata["Shock"] = new AnimationMetadata(5, 1, 0 , false);
        AnimationsMetadata["Sink"] = new AnimationMetadata(0, 1, 0, false);
        AnimationsMetadata["Sit"] = new AnimationMetadata(0, 1, 16, false);
        AnimationsMetadata["Sleep"] = new AnimationMetadata(30, 2, 0, true);
        AnimationsMetadata["Swing"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["TailWhip"] = new AnimationMetadata(20, 1, 0, true);
        AnimationsMetadata["Trip"] = new AnimationMetadata(0, 1, 8, false);
        AnimationsMetadata["Tumble"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["TumbleBack"] = new AnimationMetadata(0, 1, 0, true);
        AnimationsMetadata["Wake"] = new AnimationMetadata(0, 1, 0, false);
        AnimationsMetadata["Walk"] = new AnimationMetadata(60, 1, 0, true);

        return AnimationsMetadata;
    }
}