using UnityEngine;

public class Lever : MonoBehaviour
{
    [SerializeField] private float moveSensitivity = .1f;
    [SerializeField] private float moveRange = .7f;
    [SerializeField] private WaveParticleManager waveController;
    private Collider col;
    private bool interacting;
    private float initialY;

    private void Awake()
    {
        col = GetComponent<Collider>();
        enabled = false;
        initialY = transform.localPosition.y;
    }

    private void OnMouseDown()
    {
        interacting = Player.InRangeOf(col);
        enabled |= interacting;
    }

    private void Update()
    {
        if (enabled = interacting = interacting && Input.GetMouseButton(0))
        {
            // move
            transform.localPosition = new(transform.localPosition.x,
                                          Mathf.Clamp(transform.localPosition.y + Input.GetAxis("Mouse Y") * moveSensitivity,
                                                      initialY - moveRange, initialY + moveRange),
                                          transform.localPosition.z);
        
            // update wave
            waveController.SetHeightOffsetRatio((transform.localPosition.y - initialY) / moveRange);
        }
    }
}
