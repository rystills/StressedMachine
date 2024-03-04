using UnityEngine;

public class Player : MonoBehaviour
{
    public static new Transform transform;
    public static Player instance;

    private void Awake()
    {
        instance = this;
        transform = GetComponent<Transform>();
    }
}
