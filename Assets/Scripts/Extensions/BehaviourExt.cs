using UnityEngine;

public static class BehaviourExt
{
    public static void ReEnable(this Behaviour beh)
    {
        beh.enabled = false;
        beh.enabled = true;
    }
}
