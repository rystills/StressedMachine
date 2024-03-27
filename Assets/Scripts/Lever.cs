using UnityEngine;

public class Lever : MonoBehaviour
{
    [SerializeField] private float moveSensitivity = .1f;
    [SerializeField] private float moveRange = .7f;
    [SerializeField] private WaveParticleManager waveController;
    private Collider col;
    private bool interacting;
    private float initialY;
    [SerializeField] private AudioSource leverSnd;
    private float activePitch;
    [SerializeField] private float maxPitch;
    [SerializeField] private float pitchDecr;

    private void Awake()
    {
        col = GetComponent<Collider>();
        enabled = false;
        initialY = transform.localPosition.y;
    }

    private void OnMouseDown() => enabled |= (interacting = Player.InRangeOf(col));

    private void Update()
    {
        float moveChange = Input.GetAxis("Mouse Y") * moveSensitivity;
        if (interacting = interacting && Input.GetMouseButton(0))
        {
            // move
            transform.localPosition = new(transform.localPosition.x,
                                          Mathf.Clamp(transform.localPosition.y + moveChange,
                                                      initialY - moveRange, initialY + moveRange),
                                          transform.localPosition.z);
        
            // update wave
            waveController.SetHeightOffsetRatio((transform.localPosition.y - initialY) / moveRange);
        }

        // update pitch
        if (transform.localPosition.y <= initialY - moveRange || transform.localPosition.y >= initialY + moveRange || !Input.GetMouseButton(0)) 
            moveChange = 0;
        activePitch = Mathf.MoveTowards(Mathf.Clamp(activePitch + Mathf.Sqrt(moveChange == 0 ? 0 : Mathf.Max(Mathf.Abs(moveChange), .005f)) * Mathf.Sign(moveChange) * 2.5f, 
                                        -maxPitch, maxPitch), 0, pitchDecr * Time.deltaTime);
        if (activePitch == 0)
        {
            leverSnd.Stop();
            enabled = interacting;

        }
        else
        {
            leverSnd.PlayBiDir();
            leverSnd.pitch = Mathf.Abs(activePitch);
        }
    }
}
