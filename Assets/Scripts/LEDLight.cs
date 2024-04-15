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
    public int colInd;

    private void Awake()
    {
        lightMat = lightMR.material;
        lightMat.EnableKeyword("_EMISSION");
        lightMat.SetColor("_EmissionColor", lightColors[0]);
    }

    public void SetColInd(int newInd)
    {
        light.color = lightColors[colInd = newInd];
        lightMat.SetColor("_EmissionColor", lightMatEmissionColors[colInd]);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Player.InRangeOf(transform.position, 1.1f))
            SetColInd((colInd + (eventData.button == PointerEventData.InputButton.Left ? 1 : lightColors.Length - 1)) % lightColors.Length);
    }
}
