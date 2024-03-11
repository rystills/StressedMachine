using UnityEngine;
using UnityEngine.UI;

public class DeathAnimation : MonoBehaviour
{
    public static DeathAnimation instance;
    [SerializeField] private float animOutDir;
    private static Material mat;
    private static float deathTime;
    private float prevElapsed;

    private void Awake()
    {
        instance = this;
        mat = GetComponent<Image>().material;
        mat.SetFloat("animOutDir", animOutDir);
        gameObject.SetActive(false);
    }

    public static bool Play()
    {
        if (instance.gameObject.activeSelf) return false;
        mat.SetFloat("startTime", deathTime = Time.timeSinceLevelLoad);
        instance.gameObject.SetActive(true);
        instance.prevElapsed = 0;
        return true;
    }

    private void Update()
    {
        float elapsedFrac = (Time.timeSinceLevelLoad - deathTime) / animOutDir;
        if (elapsedFrac >= .5f && prevElapsed < .5f)
            GameState.Load();
        gameObject.SetActive(elapsedFrac < 1);
        prevElapsed = elapsedFrac;
    }
}
