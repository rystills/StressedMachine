using UnityEngine;

public static class SoundExt
{
    /// <summary>
    /// Play a sound either forwards or backwards
    /// </summary>
    /// <param name="src">the sound to play</param>
    /// <param name="isInverted">whether or not to play it backwards</param>
    /// <param name="respectIsPlaying">if true, an actively playing sound will continue rather than playing again</param>
    public static void PlayBidir(AudioSource src, bool isInverted, bool respectIsPlaying)
    {
        src.pitch = isInverted ? -1 : 1;
        if (!respectIsPlaying || !src.isPlaying)
        {
            src.timeSamples = isInverted ? src.clip.samples - 1 : 0;
            src.Play();
        }
    }
}
