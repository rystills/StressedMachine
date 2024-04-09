using UnityEngine;

public static class SoundExt
{
    /// <summary>
    /// Play a sound either forwards or backwards
    /// </summary>
    /// <param name="src">the sound to play</param>
    /// <param name="isInverted">whether or not to play it backwards</param>
    /// <param name="respectIsPlaying">if true, an actively playing sound will continue rather than playing again</param>
    public static void PlayBiDir(this AudioSource src, bool isInverted = false, bool respectIsPlaying = true)
    {
        src.pitch = isInverted ? -1 : 1;
        if (!respectIsPlaying || !src.isPlaying)
        {
            src.timeSamples = isInverted ? src.clip.samples - 1 : 0;
            src.Play();
        }
    }

    public static void PlayClip(this AudioSource src, AudioClip clip)
    {
        src.clip = clip;
        src.Play();
    }

    public static void PlayAtPitch(this AudioSource src, float newPitch)
    {
        src.pitch = newPitch;
        src.Play();
    }
    
    public static void PlayClipAtPitch(this AudioSource src, AudioClip clip, float newPitch)
    {
        src.clip = clip;
        src.PlayAtPitch(newPitch);
    }
}
