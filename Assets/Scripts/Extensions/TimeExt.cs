using UnityEngine;

public static class TimeExt
{
    public static int frame => Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime);
    public static int FramesSince(int initialFrame) => frame - initialFrame;
    public static float Since(float initialTime) => Time.time - initialTime;
    public static float UnscaledSince(float initialTime) => Time.unscaledTime - initialTime;
    public static int SecondsToFrames(float seconds) => Mathf.RoundToInt(seconds * (1.0f / Time.fixedDeltaTime));
}
