using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    [SerializeField] private float fadeDuration;
    private float startTime;
    [SerializeField] private MaskableGraphic graphic;

    private void OnEnable() => startTime = Time.time;

    private void Update()
    {
        graphic.color = new(graphic.color.r, graphic.color.g, graphic.color.b, TimeExt.Since(startTime) / fadeDuration);
        enabled = TimeExt.Since(startTime) < fadeDuration;
    }
}
