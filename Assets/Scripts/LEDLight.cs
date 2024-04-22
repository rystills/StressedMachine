using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LEDLight : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private new Light light;
    [SerializeField] private MeshRenderer lightMR;
    private Material lightMat;
    private Color[] lightColors = { Color.red,
                                    Color.green,
                                    Color.blue };
    private Color[] lightMatEmissionColors = { new(2f, 0f, 0f, 4f),
                                               new(0f, 2f, 0f, 4f),
                                               new(0f, 0f, 2f, 4f) };
    [NonSerialized] public int colInd = 2;
    [SerializeField] private AudioSource clickSnd;
    [NonSerialized] public bool locked;

    private void Awake()
    {
        lightMat = lightMR.material;
        lightMat.EnableKeyword("_EMISSION");
        lightMat.SetColor("_EmissionColor", lightColors[2]);
    }

    public void SetColInd(int newInd, bool playSound = true)
    {
        if (playSound) clickSnd.PlayBiDir(newInd == LEDMachine.rgbMax, false);
        colInd = newInd;
        light.color = lightColors[colInd];
        lightMat.SetColor("_EmissionColor", lightMatEmissionColors[colInd]);
        LEDMachine.SendColorsToShader();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!locked && Player.InRangeOf(transform.position, 1.3f))
            SetColInd((colInd + (eventData.button == PointerEventData.InputButton.Left ? 1 : lightColors.Length - 1)) % lightColors.Length);
    }
}
