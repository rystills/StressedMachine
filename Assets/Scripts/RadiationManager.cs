using UnityEngine;

public class RadiationManager : MonoBehaviour
{
    [SerializeField] private Material mat;
    [SerializeField] private HingeDoor door;
    [SerializeField] private float radiationIncr;
    [SerializeField] private float radiationDecr;
    public static float radiationLevel;

    private void Update()
    {
        radiationLevel = Mathf.Clamp01(radiationLevel + (door.openPercentage * radiationIncr
                       - (int)(1 - door.openPercentage) * radiationDecr) * Time.deltaTime);
        mat.SetFloat("strength", radiationLevel);
        
        if (radiationLevel == 1) Player.Die();
    }
}
